using UnityEngine;

public abstract class BoneBase
{
	public abstract Vector3 PrevJoint { get; }
	
	public abstract Vector3 NextJoint { get; }
	
	public abstract Vector3 Center { get; }
	
	public abstract Vector3 Direction { get; }
	
	public abstract Quaternion Rotation { get; }
	
	public abstract BoneType Type { get; }
	
	public abstract float Length { get; }

	public enum BoneType
	{
		TYPE_INVALID = -1,
		TYPE_METACARPAL = 0, // Bone in the palm of the hand
		TYPE_PROXIMAL = 1, // First bone in the finger (closest to the palm)
		TYPE_INTERMEDIATE = 2, // Middle bone in the finger
		TYPE_DISTAL = 3 // Last bone in the finger (closest to the tip)
	}
}
