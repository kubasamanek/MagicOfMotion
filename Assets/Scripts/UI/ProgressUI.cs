using TMPro;
using UnityEngine;

/// <summary>
/// Used during Tutorial. Updated by TaskCheckpoints.
/// </summary>
public class ProgressUI : MonoBehaviour
{
	public TextMeshProUGUI progressText; 

	private void Update()
	{
		if (TutorialManager.Instance.CurrentCheckpoint is TaskCheckpoint taskCheckpoint)
		{
			progressText.text = taskCheckpoint.GetProgressText();
		}
	}
}
