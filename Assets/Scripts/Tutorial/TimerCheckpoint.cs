using System.Collections;
using UnityEngine;

/// <summary>
/// Handles timer checkpoint type, which gets activated after time elapses. Used for free play lectures or as a welcome lecture.
/// </summary>
public class TimerCheckpoint : TutorialCheckpoint
{
	public float ActivationDelay;
	public float LastStopped = 0;

	public bool IsPaused = false;
	private float _timerStartTime;

	public void StartActivationDelay()
	{
		_timerStartTime = Time.time;
		IsPaused = false;
		StartCoroutine(ActivationDelayCoroutine());
	}

	private IEnumerator ActivationDelayCoroutine()
	{
		float remainingTime = ActivationDelay;

		while (remainingTime > 0)
		{
			if (IsPaused)
			{
				yield return null;
			}
			else
			{
				_timerStartTime = Time.time;

				yield return new WaitForSeconds(0.1f);  

				float timeElapsed = Time.time - _timerStartTime;
				remainingTime -= timeElapsed;
			}
		}

		ActivateCheckpoint();
	}

	/// <summary>
	/// Checkpoint timer is paused. 
	/// Typically when the checkpoint was repeated by the player and we want to stop the UI countdown, so the UI then does not overlap.
	/// </summary>
	public void PauseTimer()
	{
		if (!IsPaused)
		{
			IsPaused = true;
		}
	}

	/// <summary>
	/// Checkpoint timer is resumed. 
	/// </summary>
	public void ResumeTimer()
	{
		if (IsPaused)
		{
			_timerStartTime = Time.time;
			IsPaused = false;
		}
	}

}
