using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Transforms 2D hand data received from an external source into 3D space within Unity.
/// </summary>
public class RgbDataTransformer : MonoBehaviour
{
	private const int ORIGINAL_SCREEN_WIDTH = 600;
	private const int ORIGINAL_SCREEN_HEIGHT = 600;
	private const int NORMALIZE_DEPTH_FACTOR = 1000;

	private const float X_OFFSET = -0.8f;
	private const float Y_OFFSET = -0.9f;

	private RgbConnectionManager _dataReceiver;
	private Transform _playerTransform;

	public Action<RgbHand> OnHandCreated;

	// Storage for hand positions differentiated by hand type
	private Dictionary<HandType, List<Vector3>> _handPositions = new Dictionary<HandType, List<Vector3>>
	{
		{ HandType.Right, new List<Vector3>() },
		{ HandType.Left, new List<Vector3>() }
	};

	[SerializeField] private float _distanceFromPlayer = 1f;
	[SerializeField] private float _size = 3f;

	public RgbHand RightHand;
	public RgbHand LeftHand;

	private void Start()
	{
		_playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
		_dataReceiver = RgbConnectionManager.Instance;
	}

	void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		_playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
		_dataReceiver = RgbConnectionManager.Instance;
	}

	/// <summary>
	/// Continuously transforms received 2D data into 3D hand positions and structures.
	/// </summary>
	private void Update()
	{
		if(GameManager.Instance.DeviceType == InputDeviceType.RGBCamera)
			TransformData(_dataReceiver.Data);
	}

	/// <summary>
	/// Transforms received string data into 3D structures, normalizes joint positions,
	/// corrects hand size, and applies positional offsets.
	/// </summary>
	/// <param name="receivedData">String data representing hand joint positions.</param>
	private void TransformData(string receivedData)
	{
		if (receivedData == "[]")
		{
			RightHand = LeftHand = null;
			return;
		}

		ParseData(receivedData);

		foreach (var hand in _handPositions)
		{
			if (hand.Value.Count == 0)
			{
				if (hand.Key == HandType.Left) LeftHand = null;
				else RightHand = null;

				continue;
			}

			NormalizeJointPositions(hand.Value);
			CorrectHandSize(hand.Value);
			
			TransformToPlayerSpace(hand.Value);

			CorrectJointPositions(hand.Value);

			//CorrectSize(hand.Value);

			OffsetForward(hand.Value, _distanceFromPlayer);

			float distance = GetCurrentSizeMeasure(hand.Value[5], hand.Value[17]);

			CreateHandStructures(hand.Value, hand.Key);
		}

		OnHandCreated?.Invoke(RightHand);

		ClearTmpData();
	}

	/// <summary>
	/// Processes received data to create hand structures.
	/// </summary>
	/// <param name="jointPositions">The list of joint positions for a hand.</param>
	/// <param name="type">The type of the hand (left or right).</param>
	private void CreateHandStructures(List<Vector3> jointPositions, HandType type)
	{
		var bones = MakeBones(jointPositions);
		var fingers = MakeFingers(bones);
		var palm = CreatePalm(jointPositions, type);
		CreateHand(fingers, palm, type);
	}

	/// <summary>
	/// Normalizes joint positions based on original screen dimensions and depth factor.
	/// </summary>
	/// <param name="jointPositions">The list of joint positions for a hand.</param>
	private void NormalizeJointPositions(List<Vector3> jointPositions)
	{
		float screenWidth = ORIGINAL_SCREEN_WIDTH; 
		float screenHeight = ORIGINAL_SCREEN_HEIGHT;

		for (int i = 0; i < jointPositions.Count; i++)
		{
			Vector3 normalizedPos = new Vector3(
				jointPositions[i].x / screenWidth,
				jointPositions[i].y / screenHeight,
				jointPositions[i].z / NORMALIZE_DEPTH_FACTOR);

			jointPositions[i] = normalizedPos;
		}
	}

	/// <summary>
	/// Measures the distance between the under index and under pinky joints to determine hand size.
	/// </summary>
	/// <param name="underIndex">Position of the joint under the index finger.</param>
	/// <param name="underPinky">Position of the joint under the pinky finger.</param>
	/// <returns>The distance measured.</returns>
	private float GetCurrentSizeMeasure(Vector3 underIndex,  Vector3 underPinky)
	{
		return Vector3.Distance(underPinky, underIndex);

	}

	/// <summary>
	/// Transforms all joint positions to player's space.
	/// </summary>
	/// <param name="jointPositions">The list of joint positions for a hand.</param>
	private void TransformToPlayerSpace(List<Vector3> jointPositions)
	{
		for (int i = 0; i < jointPositions.Count; i++)
		{
			jointPositions[i] = _playerTransform.TransformPoint(jointPositions[i]);
		}

	}

	/// <summary>
	/// Offset all joint positions in player's forward direction.
	/// </summary>
	/// <param name="jointPositions">The list of joint positions for a hand.</param>
	/// <param name="distanceForward">Distance to move the joints forward.</param>
	private void OffsetForward(List<Vector3> jointPositions, float distanceForward)
	{
		Vector3 forwardOffset = _playerTransform.forward * distanceForward;

		for (int i = 0; i < jointPositions.Count; i++)
		{
			jointPositions[i] = jointPositions[i] + forwardOffset;
		}
	}

	/// <summary>
	/// Resize the hand data to make the hand bigger.
	/// </summary>
	/// <param name="jointPositions">The list of joint positions for a hand.</param>
	private void CorrectHandSize(List<Vector3> jointPositions)
	{
		for (int i = 0; i < jointPositions.Count; i++)
		{
			jointPositions[i] = jointPositions[i] * _size;
		}

	}

	/// <summary>
	/// Clears temporary data for both hands.
	/// </summary>
	private void ClearTmpData()
	{
		_handPositions[HandType.Right].Clear();
		_handPositions[HandType.Left].Clear();
	}

	/// <summary>
	/// Offsets joint positions on x and y axis, to achieve optimal hands positions.
	/// </summary>
	/// <param name="jointPositions">The list of joint positions for a hand.</param>
	private void CorrectJointPositions(List<Vector3> jointPositions)
	{
		for (int i = 0; i < jointPositions.Count; i++)
		{
			jointPositions[i] += _playerTransform.right * X_OFFSET;
			jointPositions[i] += _playerTransform.up * Y_OFFSET;
		}
	}

	/// <summary>
	/// Creates bone structures from joint data.
	/// </summary>
	/// <param name="jointPositions">The list of joint positions for a hand.</param>
	/// <returns>List of bone structures created.</returns>
	private List<RgbBone> MakeBones(List<Vector3> jointPositions)
	{
		List<RgbBone> bones = new List<RgbBone>
		{
			// Thumb
			new RgbBone(jointPositions[1], jointPositions[2], RgbBone.BoneType.TYPE_PROXIMAL),
			new RgbBone(jointPositions[2], jointPositions[3], RgbBone.BoneType.TYPE_INTERMEDIATE),
			new RgbBone(jointPositions[3], jointPositions[4], RgbBone.BoneType.TYPE_DISTAL),

			// Index
			new RgbBone(jointPositions[0], jointPositions[5], RgbBone.BoneType.TYPE_METACARPAL),
			new RgbBone(jointPositions[5], jointPositions[6], RgbBone.BoneType.TYPE_PROXIMAL),
			new RgbBone(jointPositions[6], jointPositions[7], RgbBone.BoneType.TYPE_INTERMEDIATE),
			new RgbBone(jointPositions[7], jointPositions[8], RgbBone.BoneType.TYPE_DISTAL),

			// Middle
			new RgbBone(jointPositions[0], jointPositions[9], RgbBone.BoneType.TYPE_METACARPAL),
			new RgbBone(jointPositions[9], jointPositions[10], RgbBone.BoneType.TYPE_PROXIMAL),
			new RgbBone(jointPositions[10], jointPositions[11], RgbBone.BoneType.TYPE_INTERMEDIATE),
			new RgbBone(jointPositions[11], jointPositions[12], RgbBone.BoneType.TYPE_DISTAL),

			// Ring
			new RgbBone(jointPositions[0], jointPositions[13], RgbBone.BoneType.TYPE_METACARPAL),
			new RgbBone(jointPositions[13], jointPositions[14], RgbBone.BoneType.TYPE_PROXIMAL),
			new RgbBone(jointPositions[14], jointPositions[15], RgbBone.BoneType.TYPE_INTERMEDIATE),
			new RgbBone(jointPositions[15], jointPositions[16], RgbBone.BoneType.TYPE_DISTAL),

			// Pinky
			new RgbBone(jointPositions[0], jointPositions[17], RgbBone.BoneType.TYPE_METACARPAL),
			new RgbBone(jointPositions[17], jointPositions[18], RgbBone.BoneType.TYPE_PROXIMAL),
			new RgbBone(jointPositions[18], jointPositions[19], RgbBone.BoneType.TYPE_INTERMEDIATE),
			new RgbBone(jointPositions[19], jointPositions[20], RgbBone.BoneType.TYPE_DISTAL)
		};

		Debug.DrawLine(jointPositions[5], jointPositions[8], Color.cyan);

		return bones;
	}

	/// <summary>
	/// Creates finger structures from bone structures.
	/// </summary>
	/// <param name="bones">The list of bone structures for a hand.</param>
	/// <returns>List of finger structures created.</returns>
	private List<RgbFinger> MakeFingers(List<RgbBone> bones)
	{
		List<RgbFinger> fingers = new List<RgbFinger>
		{
			new RgbFinger(null, bones[0], bones[1], bones[2], RgbFinger.FingerType.TYPE_THUMB),
			new RgbFinger(bones[3], bones[4], bones[5], bones[6], RgbFinger.FingerType.TYPE_INDEX),
			new RgbFinger(bones[7], bones[8], bones[9], bones[10], RgbFinger.FingerType.TYPE_MIDDLE),
			new RgbFinger(bones[11], bones[12], bones[13], bones[14], RgbFinger.FingerType.TYPE_RING),
			new RgbFinger(bones[15], bones[16], bones[17], bones[18], RgbFinger.FingerType.TYPE_PINKY)
		};
		return fingers;
	}

	/// <summary>
	/// Create palm structure, defined by 3 key points on hand.
	/// 1. Wrist, 2. Under pinky, 3. Under index
	/// </summary>
	/// <param name="jointPositions">The list of joint positions for a hand.</param>
	/// <param name="type">The type of the hand (left or right).</param>
	/// <returns></returns>
	private RgbPalm CreatePalm(List<Vector3> jointPositions, HandType type)
	{
		return new RgbPalm(jointPositions[0], jointPositions[5], jointPositions[17], type);
	}


	private void CreateHand(List<RgbFinger> fingers, RgbPalm palm, HandType type)
	{
		if (type == HandType.Right)
			RightHand = new RgbHand(fingers, palm, type);
		else
			LeftHand = new RgbHand(fingers, palm, type);
	}

	/// <summary>
	/// Parses raw data received from external source to Vector3 data about each hand.
	/// </summary>
	/// <param name="data">Raw data coming from external source passed by RgbDataReciever.</param>
	private void ParseData(string data)
	{
		// Remove the outer brackets
		data = data.Trim(new char[] { '[', ']' });
		string[] parts = data.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

		HandType currentHand = HandType.Invalid;
		List<Vector3> currentHandPositions = new List<Vector3>();

		for (int i = 0; i < parts.Length; i++)
		{
			// Check for hand identifier
			if (parts[i].Trim() == "'R'" || parts[i].Trim() == "'L'")
			{
				// If not the first hand, add the previous hand's positions to the dictionary
				if (currentHand != HandType.Invalid)
				{
					_handPositions[currentHand] = new List<Vector3>(currentHandPositions);
					currentHandPositions.Clear();
				}

				// Update current hand identifier
				currentHand = parts[i].Trim().Trim('\'') == "R" ? HandType.Right : HandType.Left;
			}
			else
			{
				// Parse coordinate data
				try
				{
					float x = float.Parse(parts[i]);
					float y = float.Parse(parts[++i]);
					float z = -float.Parse(parts[++i]); 

					currentHandPositions.Add(new Vector3(x, y, z));
				}
				catch (Exception ex)
				{
					Debug.LogError("Error parsing coordinate data: " + ex.Message);
				}
			}
		}

		// Add the last hand's positions to the dictionary
		if (currentHand != HandType.Invalid && currentHandPositions.Count > 0)
		{
			_handPositions[currentHand] = new List<Vector3>(currentHandPositions);
		}
	}
}
