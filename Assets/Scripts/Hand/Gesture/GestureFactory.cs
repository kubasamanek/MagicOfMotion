using System;

/// <summary>
/// This class provides a factory for creating gesture objects associated with spells based on the type of motion capture device active in the system.
/// </summary>
public static class GestureFactory
{
	public static Gesture CreateFireballGesture(InputDeviceType deviceType)
	{
		switch (deviceType)
		{
			case InputDeviceType.UltraLeap:
				return new FireballGestureLeap(); 
			case InputDeviceType.RGBCamera:
				return new FireballGestureRgb(); 
			default:
				throw new ArgumentException("Unsupported device type", nameof(deviceType));
		}
	}

	public static Gesture CreateElectricityGesture(InputDeviceType deviceType)
	{
		switch (deviceType)
		{
			case InputDeviceType.UltraLeap:
				return new ElectricityGestureLeap();
			case InputDeviceType.RGBCamera:
				return new ElectricityGestureRgb();
			default:
				throw new ArgumentException("Unsupported device type", nameof(deviceType));
		}
	}

	public static Gesture CreateForceGesture(InputDeviceType deviceType)
	{
		switch (deviceType)
		{
			case InputDeviceType.UltraLeap:
				return new ForceSpellGestureLeap();
			case InputDeviceType.RGBCamera:
				return null;
			default:
				throw new ArgumentException("Unsupported device type", nameof(deviceType));
		}
	}

	public static Gesture CreatePushForceGesture(InputDeviceType deviceType)
	{
		switch (deviceType)
		{
			case InputDeviceType.UltraLeap:
				return new PushForceSpellGestureLeap();
			case InputDeviceType.RGBCamera:
				return null;
			default:
				throw new ArgumentException("Unsupported device type", nameof(deviceType));
		}
	}

	public static Gesture CreateStopForceGesture(InputDeviceType deviceType)
	{
		switch (deviceType)
		{
			case InputDeviceType.UltraLeap:
				return new StopForceSpellGestureLeap();
			case InputDeviceType.RGBCamera:
				return null;
			default:
				throw new ArgumentException("Unsupported device type", nameof(deviceType));
		}
	}

	public static Gesture CreateShieldGesture(InputDeviceType deviceType)
	{
		switch (deviceType)
		{
			case InputDeviceType.UltraLeap:
				return new ShieldSpellGestureLeap();
			case InputDeviceType.RGBCamera:
				return new ShieldSpellGestureLeap();
			default:
				throw new ArgumentException("Unsupported device type", nameof(deviceType));
		}
	}
}
