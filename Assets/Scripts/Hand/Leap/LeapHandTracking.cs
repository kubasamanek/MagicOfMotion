using System.Collections;
using UnityEngine;
using Leap;
using Leap.Unity;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the tracking of hand data using Leap Motion technology throughout different scenes in a Unity application.
/// This class handles hand tracking updates, ensures persistence across scene loads, and refreshes tracking components as needed.
/// </summary>
public class LeapHandTracking : MonoBehaviour
{
	public static LeapHandTracking Instance;

	[SerializeField] LeapProvider leapProvider;

	public Hand LeftHand;
	public Hand RightHand;
	private Hand[] _hands = new Hand[2];

	public LeapHand LeftLeapHand;
	public LeapHand RightLeapHand;

	// Singleton
	void Awake()
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

	/// <summary>
	/// Registers events for scene loading and frame updates when the script is enabled.
	/// </summary>
	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		if(leapProvider != null) leapProvider.OnUpdateFrame += OnUpdateFrame;
	}
	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		if (leapProvider != null) leapProvider.OnUpdateFrame -= OnUpdateFrame;
	}

	/// <summary>
	/// Re-acquires the LeapProvider component when a new scene is loaded and refreshes this script to ensure continuous tracking.
	/// </summary>
	/// <param name="scene">The scene that has been loaded.</param>
	/// <param name="mode">The mode with which the scene was loaded.</param>
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		leapProvider = GameObject.FindGameObjectWithTag("LeapService")?.GetComponent<LeapProvider>();
		StartCoroutine(RefreshScript());
	}

	/// <summary>
	/// Temporarily disables and then re-enables this component to reset Leap Motion tracking after a scene load.
	/// </summary>
	private IEnumerator RefreshScript()
	{
		this.enabled = false;
		yield return null; // Wait for one frame
		this.enabled = true;
	}

	/// <summary>
	/// Updates and stores the current hand data from Leap Motion each frame.
	/// Processes the hands individually and updates public properties for use in other components.
	/// </summary>
	/// <param name="frame">The frame data containing hand tracking information from Leap Motion.</param>
	void OnUpdateFrame(Frame frame)
	{
		LeftHand = frame.GetHand(Chirality.Left);
		RightHand = frame.GetHand(Chirality.Right);

		if(LeftHand != null)
			LeftLeapHand = new LeapHand(LeftHand);
		else 
			LeftLeapHand = null;

		if (RightHand != null)
			RightLeapHand = new LeapHand(RightHand);
		else
			RightLeapHand = null;

		_hands[0] = LeftHand;
		_hands[1] = RightHand;
	}
}
