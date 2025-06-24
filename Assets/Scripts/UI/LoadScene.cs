using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script to load scene by name and device.
/// </summary>
public class LoadScene : MonoBehaviour
{
	public void LoadSceneByName(string nameAndDevice)
	{
		string[] splitData = nameAndDevice.Split(',');
		string sceneName = splitData[0];
		InputDeviceType deviceType = (InputDeviceType)System.Enum.Parse(typeof(InputDeviceType), splitData[1]);
		GameConfig.DeviceType = deviceType;
		SceneManager.LoadScene(sceneName);
	}   
}
