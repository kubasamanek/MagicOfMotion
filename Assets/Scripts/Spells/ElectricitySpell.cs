
using DigitalRuby.LightningBolt;
using UnityEngine;

public class ElectricitySpell : LastingSpell
{
	[SerializeField] private float _manaCost;
	[SerializeField] private float _usingManaCost;

	public override float GetManaCost() => _manaCost;
	public override float GetUsingManaCost() => _usingManaCost;

	private float _initialTime = 0f;
	private float _timeToCharge = 3f;

	private GameObject _activeLightning = null;
	private Transform _player;

	private bool _charged = false;

	[SerializeField] private GameObject _lightningPrefab;

	private new void Start()
	{
		base.Start();
		_player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
		Gesture = GestureFactory.CreateElectricityGesture(GameManager.Instance.DeviceType);
	}

	public override bool DetectStart()
	{
		return Gesture.StartPose();
	}

	public override void AfterStartEffect()
	{
		_activeLightning = Instantiate(_lightningPrefab, Vector3.zero, Quaternion.identity);
		_initialTime = Time.time;
		AudioManager.Instance.PlaySFX("Electricity");
	}

	public override void PerformingEffect()
	{
		var bolt = _activeLightning.GetComponent<LightningBoltScript>();

		bolt.StartPosition = _player.position + 10f * Vector3.up;
		bolt.EndPosition = HandManager.GetPalmPosition(HandType.Right);

		_charged = Time.time - _initialTime >= _timeToCharge;
	}

	public override bool IsSpellBroken()
	{
		if (!HandManager.Instance.AreBothHandsPresent()) return true;
		return Gesture.BreakPose() && !_charged;
	}

	public override void BrokenEffect()
	{
		AudioManager.Instance.StopSFX("Electricity");

		Destroy(_activeLightning.gameObject);
		_activeLightning = null;
		_charged = false;
		_initialTime = 0;
	}

	public override bool ShouldCast()
	{
		return Gesture.CastPose() && _charged;
	}

	public override void CastSpell()
	{
		AimController.ToggleRenderer(false);
	}

	public override void AfterCastEffect()
	{
		AudioManager.Instance.PlaySFX("ElectricityCharged");
	}

	public override void KeepCasting()
	{
		var bolt = _activeLightning.GetComponent<LightningBoltScript>();
		var lineRenderer = _activeLightning.GetComponent<LineRenderer>();

		if (AreHandsFacingEachOther() || AimController.Target == Vector3.zero)
		{
			bolt.StartPosition = HandManager.GetPalmPosition(HandType.Right);
			bolt.EndPosition = HandManager.GetPalmPosition(HandType.Left);
			lineRenderer.startWidth = lineRenderer.endWidth = 0.1f;
		} 
		else
		{
			bolt.StartPosition = HandManager.GetPalmPosition(HandType.Right);
			bolt.EndPosition = AimController.Target;
			lineRenderer.startWidth = lineRenderer.endWidth = 0.6f;
			if (AimController.AimedOn != null && AimController.AimedOn.CompareTag("Enemy"))
				AimController.AimedOn.GetComponent<ElectrifyChain>().Electrify(false);
		}
	}

	public override bool ShouldFinishCasting()
	{
		if (!HandManager.AreBothHandsPresent()) return true;
		if (!ManaSystem.EnoughMana(GetUsingManaCost()))
		{
			if(AimController.AimedOn != null && AimController.AimedOn.TryGetComponent(out ElectrifyChain chain))
				StartCoroutine(chain.End());
			return true;
		}

		return HandManager.AreBothHandsClosed();
	}

	public override void FinishCasting()
	{
		AudioManager.Instance.StopSFX("Electricity");
		AimController.ToggleRenderer(true);

		Destroy(_activeLightning.gameObject);
		_activeLightning = null;
		_charged = false;
		_initialTime = 0;
	}

	private bool AreHandsFacingEachOther()
	{
		Vector3 rightHandDirection = HandManager.GetPalmNormal(HandType.Right);
		Vector3 leftHandDirection = HandManager.GetPalmNormal(HandType.Left);

		Vector3 rightHandPosition = HandManager.GetPalmPosition(HandType.Right);
		Vector3 leftHandPosition = HandManager.GetPalmPosition(HandType.Left);

		Vector3 fromRightToLeft = (leftHandPosition - rightHandPosition).normalized;
		Vector3 fromLeftToRight = -fromRightToLeft; 

		float dotRight = Vector3.Dot(rightHandDirection.normalized, fromRightToLeft);
		float dotLeft = Vector3.Dot(leftHandDirection.normalized, fromLeftToRight);

		return dotRight > 0.6 && dotLeft > 0.6;	

	}

}