using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles task checkpoint type, which needs gets activated upon task completition.
/// </summary>
public class TaskCheckpoint : TutorialCheckpoint
{
	public UnityEvent OnTaskTriggered;
	public string ProgressText;

	[SerializeField] private GameObject _progressUIPrefab;

	public int RequiredTriggers;

	private GameObject _progressUIInstance;
	private int _currentTriggers = 0;
	private bool _activationInvoked = false;

	private void OnEnable()
	{
		OnTaskTriggered.AddListener(OnTaskAction);

		Canvas canvas = FindObjectOfType<Canvas>();
		if (canvas != null)
		{
			Instantiate(_progressUIPrefab, canvas.transform);
		}
	}

	private void OnDisable()
	{
		OnTaskTriggered.RemoveListener(OnTaskAction);
	}

	private void OnTaskAction()
	{
		_currentTriggers++;
		if (_currentTriggers >= RequiredTriggers && !_activationInvoked)
		{
			_activationInvoked = true;
			StartCoroutine(DelayedActivation());
		}
	}

	private IEnumerator DelayedActivation()
	{
		yield return new WaitForSeconds(0.5f); 
		ActivateCheckpoint();  
	}

	/// <summary>
	/// Format the progress text displayed during task checkpoint.
	/// </summary>
	/// <returns>Formatted progress text</returns>
	public string GetProgressText()
	{
		return $"{ProgressText}: {_currentTriggers}/{RequiredTriggers}";
	}

	/// <summary>
	/// Called when task action was triggered.
	/// </summary>
	public override void TriggerAction()
	{
		OnTaskTriggered.Invoke();
	}

	/// <summary>
	/// Completes this checkpoint and destroys progress text.
	/// </summary>
	public override void CompleteCheckpoint()
	{
		if (_progressUIInstance != null)
		{
			Destroy(_progressUIInstance);
			_progressUIInstance = null;
		}

		base.CompleteCheckpoint();
	}
}
