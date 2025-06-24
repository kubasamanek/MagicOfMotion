using UnityEngine;

/// <summary>
/// Handles popup which appears when player stops following the path during MovementTutorial.
/// </summary>
public class FollowThePathPopup : MonoBehaviour
{
    [SerializeField] private GameObject _popup;

    void Start()
    {
        _popup.SetActive(false);    
    }

	private void OnTriggerEnter(Collider other)
	{
        if (other.gameObject.CompareTag("Player"))
        {
            _popup.SetActive(true);
        }
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			_popup.SetActive(false);
		}
	}

}
