using UnityEngine;

/// <summary>
/// Base class for all NPC types, providing common properties and functions such as health management,
/// ragdoll effects, and interaction with player through aiming and attacks.
/// </summary>
public abstract class Npc : MonoBehaviour
{
	protected RagdollToggle _ragDoll;
	protected float health = 100f;
	protected GameObject _player;

	[SerializeField] protected float ragdollForceThreshold = 10f;
	public bool IsRagDoll { get; private set; }
	public bool Dead { get; protected set; }

	public bool AimedOn = false;
	public Animator Animator;

	private AimControllerBase _aimController;

	public enum State
	{
		Idle,
		Ragdoll,
		Standup,
		Attack
	}

	protected virtual void Start()
	{
		_ragDoll = GetComponent<RagdollToggle>();
		_player = GameObject.FindGameObjectWithTag("Player");
		Dead = false;
		_aimController = GameManager.Instance.DeviceType == InputDeviceType.UltraLeap
			? FindObjectOfType<LeapAimController>().GetComponent<LeapAimController>()
			: FindObjectOfType<RgbAimController>().GetComponent<RgbAimController>();
		Animator = GetComponent<Animator>();
	}

	/// <summary>
	/// Activates ragdoll state.
	/// </summary>
	public void ToggleRagDoll()
	{
		_ragDoll.RagDollOn();
		IsRagDoll = true;
	}

	/// <summary>
	/// Applies damage to the NPC and checks if it should be killed.
	/// </summary>
	/// <param name="damage">Amount of damage to apply.</param>
	public virtual void TakeDamage(float damage)
	{
		health -= damage;
		if (health <= 0)
		{
			ToggleRagDoll();
			Dead = true;
			OnDeath();
		}
	}

	/// <summary>
	/// Abstract method defined in derived classes to handle NPC death.
	/// </summary>
	protected abstract void OnDeath();

	/// <summary>
	/// Handles collision events to possibly trigger ragdoll based on the force of impact.
	/// </summary>
	/// <param name="collision">Collision data.</param>
	private void OnCollisionEnter(Collision collision)
	{
		float collisionForce = collision.relativeVelocity.magnitude * (collision.rigidbody?.mass ?? 0);
		if (collisionForce >= ragdollForceThreshold)
		{
			OnImpact(collisionForce);
			ToggleRagDoll();
		}
	}

	/// <summary>
	/// Abstract method defined in derived classes to handle impacts that may result in ragdoll effects.
	/// </summary>
	/// <param name="force">Force of the impact.</param>
	protected abstract void OnImpact(float force);

	protected virtual void Update()
    {
		AimedOn = _aimController.AimedOn == this.gameObject;
		IsRagDoll = _ragDoll.CurrentState == RagdollToggle.State.Ragdoll;
	}
}
