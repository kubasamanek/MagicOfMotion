using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages loading screens across different scenes, ensuring a smooth transition and providing visual feedback on loading progress.
/// </summary>
public class LoadingScreenManager : MonoBehaviour
{
	public static LoadingScreenManager Instance { get; private set; }

	[SerializeField] private GameObject loadingScreenPrefab;
	private GameObject loadingScreenInstance;
	private Slider progressBar;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	/// <summary>
	/// Initiates the loading of a new scene by name, and manages the display of the loading screen and progress bar.
	/// </summary>
	public void LoadScene(string sceneName)
	{
		if(AudioManager.Instance != null)
			AudioManager.Instance.StopAllSFX();
		
		if (loadingScreenInstance == null)
		{
			Canvas canvas = FindObjectOfType<Canvas>();
			loadingScreenInstance = Instantiate(loadingScreenPrefab, canvas.transform);
			progressBar = loadingScreenInstance.GetComponentInChildren<Slider>();
		}

		StartCoroutine(LoadAsync(sceneName));
	}

	/// <summary>
	/// Coroutine to load the scene asynchronously, updating the progress bar as it loads.
	/// </summary>
	/// <param name="sceneName">The name of the scene to load.</param>
	IEnumerator LoadAsync(string sceneName)
	{
		AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
		operation.allowSceneActivation = false;  

		loadingScreenInstance.SetActive(true);

		while (!operation.isDone)
		{
			float progress = Mathf.Clamp01(operation.progress / 0.9f);

			if (operation.progress >= 0.9f)
			{
				operation.allowSceneActivation = true;  
			}

			progressBar.value = progress;
			yield return null;
		}

		if(loadingScreenInstance != null)
		{
			loadingScreenInstance.SetActive(false);
		}
	}

}
