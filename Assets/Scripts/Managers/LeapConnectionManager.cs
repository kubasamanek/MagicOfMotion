using UnityEngine;
using Leap.Unity;

/// <summary>
/// Manages the connection status of the Leap Motion controller, 
/// ensuring the device's connection and data streaming status are monitored and maintained throughout the application.
/// </summary>
public class LeapConnectionManager : MonoBehaviour
{
	public static LeapConnectionManager Instance;

	private bool _isLeapConnected = false;
	private bool _isStreaming = false;

	// Singleton
	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Update()
	{
		UpdateLeapConnectionStatus();
	}

	/// <summary>
	/// Updates the connection and streaming status by checking if the LeapServiceProvider is connected and if data frames are being received.
	/// </summary>
	private void UpdateLeapConnectionStatus()
	{
		if (FindObjectOfType<LeapServiceProvider>() is LeapServiceProvider leapServiceProvider)
		{
			_isLeapConnected = leapServiceProvider.IsConnected();
			_isStreaming = leapServiceProvider.CurrentFrame != null && leapServiceProvider.CurrentFrame.Id > 0;
		}
		else
		{
			_isLeapConnected = false;
			_isStreaming = false;
		}
	}

	public bool IsLeapConnected()
	{
		return _isLeapConnected;
	}

	public bool IsStreaming()
	{
		return _isStreaming;
	}
}
