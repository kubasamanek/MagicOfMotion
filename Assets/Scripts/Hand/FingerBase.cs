using UnityEngine;

public abstract class FingerBase 
{
	public abstract Vector3 TipPosition { get; }
	
	public abstract Vector3 Direction { get; }
	
	public abstract bool IsExtended { get; }
	
	public abstract BoneBase[] Bones { get; }
	
	public abstract BoneBase GetBone(BoneBase.BoneType boneType);

	public enum FingerType
	{
		TYPE_THUMB,
		TYPE_INDEX,
		TYPE_MIDDLE,
		TYPE_RING,
		TYPE_PINKY,
		TYPE_UNKNOWN
	}
	public abstract FingerType Type { get; }

}
