using UnityEngine;

/// <summary>
/// Script to quit the application.
/// </summary>
public class QuitGameButton : MonoBehaviour
{
	public void QuitGame()
	{
		Debug.Log("Quit game requested");
		Application.Quit();
	}
}
