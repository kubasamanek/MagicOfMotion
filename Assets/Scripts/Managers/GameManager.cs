using Leap.Unity;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	[SerializeField] private InputDeviceType _deviceType; 

	private GameObject _leftRgbHand;
	private GameObject _rightRgbHand;
    private LeapServiceProvider _service;

	public void SetDeviceType(InputDeviceType deviceType)
	{
		_deviceType = deviceType;
		UpdateDeviceState();
	}

	void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		_deviceType = GameConfig.DeviceType;
		_service = GameObject.FindGameObjectWithTag("LeapService")?.GetComponent<LeapServiceProvider>();
		_leftRgbHand = GameObject.FindGameObjectWithTag("Player")?.transform.GetChild(1)?.gameObject;
		_rightRgbHand = GameObject.FindGameObjectWithTag("Player")?.transform.GetChild(0)?.gameObject;
		UpdateDeviceState();
	}

	// Property to encapsulate _deviceType and react to changes
	public InputDeviceType DeviceType
	{
		get => _deviceType;
		set
		{
			if (_deviceType != value)
			{
				_deviceType = value;
				UpdateDeviceState();
			}
		}
	}

	/// <summary>
	/// Changes scene setup to work for the selected device.
	/// </summary>
	private void UpdateDeviceState()
	{
		switch (DeviceType)
		{
			case InputDeviceType.UltraLeap:
				if(_leftRgbHand != null && _rightRgbHand != null)
				{
					_leftRgbHand.SetActive(false);
					_rightRgbHand.SetActive(false);
				}
				_service?.gameObject.SetActive(true);
				//_leapHands.SetActive(true);	
				break;
			case InputDeviceType.RGBCamera:
				_leftRgbHand.SetActive(true);
				_rightRgbHand.SetActive(true);
				_service?.gameObject.SetActive(false);
				//_leapHands.SetActive(false);
				break;
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject); 
		}
		else if (Instance != this)
		{
			Destroy(gameObject);
		}

		SetDeviceType(GameConfig.DeviceType);
	}

	private void OnDestroy()
	{
		if(Instance == this && DeviceType == InputDeviceType.RGBCamera)
		{
			StopPythonScript();
		}
	}

	/// <summary>
	/// Uses taskkill to kill the motion capture script running in background.
	/// </summary>
	private void StopPythonScript()
	{
		string cmdCommand = "/C taskkill /IM rgb_capture.exe /F";

		ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe")
		{
			Arguments = cmdCommand,
			UseShellExecute = true,
			CreateNoWindow = false,
			RedirectStandardOutput = false,
			RedirectStandardError = false
		};

		using (Process cmd = new Process { StartInfo = startInfo })
		{
			cmd.Start();
			cmd.WaitForExit();

			UnityEngine.Debug.Log("Attempted to stop Python script.");
		}
	}

	private void Start()
	{
		UpdateDeviceState();
	}

	private void OnValidate()
	{
		UpdateDeviceState();
	}
}
