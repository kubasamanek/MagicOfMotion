using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles exit to the menu upon escape key press.
/// </summary>
public class ExitSandbox : MonoBehaviour
{
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			AudioManager.Instance.StopAllSFX();
			AudioManager.Instance.MusicOn = false;
			SceneManager.LoadScene("Menu");
		}
	}
}
