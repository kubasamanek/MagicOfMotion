/// <summary>
/// Represents the base class for all spells in the game.
/// Defines common properties and handles different states of the spell.
/// </summary>
public abstract class LastingSpell : Spell, ILastingSpell
{
	public virtual float GetUsingManaCost() => 0f;

	protected new void Update()
	{
		switch (SpellState)
		{
			case SpellState.None:
				if(DetectStart()) SpellState = SpellState.IsStarted;
				break;
			case SpellState.IsStarted:
				AfterStartEffect();
				SpellState = SpellState.IsPerformed;
				break;
			case SpellState.IsPerformed:
				if (IsSpellBroken()) {
					SpellState = SpellState.IsBroken;
				}
				else if (ShouldCast())
				{
					CastSpell();
					SpellState = SpellState.IsCasted;
				}
				else
				{
					PerformingEffect();
				}
				break;
			case SpellState.IsBroken:
				BrokenEffect();
				SpellState = SpellState.None;
				break;
			case SpellState.IsCasted:
				AfterCastEffect();
				SpellState = SpellState.IsCasting;
				break;
			case SpellState.IsCasting:
				ManaSystem.UseMana(GetUsingManaCost());
				if(ShouldFinishCasting() || !ManaSystem.EnoughMana(GetUsingManaCost())) SpellState = SpellState.IsFinished;
				if(SpellState == SpellState.IsCasting) KeepCasting();
				break;
			case SpellState.IsFinished:
				FinishCasting();
				SpellState = SpellState.None;
				break;
		}
	}

	/// <summary>
	/// Determines if the casting should stop.
	/// </summary>
	/// <returns>True if the conditions to stop casting are met.</returns>
	public abstract bool ShouldFinishCasting();

	/// <summary>
	/// Keeps executing the spell's effect over time.
	/// </summary>
	public abstract void KeepCasting();

	/// <summary>
	/// Executes spell finish effect and cleanup.
	/// </summary>
	public abstract void FinishCasting();
}


