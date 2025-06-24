using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Base class for all checkpoint types. Handles activation and completition which is shared through all the types.
/// </summary>
public abstract class TutorialCheckpoint : MonoBehaviour
{
	public int ID;
	public CheckpointData CheckpointData;
	public GameObject ActiveUI = null;
	public bool SkipForRGB = false;
	public bool Repeated = false;
	public bool DisplayTime = false;
	public float TimeTaken { get; private set; }
	
	private float _startTime;
	private bool _timerRunning = false;

	/// <summary>
	/// Called upon checkpoint activation. 
	/// It activates this checkpoint's popup and plays the lecture video.
	/// </summary>
	/// <param name="repeated">True if called by repeat gesture, false if called automatically</param>
	/// <param name="skip">True if popup should be skipped</param>
	public virtual void ActivateCheckpoint(bool repeated = false, bool skip = false)
	{
		if(!repeated)
			StopTimer();

		if (skip)
		{
			CompleteCheckpoint();
			return;
		}

		Repeated = repeated;
		Canvas canvas = FindObjectOfType<Canvas>();
		ActiveUI = Instantiate(CheckpointData.UI, canvas.transform, false);
		ActiveUI.transform.localPosition = Vector3.zero;

		TutorialPopup popup = ActiveUI.GetComponent<TutorialPopup>();
		if (popup != null)
		{
			if (DisplayTime)
			{
				popup.SetTimerText(TimeTaken);
				TutorialManager.Instance.SaveTime(TimeTaken);
			}
			popup.Setup(CheckpointData, this);
		}

		VideoPlayer videoPlayer = ActiveUI.GetComponentInChildren<VideoPlayer>();
		if (videoPlayer != null)
		{
			videoPlayer.playOnAwake = false;
			videoPlayer.renderMode = VideoRenderMode.RenderTexture;

			videoPlayer.targetTexture = CheckpointData.RenderTexture;
			videoPlayer.clip = GameManager.Instance.DeviceType == InputDeviceType.UltraLeap ? CheckpointData.LeapVideo : CheckpointData.RgbVideo ?? CheckpointData.LeapVideo;
			videoPlayer.isLooping = true;

			RawImage rawImage = ActiveUI.GetComponentInChildren<RawImage>(); 
			rawImage.texture = CheckpointData.RenderTexture;

			videoPlayer.Play();
		}
	}

	/// <summary>
	/// Starts timer for this checkpoint.
	/// </summary>
	public void StartTimer()
	{
		_startTime = Time.time;
		_timerRunning = true;
	}

	/// <summary>
	/// Stops timer for this checkpoint.
	/// </summary>
	public void StopTimer()
	{
		if (_timerRunning)
		{
			TimeTaken = Time.time - _startTime;
			_timerRunning = false;
		}
	}

	public virtual void TriggerAction() { }

	/// <summary>
	/// Completes checkpoint and triggers next one.
	/// </summary>
	public virtual void CompleteCheckpoint()
	{
		this.gameObject.SetActive(false);
		ActiveUI = null;
		if (!Repeated)
			TutorialManager.Instance.TriggerNextCheckpoint();
	}
}
