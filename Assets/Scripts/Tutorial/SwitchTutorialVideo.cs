using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Used to set the correct video based on input motion capture device.
/// </summary>
public class SwitchTutorialVideo : MonoBehaviour
{
    public VideoClip LeapTutorial;
    public VideoClip RgbTutorial;

    void Start()
    {
        if(GameManager.Instance.DeviceType == InputDeviceType.UltraLeap)
        {
            this.GetComponent<VideoPlayer>().clip = LeapTutorial;
        } 
        else if (GameManager.Instance.DeviceType == InputDeviceType.RGBCamera)
        {
            this.GetComponent<VideoPlayer>().clip = RgbTutorial;
        }
    }

}
