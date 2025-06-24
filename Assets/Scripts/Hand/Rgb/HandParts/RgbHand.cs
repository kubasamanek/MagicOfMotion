using Leap;
using System.Collections.Generic;
using UnityEngine;

public class RgbHand : HandBase
{
	private List<RgbFinger> _fingers;
	private RgbPalm _palm;
	private HandType _handType;
	private Transform _playerTransform;

	public RgbHand(List<RgbFinger> fingers, RgbPalm palm, HandType handType)
	{
		_fingers = fingers;
		_palm = palm;
		_handType = handType;
		_playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
	}

	public override Vector3 PalmPosition => _palm.GetCenter();

	public override Vector3 PalmVelocity
	{
		get
		{
			return Vector3.zero;
		}
	}

	public override Arm Arm => null;

	public override Vector3 Wrist => _palm.Wrist;

	public override Vector3 PalmNormal => _palm.GetTargetRotation() * Vector3.down;

	public override Vector3 Direction => _palm.GetTargetRotation() * Vector3.forward;

	public override Quaternion Rotation => _palm.GetTargetRotation();

	public override FingerBase[] Fingers => _fingers.ToArray();

	public override bool IsLeft => _handType == HandType.Left;

	public override bool IsRight => _handType == HandType.Right;

	public HandType Type => _handType;

	public RgbPalm Palm => _palm;

}