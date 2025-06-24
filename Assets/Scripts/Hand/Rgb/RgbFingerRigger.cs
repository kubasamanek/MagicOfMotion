using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the rigging of fingers in a 3D model based on RGB hand tracking data.
/// It uses smoothing to transition finger movements for a more natural motion in a virtual environment.
/// </summary>
public class RgbFingerRigger : MonoBehaviour
{
    [System.Serializable]
    private class Finger
	{
		public Transform metacarpal;
		public Transform proximal;
		public Transform middle;
		public Transform distal;
    }

    [SerializeField] private List<Finger> _fingers = new List<Finger>();

	[SerializeField] private Finger _thumb; 

    private float _fingerRotationSmoothSpeed = 12f;
    private HandType _handType;

	private HandManager _handManager;

    void Start()
    {
        _handType = GetComponent<RgbHandController>().HandType;
		_handManager = HandManager.Instance;
    }

    void Update()
    {
        if (!_handManager.IsHandPresent(_handType)) return;

		UpdateFingerRotations();		
    }

	/// <summary>
	/// Update all fingers' rotations based on transformed data from the hand tracking.
	/// This function interpolates between the current and target rotations to achieve smooth transitions.
	/// </summary>
	private void UpdateFingerRotations()
	{
		for (int i = 0; i < _fingers.Count; i++)
		{
			// Slerp smoothens the rotation towards the target bone rotation for each phalanx.

			_fingers[i].proximal.localRotation =
				Quaternion.Slerp(
					_fingers[i].proximal.localRotation,
					_handManager.GetFingerBoneRotation(_handType, _fingers.Count - i, BoneBase.BoneType.TYPE_PROXIMAL),
					_fingerRotationSmoothSpeed * Time.deltaTime);

			_fingers[i].middle.localRotation =
				Quaternion.Slerp(
					_fingers[i].middle.localRotation,
					_handManager.GetFingerBoneRotation(_handType, _fingers.Count - i, BoneBase.BoneType.TYPE_INTERMEDIATE),
					_fingerRotationSmoothSpeed * Time.deltaTime);

			_fingers[i].distal.localRotation =
				Quaternion.Slerp(
					_fingers[i].distal.localRotation,
					_handManager.GetFingerBoneRotation(_handType, _fingers.Count - i, BoneBase.BoneType.TYPE_DISTAL),
					_fingerRotationSmoothSpeed * Time.deltaTime);

			// Calculate the average rotation z-component and apply a target rotation to the metacarpal.
			float average = (_fingers[i].proximal.localRotation.z + _fingers[i].middle.localRotation.z + _fingers[i].distal.localRotation.z) / 3.0f;
			Vector3 currentEulers = _fingers[i].metacarpal.localEulerAngles;
			Quaternion targetRotation = Quaternion.Euler(currentEulers.x, currentEulers.y, average * 60f);

			_fingers[i].metacarpal.localRotation = Quaternion.Slerp(
				_fingers[i].metacarpal.localRotation,
				targetRotation,
				_fingerRotationSmoothSpeed * Time.deltaTime);
		}

		// Special handling for thumb metacarpal and proximal rotations, which change based on whether the hand is open or closed.
		Quaternion targetOpenMetacarpal = Quaternion.Euler(43f, -2.5f, -6f);
		Quaternion targetClosedMetacarpal = Quaternion.Euler(43f, 20f, -15f);
		Quaternion metacarpalTarget = _handManager.IsHandClosed(_handType) ? targetClosedMetacarpal : targetOpenMetacarpal;

		Quaternion targetOpenProximal = Quaternion.Euler(6.4f, -2.5f, -27f);
		Quaternion targetClosedProximal = Quaternion.Euler(-15f, -10f, -27f);
		Quaternion proximalTarget = _handManager.IsHandClosed(_handType) ? targetClosedProximal : targetOpenProximal;


		_thumb.metacarpal.localRotation = Quaternion.Slerp(
				_thumb.metacarpal.localRotation,
				metacarpalTarget,
				_fingerRotationSmoothSpeed * Time.deltaTime
			);

		_thumb.middle.localRotation = Quaternion.Slerp(
				_thumb.middle.localRotation,
				proximalTarget,
				_fingerRotationSmoothSpeed * Time.deltaTime
			);
	}
}
