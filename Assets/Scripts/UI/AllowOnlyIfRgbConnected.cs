using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script is attached to buttons which are only allowed to be pressed if RGB device is connected.
/// </summary>
public class AllowOnlyIfRgbConnected : MonoBehaviour
{
    private RgbConnectionManager _connectionManager;
    public string SceneToLoad;

    void Start()
    {
        _connectionManager = FindObjectOfType<RgbConnectionManager>();
    }

    /// <summary>
    /// Starts the game.
    /// </summary>
    public void StartGame()
    {
        if(_connectionManager.IsStreaming)
        {
            GameConfig.DeviceType = InputDeviceType.RGBCamera;
			LoadingScreenManager.Instance.LoadScene(SceneToLoad);
		}
	}

	/// <summary>
	/// Changes the color of the button to indicate if it's interactable or not.
	/// </summary>
	void Update()
    {
        this.GetComponent<Image>().color = _connectionManager.IsStreaming ? Color.white : Color.gray;
    }
}
