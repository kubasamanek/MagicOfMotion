public interface ILastingSpell
{
	/// <summary>
	/// Determines if the conditions to finish the casting are met.
	/// </summary>
	/// <returns>True if spell should finish casting, otherwise false.</returns>
	bool ShouldFinishCasting();

	/// <summary>
	/// Do longing casting effect, if there's one.
	/// </summary>
	void KeepCasting();

	/// <summary>
	/// Finish casting effect and cleanup.
	/// </summary>
	void FinishCasting();
}