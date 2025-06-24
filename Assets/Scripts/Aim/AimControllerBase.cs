using UnityEngine;

/// <summary>
/// Base class for aim contolling
/// This class holds current target and aiming direction + handles aiming visualization
/// </summary>
public abstract class AimControllerBase : MonoBehaviour
{
	public Vector3 Target { get; protected set; }
	public GameObject AimedOn { get; protected set; }
	public Vector3 AimingDirection { get; protected set; }

	protected HandManager HandManager;
	protected LineRenderer LineRenderer;

	private GameObject _lastAimedOn;

	protected void Start()
	{
		HandManager = HandManager.Instance;
		LineRenderer = GetComponent<LineRenderer>();
	}

	protected void Update()
	{
		if (_lastAimedOn && _lastAimedOn.TryGetComponent(out Outline outline))
		{
			outline.enabled = false;
		}

		OutlineIfInteractable();

		_lastAimedOn = AimedOn;
	}

	public void ToggleRenderer(bool state)
	{
		LineRenderer.enabled = state;
	}

	/// <summary>
	/// Outlines enemy if the aim is currently targeting him.
	/// </summary>
	private void OutlineIfInteractable()
	{
		if (AimedOn == null) return;

		if(AimedOn.TryGetComponent(out Outline outline))
		{
			outline.enabled = true;
		}

	}

	/// <summary>
	/// Uses line renderer to draw line in the aiming direction from the starting position.
	/// </summary>
	/// <param name="from">From where - start of the line.</param>
	protected void DrawAim(Vector3 from)
	{
		LineRenderer.enabled = true;
		LineRenderer.startWidth = LineRenderer.endWidth = 0.01f;

		// Set positions for the first line
		LineRenderer.positionCount = 2;
		LineRenderer.SetPosition(0, from);
		LineRenderer.SetPosition(1, Target);
	}


	/// <summary>
	/// Uses line renderer to draw line in the aiming direction from the starting position.
	/// </summary>
	/// <param name="from">From where - start of the line.</param>
	protected void DrawAim(Vector3 from, Vector3 direction)
	{
		LineRenderer.enabled = true;
		LineRenderer.startWidth = LineRenderer.endWidth = 0.01f;

		// Set positions for the first line
		LineRenderer.positionCount = 2;
		LineRenderer.SetPosition(0, from);
		LineRenderer.SetPosition(1, from + direction.normalized * 5f);
	}

}