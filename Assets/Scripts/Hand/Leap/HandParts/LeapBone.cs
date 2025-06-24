using UnityEngine;
public class LeapBone : BoneBase
{
	private Leap.Bone bone;

	public LeapBone(Leap.Bone bone)
	{
		this.bone = bone;
	}

	public override Vector3 PrevJoint => bone.PrevJoint; 
	
	public override Vector3 NextJoint => bone.NextJoint; 
	
	public override Vector3 Center => bone.Center; 
	
	public override Vector3 Direction => bone.Direction; 
	
	public override Quaternion Rotation => bone.Rotation;
	
	public override float Length => bone.Length;
	
	public override BoneType Type => (BoneType)bone.Type; 
}
