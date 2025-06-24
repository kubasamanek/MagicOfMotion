using UnityEngine;

public class ElectricityGestureRgb : Gesture
{
	private float _minimalYPosition = 0.5f;
	public ElectricityGestureRgb() : base() { }

	// In words:
	// Start if hand is pointing up with index finger and is high enough to start charging.
	public override bool StartPose()
	{
		if (!HandManager.AreBothHandsPresent()) return false;

		bool isPointing = HandManager.IsHandPointing(HandType.Right);
		Vector3 pointingDirection = HandManager.GetPointingDirection(HandType.Right);
		float dotProduct = Vector3.Dot(pointingDirection.normalized, Vector3.up);

		return isPointing && dotProduct > 0.8f && RightHandHighEnough();
	}

	// In words:
	// Break if hand broke the StartPose before the spell was charged.
	public override bool BreakPose()
	{
		if (!HandManager.AreBothHandsPresent()) return true;

		bool isPointing = HandManager.IsHandPointing(HandType.Right);
		Vector3 pointingDirection = HandManager.GetPointingDirection(HandType.Right);
		float dotProduct = Vector3.Dot(pointingDirection.normalized, Vector3.up);

		return !isPointing || dotProduct < 0.8f || !RightHandHighEnough();
	}

	// In words:
	// Cast if hand stayed in StartPose for the whole charging time.
	public override bool CastPose()
	{
		return StartPose();
	}

	/// <summary>
	/// Check if right hand is high enough for a spell to start.
	/// </summary>
	/// <returns>True if it's higher than the limit, otherwise false</returns>
	private bool RightHandHighEnough()
	{
		float ropeY = GameObject.FindGameObjectWithTag("Player").transform.position.y;

		float rightDistanceY = HandManager.GetPalmPosition(HandType.Right).y - ropeY;

		return rightDistanceY > _minimalYPosition;
	}
}
