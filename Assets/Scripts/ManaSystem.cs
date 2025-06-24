using System;
using UnityEngine;

/// <summary>
/// Handles mana system in the game.
/// </summary>
public class ManaSystem : MonoBehaviour
{
	public float maxMana = 100;
	public float currentMana;
	public event Action<float> OnManaChanged = delegate { };

	private void Start()
	{
		currentMana = maxMana;
	}

	private void Update()
	{
		RegenerateMana(0.05f);
	}

	public void UseMana(float amount)
	{
		currentMana -= amount;
		currentMana = Mathf.Clamp(currentMana, 0, maxMana);
		OnManaChanged(currentMana); 
	}

	public bool EnoughMana(float amount)
	{
		return currentMana >= amount;
	}

	public void RegenerateMana(float amount)
	{
		currentMana += amount;
		currentMana = Mathf.Clamp(currentMana, 0, maxMana);
		OnManaChanged(currentMana);
	}
}