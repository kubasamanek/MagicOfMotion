using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A class that handles destructible objects which can be destroyed and replaced by a destroyed version when an impact threshold is exceeded.
/// </summary>
public class Destructible : MonoBehaviour
{
    [SerializeField] private GameObject DestroyedVersion;
    private GameObject _destroyedVersionInstance;
	private float _forceThreshold = 70f;

	/// <summary>
	/// Handles the collision event by checking the impact force and destroying the object if the force exceeds the threshold.
	/// </summary>
	/// <param name="collision">Information about the collision, including force.</param>
	private void OnCollisionEnter(Collision collision)
	{

		float impactForce = collision.relativeVelocity.magnitude;

		if (impactForce >= _forceThreshold)
		{
			Destroy();

			if (_destroyedVersionInstance != null)
			{
				ForceToChunks(collision.contacts[0].point, impactForce * 5);
			}
		}
	}

	/// <summary>
	/// Called when object is destroyed. This object is replaced by it's destroyed version.
	/// </summary>
	public void Destroy()
    {
        DestroyedVersion.transform.localScale = this.transform.localScale;
		_destroyedVersionInstance = Instantiate(DestroyedVersion, transform.position, transform.rotation);
        Destroy(gameObject);
    }

	/// <summary>
	/// Applies force to chunks of the destroyed version.
	/// </summary>
	/// <param name="forceSource">Force source position.</param>
	/// <param name="force">Force amount.</param>
    public void ForceToChunks(Vector3 forceSource, float force)
    {
		List<Rigidbody> rbs = _destroyedVersionInstance.GetComponentsInChildren<Rigidbody>().ToList();
        
        foreach(var rb in rbs)
        {
			rb.AddExplosionForce(force / 10, forceSource, 1);
		}

	}

}
