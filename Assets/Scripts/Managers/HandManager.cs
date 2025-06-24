using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages hand-related data and functions in the application. This includes tracking hand movements,
/// gestures, and providing utilities to interact with hand data across different input devices.
/// </summary>
public class HandManager : MonoBehaviour
{
	public static HandManager Instance;

    [SerializeField]
    private LeapHandTracking _trackingData;

	[SerializeField]
	private RgbDataTransformer _trackingDataRgb;

	[SerializeField]
	private GameObject _leapHandsPrefab;

	private GameObject _rgbLeftHand;
	private GameObject _rgbRightHand;

	private GameObject _player;

    private HandBase _leftHand;
    private HandBase _rightHand;

	private GameObject _lastHands;

	private Vector3 _leftPrevPosition = Vector3.zero;
	private Vector3 _rightPrevPosition = Vector3.zero;

	private float _facingPlayerDotProductLimit = 0.6f;

	// Singleton
	public void Awake()
	{
		_rgbLeftHand = GameObject.FindGameObjectWithTag("Player").transform.GetChild(1).gameObject;
		_rgbRightHand = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).gameObject;

		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	/// <summary>
	/// Reinitializes hand representations when a new scene is loaded.
	/// </summary>
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (_lastHands != null) Destroy(_lastHands);
		_lastHands = Instantiate(_leapHandsPrefab, transform.position, transform.rotation, transform);

		_player = GameObject.FindGameObjectWithTag("Player");
		if (_player != null)
		{
			_rgbLeftHand = _player.transform.GetChild(1).gameObject;
			_rgbRightHand = _player.transform.GetChild(0).gameObject;
		}
	}

	/// <summary>
	/// Updates hand data each frame based on the current device type and handles the activation and
	/// deactivation of hand representations.
	/// </summary>
	void Update()
    {
		if (GameManager.Instance.DeviceType == InputDeviceType.RGBCamera)
		{
			_leftHand = _trackingDataRgb.LeftHand;
			_rightHand = _trackingDataRgb.RightHand;
			if(_lastHands != null)
			{
				_lastHands.SetActive(false);
			}

			if(_rgbRightHand != null && _rgbLeftHand != null) { 
				_rgbRightHand.SetActive(_rightHand != null);
				_rgbLeftHand.SetActive(_leftHand != null);
			}
		}
		else if (GameManager.Instance.DeviceType == InputDeviceType.UltraLeap)
		{
			if (_lastHands != null)
			{
				_lastHands.SetActive(true);
			}

			_leftHand = _trackingData.LeftLeapHand;
			_rightHand = _trackingData.RightLeapHand;
			if (_rightHand != null && float.IsNaN(_rightHand.PalmVelocity.y))
			{
				Debug.LogError("Leap hand velocity not calculated properly. Please restart the service.");
			}
		}

		_leftPrevPosition = _leftHand != null ? _leftHand.PalmPosition : _leftPrevPosition;
		_rightPrevPosition = _rightHand != null ? _rightHand.PalmPosition : _rightPrevPosition;
	}

	/// <summary>
	/// Returns the current palm position of the specified hand.
	/// </summary>
	public Vector3 GetPalmPosition(HandType handeness)
	{
		HandBase hand = GetHandByType(handeness);
		return hand.PalmPosition;
	}

	/// <summary>
	/// Returns the elbow and wrist positions for constructing the arm line of the specified hand.
	/// </summary>
	public List<Vector3> GetElbowPositions(HandType handeness)
	{
		HandBase hand = GetHandByType(handeness);

		List<Vector3> arm = new List<Vector3>
		{
			hand.Arm != null ? hand.Arm.ElbowPosition : hand.Wrist + Vector3.down,
			hand.Wrist
		};

		return arm;
	}

	/// <summary>
	/// Checks if a hand is moving with a velocity exceeding the specified threshold.
	/// </summary>
	public bool IsHandMoving(HandType handeness, float velocityNeeded)
	{
		return IsHandMovingUp(handeness, velocityNeeded) || IsHandMovingDown(handeness, velocityNeeded);
	}

	/// <summary>
	/// Checks if a hand is closed (all fingers folded).
	/// </summary>
	public bool IsHandClosed(HandType handeness)
	{
		HandBase hand = GetHandByType(handeness);
		if (hand == null) return false;

		foreach (var finger in hand.Fingers)
		{
			if (finger.IsExtended)
				return false;
				
		}
		return true;
	}

	/// <summary>
	/// Checks if a hand is open (all fingers extended).
	/// </summary>
	public bool IsHandOpen(HandType handeness)
	{
		HandBase hand = GetHandByType(handeness);
		if (hand == null) return false;

		foreach (var finger in hand.Fingers)
		{
			if (!finger.IsExtended)
				return false;
		}
		return true;
	}

	/// <summary>
	/// Determines if both hands are open.
	/// </summary>
	public bool AreBothHandsOpen()
	{
		return IsHandOpen(HandType.Left) && IsHandOpen(HandType.Right);
	}

	/// <summary>
	/// Checks if a specific finger is extended in the specified hand.
	/// </summary>
	/// <param name="handeness">Which hand to check.</param>
	/// <param name="fingerIndex">Index of the finger to check.</param>
	/// <returns>True if the finger is extended; otherwise, false.</returns>
	public bool IsFingerExtended(HandType handeness, int fingerIndex)
	{
		HandBase hand = GetHandByType(handeness);
		if (hand == null) return false;
		return hand.Fingers[fingerIndex].IsExtended;
	}

	/// <summary>
	/// Determines if the hand is moving upwards faster than the specified velocity threshold.
	/// </summary>
	/// <param name="handeness">Which hand to check.</param>
	/// <param name="velocityNeeded">Velocity threshold to exceed.</param>
	/// <returns>True if moving up faster than the threshold; otherwise, false.</returns>
	public bool IsHandMovingUp(HandType handeness, float velocityNeeded)
	{
		HandBase hand = GetHandByType(handeness);
		if (hand == null) return false;

		bool enoughVelocity = hand.PalmVelocity.y > velocityNeeded;

		return enoughVelocity;
	}

	/// <summary>
	/// Determines if the hand is moving forward relative to the player's facing direction, exceeding a specified velocity.
	/// </summary>
	/// <param name="handeness">Which hand to check.</param>
	/// <param name="velocityNeeded">Velocity threshold to exceed.</param>
	/// <returns>True if moving forward faster than the threshold; otherwise, false.</returns>
	public bool IsHandMovingForward(HandType handeness, float velocityNeeded)
	{
		HandBase hand = GetHandByType(handeness);
		if (hand == null) return false;

		var playerForward = _player.transform.forward;
		Vector3 handVelocityDirection = hand.PalmVelocity.normalized;

		float dotProduct = Vector3.Dot(handVelocityDirection, playerForward);

		bool isMovingForward = dotProduct > 0;
		bool enoughVelocity = hand.PalmVelocity.magnitude > velocityNeeded;

		return isMovingForward && enoughVelocity;
	}

	/// <summary>
	/// Determines the direction of a specific finger in the specified hand.
	/// </summary>
	/// <param name="handeness">Which hand to check.</param>
	/// <param name="fingerIndex">Index of the finger to check.</param>
	/// <returns>Direction vector of the finger; returns Vector3.zero if hand is not present.</returns>
	public Vector3 GetFingerDirection(HandType handeness, int fingerIndex)
	{
		if (!IsHandPresent(handeness)) return Vector3.zero;
		HandBase hand = GetHandByType(handeness);
		FingerBase finger = hand.Fingers[fingerIndex];
		return finger.Direction;
	}

	/// <summary>
	/// Checks if the hand is moving downwards faster than the specified velocity threshold.
	/// </summary>
	/// <param name="handeness">Which hand to check.</param>
	/// <param name="velocityNeeded">Velocity threshold to exceed.</param>
	/// <returns>True if moving down faster than the threshold; otherwise, false.</returns>
	public bool IsHandMovingDown(HandType handeness, float velocityNeeded)
	{
		HandBase hand = GetHandByType(handeness);
		if (hand == null) return false;

		bool enoughVelocity = hand.PalmVelocity.y < -velocityNeeded;

		return enoughVelocity;
	}

	/// <summary>
	/// Returns hand reference based on chirality. 
	/// </summary>
	/// <param name="chirality">Left/Right</param>
	/// <returns>Hand reference based on desired handeness</returns>
	private HandBase GetHandByType(HandType chirality)
	{
		return chirality == HandType.Left ? _leftHand : _rightHand;
	}

	/// <summary>
	/// Checks if a hand is present by retrieving the specified hand's data.
	/// </summary>
	/// <param name="handeness">Which hand to check.</param>
	/// <returns>True if the hand data is available; otherwise, false.</returns>
	public bool IsHandPresent(HandType handeness)
	{
		HandBase hand = GetHandByType(handeness);
		return hand != null;
	}

	/// <summary>
	/// Determines if both hands are present by checking the availability of both left and right hand data.
	/// </summary>
	/// <returns>True if both hands are present; otherwise, false.</returns>
	public bool AreBothHandsPresent()
	{
		return IsHandPresent(HandType.Left) && IsHandPresent(HandType.Right);
	}

	/// <summary>
	/// Checks if both hands are closed (all fingers folded) by evaluating the closed state of each hand.
	/// </summary>
	/// <returns>True if both hands are closed; otherwise, false.</returns>
	public bool AreBothHandsClosed()
	{
		return IsHandClosed(HandType.Left) && IsHandClosed(HandType.Right);
	}

	/// <summary>
	/// Determines if the hand is pointing by checking if the index finger is extended and all other fingers are folded.
	/// </summary>
	/// <param name="handeness">Which hand to check.</param>
	/// <returns>True if the hand is pointing as defined; otherwise, false.</returns>
	public bool IsHandPointing(HandType handeness)
	{
		HandBase hand = GetHandByType(handeness);

		foreach (var finger in hand.Fingers)
		{
			if (finger.Type == FingerBase.FingerType.TYPE_INDEX)
			{
				if (!finger.IsExtended) return false;
			}
			else if(finger.IsExtended)
				return false;
		}

		return true;
	}

	/// <summary>
	/// Checks if the hand is closed but the thumb is extended.
	/// </summary>
	/// <param name="handeness">Which hand to check.</param>
	/// <returns>True if the hand is closed except for the thumb; otherwise, false.</returns>
	public bool IsHandClosedWithoutThumb(HandType handeness)
	{
		HandBase hand = GetHandByType(handeness);

		if (IsHandClosed(handeness)) return true;

		int[] closedIndexes = { 1, 2, 3, 4 };

		foreach(int i in closedIndexes)
		{
			if (hand.Fingers[i].IsExtended) return false;
		}

		return true;
	}

	/// <summary>
	/// Checks if both hands are closed except for the thumbs.
	/// </summary>
	/// <returns>True if both hands meet the criteria; otherwise, false.</returns>

	public bool AreHandsClosedWithoutThumbs()
	{
		return IsHandClosedWithoutThumb(HandType.Left) && IsHandClosedWithoutThumb(HandType.Right);
	}

	/// <summary>
	/// Returns the tip position of a specified finger on the given hand.
	/// </summary>
	/// <param name="handeness">Which hand to check.</param>
	/// <param name="index">Index of the finger to check.</param>
	/// <returns>Position vector of the finger tip.</returns>
	public Vector3 GetTipPosition(HandType handeness, int index)
	{
		HandBase hand = GetHandByType(handeness);
		return hand.Fingers[index].TipPosition;
	}

	/// <summary>
	/// Gets the normal of the palm for the specified hand.
	/// </summary>
	/// <param name="handeness">Which hand to check.</param>
	/// <returns>Normal vector of the palm.</returns>

	public Vector3 GetPalmNormal(HandType handeness)
	{
		HandBase hand = GetHandByType(handeness);
		return hand.PalmNormal;
	}

	/// <summary>
	/// Retrieves the pointing direction of the index finger for the specified hand.
	/// </summary>
	/// <param name="handeness">Which hand to check.</param>
	/// <returns>Direction vector of the index finger.</returns>
	public Vector3 GetPointingDirection(HandType handeness) {
		HandBase hand = GetHandByType(handeness);
		return hand.Fingers[1].Direction;
	}

	/// <summary>
	/// Checks if the specified hand is facing the player.
	/// </summary>
	/// <param name="handeness">Which hand to check.</param>
	/// <returns>True if the palm normal is facing towards the player, otherwise false.</returns>

	public bool IsHandFacingPlayer(HandType handeness)
	{
		Vector3 palmNormal = GetPalmNormal(handeness);

		float dotProduct = Vector3.Dot(_player.transform.forward, palmNormal);

		return dotProduct < -_facingPlayerDotProductLimit;
	}

	/// <summary>
	/// Checks if the specified hand is facing away from the player.
	/// </summary>
	/// <param name="handeness">Which hand to check.</param>
	/// <returns>True if the palm normal is facing away from the player, otherwise false.</returns>

	public bool IsHandFacingFromPlayer(HandType handeness)
	{
		Vector3 palmNormal = GetPalmNormal(handeness);

		float dotProduct = Vector3.Dot(_player.transform.forward, palmNormal);

		return dotProduct > _facingPlayerDotProductLimit;
	}

	/// <summary>
	/// Checks if both hands are facing the player.
	/// </summary>
	/// <returns>True if both hands are facing the player, otherwise false.</returns>

	public bool AreHandsFacingPlayer()
	{
		return IsHandFacingPlayer(HandType.Left) && IsHandFacingPlayer(HandType.Right);
	}

	/// <summary>
	/// Checks if a specific hand is displaying a peace sign gesture.
	/// </summary>
	/// <param name="handeness">Which hand to check.</param>
	/// <returns>True if the hand forms a peace sign, otherwise false.</returns>

	public bool IsPeaceSign(HandType handeness)
	{
		if (!IsHandPresent(handeness)) return false;
		return IsFingerExtended(handeness, 1)
			&& IsFingerExtended(handeness, 2)
			&& !IsFingerExtended(handeness, 3)
			&& !IsFingerExtended(handeness, 4)
			&& !IsFingerExtended(handeness, 0);
	}

	/// <summary>
	/// Checks if both hands are making a peace sign gesture.
	/// </summary>
	/// <returns>True if both hands form peace signs, otherwise false.</returns>

	public bool BothHandsDoingPeaceSign()
	{
		return IsPeaceSign(HandType.Left) && IsPeaceSign(HandType.Right);
	}

	/// <summary>
	/// Calculates the rotation of a specific bone in a finger of the specified hand.
	/// </summary>
	/// <param name="handeness">Which hand to check.</param>
	/// <param name="fingerIndex">Index of the finger.</param>
	/// <param name="boneType">Type of bone to calculate rotation for.</param>
	/// <returns>Quaternion representing the rotation of the bone.</returns>

	public Quaternion GetFingerBoneRotation(HandType handeness, int fingerIndex, BoneBase.BoneType boneType)
	{
		HandBase hand = GetHandByType(handeness);
		FingerBase finger = hand.Fingers[fingerIndex];
		BoneBase bone = finger.GetBone(boneType);

		BoneBase previousBone = finger.GetBone(GetPreviousBoneType(boneType));

		Vector3 a = previousBone.PrevJoint;
		Vector3 b = bone.PrevJoint;
		Vector3 c = bone.NextJoint;

		Vector3 BA = a - b;
		Vector3 BC = c - b;

		float angleRadians = Mathf.Acos(Vector3.Dot(BA.normalized, BC.normalized));
		float angleDegrees = angleRadians * Mathf.Rad2Deg;
		float newZ = Mathf.Clamp(Mathf.Abs(angleDegrees - 180), 0, 90);
		return Quaternion.Euler(0, 0, -newZ);

	}

	/// <summary>
	/// Returns the bone type that precedes the given bone type in the hand structure.
	/// </summary>
	/// <param name="type">Type of bone to find the predecessor for.</param>
	/// <returns>Type of the preceding bone, or TYPE_INVALID if none exists.</returns>

	public BoneBase.BoneType GetPreviousBoneType(BoneBase.BoneType type)
	{
		switch(type)
		{
			case BoneBase.BoneType.TYPE_METACARPAL:
				return BoneBase.BoneType.TYPE_INVALID;
			case BoneBase.BoneType.TYPE_PROXIMAL:
				return BoneBase.BoneType.TYPE_METACARPAL;
			case BoneBase.BoneType.TYPE_INTERMEDIATE:
				return BoneBase.BoneType.TYPE_PROXIMAL;
			case BoneBase.BoneType.TYPE_DISTAL:
				return BoneBase.BoneType.TYPE_INTERMEDIATE;
			default:
				return BoneBase.BoneType.TYPE_INVALID;
		}
	}

	/// <summary>
	/// Checks if a "thumbs down" gesture is being made with the specified hand.
	/// </summary>
	public bool ThumbsDownGesture(HandType handeness)
	{
		if (!IsHandPresent(handeness)) return false;
		bool thumbOpen = !IsFingerExtended(handeness, 1)
			&& !IsFingerExtended(handeness, 2)
			&& !IsFingerExtended(handeness, 3)
			&& !IsFingerExtended(handeness, 4)
			&& IsFingerExtended(handeness, 0);

		Vector3 direction = GetFingerDirection(handeness, 0);

		float dotProduct = Vector3.Dot(Vector3.down, direction);

		return thumbOpen && dotProduct > 0.6f;

	}
}
