using Leap;
using System.Linq;
using UnityEngine;

public class LeapHand : HandBase
{
    private Leap.Hand hand;

	public LeapHand(Leap.Hand hand)
	{
		this.hand = hand;
	}

	public override Arm Arm => hand.Arm;

	public override Vector3 Wrist => hand.WristPosition;
	
	public override Vector3 PalmPosition => hand.PalmPosition;

	public override Vector3 PalmVelocity => hand.PalmVelocity;

	public override Vector3 PalmNormal => hand.PalmNormal;
	
	public override Vector3 Direction => hand.Direction;
	
	public override FingerBase[] Fingers => hand.Fingers.Select(b => new LeapFinger(b)).ToArray();

	public override Quaternion Rotation => hand.Rotation;

	public override bool IsLeft => hand.IsLeft;

	public override bool IsRight => hand.IsRight;
}
