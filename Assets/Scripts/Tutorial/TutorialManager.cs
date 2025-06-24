using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages tutorial flow across different scenes and checkpoints, ensuring users are guided through a series of instructional steps.
/// </summary>
public class TutorialManager : MonoBehaviour
{
	public static TutorialManager Instance { get; private set; }
	public TutorialCheckpoint CurrentCheckpoint => _currentCheckpoint;

	private List<string> _sceneList = new List<string>();

	private Queue<string> _sceneQueue = new Queue<string> { };

	private TutorialCheckpoint _lastCheckpoint = null;
	private Queue<TutorialCheckpoint> _activeQueue = new Queue<TutorialCheckpoint>();
	private TutorialCheckpoint _currentCheckpoint;

	private List<float> _performanceTimes = new List<float>();

	[SerializeField] private GameObject _hintUI;
	public bool TutorialOver = false;

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

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		Debug.Log("OnSceneLoaded: " + scene.name);
		if(scene.name == "Menu")
		{
			_sceneQueue = new Queue<string>();
		}
		else if (scene.name == "MovementTutorial")
		{
			InitializeSceneQueue();
		} else if (scene.name.StartsWith("RgbPerformance"))
		{
			GameConfig.DeviceType = InputDeviceType.RGBCamera;
			GameManager.Instance.SetDeviceType(InputDeviceType.RGBCamera);
			GameObject spellManager = GameObject.Find("SpellManager");
			spellManager.GetComponent<FireballSpell>().SwitchController();
			spellManager.GetComponent<FireballSpell>().ChangeGesture();

			InitializePerformanceSceneQueue();
		} else if (scene.name.StartsWith("LeapPerformance"))
		{
			GameConfig.DeviceType = InputDeviceType.UltraLeap;
			GameManager.Instance.SetDeviceType(InputDeviceType.UltraLeap);
			GameObject spellManager = GameObject.Find("SpellManager");
			spellManager.GetComponent<FireballSpell>().SwitchController();
			spellManager.GetComponent<FireballSpell>().ChangeGesture();

			InitializePerformanceSceneQueue();
		}

		SetupCheckpointsForCurrentScene();

		_hintUI = FindFirstObjectByType<Canvas>().transform.Find("HintBox")?.gameObject;	
	}

	/// <summary>
	/// Initializes and configures the scene queue for performance test scenes.
	/// </summary>
	private void InitializePerformanceSceneQueue()
	{
		TutorialOver = false;
		_currentCheckpoint = null;
		_lastCheckpoint = null;
		_activeQueue = new Queue<TutorialCheckpoint>();
		_hintUI = null;

		_sceneList = new List<string>()
		{
			"LeapPerformanceFireball",
			"LeapPerformanceMovement",
			"RgbPerformanceAim",
			"RgbPerformanceFireball",
			"RgbPerformanceFireballNoSnapping",
			"RgbPerformanceMovement",
			"Menu"
		};

		if (_sceneQueue.Count == 0)
		{
			Debug.Log("Rewriting scene queue to PERFORMANCE");
			_sceneQueue = new Queue<string>(_sceneList);
		}
	}

	/// <summary>
	/// Initializes the scene queue based on the current scene and device type.
	/// </summary>
	private void InitializeSceneQueue()
	{
		TutorialOver = false;
		_currentCheckpoint = null;
		_lastCheckpoint = null;
		_activeQueue = new Queue<TutorialCheckpoint>();
		_hintUI = null;

		List<string> sceneListRgb = new List<string>()
		{
			"FireballTutorial",
			"ElectrifyTutorial",
			"ShieldTutorial",
			"Sandbox"
		};

		List<string> sceneListLeap = new List<string>()
		{
			"FireballTutorial",
			"ElectrifyTutorial",
			"TelekinesisTutorial",
			"ShieldTutorial",
			"Sandbox"
		};

		_sceneList = GameManager.Instance.DeviceType == InputDeviceType.UltraLeap ? sceneListLeap : sceneListRgb;

		// For editor scene load
		string currentSceneName = SceneManager.GetActiveScene().name;
		int currentSceneIndex = _sceneList.IndexOf(currentSceneName);
		Debug.Log("Rewriting scene queue to TUTORIAL");

		if (currentSceneIndex != -1)
		{
			_sceneQueue = new Queue<string>(_sceneList.Skip(currentSceneIndex + 1));
		}
		else
		{
			_sceneQueue = new Queue<string>(_sceneList);
		}

	}

	/// <summary>
	/// Sets up tutorial checkpoints for the current scene, organizing them by their IDs.
	/// </summary>
	private void SetupCheckpointsForCurrentScene()
	{
		GameObject[] checkpointObjects = GameObject.FindGameObjectsWithTag("Checkpoint");
		List<TutorialCheckpoint> checkpoints = new List<TutorialCheckpoint>();

		foreach (GameObject obj in checkpointObjects)
		{
			TutorialCheckpoint checkpoint = obj.GetComponent<TutorialCheckpoint>();
			if (checkpoint != null)
			{
				checkpoints.Add(checkpoint);
			}
		}

		checkpoints = checkpoints.OrderBy(c => c.ID).ToList();

		foreach (TutorialCheckpoint checkpoint in checkpoints)
		{
			checkpoint.gameObject.SetActive(false);
		}

		_activeQueue.Clear();
		foreach (TutorialCheckpoint checkpoint in checkpoints)
		{
			_activeQueue.Enqueue(checkpoint);
		}

		if (_activeQueue.Count > 0)
		{
			TriggerNextCheckpoint();
		}
	}

	/// <summary>
	/// Triggers the next checkpoint in the queue, handling different checkpoint types and conditions.
	/// </summary>
	public void TriggerNextCheckpoint()
	{
		if (_currentCheckpoint != null)
		{
			_currentCheckpoint.gameObject.SetActive(false);
			_lastCheckpoint = _currentCheckpoint;
		}

		if (_activeQueue.Count > 0)
		{
			_currentCheckpoint = _activeQueue.Dequeue();
			if(_currentCheckpoint.SkipForRGB && GameManager.Instance.DeviceType == InputDeviceType.RGBCamera)
			{
				TriggerNextCheckpoint();
				return;
			}
			_currentCheckpoint.gameObject.SetActive(true);
			_currentCheckpoint.StartTimer();

			if (_currentCheckpoint is TimerCheckpoint timerCheckpoint)
			{
				timerCheckpoint.StartActivationDelay();
			} 
		}
		else
		{
			Debug.Log("Tutorial stage completed.");
			LoadNextTutorialStage();
		}
	}

	public void StopCurrentCheckpointTimer()
	{

	}

	/// <summary>
	/// Checks if the current scene is a performance scene, used to modify behavior or configurations.
	/// </summary>
	private bool IsPerformanceScene()
	{
		string sceneName = SceneManager.GetActiveScene().name;
		return sceneName.StartsWith("RgbPerformance") || sceneName.StartsWith("LeapPerformance");
	}

	private void Update()
	{
		if(!TutorialOver && _hintUI != null && !IsPerformanceScene())
			HandleHints();

		if(_lastCheckpoint?.ActiveUI == false 
			&& _currentCheckpoint is TimerCheckpoint timerCheckpoint
			&& timerCheckpoint.IsPaused)
		{
			timerCheckpoint.ResumeTimer();
		}

		if (!TutorialOver 
			&& _lastCheckpoint?.ActiveUI == null
			&& _currentCheckpoint?.ActiveUI == null)
			HandleCheckpointRepeat();

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			AudioManager.Instance.StopAllSFX();
			AudioManager.Instance.MusicOn = false;
			SceneManager.LoadScene("Menu");
		}
	}

	/// <summary>
	/// Handles repeated activation of checkpoints based  with specific gesture.
	/// </summary>
	private void HandleCheckpointRepeat()
	{
		if(HandManager.Instance.ThumbsDownGesture(HandType.Left) 
			&& HandManager.Instance.ThumbsDownGesture(HandType.Right)
			&& (_lastCheckpoint.ActiveUI?.activeSelf ?? false) == false)
		{
			_lastCheckpoint.ActivateCheckpoint(repeated: true);
			if(_currentCheckpoint is TimerCheckpoint timerCheckpoint)
				timerCheckpoint.PauseTimer();
		}
	}

	/// <summary>
	/// Manages hint display based on the device type and current checkpoint.
	/// </summary>
	private void HandleHints()
	{
		string hint = GameManager.Instance.DeviceType == InputDeviceType.UltraLeap 
			? _currentCheckpoint.CheckpointData.HintLeap 
			: _currentCheckpoint.CheckpointData.HintRgb;

		if (hint != "")
		{
			_hintUI.SetActive(true);
			_hintUI.GetComponentInChildren<TextMeshProUGUI>().text = hint;
		}
		else
		{
			_hintUI.SetActive(false);
			_hintUI.GetComponentInChildren<TextMeshProUGUI>().text = "";
		}
	}

	/// <summary>
	/// Advances to the next stage in the tutorial sequence, loading the next scene.
	/// </summary>
	public void LoadNextTutorialStage()
	{
		if (_sceneQueue.Count == 0)
		{
			Debug.LogError("Scene queue is empty. Cannot load next tutorial stage.");
			return;
		}

		string nextSceneName = _sceneQueue.Dequeue();
		LoadingScreenManager.Instance.LoadScene(nextSceneName);
		if (nextSceneName == "Sandbox")
			TutorialOver = true;
	}

	public void SaveTime(float time)
	{
		_performanceTimes.Add(time);
	}

	public void FlushTimes()
	{
		if (_performanceTimes.Count == 0) return;
		string filePath = Path.Combine(Application.persistentDataPath, "floats.txt");

		// Use StreamWriter to write the list of floats to the file.
		using (StreamWriter writer = new StreamWriter(filePath))
		{
			int i = 1;
			foreach (float f in _performanceTimes)
			{
				writer.WriteLine(i++.ToString() + ": " + f.ToString("F2")); 
			}
		}

		_performanceTimes.Clear();

		Debug.Log("Saved floats to " + filePath);
	}


}
