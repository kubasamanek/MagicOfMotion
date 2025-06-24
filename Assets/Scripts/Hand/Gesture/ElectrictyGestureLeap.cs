using UnityEngine;

public class ElectricityGestureLeap : Gesture
{
	private float _minimalYPosition = 0.65f;
	private bool _charged = false;

	private float _initialTime = 0f;
	private float _timeToCharge = 2f;

	public ElectricityGestureLeap() : base() { }

	// In words:
	// Start if hand is open with palm facing up and hand is high enough to start charging.
	public override bool StartPose()
	{
		if (!HandManager.AreBothHandsPresent()) return false;

		bool isOpen = HandManager.IsHandOpen(HandType.Right);

		Vector3 palmNormal = HandManager.GetPalmNormal(HandType.Right);
		float dotProduct = Vector3.Dot(palmNormal.normalized, Vector3.up);

		if (isOpen && dotProduct > 0.8f && RightHandHighEnough())
		{
			_initialTime = Time.time;
			return true;
		}

		return false;
	}

	// In words:
	// Break if hand broke the StartPose before the spell was charged.
	public override bool BreakPose()
	{
		_charged = Time.time - _initialTime > _timeToCharge;

		if (_charged) return false;
		if (!HandManager.AreBothHandsPresent()) return true;

		bool isOpen = HandManager.IsHandOpen(HandType.Right);
		Vector3 palmNormal = HandManager.GetPalmNormal(HandType.Right);
		float dotProduct = Vector3.Dot(palmNormal.normalized, Vector3.up);

		return !isOpen || dotProduct < 0.8f || !RightHandHighEnough();
	}

	// In words:
	// Break if hand stayed in the StartPose for the whole charging time.
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
