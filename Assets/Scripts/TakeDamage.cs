using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Handles player taking damage. Applies vignette effect and sound effect to indicate that player has been hit.
/// </summary>
public class TakeDamage : MonoBehaviour
{
    public float Health = 100f;
    private PostProcessVolume _volume;
    private Vignette _vignette;
    public float Intensity;

	private void Start()
	{
		_volume = GetComponentInChildren<PostProcessVolume>();
        _volume.profile.TryGetSettings(out _vignette);

        _vignette.enabled.Override(false);
	}

    /// <summary>
    /// Handles vignette. Increasity is suddently increased, then decreased over time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator TakeDamageEffect()
    {
        Intensity = 0.275f;

        _vignette.enabled.Override(true);
        _vignette.intensity.Override(Intensity);

        Debug.Log(_vignette.intensity);

        yield return new WaitForSeconds(0.4f);

        while(Intensity > 0)
        {
            Intensity -= 0.01f;
            if (Intensity < 0) Intensity = 0;

            _vignette.intensity.Override(Intensity);

            yield return new WaitForSeconds(0.1f);
        }

        _vignette.enabled.Override(false);
        yield break;
    }

	public void DoDamage(float damage)
    {
		Health -= damage;
        StartCoroutine(TakeDamageEffect());
    }

}
