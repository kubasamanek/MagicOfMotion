using UnityEngine;

/// <summary>
/// Handles subtle camera shake when moving to simulate riding on a horse.
/// </summary>
public class CameraShake : MonoBehaviour
{
	[SerializeField] private GameObject _camera;

	private float _targetYPosition;
	private float _initialYPosition;

	private void Start()
	{
		_initialYPosition = _camera.transform.localPosition.y;
		_targetYPosition = _initialYPosition;
	}

	private void Update()
	{
		Vector3 newPos = new Vector3(_camera.transform.localPosition.x, _targetYPosition, _camera.transform.localPosition.z);
		_camera.transform.localPosition = Vector3.Lerp(_camera.transform.localPosition, newPos, Time.deltaTime * 10f);
	}

	// Called as event during the horse Run animation (at start)
	public void CameraShakeStart()
	{
		_targetYPosition = _initialYPosition + 0.2f;
	}

	// Called as event during the horse Run animation (at the middle)
	public void CameraShakeHalfPoint()
	{
		_targetYPosition = _initialYPosition;

	}
}