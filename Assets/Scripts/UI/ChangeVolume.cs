using UnityEngine;
using UnityEngine.UI;

public class ChangeVolume : MonoBehaviour
{
	public Slider _volumeSlider;  

	void Start()
	{
		if (_volumeSlider != null)
		{
			_volumeSlider.onValueChanged.AddListener(AdjustVolume);
		}
	}

	public void AdjustVolume(float volume)
	{
		AudioManager.Instance.AdjustMusicVolume(volume);
	}
}
