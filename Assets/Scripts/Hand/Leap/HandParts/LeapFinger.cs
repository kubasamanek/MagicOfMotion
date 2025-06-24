using System.Linq;
using UnityEngine;

public class LeapFinger : FingerBase
{
	private Leap.Finger finger;

	public LeapFinger(Leap.Finger finger)
	{
		this.finger = finger;
	}

	public override Vector3 TipPosition => finger.TipPosition;

	public override Vector3 Direction => finger.Direction;
	
	public override bool IsExtended => finger.IsExtended;
	
	public override BoneBase[] Bones => finger.bones.Select(b => new LeapBone(b)).ToArray(); 

	public override BoneBase GetBone(BoneBase.BoneType boneType) => new LeapBone(finger.Bone((Leap.Bone.BoneType)boneType));
	
	public override FingerType Type => (FingerType)finger.Type; 
}
