using UnityEngine;

public class RgbPalm
{
	public Vector3 Wrist;
	public Vector3 UnderIndex;
	public Vector3 UnderPinky;
	public HandType HandType;

	public RgbPalm(Vector3 wrist,
		Vector3 underIndex,
		Vector3 underPinky,
		HandType type) 
	{
		Wrist = wrist;
		UnderIndex = underIndex;
		UnderPinky = underPinky;
		HandType = type;
	}

	public Vector3 GetCenter()
	{
		return (Wrist + UnderIndex + UnderIndex) / 3.0f;
	}

	public Quaternion GetTargetRotation()
	{
		Debug.DrawLine(Wrist, UnderIndex, Color.black, 0f, false);
		Debug.DrawLine(UnderIndex, UnderPinky, Color.black, 0f, false);
		Debug.DrawLine(UnderPinky, Wrist, Color.black, 0f, false);

		Vector3 qb = UnderIndex - Wrist;
		Vector3 qc = UnderPinky - Wrist;
		Vector3 n = Vector3.Cross(qb, qc);

		Vector3 unitY = n.normalized;

		Vector3 midPoint = (UnderIndex + UnderPinky) / 2;
		Vector3 unitZ = (midPoint - Wrist).normalized;
		//Vector3 unitZ = qc.normalized;

		Vector3 unitX = Vector3.Cross(unitY, unitZ);
		Debug.DrawLine(Wrist, Wrist - unitX * 5, Color.red, 0f, false);
		Debug.DrawLine(Wrist, Wrist + unitY * 5, Color.blue, 0f, false);

		float handenessCoeficient = HandType == HandType.Left ? -1 : 1;
		Quaternion targetRotation = Quaternion.LookRotation(-unitX * handenessCoeficient, unitY * handenessCoeficient);
		return targetRotation;
	}
}
