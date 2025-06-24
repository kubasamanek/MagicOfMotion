using UnityEngine;

public class RgbBone : BoneBase
{
	private Vector3 _prevJoint;
	private Vector3 _nextJoint;
	private BoneBase.BoneType _type;

	public RgbBone(Vector3 prev, Vector3 next, BoneBase.BoneType type)
	{
		_prevJoint = prev;
		_nextJoint = next;
		_type = type;
	}

	public override Vector3 PrevJoint => _prevJoint;
	
	public override Vector3 NextJoint => _nextJoint;
	
	public override Vector3 Center => (_prevJoint + _nextJoint) / 2;
	
	public override Vector3 Direction => (_nextJoint - _prevJoint).normalized;
	
	public override Quaternion Rotation => Quaternion.identity;

	public override BoneBase.BoneType Type => _type;
	
	public override float Length => Vector3.Distance(PrevJoint, NextJoint);
}