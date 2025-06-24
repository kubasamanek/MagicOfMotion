using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script is attached to buttons which are only allowed to be pressed if Ultraleap device is connected.
/// </summary>
public class AllowOnlyIfLeapConnected : MonoBehaviour
{
	private LeapConnectionManager _connectionManager;
	public string SceneToLoad;

	void Start()
	{
		_connectionManager = FindObjectOfType<LeapConnectionManager>();
	}

	/// <summary>
	/// Starts the game.
	/// </summary>
	public void StartGame()
	{
		if (_connectionManager.IsLeapConnected() && _connectionManager.IsStreaming())
		{
			GameConfig.DeviceType = InputDeviceType.UltraLeap;
			if(GameManager.Instance != null)
				GameManager.Instance.SetDeviceType(GameConfig.DeviceType);
			
			LoadingScreenManager.Instance.LoadScene(SceneToLoad);
		}
	}

	/// <summary>
	/// Changes the color of the button to indicate if it's interactable or not.
	/// </summary>
	void Update()
	{
		this.GetComponent<Image>().color = _connectionManager.IsLeapConnected() && _connectionManager.IsStreaming() ? Color.white : Color.gray;
	}
}
