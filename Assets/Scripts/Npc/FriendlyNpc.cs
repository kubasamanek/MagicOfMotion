using UnityEngine;

/// <summary>
/// Manages friendly NPC behavior. Changes sounds based of gender. Manages animations.
/// </summary>
public class FriendlyNpc : Npc
{
	public bool IsWoman;
	
	public bool Sitting = false;
	public bool Walking = false;

	[SerializeField] private int _animationIndex = 0; 

	protected override void OnDeath()
	{
	}

	protected override void OnImpact(float force)
	{
		_animationIndex = 0;
		if (IsWoman)
			AudioManager.Instance.PlaySFXAtPosition("WomanGrunt", this.transform.position);
		else
			AudioManager.Instance.PlaySFXAtPosition("ManGrunt", this.transform.position);
	}

	protected override void Start()
	{
		base.Start();
		Animator = GetComponent<Animator>();
		SetInitialAnimation();
	}

	private void SetInitialAnimation()
	{
		Animator.SetInteger("AnimationIndex", _animationIndex);
	}

	protected override void Update()
	{
		base.Update();
		if (Sitting)
		{
			_animationIndex = Random.Range(0, 5);
			SetInitialAnimation();
		}
	}

}
