using UnityEngine;

/// <summary>
/// Represents a sound in the game.
/// </summary>
[System.Serializable]
public class Sound
{
    public string Name;
    public AudioClip Clip;

	[Range(0f, 1f)] public float Volume;
    public float Pitch;

    public bool Loop;

    [HideInInspector]
    public AudioSource Source;
}
