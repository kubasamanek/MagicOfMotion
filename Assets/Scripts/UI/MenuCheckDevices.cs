using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles menu active device indicators. Red indicates that the device is not connected, green indicates that the device is connected and working.
/// </summary>
public class MenuCheckDevices : MonoBehaviour
{
    private LeapConnectionManager _connectionManagerLeap;
    [SerializeField] private RgbConnectionManager _connectionManagerRgb;
    [SerializeField] private InputDeviceType _input;

    [SerializeField] private TextMeshProUGUI _text;

	private void Start()
    {
		_connectionManagerLeap = LeapConnectionManager.Instance;
		_connectionManagerRgb = FindFirstObjectByType<RgbConnectionManager>();
	}

	private void Update()
	{
		if(_connectionManagerRgb == null)
			_connectionManagerRgb = FindFirstObjectByType<RgbConnectionManager>();

		if (_input == InputDeviceType.UltraLeap)
		    CheckLeapDevice();
        else 
            CheckRgbDevice();
	}

	public void CheckLeapDevice()
    {
        if (_connectionManagerLeap.IsLeapConnected() && _connectionManagerLeap.IsStreaming())
        {
			this.GetComponent<Image>().color = Color.green;
            _text.text = "Leap device recognized."; 
		}
		else
		{
			this.GetComponent<Image>().color = Color.red;
			_text.text = "Leap device NOT recognized.";
		}

	}

    public void CheckRgbDevice()
    {
		if (_connectionManagerRgb.IsStreaming)
		{
			this.GetComponent<Image>().color = Color.green;
			_text.text = "RGB device recognized.";
		}
		else
		{
			this.GetComponent<Image>().color = Color.red;
			_text.text = "RGB device NOT recognized.";
		}
	}

}
