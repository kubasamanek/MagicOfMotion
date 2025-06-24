using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents the Fireball casted with the fireball spell.
/// Fireball explodes on collision and applies force to near objects.
/// </summary>
public class Fireball : MonoBehaviour
{
	[SerializeField] private float _explosionRadius = 6f;
	[SerializeField] private float _explosionForce = 4000f;

	[SerializeField] private GameObject _explosionPrefab;
	private GameObject _directCollision;

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Enemy"))
			if (collision.gameObject.TryGetComponent(out Enemy enemy))
				enemy.TakeDamage(50f);

		_directCollision = collision.gameObject;
		Explode();
		AudioManager.Instance.PlaySFXAtPosition("Explosion", this.gameObject.transform.position);
		Destroy(this.gameObject);
	}

	/// <summary>
	/// Explode effect of the fireball. 
	/// Apply force to all rigid bodies that are nearby.
	/// Spawn an explosion particle effect.
	/// </summary>
	void Explode()
	{
		var explosionInstance = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

		// Make sure the particle system gets destroyed when explosion effect ends.
		ParticleSystem ps = explosionInstance.GetComponent<ParticleSystem>();
		Destroy(explosionInstance, ps.main.duration + ps.main.startLifetime.constantMax);

		Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius);
		foreach (Collider hit in colliders)
		{
			Enemy enemyScript = hit.GetComponentInParent<Enemy>(); 
			if (enemyScript != null)
			{
				enemyScript.ToggleRagDoll();
				List<Rigidbody> rbs = hit.GetComponentsInChildren<Rigidbody>().ToList();
				foreach(var r in rbs)
				{
					var distance = Vector3.Distance(hit.transform.position, this.transform.position);
					var force = distance > 1 ? _explosionForce / distance / rbs.Count : _explosionForce / rbs.Count;
					r.AddExplosionForce(force, transform.position, _explosionRadius);
				}
			}
			Rigidbody rb = hit.GetComponent<Rigidbody>();
			if (rb != null)
			{
				if (hit.gameObject == _directCollision && hit.TryGetComponent<Destructible>(out var destructible))
				{
					destructible.Destroy();
					destructible.ForceToChunks(transform.position, _explosionForce);
				} else
				{
					rb.AddExplosionForce(_explosionForce, transform.position, _explosionRadius);
				}
			}
		}
	}
}