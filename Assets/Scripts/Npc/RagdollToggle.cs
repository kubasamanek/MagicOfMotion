using System.Linq;
using UnityEngine;

/// <summary>
/// Controls the ragdoll state of a character, 
/// allowing toggling between active ragdoll physics and animated states.
/// 
/// An object needs to be a ragdoll in order to use this.
/// An object needs to have a standup animation in order to use this.
/// </summary>
public class RagdollToggle : MonoBehaviour
{
	private const string STAND_UP_ANIMATION_NAME = "StandUp";

	[System.Serializable]
	private class BoneTransform
	{
		[SerializeField] public Vector3 Position { get; set; }
		[SerializeField] public Quaternion Rotation { get; set; }
	}

	public enum State
	{
		Idle,
		Ragdoll,
		StandingUp,
		ResettingBones
	}

	[SerializeField] private BoneTransform[] _standUpBoneTransforms;
	[SerializeField] private BoneTransform[] _ragdollBoneTransforms;
	[SerializeField] private Transform[] _bones;
	[SerializeField] private GameObject _rig;

	[SerializeField] private float _minTimeToWakeUp = 4f;
	[SerializeField] private float _maxTimeToWakeUp = 8f;

	[SerializeField] private float _timeToResetBones;
	private float _timeElapsedResettingBones;

	private State _currentState { get; set; }

	private Rigidbody _rb;
	private Animator _ac;
	private BoxCollider _bc;

	private Transform _hipsBone;

	private Collider[] _ragdollColliders;
	private Rigidbody[] _ragdollBodies;


	private float _timeToWakeUp;

	public State CurrentState => _currentState;

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.tag == "Player")
		{
			RagDollOn();
		}
		else if (collision.collider.tag == "Fireball")
		{
			RagDollOn();
			var vectorToPlayer = GameObject.FindGameObjectsWithTag("Player").First().transform.position - collision.transform.position;
			vectorToPlayer.Normalize();
			_hipsBone.GetComponent<Rigidbody>().AddExplosionForce(300, collision.transform.position + vectorToPlayer, 2f, 1f, ForceMode.Impulse);
		}
	}

	/// <summary>
	/// Initializes component references and prepares ragdoll and stand-up transformations.
	/// </summary>
	void Start()
	{
		_rb = GetComponent<Rigidbody>();
		_ac = GetComponent<Animator>();
		_bc = GetComponent<BoxCollider>();

		_hipsBone = _ac.GetBoneTransform(HumanBodyBones.Hips);
		_bones = _hipsBone.GetComponentsInChildren<Transform>();

		InitializeBoneTransformArrays();
		GetRagDollBits();
		RagDollOff();
	}

    /// <summary>
    /// Updates the ragdoll state machine.
    /// </summary>
	void Update()
	{
		if (this.TryGetComponent(out Enemy enemy) && enemy.Dead)
			_currentState = State.Ragdoll;

		switch (_currentState)
		{
			case State.Ragdoll:
				RagDollBehaviour();
				break;
			case State.StandingUp:
				StandingUpBehaviour();
				break;
			case State.ResettingBones:
				ResettingBonesBehaviour();
				break;

		}
	}

	/// <summary>
	/// Initializes the bone transform arrays for storing start and ragdoll bone positions and rotations.
	/// </summary>
	private void InitializeBoneTransformArrays()
	{
		_standUpBoneTransforms = new BoneTransform[_bones.Length];
		_ragdollBoneTransforms = new BoneTransform[_bones.Length];
		for (int bIndex = 0; bIndex < _bones.Length; bIndex++)
		{
			_standUpBoneTransforms[bIndex] = new BoneTransform();
			_ragdollBoneTransforms[bIndex] = new BoneTransform();
		}

		PopulateAnimationStartBoneTransforms(STAND_UP_ANIMATION_NAME);
	}

	/// <summary>
	/// Handles behavior when character is standing up, 
	/// transitioning to idle state when the stand-up animation ended.
	/// </summary>
	private void StandingUpBehaviour()
	{
		if(_ac.GetCurrentAnimatorStateInfo(0).IsName(STAND_UP_ANIMATION_NAME) == false)
		{
			_currentState = State.Idle;
		}
	}

	/// <summary>
	/// Manages the resetting of bone positions and rotations to match the standing animation, 
	/// transitioning to standing up state upon completion.
	/// </summary>
	private void ResettingBonesBehaviour()
	{
		_timeElapsedResettingBones += Time.deltaTime;
		float elapsedPercentage = _timeElapsedResettingBones / _timeToResetBones;

		for(int bIndex = 0; bIndex < _bones.Length; ++bIndex)
		{
			_bones[bIndex].localPosition = Vector3.Lerp(
				_ragdollBoneTransforms[bIndex].Position,
				_standUpBoneTransforms[bIndex].Position,
				elapsedPercentage);

			_bones[bIndex].localRotation = Quaternion.Lerp(
				_ragdollBoneTransforms[bIndex].Rotation,
				_standUpBoneTransforms[bIndex].Rotation,
				elapsedPercentage);
		}

		if(elapsedPercentage >= 1)
		{
			CompleteBoneReset();
		}
	}

	/// <summary>
	/// Completes the bone reset process, changing state to standing up and playing the animation.
	/// </summary>
	private void CompleteBoneReset()
	{
		_currentState = State.StandingUp;
		RagDollOff();
		_ac.Play(STAND_UP_ANIMATION_NAME);
	}

	/// <summary>
	/// Manages the ragdoll state, counting down until the character begins to stand up.
	/// </summary>
	private void RagDollBehaviour()
	{	
		_timeToWakeUp -= Time.deltaTime;
		if(_timeToWakeUp < 0 )
		{
			StartBoneResettingProcess();
		}
	}

	/// <summary>
	/// Starts the process of resetting bones to their pre-ragdoll state in preparation for standing up.
	/// </summary>
	private void StartBoneResettingProcess()
	{
		AlignRotationToHips();
		AlignPositionToHips();
		PopulateBoneTransforms(_ragdollBoneTransforms);
		_currentState = State.ResettingBones;
		_timeElapsedResettingBones = 0;
	}

	/// <summary>
	/// Aligns the character's position to the hips transform, adjusting for ground contact.
	/// </summary>
	private void AlignPositionToHips()
	{
		Vector3 originalHipsPosition = _hipsBone.position;
		transform.position = _hipsBone.position;

		// Ensure the feet are correctly positioned on the ground.
		Vector3 posOffset = _standUpBoneTransforms[0].Position;
		posOffset.y = 0;
		posOffset = transform.rotation * posOffset;
		transform.position -= posOffset;

		// Adjust y position to be exactly on the ground.
		if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
		{
			transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
		}

		// Restore the hip's bone position
		_hipsBone.position = originalHipsPosition;
	}

	/// <summary>
	/// Aligns the character's rotation to match the direction of the hips bone.
	/// </summary>
	private void AlignRotationToHips()
	{
		Vector3 ogHipsPosition = _hipsBone.position;
		Quaternion ogHipsRotation = _hipsBone.rotation;

		Vector3 finalDirection = -_hipsBone.up;
		finalDirection.y = 0;
		finalDirection = finalDirection.normalized;

		Quaternion fromToRotation = Quaternion.FromToRotation(transform.forward, finalDirection);
		transform.rotation *= fromToRotation;
		
		_hipsBone.position = ogHipsPosition;
		_hipsBone.rotation = ogHipsRotation;
	}

	/// <summary>
	/// Retrieves all necessary ragdoll components from the character rig.
	/// </summary>
	public void GetRagDollBits()
    {
		_ragdollBodies = _rig.GetComponentsInChildren<Rigidbody>();
		_ragdollColliders = _rig.GetComponentsInChildren<Collider>();
	}

	/// <summary>
	/// Activates the ragdoll physics, disabling the animator and enabling the physics components.
	/// </summary>
	public void RagDollOn()
    {
		_ac.enabled = false;

		EnableRagdoll(true);

		_bc.enabled = false;
		_rb.isKinematic = true;
		_currentState = State.Ragdoll;
		_timeToWakeUp = Random.Range(_minTimeToWakeUp, _maxTimeToWakeUp);
	}

	/// <summary>
	/// Deactivates the ragdoll physics, enabling the animator and returning control to the animation system.
	/// </summary>
	void RagDollOff()
	{
		_currentState = State.StandingUp;

		EnableRagdoll(false);

		_bc.enabled = true;
		_rb.isKinematic = false;
		_ac.enabled = true;
		//this.GetComponent<Npc>().currentState = Enemy.State.Idle;
	}

	/// <summary>
	/// Enables or disables ragdoll components based on the given state.
	/// </summary>
	/// <param name="enabled">Whether to enable or disable ragdoll physics components.</param>
	private void EnableRagdoll(bool enabled)
	{
		foreach (Collider col in _ragdollColliders)
			col.enabled = enabled;

		foreach (Rigidbody rb in _ragdollBodies)
			rb.isKinematic = !enabled;
	}

	/// <summary>
	/// Stores the current transforms of bones into the provided BoneTransform array.
	/// </summary>
	/// <param name="boneTransforms">The array to populate with the current bone transforms.</param>
	private void PopulateBoneTransforms(BoneTransform[] boneTransforms)
	{
		for(int bIndex = 0;  bIndex < _bones.Length; bIndex++)
		{
			boneTransforms[bIndex].Position = _bones[bIndex].localPosition;
			boneTransforms[bIndex].Rotation = _bones[bIndex].localRotation;
		}
	}

	/// <summary>
	/// Samples an animation clip at start to populate bone transform data.
	/// </summary>
	/// <param name="clipName">The name of the animation clip to sample.</param>
	/// <param name="boneTransforms">The array to populate with bone transform data from the animation.</param>
	private void PopulateAnimationStartBoneTransforms(string clipName)
	{
		Vector3 posBeforeSampling = transform.position;
		Quaternion rotBeforeSampling = transform.rotation;

		foreach(AnimationClip c in _ac.runtimeAnimatorController.animationClips)
		{
			if(c.name == clipName)
			{
				// Temporarily apply the first frame of the animation,
				c.SampleAnimation(gameObject, 0);

				// Then populate bone transforms from this frame.
				PopulateBoneTransforms(_standUpBoneTransforms);
				break;
			}
		}

		// Restore the position and rotation
		transform.position = posBeforeSampling;
		transform.rotation = rotBeforeSampling;
	}
}
