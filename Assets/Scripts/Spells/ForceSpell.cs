using UnityEngine;

public class ForceSpell : LastingSpell
{
	[SerializeField] private float _manaCost;
	[SerializeField] private float _usingManaCost;

	public override float GetManaCost() => _manaCost;
	public override float GetUsingManaCost() => _usingManaCost;

	[SerializeField] private GameObject _forcePrefab;

	private GameObject _forceGameObject;
	private GameObject _controlledObject;

	private Gesture _pushGesture;
	private Gesture _stopGesture;

	private bool _pushStarted = false;
	private bool _stopStarted = false;
	private bool _stopJustFinished = false;
	private bool _pushJustFinished = false;

	private Vector3 _forcePos;
	private bool _forceOn;

	private new void Start()
	{
		base.Start();
		this.HandManager = HandManager.Instance;
		Gesture = GestureFactory.CreateForceGesture(GameManager.Instance.DeviceType);
		_pushGesture = GestureFactory.CreatePushForceGesture(GameManager.Instance.DeviceType);
		_stopGesture = GestureFactory.CreateStopForceGesture(GameManager.Instance.DeviceType);
	}

	new void Update()
	{
		if (Gesture == null) return;
		base.Update();
	}

	public override bool DetectStart()
	{
		return Gesture.StartPose();
	}

	public override bool IsSpellBroken()
	{
		return Gesture.BreakPose();
	}

	public override bool ShouldCast()
	{
		if (AimController.AimedOn == null) return false;

		bool aimingOnGrabbable = AimController.AimedOn.GetComponent<Grabbable>() != null;
		if (aimingOnGrabbable) _controlledObject = AimController.AimedOn;
		else _controlledObject = null;

		return Gesture.CastPose();
	}

	public override void CastSpell()
	{
		if (!HandManager.AreBothHandsPresent()) return;

		Vector3 direction = HandManager.GetPointingDirection(HandType.Right);
		Vector3 tip = HandManager.GetTipPosition(HandType.Right, 1);

		_forcePos = tip + 5 * direction;
		_forceOn = true;
	}

	public override void AfterCastEffect()
	{
		_forceGameObject = GameObject.Instantiate(_forcePrefab, _forcePos, Quaternion.identity);
		if (_controlledObject != null)
			AudioManager.Instance.PlaySFX("ForceSpellCast");
	}

	public override bool ShouldFinishCasting()
	{
		if (_controlledObject == null || _pushJustFinished || _stopJustFinished)
		{
			_pushJustFinished = false;
			_stopJustFinished = false;
			return true;
		}

		//var distance = Vector3.Distance(hips.transform.position, _forcePos);
		return !(HandManager.IsFingerExtended(HandType.Left, 1)); //&& distance < 16f);
	}

	public override void KeepCasting()
	{
		HandlePushSpell();
		HandleStopSpell();

		_forceGameObject.transform.position = _forcePos;
		_forcePos = HandManager.GetTipPosition(HandType.Left, 1) + HandManager.GetPointingDirection(HandType.Left) * 5;

		if (_controlledObject != null && _forceOn)
		{
			var rb = _controlledObject.GetComponent<Grabbable>().Grab();
			if (rb != null)
			{
				ApplySpringForce(rb, _forcePos); 
			}
		}
	}

	public override void FinishCasting()
	{
		Destroy(_forceGameObject.gameObject);
		_forceOn = false;
		_controlledObject = null;
		SpellState = SpellState.None;
	}

	private void ApplySpringForce(Rigidbody rb, Vector3 targetPosition)
	{
		Vector3 displacement = targetPosition - rb.position;
		float distance = displacement.magnitude;

		// Dynamic adjustment based on distance
		float springConstant = Mathf.Lerp(100f, 500f, 1f - Mathf.Clamp01(distance / 5f)); 
		float dampingRatio = Mathf.Lerp(5f, 20f, 1f - Mathf.Clamp01(distance / 5f)); 

		Vector3 velocity = rb.velocity;
		Vector3 springForce = (displacement.normalized * springConstant * distance) - (velocity * dampingRatio);

		rb.AddForce(springForce, ForceMode.Force);
	}

	private void HandlePushSpell()
	{
		if (_pushGesture.StartPose()) _pushStarted = true;
		else if (_pushGesture.BreakPose())
		{
			_pushStarted = false;
		}

		if(_pushStarted && _pushGesture.CastPose())
		{
			var directionOfForce = HandManager.Instance.GetPointingDirection(HandType.Left);
			var forceMagnitude = 350f;
			_controlledObject.GetComponent<Rigidbody>().AddForce(directionOfForce.normalized * forceMagnitude, ForceMode.Impulse);
			
			_pushStarted = false;
			_pushJustFinished = true;
		}
	}

	private void HandleStopSpell()
	{
		if (_stopGesture.StartPose()) _stopStarted = true;
		else if (_stopGesture.BreakPose())
		{
			_stopStarted = false;
		}

		if (_stopStarted && _stopGesture.CastPose())
		{
			_controlledObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
			_controlledObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			_stopStarted = false;
			_stopJustFinished = true;
		}
	}

}


