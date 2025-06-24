using UnityEngine;

/// <summary>
/// Manages enemy behavior including states of idling, attacking, and responses to player detection.
/// This class uses mood to determine aggression and handles animations and attacks like arrow shooting.
/// </summary>
public class Enemy : Npc
{
	[SerializeField] private GameObject _arrowPrefab;
	[SerializeField] private Transform _arrowSpawnPoint;

	private float _detectionRadius = 15f;
	private float _attackDelayMin = 4f;
	private float _attackDelayMax = 12f;
	private float _currAttackDelay;
	private float _timeSinceLastAttack = 0f;

	private Transform _playerTarget;

	/// <summary>
	/// If mood == Aggresive, goblin attacks.
	/// If mood == Calm, golbin idle.
	/// </summary>
	public enum Mood
	{
		Calm,
		Aggresive
	}

	public State currentState = State.Idle;
	public Mood mood;

	public GameObject Arrow;

	protected override void Start()
	{
		base.Start();
		_currAttackDelay = Random.Range(_attackDelayMin, _attackDelayMax);
		_playerTarget = _player.transform.Find("TargetForArrows");

	}

	/// <summary>
	/// Handles the state transitions and actions of the enemy based on its state and mood.
	/// </summary>
	protected override void Update()
	{
		base.Update();

		switch (currentState)
		{
			case State.Idle:
				CheckForPlayer();
				break;
			case State.Attack:
				LookAtPlayer();
				if (mood == Mood.Calm) return;
				if (_timeSinceLastAttack > _currAttackDelay)
				{
					Attack();
					_currAttackDelay = Random.Range(_attackDelayMin, _attackDelayMax);
					_timeSinceLastAttack = 0f;
				}
				break;
		}

		_timeSinceLastAttack += Time.deltaTime;

	}

	/// <summary>
	/// Adjusts the enemy's orientation to face the player continuously during an attack.
	/// </summary>
	private void LookAtPlayer()
	{
		Vector3 lookPosition = _player.transform.position - transform.position;
		lookPosition.y = 0; 

		Quaternion targetRotation = Quaternion.LookRotation(lookPosition);

		float yCorrection = Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("Shoot") ? 90f : 20f;

		Quaternion correctedRotation = Quaternion.Euler(targetRotation.eulerAngles.x, targetRotation.eulerAngles.y + yCorrection, targetRotation.eulerAngles.z);

		transform.rotation = Quaternion.Slerp(transform.rotation, correctedRotation, Time.deltaTime * 5f);
	}

	/// <summary>
	/// Checks if the player is within the detection radius and transitions to attack state if so.
	/// </summary>
	private void CheckForPlayer()
	{
		if (Vector3.Distance(_player.transform.position, this.transform.position) <= _detectionRadius)
		{
			currentState = State.Attack;
			AudioManager.Instance.PlaySFX("GoblinSurprise");
		}
	}

	/// <summary>
	/// Handles the logic for shooting an arrow towards the player.
	/// </summary>
	public void ShootArrow()
	{
		if (_arrowPrefab && _arrowSpawnPoint)
		{
			Instantiate(_arrowPrefab, _arrowSpawnPoint.position, Quaternion.LookRotation(_playerTarget.position - _arrowSpawnPoint.position) * Quaternion.Euler(0, 180, 0));
			AudioManager.Instance.PlaySFXAtPosition("ArrowShoot", this.gameObject.transform.position);
			Arrow.SetActive(false);
		}
		currentState = State.Idle;
	}

	protected override void OnDeath()
	{
		currentState = State.Ragdoll;
		AudioManager.Instance.PlaySFXAtPosition("GoblinDie", this.transform.position);
	}

	protected override void OnImpact(float force)
	{
		AudioManager.Instance.PlaySFXAtPosition("GoblinGrunt", this.gameObject.transform.position);
	}

	private void Attack()
	{
		Animator.SetTrigger("Shoot");
	}

	public void ArrowPick()
	{
		Arrow.SetActive(true);
	}
}
