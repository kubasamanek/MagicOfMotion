using UnityEngine;

public class FireballGestureLeap : Gesture
{
	private float _startNormalAngle = -0.85f;
	private float _castNormalAngle = 0.6f;
	public FireballGestureLeap() : base() { }

	// In words:
	// Start if hand is closed and palm facing down.
	public override bool StartPose()
	{
		if (!HandManager.AreBothHandsPresent()) return false;

		Vector3 palmNormal = this.HandManager.GetPalmNormal(HandType.Right);
		bool handClosed = this.HandManager.IsHandClosed(HandType.Right);

		if (handClosed && palmNormal.y < _startNormalAngle)
			return true;

		return false;
	}

	// In words:
	// Break if hand was opened before palm was rotated up.
	public override bool BreakPose()
	{
		if (!HandManager.AreBothHandsPresent())
			return true;

		Vector3 palmNormal = this.HandManager.GetPalmNormal(HandType.Right);
		bool handClosed = this.HandManager.IsHandClosedWithoutThumb(HandType.Right);

		if (!handClosed && palmNormal.y < 0)
		{
			return true;
		}

		return false;
	}

	// In words:
	// Cast if palm is facing up and hand is opened.
	public override bool CastPose()
	{
		if (!HandManager.IsHandOpen(HandType.Right)
			|| !HandManager.AreBothHandsPresent())
			return false;

		Vector3 palmNormal = this.HandManager.GetPalmNormal(HandType.Right);

		if (palmNormal.y > _castNormalAngle)
			return true;

		return false;
	}

}
