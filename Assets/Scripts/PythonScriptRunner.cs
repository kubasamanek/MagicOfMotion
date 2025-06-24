using System.Diagnostics;
using TMPro;
using UnityEngine;

public class PythonScriptRunner : MonoBehaviour
{
	private Process pythonProcess = null;
	private int ProcessId;
	[SerializeField] private TextMeshProUGUI _text;
	[SerializeField] private RgbConnectionManager _connectionManager;
	private string textOnButton = "";
	private bool waiting = false;

	private void Update()
	{
		textOnButton = _connectionManager.IsStreaming ? "End script" : "Start Motion Capture (20s)";

		if (_connectionManager.IsStreaming) waiting = false;

		if(!waiting)
			_text.text = textOnButton;
	}

	public void TogglePythonScript()
	{
		// If the process is running, stop it.
		if (pythonProcess != null && !pythonProcess.HasExited)
		{
			StopPythonScript();
			waiting = false;
		}
		else
		{
			StartPythonScript();
			waiting = true;
			_text.text = "Wait...";
		}
	}

	private void StartPythonScript()
	{
		if (waiting) return;

		// Path to the .exe file
		string exePath = Application.dataPath + "/rgb_capture.exe";

		ProcessStartInfo startInfo = new ProcessStartInfo(exePath)
		{
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		pythonProcess = new Process
		{
			StartInfo = startInfo,
			EnableRaisingEvents = true 
		};

		pythonProcess.Start();
		ProcessId = pythonProcess.Id;
		Invoke("WaitTenSeconds", 25.0f);
		UnityEngine.Debug.Log("Python script started. Process ID: " + pythonProcess.Id);
	}

	private void WaitTenSeconds()
	{
		waiting = false;
	}

	private void StopPythonScript()
	{
		if (pythonProcess != null && !pythonProcess.HasExited)
		{
			string cmdCommand = "/C taskkill /IM rgb_capture.exe /F";

			ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe")
			{
				Arguments = cmdCommand,
				UseShellExecute = true,
				CreateNoWindow = false,
				RedirectStandardOutput = false,
				RedirectStandardError = false
			};

			using (Process cmd = new Process { StartInfo = startInfo })
			{
				cmd.Start();
				cmd.WaitForExit(); 

				UnityEngine.Debug.Log("Attempted to stop Python script.");
			}
		}
	}

}
