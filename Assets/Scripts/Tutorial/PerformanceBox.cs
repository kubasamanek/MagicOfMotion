using UnityEngine;

/// <summary>
/// Used to handle performance testing box behaviour. 
/// Box dissapears when player aims on it for a second.
/// </summary>
public class PerformanceBox : MonoBehaviour
{
    private AimControllerBase _aimController;
	private float _aimTimer = 0f;
	private bool _isAimedAt = false;

	void Start()
	{
        if(GameConfig.DeviceType == InputDeviceType.UltraLeap)
            _aimController = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<LeapAimController>();
        else
			_aimController = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<RgbAimController>();
	}

	void Update()
	{
		// Check if currently aimed at
		if (_aimController != null && _aimController.AimedOn == this.gameObject)
		{
			if (!_isAimedAt)
			{
				_isAimedAt = true;
				_aimTimer = 0f;
			}
			_aimTimer += Time.deltaTime;

			// Check if the aim has been held for more than a second
			if (_aimTimer >= 1f)
			{
				TutorialManager.Instance.CurrentCheckpoint?.TriggerAction();
				Destroy(gameObject);
			}
		}
		else
		{
			_isAimedAt = false;
			_aimTimer = 0f;
		}
	}

}
