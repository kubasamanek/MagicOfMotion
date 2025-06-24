using System.Collections.Generic;
using UnityEngine;

public class ShieldSpell : LastingSpell
{
	[SerializeField] private GameObject _ropePoint;
	[SerializeField] private GameObject _shieldPrefab;

	private GameObject _activeShield;
	private List<Vector3> _armPoints = new List<Vector3>();

	private HorseMovement _playerMovement;

	private bool _isPlayerMoving = false;
	private FireballSpell _fireballSpell;

	private new void Start()
	{
		base.Start();
		Gesture = GestureFactory.CreateShieldGesture(GameManager.Instance.DeviceType);
		_playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<HorseMovement>();
		_fireballSpell = this.gameObject.GetComponent<FireballSpell>();
	}

	new void Update()
	{
		if(Gesture == null) return;
		base.Update();
		_isPlayerMoving = _playerMovement.HoldingRopes;
	}

	public override bool DetectStart()
	{
		if (_isPlayerMoving) return false;

		return Gesture.StartPose();


	}

	public override bool IsSpellBroken()
	{
		return Gesture.BreakPose() || _isPlayerMoving;
	}

	public override bool ShouldCast()
	{
		return Gesture.CastPose();
	}

	public override void CastSpell()
	{
		_activeShield = Instantiate(_shieldPrefab, Vector3.zero, Quaternion.identity);
		var shield = _activeShield.GetComponent<DynamicShield>();

		var leftArm = HandManager.GetElbowPositions(HandType.Left);
		var rightArm = HandManager.GetElbowPositions(HandType.Right);

		var points = new Vector3[4];
		points[0] = leftArm[0]; // Left elbow
		points[1] = leftArm[1]; // Left wrist
		points[2] = rightArm[0]; // Right elbow
		points[3] = rightArm[1]; // Right wrist
		_armPoints.AddRange(points);

		shield.UpdateShieldMesh(_armPoints);
		_armPoints.Clear();
		_fireballSpell.enabled = false;
	}

	public override void AfterCastEffect()
	{
		AudioManager.Instance.PlaySFX("ShieldSpell");
	}

	public override bool ShouldFinishCasting()
	{
		return Gesture.BreakPose() || !HandManager.AreHandsFacingPlayer();
	}

	public override void KeepCasting()
	{
		var shield = _activeShield.GetComponent<DynamicShield>();

		var leftArm = HandManager.GetElbowPositions(HandType.Left);
		var rightArm = HandManager.GetElbowPositions(HandType.Right);

		var points = new Vector3[4];
		points[0] = leftArm[0]; // Left elbow
		points[1] = leftArm[1]; // Left wrist
		points[2] = rightArm[0]; // Right elbow
		points[3] = rightArm[1]; // Right wrist
		_armPoints.AddRange(points);

		shield.UpdateShieldMesh(_armPoints);
		_armPoints.Clear();
	}

	public override void FinishCasting()
	{
		_fireballSpell.enabled = true;
		AudioManager.Instance.PlaySFX("ShieldSpellFinish");
		Destroy(_activeShield.gameObject);
	}


}
