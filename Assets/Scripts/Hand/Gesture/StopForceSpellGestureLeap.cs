using UnityEngine;

public class StopForceSpellGestureLeap : Gesture
{
	private float _velocityNeeded = 3f;
	private float _startNormalAngle = -0.85f;

	// In words:
	// Start if hand is opened and facing down.
	public override bool StartPose()
	{
		if (!HandManager.AreBothHandsPresent()) return false;

		Vector3 palmNormal = this.HandManager.GetPalmNormal(HandType.Right);

		return palmNormal.y < _startNormalAngle && HandManager.IsHandOpen(HandType.Right);
	}

	// In words:
	// Break if hand is closed or not facing down.
	public override bool BreakPose()
	{
		if (!HandManager.AreBothHandsPresent()) return true;
		Vector3 palmNormal = this.HandManager.GetPalmNormal(HandType.Right);

		return !(HandManager.IsHandOpen(HandType.Right)
			&& palmNormal.y < _startNormalAngle);
	}

	// In words:
	// Cast if hand is moving down in StartPose.
	public override bool CastPose()
	{
		if (!HandManager.AreBothHandsPresent()) return false;

		return HandManager.IsHandMovingDown(HandType.Right, _velocityNeeded);

	}

}