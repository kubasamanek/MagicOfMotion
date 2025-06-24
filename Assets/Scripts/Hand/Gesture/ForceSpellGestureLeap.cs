public class ForceSpellGestureLeap : Gesture
{
	private float _palmNormalStartLimit = 0.7f;
	private float _velocityNeeded = 2f;

	// In words:
	// Start if hand is opened and facing up.
	public override bool StartPose()
	{
		if (!HandManager.AreBothHandsPresent()) return false;

		return HandManager.IsHandOpen(HandType.Right)
			&& HandManager.GetPalmNormal(HandType.Right).y > _palmNormalStartLimit;
	}

	// In words:
	// Break if hand is closed or not facing up.
	public override bool BreakPose()
	{
		if (!HandManager.AreBothHandsPresent()) return true;

		return !(HandManager.IsHandOpen(HandType.Right)
			&& HandManager.GetPalmNormal(HandType.Right).y > _palmNormalStartLimit);
	}

	// In words:
	// Start if hand is moving up in a StartPose.
	public override bool CastPose()
	{
		if (!HandManager.AreBothHandsPresent()) return false;

		return HandManager.IsHandMovingUp(HandType.Right, _velocityNeeded);
	}

}