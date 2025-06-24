using UnityEngine;
using UnityEngine.Video;
using TMPro;

/// <summary>
/// Manages the display and behavior of tutorial popups, including video playback and text updates,
/// and controls player movement during tutorials.
/// </summary>
public class TutorialPopup : MonoBehaviour
{
	public VideoPlayer VideoPlayer; 
	public TextMeshProUGUI TutorialText;
	public TextMeshProUGUI TimerText;

	private TutorialCheckpoint _attachedCheckpoint;
	private HorseMovement _playerMovement;

	private void Start()
	{
		_playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<HorseMovement>();
		_playerMovement.StopMovement();
		_playerMovement.enabled = false;
		VideoPlayer = GetComponentInChildren<VideoPlayer>();
		TutorialText = GetComponentInChildren<TextMeshProUGUI>();
	}

	/// <summary>
	/// Sets up the tutorial popup based on the provided checkpoint data and the current device type.
	/// </summary>
	/// <param name="checkpointData">The data containing video and text for this checkpoint.</param>
	/// <param name="attachedCheckpoint">The tutorial checkpoint this popup is associated with.</param>
	public void Setup(CheckpointData checkpointData, TutorialCheckpoint attachedCheckpoint)
	{
		_attachedCheckpoint = attachedCheckpoint;
		InputDeviceType deviceType = GameManager.Instance.DeviceType;

		if (TutorialText != null)
		{
			string txt = deviceType == InputDeviceType.UltraLeap ? checkpointData.LeapText : checkpointData.RgbText;
			if (txt == "") TutorialText.text = checkpointData.LeapText;
			else TutorialText.text = txt;
		}

		VideoClip video = deviceType == InputDeviceType.UltraLeap ? checkpointData.LeapVideo : checkpointData.RgbVideo ?? checkpointData.LeapVideo;

		if (VideoPlayer != null && video != null)
		{
			VideoPlayer.clip = video;
			VideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
			VideoPlayer.Play();
		}
		else if (VideoPlayer != null)
		{
			VideoPlayer.gameObject.SetActive(false);
		}

		if (TimerText != null && TimerText.text == "") TimerText.enabled = false;

	}

	/// <summary>
	/// Pass timer to checkpoint popup, so it gets displayed.
	/// </summary>
	/// <param name="time">Time elapsed since last checkpoint.</param>
	public void SetTimerText(float time)
	{
		TimerText.text = "Time: " + time.ToString("F2");
	}

	/// <summary>
	/// Monitors for the completion gesture (both hands doing a peace sign) to close the popup.
	/// </summary>
	private void Update()
	{
		if (HandManager.Instance.BothHandsDoingPeaceSign())
			ClosePopup();
	}

	/// <summary>
	/// Closes the popup, re-enables player movement, and marks the checkpoint as complete.
	/// </summary>
	void ClosePopup()
	{
		_playerMovement.enabled = true;
		_attachedCheckpoint.CompleteCheckpoint();
		Destroy(gameObject);
	}
}
