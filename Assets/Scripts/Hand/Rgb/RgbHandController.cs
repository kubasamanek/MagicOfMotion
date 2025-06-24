using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls and updates the hand model's position and rotation based on RGB hand tracking data.
/// </summary>
public class RgbHandController : MonoBehaviour
{

	[SerializeField] public GameObject[] boneObjects;

    [SerializeField] private GameObject _handModel;

	private RgbDataTransformer _transformer;

	[SerializeField] public HandType HandType;

	private float _xPositionMultiplier = 1.0f;
	private float _yPositionMultiplier = 1.0f;

	private Transform _playerTransform;

	private float _rotationSmoothSpeed = 5f;
	private float _positionSmoothSpeed = 8f;

	void Start()
    {
		_playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
		_transformer = GameObject.FindGameObjectWithTag("TrackingManager").GetComponent<RgbDataTransformer>();
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
		_transformer = GameObject.FindGameObjectWithTag("TrackingManager").GetComponent<RgbDataTransformer>();
	}


	// Update is called once per frame
	void Update()
	{
		RgbHand hand = HandType == HandType.Right ? _transformer.RightHand : _transformer.LeftHand;
		if (hand == null) return;
		MoveAndRotateHand(hand);
	}

	/// <summary>
	/// Moves and rotates the hand model according to the specified hand tracking data.
	/// Applies smooth transitions to both position and rotation.
	/// </summary>
	/// <param name="hand">The hand data used to update the model.</param>
	private void MoveAndRotateHand(RgbHand hand) 
	{
		Quaternion palmRotation = hand.Rotation;
		Vector3 handPosition = _playerTransform.InverseTransformPoint(hand.Wrist);

		// Slerp for smooth rotation and translation
		_handModel.transform.rotation = Quaternion.Slerp(_handModel.transform.rotation, palmRotation, _rotationSmoothSpeed * Time.deltaTime);
		_handModel.transform.localPosition = Vector3.Slerp(_handModel.transform.localPosition, handPosition, _positionSmoothSpeed * Time.deltaTime);
	}


}
