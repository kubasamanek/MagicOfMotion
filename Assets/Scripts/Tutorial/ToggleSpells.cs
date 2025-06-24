using UnityEngine;

public class ToggleSpells : MonoBehaviour
{
    public bool Fireball = true;
    public bool Electrify = true;
    public bool Telekinesis = true;
    public bool Shield = true;

	private FireballSpell FireballSpell;
	private ElectricitySpell ElectrifySpell;
	private ForceSpell TelekinesisSpell;
	private ShieldSpell ShieldSpell;

	private void Start()
	{
		FireballSpell = GetComponent<FireballSpell>();
		ElectrifySpell = GetComponent<ElectricitySpell>();
		ShieldSpell = GetComponent<ShieldSpell>();
		TelekinesisSpell = GetComponent<ForceSpell>();
	}

	private void Update()
	{
		FireballSpell.enabled = Fireball;
		ElectrifySpell.enabled = Electrify;
		ShieldSpell.enabled = Shield;
		TelekinesisSpell.enabled = Telekinesis;
	}
}
