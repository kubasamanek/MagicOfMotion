using UnityEngine;

/// <summary>
/// Implements aim controller base.
/// Rgb aim controller handles aiming with left palm normal vector.
/// </summary>
public class RgbAimController : AimControllerBase
{
	private new void Start()
	{
		base.Start();
	}

	private new void Update()
	{
		if (!HandManager.IsHandPresent(HandType.Left)) return;

		// If hand not open, aiming is off
		if (!HandManager.IsHandOpen(HandType.Left)) {
			AimedOn = null;
			Target = Vector3.zero;
			LineRenderer.enabled = false;
			return;
		}

		HandleAiming();

		base.Update();
	}

	/// <summary>
	/// Handles RGB Aiming - player aims with left palm normal and aim snaps to the closest target.
	/// </summary>
	private void HandleAiming()
	{
		Vector3 palmPosition = HandManager.GetPalmPosition(HandType.Left);
		Vector3 palmNormal = HandManager.GetPalmNormal(HandType.Left);

		var hits = SphereCastForEnemies(palmPosition, palmNormal, 3f, 50f);

		var closestEnemy = GetClosestEnemy(hits);

		// If an enemy was marked, set him as target
		if (closestEnemy != null)
		{
			AimedOn = closestEnemy;
			Vector3 pos = closestEnemy.transform.position;
			bool isRagdoll = AimedOn.TryGetComponent<Enemy>(out var _);
			Target = new Vector3(pos.x, pos.y + (isRagdoll ? 1f : 0f), pos.z);
			AimingDirection = (pos - palmPosition).normalized;
			DrawAim(palmPosition);
		}
		// If no enemy was marked, aim in the palm normal direction
		else
		{
			DrawAim(palmPosition, palmNormal);
			AimingDirection = palmNormal.normalized;
			AimedOn = null;
			Target = CastRayAndGetHitPosition(palmPosition, palmNormal);
		}

	}

	/// <summary>
	/// Only take the closes enemy to the raycast from all the hits.
	/// </summary>
	/// <param name="hits">List of raycast hits.</param>
	/// <returns>The closest enemy or null if none was near</returns>
	private GameObject GetClosestEnemy(RaycastHit[] hits)
	{
		Vector3 palmPosition = HandManager.GetPalmPosition(HandType.Left);
		Vector3 palmNormal = HandManager.GetPalmNormal(HandType.Left);
		GameObject closestEnemy = null;
		float closestAngle = Mathf.Infinity;
		float closestDistance = Mathf.Infinity;

		foreach (var hit in hits)
		{
			if (hit.transform.CompareTag("Enemy"))
			{
				if (hit.transform.gameObject.TryGetComponent<Enemy>(out var enemy))
					if (enemy.IsRagDoll) continue;
				Vector3 enemyDirection = (hit.transform.position - palmPosition).normalized;
				float angle = Vector3.Angle(palmNormal, enemyDirection);
				float distance = (hit.transform.position - palmPosition).magnitude;

				if (angle < closestAngle || (Mathf.Approximately(angle, closestAngle) && distance < closestDistance))
				{
					closestAngle = angle;
					closestDistance = distance;
					closestEnemy = hit.collider.gameObject;
				}
			}
		}

		return closestEnemy;
	}

	/// <summary>
	/// Casts a sphere raycast from a source in a direction and returns all hits.
	/// </summary>
	/// <param name="from">Source of the raycast</param>
	/// <param name="direction">Direction of the raycast</param>
	/// <param name="radius">Radius of the raycast sphere</param>
	/// <param name="maxDistance">Max distance for the raycast</param>
	/// <returns>All raycast hits</returns>
	private RaycastHit[] SphereCastForEnemies(Vector3 from, Vector3 direction, float radius, float maxDistance)
	{
		int layerMask = 1 << LayerMask.NameToLayer("Player");
		layerMask = ~layerMask; 

		RaycastHit[] hits = Physics.SphereCastAll(from, radius, direction, maxDistance, layerMask);
		return hits; 
	}

	/// <summary>
	/// Casts a ray from a source in a direction.
	/// </summary>
	/// <param name="from">Source of the raycast</param>
	/// <param name="direction">Direction of the raycast</param>
	/// <returns>Returns position of hit</returns>
	private Vector3 CastRayAndGetHitPosition(Vector3 from, Vector3 direction)
	{
		// Ignore player's collider
		int layerMask = 1 << LayerMask.NameToLayer("Player");
		layerMask = ~layerMask;

		RaycastHit hit;
		if (Physics.Raycast(from, direction, out hit, Mathf.Infinity, layerMask))
		{
			return hit.point;
		}
		return Vector3.zero;
	}

}