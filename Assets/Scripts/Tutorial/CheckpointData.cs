using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Encapsulates all data connected to a tutorial checkpoint.
/// </summary>
[CreateAssetMenu(fileName = "NewCheckpointData", menuName = "Tutorial/Checkpoint Data", order = 0)]
public class CheckpointData : ScriptableObject
{
	public string Name; 

	public VideoClip LeapVideo;
	[TextArea(1, 10)] public string LeapText;

	public VideoClip RgbVideo;
	[TextArea(1, 10)] public string RgbText;

	public RenderTexture RenderTexture;
	public GameObject UI;
	
	public string HintLeap;
	public string HintRgb;
}
