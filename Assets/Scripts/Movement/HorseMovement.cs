using UnityEngine;

public class HorseMovement : MonoBehaviour
{
	private HandManager _handManager;
	[SerializeField] private Animator _animator;

	[SerializeField] private GameObject _leftRopePoint;
	[SerializeField] private GameObject _rightRopePoint;

	[SerializeField] private LineRenderer _lineRenderer;
	[SerializeField] private BoxCollider _rightRope;
	[SerializeField] private BoxCollider _leftRope;

	private Rigidbody _rb;

	public float MaxSlopeAngle;
	public bool HoldingRopes = false;

	private RaycastHit _groundHit;
	private Vector3 _currForwardVector;

	[SerializeField] private Transform _horseHeadBone;
	private float _targetHeadRotation;

	[SerializeField] private float _speed;
	private float _rotationSpeed = 30f;

	private float _startMovementDistance = 0.25f;
	private float _startTurningDistance = 0.05f;

	private bool _turning = false;
	private bool _movingForward = false;

	private AudioSource _horseRunningSource;
	private bool _runningSoundFadingOut;

	private void Start()
	{
		_handManager = HandManager.Instance;
		_rb = GetComponent<Rigidbody>();
		_targetHeadRotation = 0;
		_horseRunningSource = AudioManager.Instance.GetSFXSource("HorseRunning");
		GameObject tutorialObject = GameObject.FindGameObjectWithTag("Tutorial");
	}

	void FixedUpdate()
    {
		if (!IsGrounded())
		{
			Vector3 extraGravity = (Physics.gravity * _rb.mass) * 2;  // Increase the multiplier as needed
			_rb.AddForce(extraGravity);
		}
		
		HandleSlopes();

		if (!_handManager.AreBothHandsPresent())
		{
			StopMovement();
			return;
		}

		// Let go of the ropes
		if (!_handManager.AreHandsClosedWithoutThumbs() && HoldingRopes)
			StopMovement();

		if (!HoldingRopes)
			CheckIfPickingRopes();

		// Don't check movement if not holding ropes or hands not closed
		if (!HoldingRopes)
		{
			StopMovement();
			return;
		}

		HandleForwardMovement();
		HandleTurning();

		if (!_movingForward && !_runningSoundFadingOut)
		{
			_speed = 0;
			_animator.SetBool("IsMoving", false);
			_runningSoundFadingOut = true;
			StartCoroutine(AudioManager.Instance.FadeOutSFX("HorseRunning", 0.4f));
		}

		if (_movingForward && !_horseRunningSource.isPlaying && _speed > 0)
		{
			AudioManager.Instance.PlaySFX("HorseRunning");
			_runningSoundFadingOut = false;
		}

		UpdatePitchBasedOnSpeed();
		_animator.SetBool("IsMoving", _movingForward);
		_animator.speed = _speed;
	}

	private void LateUpdate()
	{
		if (HoldingRopes)
			DrawRopes();
	
		UpdateHorseHeadRotation();
	}

	/// <summary>
	/// Update pitch of the horse movement sound to match the speed.
	/// </summary>
	private void UpdatePitchBasedOnSpeed()
	{
		_horseRunningSource.pitch = _speed / 3f;
	}

	/// <summary>
	/// Called when movement stops.
	/// Toggle flags, stop animation, stop sound.
	/// </summary>
	public void StopMovement()
	{
		_speed = 0;
		HoldingRopes = false;
		_lineRenderer.enabled = false;
		_turning = false;

		_animator.SetBool("IsMoving", false);
		if (_horseRunningSource.isPlaying && !_runningSoundFadingOut)
		{
			_runningSoundFadingOut = true;
			StartCoroutine(AudioManager.Instance.FadeOutSFX("HorseRunning", 0.4f));
		}
	}

	/// <summary>
	/// Configure forward vector to match the slope the player is currently grounded on
	/// </summary>
	private void HandleSlopes()
	{
		if(Physics.Raycast(transform.position, Vector3.down, out _groundHit, 2f))
		{
			float angle = Vector3.Angle(Vector3.up, _groundHit.normal);
			_currForwardVector = Vector3.ProjectOnPlane(transform.forward, _groundHit.normal).normalized;
		}
	}

	/// <summary>
	/// Rotate horse head to match the current turning angle
	/// </summary>
	private void UpdateHorseHeadRotation()
	{
		// Move to the center
		if (!HoldingRopes || !_turning)
			_targetHeadRotation = 0f;

		Vector3 currentEulerAngles = _horseHeadBone.transform.localEulerAngles;
		float newZRotation = Mathf.LerpAngle(currentEulerAngles.z, _targetHeadRotation, Time.deltaTime * 5f);
		_horseHeadBone.transform.localEulerAngles = new Vector3(currentEulerAngles.x, currentEulerAngles.y, newZRotation);
	}

	/// <summary>
	/// Handle player turning.
	/// Player turns to the side, where hand is higher than the other.
	/// </summary>
	private void HandleTurning()
	{
		float rightHandY = _handManager.GetPalmPosition(HandType.Right).y;
		float leftHandY = _handManager.GetPalmPosition(HandType.Left).y;
		float handHeightDiff = Mathf.Abs(rightHandY - leftHandY);
		_turning = handHeightDiff >= _startTurningDistance;

		// Check if difference big enough to start turning
		if (_turning)
		{
			bool turnLeft = rightHandY < leftHandY;
			float rotationAmount = handHeightDiff * 30f;

			SmoothRotatePlayer(turnLeft, handHeightDiff);
			_targetHeadRotation = turnLeft ? 20f * handHeightDiff : -20f * handHeightDiff;
		}
	}

	/// <summary>
	/// Handles actual rotation of the player.
	/// The angle of rotation is calculated based on y position difference of the hands.
	/// </summary>
	/// <param name="turnLeft">True if turn left, false if turn right.</param>
	/// <param name="handHeightDiff">Difference in y position of hands.</param>
	private void SmoothRotatePlayer(bool turnLeft, float handHeightDiff)
	{
		float sideMultiplier = turnLeft ? -1 : 1;

		float rotationSpeed = handHeightDiff > 0.25f ? _rotationSpeed * 3 : _rotationSpeed;
		float angle = rotationSpeed * Time.deltaTime * sideMultiplier;

		// Directly modify the transform's rotation to bypass rigidbody constraints
		transform.Rotate(0, angle, 0, Space.Self);
	}

	/// <summary>
	/// Handle moving forward.
	/// Speed is calculated based on average y position of the hands - the higher y, the higher speed.
	/// Also allow for backwards movement when palm normals are facing the player.
	/// </summary>
	private void HandleForwardMovement()
	{
		float leftRopeY = _leftRopePoint.transform.position.y;
		float rightRopeY = _rightRopePoint.transform.position.y;

		float rightDistanceY = _handManager.GetPalmPosition(HandType.Right).y - rightRopeY;
		float leftDistanceY = _handManager.GetPalmPosition(HandType.Left).y - leftRopeY;

		float averageDistance = (rightDistanceY + leftDistanceY) / 2;

		if(rightDistanceY > _startMovementDistance && leftDistanceY > _startMovementDistance)
		{
			var backwardsMultiply = IsMovingBackwards() ? -0.5f : 1;
			_movingForward = true;
			_speed = CalculateSpeed(averageDistance) * backwardsMultiply;
			MoveForward(_speed);
		}
		else
		{
			_movingForward = false;
		}
	}

	/// <summary>
	/// Check if player wants to move backwards.
	/// That happens when both palm normals are facing the player.
	/// </summary>
	/// <returns>True if the player is trying to move backwards.</returns>
	private bool IsMovingBackwards()
	{
		return _handManager.AreHandsFacingPlayer();
	}

	/// <summary>
	/// Speed getter.
	/// </summary>
	/// <returns></returns>
	public float GetCurrentSpeed()
	{
		return _speed;
	}

	/// <summary>
	/// Apply force to move forward.
	/// </summary>
	/// <param name="force">Amount of force to apply in the forward direction.</param>
	private void MoveForward(float force)
	{
		if (!IsGrounded()) force *= 0.5f;
		Vector3 movement = _currForwardVector * force * 8f * 70f; // * 70 because the mass is 70
		_rb.AddForce(movement, ForceMode.Force);
	}

	private bool IsGrounded()
	{
		float rayLength = 3f;
		Vector3 rayStart = transform.position;

		int layerMask = LayerMask.GetMask("Ground");

		bool isGrounded = Physics.Raycast(rayStart, Vector3.down, out _groundHit, rayLength, layerMask);
		Debug.DrawRay(rayStart, Vector3.down * rayLength, isGrounded ? Color.green : Color.red);
		return isGrounded;
	}

	/// <summary>
	/// Calculate speed based on average y distance of the hands from horse head.
	/// </summary>
	/// <param name="averageDistanceY">Average hand distance from head.</param>
	/// <returns></returns>
	private float CalculateSpeed(float averageDistanceY)
	{
		float baseSpeed = 1.0f; 
		float speedIncreaseFactor = 6f; 

		float speedMultiplier = baseSpeed + (averageDistanceY * speedIncreaseFactor);

		return speedMultiplier;
	}

	/// <summary>
	/// Check if player is picking up ropes.
	/// This happens when hands are closed next to horse head.
	/// </summary>
	private void CheckIfPickingRopes()
	{
		if (_leftRope.bounds.Contains(_handManager.GetPalmPosition(HandType.Left))
			&& _rightRope.bounds.Contains(_handManager.GetPalmPosition(HandType.Right))
			&& _handManager.AreHandsClosedWithoutThumbs())
		{
			HoldingRopes = true;
		}
	}

	/// <summary>
	/// Draw lines representing the reins.
	/// </summary>
	private void DrawRopes()
	{
		if (!_handManager.AreBothHandsPresent()) return;

		_lineRenderer.enabled = true;
		_lineRenderer.startWidth = _lineRenderer.endWidth = 0.02f;

		_lineRenderer.positionCount = 4;
		_lineRenderer.SetPosition(0, _handManager.GetPalmPosition(HandType.Left));
		_lineRenderer.SetPosition(1, _leftRopePoint.transform.position);
		_lineRenderer.SetPosition(2, _rightRopePoint.transform.position);
		_lineRenderer.SetPosition(3, _handManager.GetPalmPosition(HandType.Right));
	}
}
