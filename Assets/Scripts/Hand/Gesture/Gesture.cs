using UnityEngine;

/// <summary>
/// Defines an abstract base class for creating hand gesture controls within a game.
/// This class handles the common functionality needed for gesture recognition related to player's hand movements.
/// </summary>
public abstract class Gesture
{
	/// <summary>
	/// Gets or sets the HandManager instance for accessing hand tracking data.
	/// </summary>
	protected HandManager HandManager { get; set; }

	/// <summary>
	/// Gets or sets the Transform of the player character.
	/// </summary>
	protected Transform PlayerTransform { get; set; }

	/// <summary>
	/// Initializes a new instance of the Gesture class, setting up references to the player's Transform and the HandManager.
	/// </summary>
	public Gesture()
	{
		PlayerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
		HandManager = HandManager.Instance;
	}

	/// <summary>
	/// Determines if the gesture's starting pose is currently being formed by the player.
	/// Must be implemented by derived classes to define specific start pose conditions.
	/// </summary>
	/// <returns>True if the starting pose is correctly formed; otherwise, false.</returns>
	public abstract bool StartPose();

	/// <summary>
	/// Determines if the gesture's pose is broken or interrupted by the player.
	/// Must be implemented by derived classes to define specific break pose conditions.
	/// </summary>
	/// <returns>True if the pose is broken; otherwise, false.</returns>
	public abstract bool BreakPose();

	/// <summary>
	/// Executes the action associated with the gesture when the casting pose is achieved.
	/// Must be implemented by derived classes to define specific casting actions.
	/// </summary>
	/// <returns>True if the pose is successfully cast; otherwise, false.</returns>
	public abstract bool CastPose();
}
