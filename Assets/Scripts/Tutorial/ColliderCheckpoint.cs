using UnityEngine;

/// <summary>
/// Handles collider checkpoints which are triggered on player collision.
/// </summary>
public class ColliderCheckpoint : TutorialCheckpoint
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			ActivateCheckpoint(skip: this.CheckpointData == null);
		}
	}
}
