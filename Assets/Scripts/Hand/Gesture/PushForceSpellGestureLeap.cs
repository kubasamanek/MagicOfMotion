public class PushForceSpellGestureLeap : Gesture
{
	private float _velocityNeeded = 3f;

	// In words:
	// Start if hand is opened and facing from player.
	public override bool StartPose()
	{
		if (!HandManager.AreBothHandsPresent()) return false;

		return HandManager.IsHandOpen(HandType.Right)
			&& HandManager.IsHandFacingFromPlayer(HandType.Right);
	}

	// In words:
	// Break if hand is closed or not facing from player.
	public override bool BreakPose()
	{
		if (!HandManager.AreBothHandsPresent()) return true;

		return !(HandManager.IsHandOpen(HandType.Right)
			&& HandManager.IsHandFacingPlayer(HandType.Right))	;
	}

	// In words:
	// Cast if hand is moving from player in StartPose.
	public override bool CastPose()
	{
		if (!HandManager.AreBothHandsPresent()) return false;

		return HandManager.IsHandMovingForward(HandType.Right, _velocityNeeded);
	}

}