using System;
using UnityEngine;

/// <summary>
/// Represents the base class for all spells in the game.
/// Defines common properties and handles different states of the spell.
/// </summary>
public abstract class Spell : MonoBehaviour, ISpell
{
	public SpellState SpellState { get; set; }
	public HandManager HandManager { get; set; }
	protected AimControllerBase AimController { get; set; }
	protected Gesture Gesture { get; set; }
	protected ManaSystem ManaSystem { get; set; }
	public virtual float GetManaCost() => 0f;

	protected void Start()
	{
		SwitchController();
		HandManager = HandManager.Instance;
		ManaSystem = GameObject.FindGameObjectWithTag("Player").GetComponent<ManaSystem>();	
	}

	public void SwitchController()
	{
		var player = GameObject.FindGameObjectWithTag("Player");
		switch (GameManager.Instance.DeviceType)
		{
			case InputDeviceType.UltraLeap:
				player.GetComponentInChildren<RgbAimController>().enabled = false;
				AimController = player.GetComponentInChildren<LeapAimController>();
				AimController.enabled = true;
				break;
			case InputDeviceType.RGBCamera:
				player.GetComponentInChildren<LeapAimController>().enabled = false;
				AimController = player.GetComponentInChildren<RgbAimController>();
				AimController.enabled = true;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(GameManager.Instance.DeviceType), "Unsupported device type");
		}
	}

	protected void Update()
	{

		switch (SpellState)
		{
			case SpellState.None:
				if (DetectStart()) SpellState = SpellState.IsStarted;
				break;
			case SpellState.IsStarted:
				AfterStartEffect();
				SpellState = SpellState.IsPerformed;
				break;
			case SpellState.IsPerformed:
				if (IsSpellBroken()) SpellState = SpellState.IsBroken;
				else if (ShouldCast())
				{
					if (ManaSystem.EnoughMana(GetManaCost()))
					{
						CastSpell();
						SpellState = SpellState.IsCasted;
						ManaSystem.UseMana(GetManaCost());
					}
				}
				else PerformingEffect();
				break;
			case SpellState.IsBroken:
				BrokenEffect();
				SpellState = SpellState.None;
				break;
			case SpellState.IsCasted:
				AfterCastEffect();
				SpellState = SpellState.IsFinished;
				break;
			case SpellState.IsFinished:
				SpellState = SpellState.None;
				break;
		}
	}

	/// <summary>
	/// Determines if the conditions to start the spell have been met.
	/// </summary>
	/// <returns>True if spell was started, otherwise false.</returns>
	public abstract bool DetectStart();

	/// <summary>
	/// Executes effect after starting.
	/// </summary>
	public virtual void AfterStartEffect() { }

	/// <summary>
	/// Handles logic when spell is being performed.
	/// </summary>
	public virtual void PerformingEffect() { }

	/// <summary>
	/// Determines if the conditions which brakes the spell have been met.
	/// </summary>
	/// <returns>True if spell was broken, otherwise false.</returns>
	public abstract bool IsSpellBroken();

	/// <summary>
	/// Handles broken effect to give feedback to the player.
	/// </summary>
	public virtual void BrokenEffect() { }

	/// <summary>
	/// Determines if the conditions to cast the spell have been met.
	/// </summary>
	/// <returns>True if spell should cast, otherwise false.</returns>
	public abstract bool ShouldCast();

	/// <summary>
	/// Executes the spell's effect.
	/// </summary>
	public abstract void CastSpell();

	/// <summary>
	/// Executes effect after casting.
	/// </summary>
	public virtual void AfterCastEffect() { }


}
