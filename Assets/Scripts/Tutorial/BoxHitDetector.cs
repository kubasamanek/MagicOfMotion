using UnityEngine;

/// <summary>
/// Used for tutorial and performance testing to detect box hits. 
/// </summary>
public class BoxHitDetector : MonoBehaviour
{
	public delegate void BoxHitDelegate(GameObject box);
	public static event BoxHitDelegate OnBoxHit;

	public GameObject Rays;
	private Quaternion _rayInitialRotation;

	void Start()
	{
		if(Rays != null)
			_rayInitialRotation = Rays.transform.rotation;
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Fireball")) 
		{
			TutorialManager.Instance.CurrentCheckpoint?.TriggerAction();
			if (Rays != null)
				Destroy(Rays.gameObject);
		}
	}

	private void Update()
	{
		if(Rays != null)
		{
			Rays.transform.position = this.transform.position;
			Rays.transform.rotation = _rayInitialRotation;
		}
	}
}
