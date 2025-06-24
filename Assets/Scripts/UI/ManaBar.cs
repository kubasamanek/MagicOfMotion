using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mana bar handler.
/// </summary>
public class ManaBar : MonoBehaviour
{
    public Slider slider;
    private ManaSystem _manaSystem;  

	private void OnEnable()
	{
		_manaSystem = GameObject.FindGameObjectWithTag("Player").GetComponent<ManaSystem>();
        _manaSystem.OnManaChanged += UpdateManaUI;
	}

	private void OnDisable()
	{
		_manaSystem.OnManaChanged -= UpdateManaUI;
	}

	private void UpdateManaUI(float mana)
    {
        slider.value = mana;
    }

}
