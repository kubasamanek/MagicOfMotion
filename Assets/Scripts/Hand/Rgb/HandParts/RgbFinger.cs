using UnityEngine;

public class RgbFinger : FingerBase
{
	private BoneBase[] _bones = new BoneBase[4];
	private FingerType _type;

	public RgbFinger(
		BoneBase metacarpal, 
		BoneBase proximal, 
		BoneBase intermediate, 
		BoneBase distal, 
		FingerType type)
	{
		_bones[0] = metacarpal; 
		_bones[1] = proximal;
		_bones[2] = intermediate;
		_bones[3] = distal;
		_type = type;
	}

	public override Vector3 TipPosition => _bones[3].NextJoint;
	
	public override Vector3 Direction => (_bones[3].NextJoint - _bones[1].PrevJoint).normalized;
	
	public override BoneBase[] Bones => _bones;

	public override FingerType Type => _type;

	public override bool IsExtended
	{
		get
		{
			// Get the direction of the first (proximal) bone.
			Vector3 firstBoneDirection = (_bones[1].NextJoint - _bones[1].PrevJoint).normalized;

			// Get the direction of the last (distal) bone.
			Vector3 lastBoneDirection = (_bones[_bones.Length - 1].NextJoint - _bones[_bones.Length - 1].PrevJoint).normalized;

			// Compare the direction of the first and last bone.
			// If the angle between them is small, it indicates the finger is extended/pointing.
			return Vector3.Angle(firstBoneDirection, lastBoneDirection) < 30f;
		}
	}

	public override BoneBase GetBone(BoneBase.BoneType boneType)
	{
		foreach (var bone in _bones)
		{
			if (bone == null) continue;
			if (bone.Type == boneType)
				return bone;
		}
		return null; 
	}
}