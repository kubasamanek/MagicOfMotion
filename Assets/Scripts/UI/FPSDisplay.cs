using UnityEngine;

/// <summary>
/// Handles FPS display for test purposes.
/// </summary>
public class FPSDisplay : MonoBehaviour
{
	private float _fps;
	public TMPro.TextMeshProUGUI FPSCounterText;

	void Start()
	{
		InvokeRepeating("GetFPS", 1, 1);
	}


	void GetFPS()
	{
		_fps = (int)(1f / Time.unscaledDeltaTime);
		FPSCounterText.text = "FPS: " + _fps.ToString();
	}
}
