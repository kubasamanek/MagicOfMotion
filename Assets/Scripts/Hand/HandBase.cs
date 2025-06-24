using Leap;
using UnityEngine;

public abstract class HandBase 
{
    public abstract Vector3 PalmPosition { get; }

    public abstract Vector3 PalmVelocity { get; }

	public abstract Vector3 PalmNormal { get; }

	public abstract Vector3 Direction { get; }

	public abstract Vector3 Wrist { get; }

	public abstract Arm Arm { get; }

	public abstract Quaternion Rotation { get; }

	public abstract FingerBase[] Fingers { get; }
	
	public abstract bool IsLeft {  get; }
	
	public abstract bool IsRight { get; }

}
