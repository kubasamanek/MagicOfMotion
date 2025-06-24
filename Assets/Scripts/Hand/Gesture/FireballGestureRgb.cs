
using UnityEngine;

public class FireballGestureRgb : Gesture
{
	private float _dotProductStartLimit = 0.8f;
	private float _dotProductCastLimit = -0.6f;

	public FireballGestureRgb() : base() { }

	// In words:
	// Start if hand is closed and palm is facing player.
	public override bool StartPose()
	{
		if (!HandManager.AreBothHandsPresent()) return false;

		return ClosedHandFacingPlayer(HandType.Right) && !ClosedHandFacingPlayer(HandType.Left);
	}

	// In words:
	// Break if hand was opened before palm was rotated from player.
	public override bool BreakPose()
	{
		if (!HandManager.AreBothHandsPresent())
			return true;

		Vector3 palmNormal = this.HandManager.GetPalmNormal(HandType.Right);
		Vector3 palmPosition = this.HandManager.GetPalmPosition(HandType.Right);
		//bool handClosed = this.HandManager.IsHandClosed(Chirality.Right);
		bool handClosed = this.HandManager.IsHandClosedWithoutThumb(HandType.Right);

		Vector3 directionToPlayer = (PlayerTransform.position - palmPosition).normalized;
		float dotProduct = Vector3.Dot(palmNormal, directionToPlayer);

		if (!handClosed && dotProduct > _dotProductCastLimit)
			return true;

		return false;
	}

	// In words:
	// Cast if hand is opened and palm facing from player.
	public override bool CastPose()
	{
		if (!HandManager.IsHandOpen(HandType.Right)
			|| !HandManager.AreBothHandsPresent())
			return false;

		Vector3 palmNormal = this.HandManager.GetPalmNormal(HandType.Right);
		Vector3 palmPosition = this.HandManager.GetPalmPosition(HandType.Right);

		Vector3 directionToPlayer = (PlayerTransform.position - palmPosition).normalized;
		float dotProduct = Vector3.Dot(palmNormal, directionToPlayer);

		if (dotProduct < _dotProductCastLimit)
			return true;

		return false;
	}

	/// <summary>
	/// Checks if a hand is closed and is facing player.
	/// </summary>
	/// <param name="handType">Left/Right hand</param>
	/// <returns>True if hand facing player and closed</returns>
	private bool ClosedHandFacingPlayer(HandType handType)
	{

		Vector3 palmNormal = this.HandManager.GetPalmNormal(handType);
		Vector3 palmPosition = this.HandManager.GetPalmPosition(handType);
		bool handClosed = this.HandManager.IsHandClosed(handType);

		Vector3 directionToPlayer = (PlayerTransform.position - palmPosition).normalized;
		float dotProduct = Vector3.Dot(palmNormal, directionToPlayer);

		if (handClosed && dotProduct > _dotProductStartLimit)
			return true;

		return false;
	}
}