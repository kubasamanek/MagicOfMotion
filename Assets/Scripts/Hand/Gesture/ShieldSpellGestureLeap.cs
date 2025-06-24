
using UnityEngine;

public class ShieldSpellGestureLeap : Gesture
{
	private float _minimalYPosition = 0.5f;

	// In words:
	// Start if both hands are closed and facing from player.
	public override bool StartPose()
	{
		if (!HandManager.AreBothHandsPresent()) return false;

		return !HandManager.AreHandsFacingPlayer()
			&& HandsHighEnough()
			&& HandManager.AreBothHandsClosed();
	}

	// In words:
	// Break if hand is opened or not facing player.
	public override bool BreakPose()
	{
		if (!HandManager.AreBothHandsPresent()
			|| HandManager.AreBothHandsOpen()
			|| !HandsHighEnough()) return true;

		return false;
	}

	// In words:
	// Cast if hands are still closed and facing player.
	public override bool CastPose()
	{
		return HandManager.AreBothHandsClosed()
			&& HandsHighEnough()
			&& HandManager.AreHandsFacingPlayer();
	}

	/// <summary>
	/// Check if both hands are high enough to start the shield spell.
	/// </summary>
	/// <returns>True if hands above the limit</returns>
	private bool HandsHighEnough()
	{
		float ropeY = GameObject.FindGameObjectWithTag("Player").transform.position.y;

		float rightDistanceY = HandManager.GetPalmPosition(HandType.Right).y - ropeY;
		float leftDistanceY = HandManager.GetPalmPosition(HandType.Left).y - ropeY;

		if (rightDistanceY > _minimalYPosition && leftDistanceY > _minimalYPosition)
			return true;

		return false;
	}


}