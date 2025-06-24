using Leap.Unity;
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages all audio in the game.
/// Handles music and SFX sounds.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

	public bool MusicOn = false;

    public Sound[] Music, SFX;
    public AudioSource MusicSource;

	private void Start()
	{
		if (MusicOn)
			PlayRandomMusic();
	}

	private void Update()
	{
		// Check if music is playing or not
		if (!MusicSource.isPlaying)
			PlayRandomMusic();
	}

	public void StopMusic()
	{
		Music.ForEach(t => t.Source.Stop());
	}

	/// <summary>
	/// Singleton pattern
	/// </summary>
	private void Awake()
	{
		if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

		CreateSourcesForSFX();
	}

	/// <summary>
	/// Creates seperate source for each sound in the game, 
	/// so they can play parallel to each other.
	/// </summary>
	private void CreateSourcesForSFX()
	{
		foreach (Sound s in SFX)
		{
			s.Source = gameObject.AddComponent<AudioSource>();
			s.Source.clip = s.Clip;

			s.Source.volume = s.Volume;
			s.Source.pitch = s.Pitch;
			s.Source.loop = s.Loop;
		}
	}

	/// <summary>
	/// Start playing Music by name.
	/// </summary>
	/// <param name="name">Name of the music file to play.</param>
	public void PlayMusic(string name)
    {
        Sound s = Array.Find(Music, x => x.Name == name);

        if (s == null) Debug.LogError("Music Not Found!");
        else
        {
            MusicSource.clip = s.Clip;
			MusicSource.volume = s.Volume;
            MusicSource.Play();
        }
    }

	/// <summary>
	/// Play SFX sound by name.
	/// </summary>
	/// <param name="name">Name of the SFX sound to play.</param>
    public void PlaySFX(string name)
    {
		Sound s = Array.Find(SFX, x => x.Name == name);
		if (s == null) Debug.LogError("Sound Not Found!");
		else
		{
            s.Source.Play();
		}
	}

	/// <summary>
	/// Plays sound effect on a specified location.
	/// </summary>
	/// <param name="name">Name of the sound effect</param>
	/// <param name="position">Where to play the sound effect</param>
	public void PlaySFXAtPosition(string name, Vector3 position)
	{
		Sound s = Array.Find(SFX, x => x.Name == name);
		if (s == null)
		{
			Debug.LogError("Sound Not Found!");
			return;
		}

		GameObject tempGO = new GameObject("TempAudioSource");
		tempGO.transform.position = position; 
		AudioSource audioSource = tempGO.AddComponent<AudioSource>();

		// Copy attributes
		audioSource.clip = s.Clip;
		audioSource.volume = s.Volume;
		audioSource.pitch = s.Pitch;
		audioSource.loop = s.Loop;
		audioSource.spatialBlend = 1f; 

		audioSource.Play();

		Destroy(tempGO, s.Clip.length / audioSource.pitch);
	}

	/// <summary>
	/// Stop SFX sound by name if currently playing.
	/// </summary>
	/// <param name="name">Name of the SFX sound to stop.</param>
    public void StopSFX(string name)
    {
		Sound s = Array.Find(SFX, x => x.Name == name);
		if (s == null) Debug.LogError("Sound Not Found!");
		else
		{
			s.Source.Stop();
		}
	}
	
	/// <summary>
	/// Stops all currently playing sound effects.
	/// </summary>
	public void StopAllSFX()
	{
		foreach(var s in SFX)
		{
			s.Source.Stop();
		}
	}

	/// <summary>
	/// Stop SFX sound with fade out effect.
	/// </summary>
	/// <param name="name">Name of the SFX sound to stop.</param>
	/// <param name="fadeTime">Duration of the fade out effect.</param>
	/// <returns></returns>
	public IEnumerator FadeOutSFX(string name, float fadeTime)
	{
		Sound s = Array.Find(SFX, sound => sound.Name == name);
		if (s == null || s.Source == null || !s.Source.isPlaying)
		{
			if (s.Source.isPlaying)
				Debug.LogError("Sound Not Found! > " + s.Name);
			yield break; 
		}

		float startVolume = s.Source.volume;

		// Gradually decrease the volume
		while (s.Source.volume > 0)
		{
			s.Source.volume -= startVolume * Time.deltaTime / fadeTime;
			yield return null; 
		}

		s.Source.Stop();
		s.Source.volume = startVolume; 
	}

	/// <summary>
	/// Get reference to AudioSource of a specified SFX sound.
	/// </summary>
	/// <param name="name">Name of the SFX sound to get a source reference of.</param>
	/// <returns></returns>
	public AudioSource GetSFXSource(string name)
    {
		Sound s = Array.Find(SFX, x => x.Name == name);
		if (s == null) Debug.LogError("Sound Not Found!");
		else
		{
			return s.Source;
		}

        return null;
	}

	/// <summary>
	/// Mute/Turn on music.
	/// </summary>
	public void ToggleMusic()
    {
        MusicSource.mute = !MusicSource.mute;
    }

	/// <summary>
	/// Set music volume.
	/// </summary>
	/// <param name="volume">Volume from 0 to 1.</param>
    public void SetMusicVolume(float volume)
    {
        MusicSource.volume = volume;
    }

	/// <summary>
	/// Plays random music track from music list.
	/// </summary>
	private void PlayRandomMusic()
	{
		if (Music.Length == 0)
		{
			Debug.LogError("No music tracks available!");
			return;
		}

		int randomIndex = UnityEngine.Random.Range(0, Music.Length);
		Sound s = Music[randomIndex];
		if (s == null)
		{
			Debug.LogError("Music Not Found!");
		}
		else
		{
			MusicSource.clip = s.Clip;
			MusicSource.volume = s.Volume;
			MusicSource.Play();
		}
	}

	/// <summary>
	/// Adjusts the volume of the music based on a UI slider's value.
	/// </summary>
	/// <param name="volume">New volume level (0.0 to 1.0).</param>
	public void AdjustMusicVolume(float volume)
	{
		if (MusicSource != null)
		{
			MusicSource.volume = volume;
		}
		else
		{
			Debug.LogError("Music source is not initialized!");
		}
	}
}
