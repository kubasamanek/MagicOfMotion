using System.Collections;
using UnityEngine;

/// <summary>
/// Represents a fireball spell, inheriting from the base spell class.
/// Fireball spell creates a fireball, which is casted towards the target currently aimed on.
/// Fireball causes explosion, applying force to objects that are nearby.
/// </summary>
public class FireballSpell : Spell
{
	[SerializeField] private float _manaCost;
	public override float GetManaCost() => _manaCost;

	[SerializeField] private GameObject _fireballPrefab;
	[SerializeField] private GameObject _smokePrefab;

	private GameObject _player;
	private GameObject _smoke;
		
	private float _initialTime;
	private float _maxDurationUntilCasted = 5f;

	private float _fireballSpeed = 3f;

	private HorseMovement _playerMovement;
	private bool _isPlayerMoving = false;

	private new void Start()
	{
		base.Start();
		_player = GameObject.FindGameObjectWithTag("Player");
		_playerMovement = _player.GetComponent<HorseMovement>();
		Gesture = GestureFactory.CreateFireballGesture(GameManager.Instance.DeviceType);
	}

	public void ChangeGesture()
	{
		Gesture = GestureFactory.CreateFireballGesture(GameManager.Instance.DeviceType);
	}

	new void Update()
	{
		base.Update();
		_isPlayerMoving = _playerMovement.HoldingRopes;
	}

	/// <summary>
	/// Fireball spell starts when palm faces downward and the hand is closed.
	/// </summary>
	/// <returns>True if those conditions are met.</returns>
	public override bool DetectStart()
	{
		if (Gesture.StartPose() && !_isPlayerMoving)
		{
			_initialTime = Time.time;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Create smoke effect to give feedback to player.
	/// </summary>
	public override void AfterStartEffect()
	{
		if (HandManager.IsHandPresent(HandType.Right))
		{
			Vector3 palmPos = HandManager.GetPalmPosition(HandType.Right);
			_smoke = GameObject.Instantiate(_smokePrefab, palmPos, Quaternion.identity);
		}
	}

	/// <summary>
	/// Moves the smoke to the hand's position.
	/// </summary>
	public override void PerformingEffect()
	{
		Vector3 palmPos = HandManager.GetPalmPosition(HandType.Right);
		if(_smoke != null)
			_smoke.gameObject.transform.position = palmPos;
	}

	/// <summary>
	/// Fireball spell breaks when the hand is opened before turning the palm upwards
	/// or if the spell player didn't cast the spell for too long.
	/// </summary>
	/// <returns>True if those conditions are met.</returns>
	public override bool IsSpellBroken()
	{
		float duration = Time.time - _initialTime;

		if (Gesture.BreakPose() 
			|| _isPlayerMoving
			|| duration > _maxDurationUntilCasted) 
			return true;

		return false;
	}

	/// <summary>
	/// The smoke should stop when casting gesture is broken.
	/// </summary>
	public override void BrokenEffect()
	{
		try
		{
			StartCoroutine(FadeOutSmoke(0.3f));
		}
		catch {}
	}

	/// <summary>
	/// Fireball spell should be casted when the palm is facing upward and is suddenly opened.
	/// </summary>
	/// <returns>True if those condition are met.</returns>
	public override bool ShouldCast()
	{
		return Gesture.CastPose();
	}

	/// <summary>
	/// Casts a fireball from the palm position towards the target.
	/// Fireball is a prefab with Fireball.cs script attached to it.
	/// </summary>
	public override void CastSpell()
	{
		Vector3 palmPos = HandManager.GetPalmPosition(HandType.Right);
		Vector3 otherPalmPos = HandManager.GetPalmPosition(HandType.Left);

		GameObject fireball = GameObject.Instantiate(_fireballPrefab, palmPos, Quaternion.identity);
		
		// Fireball should ignore players collider.
		Physics.IgnoreCollision(fireball.GetComponent<Collider>(), _player.GetComponent<Collider>());
		Rigidbody rb = fireball.GetComponent<Rigidbody>();

		Vector3 launchDirection = AimController.Target != Vector3.zero
			? AimController.Target - HandManager.GetPalmPosition(HandType.Right)
			: (otherPalmPos + AimController.AimingDirection * 100) - palmPos;

		rb.AddForce(launchDirection.normalized * 10 * _fireballSpeed, ForceMode.VelocityChange);

		AudioManager.Instance.PlaySFX("FireballSpawn");
	}

	/// <summary>
	/// Handles stopping the smoke effect when the fireball casts.
	/// </summary>
	public override void AfterCastEffect()
	{
		StartCoroutine(FadeOutSmoke(0.3f));
	}

	IEnumerator FadeOutSmoke(float fadeOutTime)
	{
		if (_smoke == null) yield return null;
		
		var particleSystem = _smoke.GetComponent<ParticleSystem>();

		var emission = particleSystem.emission;
		emission.rateOverTime = 0;

		// Fade out the particle system's opacity
		var main = particleSystem.main;
		float startAlpha = main.startColor.color.a;

		float time = 0;
		while (time < fadeOutTime)
		{
			if (_smoke == null)
				break;
			time += Time.deltaTime;
			float alpha = Mathf.Lerp(startAlpha, 0, time / fadeOutTime);

			var startColor = main.startColor;
			startColor.color = new Color(startColor.color.r, startColor.color.g, startColor.color.b, alpha);
			main.startColor = startColor;

			yield return null; 
		}
		

		if(_smoke != null)
			Destroy(_smoke.gameObject);
	}
}
