using UnityEngine;

/// <summary>
/// Manages the ambient sound within a designated zone in the game environment.
/// This class handles playing and stopping ambient audio based on the player's presence within a trigger zone.
/// </summary>
public class AmbienceZone : MonoBehaviour
{
	private AudioSource _audioSource;

	private void Start()
	{
		_audioSource = GetComponent<AudioSource>();
		if (_audioSource == null)
		{
			Debug.LogError("AmbienceZone: No AudioSource component found!");
		}
		else
		{
			_audioSource.loop = true;  // Ensure the sound loops
		}
	}

	/// <summary>
	/// Plays the ambient sound when the player enters the trigger zone, if the sound is not already playing.
	/// </summary>
	/// <param name="other">The collider that triggered this event.</param>
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			if (!_audioSource.isPlaying)
			{
				_audioSource.Play();
			}
		}
	}

	/// <summary>
	/// Stops the ambient sound when the player exits the trigger zone.
	/// </summary>
	/// <param name="other">The collider that triggered this event.</param>
	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			_audioSource.Stop();
		}
	}
}
