using UnityEngine;

/// <summary>
/// Attached to objects which can be manipulated using telekinesis.
/// </summary>
[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour
{
	private void Awake()
	{
		GetComponent<Outline>().enabled = false;    
	}

    /// <summary>
    /// Called when telekinesis is called on this object.
    /// </summary>
    /// <returns>Rigidbody of this object. If the object was an npc, return hips of the npc.</returns>
	public Rigidbody Grab()
    {
        if(TryGetComponent<Npc>(out var npc))
        {
            npc.ToggleRagDoll();
            return npc.transform.Find("mixamorig:Hips").GetComponent<Rigidbody>();
        }

        return this.GetComponent<Rigidbody>();
    }
}
