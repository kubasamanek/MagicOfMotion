using UnityEngine;
public class Arrow : MonoBehaviour
{
	private float _speed = 10f;
	private float _timeToLive = 5f; 

	private Outline _outline;
	
	void Start()
	{
		_outline = GetComponent<Outline>();
		Destroy(gameObject, _timeToLive); 
	}

	void Update()
	{
		if(this.GetComponent<Rigidbody>().isKinematic == false)
			transform.Translate(Vector3.back * _speed * Time.deltaTime);
	}

	/// <summary>
	/// Handles collider entry differently, based on hit type.
	/// </summary>
	/// <param name="other">Collider that was hit.</param>
	private void OnTriggerEnter(Collider other)
	{
		// Check if player hit
		if (other.CompareTag("Player"))
		{
			other.GetComponent<TakeDamage>().DoDamage(10f);
			AudioManager.Instance.PlaySFX("ArrowHitPlayer");
			Destroy(gameObject);
		}

		if (other.CompareTag("Shield")) 
		{
			ShieldHit(other);

		} 

		else if (other.TryGetComponent<Enemy>(out Enemy enemy))
		{
			EnemyHit(enemy);
		}

		else if (other.TryGetComponent<Grabbable>(out Grabbable hit))
		{
			GrabbableHit(hit);
		}


	}

	/// <summary>
	/// Called when arrow hit the shield. Arrow is attached as child of the shield to simulate the stick effect.
	/// </summary>
	/// <param name="hit">Shield collider</param>
	private void ShieldHit(Collider hit)
	{
		TutorialManager.Instance.CurrentCheckpoint?.TriggerAction();

		if (GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().isKinematic = true;
		}

		transform.SetParent(hit.transform, true);
		AudioManager.Instance.PlaySFX("ArrowHitShield");
		this.GetComponent<BoxCollider>().enabled = false;
		_outline.enabled = false;
	}

	/// <summary>
	/// Called when arrow hits another enemy. It damages the enemy and destroys the arrow.
	/// </summary>
	/// <param name="enemy">Hit enemy reference.</param>
	private void EnemyHit(Enemy enemy)
	{
		enemy.TakeDamage(20f);
		Destroy(gameObject);
	}

	/// <summary>
	/// Called when arrow hit a grabbable object. It gets stuck to object as it gets to shield, so player can defend himself with other objects.
	/// </summary>
	/// <param name="hit">Grabbable reference.</param>
	private void GrabbableHit(Grabbable hit)
	{
		TutorialManager.Instance.CurrentCheckpoint?.TriggerAction();

		if (GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().isKinematic = true; // Stop physics calculations for the arrow
		}
		transform.SetParent(hit.transform, true);
		AudioManager.Instance.PlaySFX("ArrowHitShield");
		this.GetComponent<BoxCollider>().enabled = false;
		_outline.enabled = false;
	}
}
