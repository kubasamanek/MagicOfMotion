using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Used to end the tutorial.
/// </summary>
public class ExitTutorial : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
        if (other.CompareTag("Player"))
        {
            AudioManager.Instance.StopAllSFX();
            SceneManager.LoadScene("Sandbox");
        }
	}
}
