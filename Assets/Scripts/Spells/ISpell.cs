/// <summary>
/// Defines a template for all spells.
/// Each spell is basically a simple state machine, where each state needs to be handled.
/// </summary>
public interface ISpell
{
    HandManager HandManager { get; set; }
	SpellState SpellState { get; set; }

	/// <summary>
	/// Determines if the conditions to start the spell have been met.
	/// </summary>
	/// <returns>True if spell was started, otherwise false.</returns>
	bool DetectStart();

	/// <summary>
	/// Determines if the conditions to break the spell have been met.
	/// </summary>
	/// <returns>True if spell was broken, otherwise false.</returns>
	bool IsSpellBroken();

	/// <summary>
	/// Handles broken effect to give feedback to the player.
	/// </summary>
	void BrokenEffect();

	/// <summary>
	/// Determines if the conditions to cast the spell have been met.
	/// </summary>
	/// <returns>True if spell was casted, otherwise false.</returns>
	bool ShouldCast();

	/// <summary>
	/// Executes spell's effect.
	/// </summary>
	void CastSpell();

	/// <summary>
	/// Handles after cast effect of the spell.
	/// </summary>
	void AfterCastEffect();
}
