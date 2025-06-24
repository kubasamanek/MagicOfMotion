using DigitalRuby.LightningBolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ElectrifyChain is attached to enemies and is used as support to the ElectrifySpell.
/// It handles the effect transition onto nearby enemies. 
/// </summary>
public class ElectrifyChain : MonoBehaviour
{
	public bool IsElectrified {  get; private set; }

	private Enemy _enemyScript;
	private bool _electrifiedPassedOn = false;
	private float _electrifyReach = 6f;

	[SerializeField] private bool _isForTutorial; 
	[SerializeField] private GameObject _smokePrefab;
	[SerializeField] private GameObject _lightningPrefab;

	private GameObject _activeSmoke;

	private List<GameObject> _activeLightnings = new List<GameObject>();

	void Start()
    {
		IsElectrified = false;
		_enemyScript = GetComponent<Enemy>();	
	}

	private void Update()
	{
		if (IsElectrified && !_enemyScript.AimedOn && !_electrifiedPassedOn)
		{
			StartCoroutine(End());
		}
	}

	public IEnumerator End()
	{
		yield return new WaitForSeconds(2f);
		Unelectrify();
		UnelectrifyOthers();
	}

	/// <summary>
	/// Trigger for electrify tutorial
	/// </summary>
	private void TutorialTriggerCheck()
	{
		if (_isForTutorial)
			TutorialManager.Instance.CurrentCheckpoint?.TriggerAction();
	}

	/// <summary>
	/// Electrify this enemy, pass it on the other close enemies.
	/// </summary>
	/// <param name="passedOn">True if this comes from another enemy, false if direct cast hit</param>
	public void Electrify(bool passedOn)
	{
		if (IsElectrified) return;
		if (!passedOn)
		{
			TutorialTriggerCheck();
			ElectrifyOthers();
		}
		_electrifiedPassedOn = passedOn;
		IsElectrified = true;
		_enemyScript.Animator.SetBool("Electrified", true);
		AudioManager.Instance.PlaySFX("GoblinScream");
	}

	/// <summary>
	/// Unelectrify this enemy
	/// </summary>
	public void Unelectrify()
	{
		foreach(var l in _activeLightnings)
			Destroy(l.gameObject);
		_activeLightnings.Clear();
		if(_activeSmoke == null)
			_activeSmoke = Instantiate(_smokePrefab, this.transform.position, Quaternion.identity);
		StartCoroutine(DestroySmokeWithDelay());
		_enemyScript.ToggleRagDoll();
		_electrifiedPassedOn = false;
		IsElectrified = false;
		AudioManager.Instance.StopSFX("GoblinScream");
		_enemyScript.Animator.SetBool("Electrified", false);
	}

	/// <summary>
	/// Electrify enemies that are close to firstly electrified target
	/// </summary>
	private void ElectrifyOthers()
	{
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, _electrifyReach);

		foreach (var hitCollider in hitColliders)
		{
			if (hitCollider.CompareTag("Enemy"))
			{
				var enemy = hitCollider.GetComponent<Enemy>(); 
				if (enemy != null)
				{
					var newLightning = Instantiate(_lightningPrefab, Vector3.zero, Quaternion.identity);

					var lightningScript = newLightning.GetComponent<LightningBoltScript>();
					lightningScript.StartPosition = this.transform.position + Vector3.up;
					lightningScript.EndPosition = enemy.transform.position + Vector3.up;

					var linerenderer = newLightning.GetComponent<LineRenderer>();
					linerenderer.startWidth = 0.6f;
					linerenderer.endWidth = 0.6f;

					_activeLightnings.Add(newLightning);

					enemy.GetComponent<ElectrifyChain>().Electrify(true); 
				}
			}
		}
	}

	/// <summary>
	/// Pass the unelectrify effect to other enemies
	/// </summary>
	private void UnelectrifyOthers()
	{
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, _electrifyReach);

		foreach (var hitCollider in hitColliders)
		{
			if (hitCollider.CompareTag("Enemy"))
			{
				var enemy = hitCollider.GetComponent<Enemy>(); 
				if (enemy != null)
				{
					enemy.GetComponent<ElectrifyChain>().Unelectrify(); 
				}
			}
		}
	}

	private IEnumerator DestroySmokeWithDelay()
	{
		yield return new WaitForSeconds(3f);
		if(_activeSmoke != null)
		{
			Destroy(_activeSmoke.gameObject);
			_activeSmoke = null; 
		}
	}

}
