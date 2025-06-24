using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dynamic shield prefab initialized, when shield spell is casted. 
/// The mesh of the shield dynamically changes to match the wrist and elbow positions of both hands.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DynamicShield : MonoBehaviour
{
	public Vector3[] points = new Vector3[4]; 
	private BoxCollider _boxCollider;

	private const string MESH_NAME = "Dynamic Shield";
	private Mesh _mesh;

	private void Awake()
	{
		GetComponent<MeshFilter>().mesh = _mesh = new Mesh();
		_boxCollider = GetComponentInChildren<BoxCollider>();
		_mesh.name = MESH_NAME;
	}

	/// <summary>
	/// Real-time update of the shield mesh.
	/// </summary>
	/// <param name="newPoints">New points to form the shield</param>
	public void UpdateShieldMesh(List<Vector3> newPoints)
	{
		GetComponent<MeshFilter>().mesh = _mesh = new Mesh();
		_mesh.name = MESH_NAME;

		_mesh.Clear();

		Vector3[] localPoints = new Vector3[newPoints.Count];
		for (int i = 0; i < newPoints.Count; i++)
			localPoints[i] = transform.InverseTransformPoint(newPoints[i]);

		_mesh.vertices = localPoints;

		// Triangles defined by their indexes
		int[] triangles = { 0, 1, 2, 1, 3, 2 };
		_mesh.triangles = triangles;

		// Important for correct lightning
		_mesh.RecalculateNormals(); 

		UpdateShieldCollider(newPoints);
	}

	/// <summary>
	/// Sets the boxcollider to wrap around the shield.
	/// </summary>
	/// <param name="points">Shield points</param>
	private void UpdateShieldCollider(List<Vector3> points)
	{
		if (points.Count < 4)
		{
			Debug.LogError("Shield requires at least 4 points.");
			return;
		}

		// Calculate the center of the shield in world space.
		Vector3 worldCenter = (points[0] + points[1] + points[2] + points[3]) / 4;

		_boxCollider.transform.position = worldCenter;

		// Calculate the size of the collider based on the points.
		Vector3 widthVector = points[2] - points[0]; 
		Vector3 heightVector = points[1] - points[0]; 
		float width = widthVector.magnitude;
		float height = heightVector.magnitude;
		_boxCollider.size = new Vector3(width, height, 0.1f); 

		// Setting the rotation of the collider to align with the shield's orientation in world space.
		Vector3 forwardDirection = Vector3.Cross(widthVector, heightVector).normalized;
		_boxCollider.transform.rotation = Quaternion.LookRotation(forwardDirection, Vector3.up);
	}

}