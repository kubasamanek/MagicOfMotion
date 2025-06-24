using UnityEngine;

/// <summary>
/// Implements aim controller base.
/// Leap aim controller handles aiming with left index finger.
/// </summary>
public class LeapAimController : AimControllerBase
{
	private new void Start()
    {
        base.Start();
    }

    private new void Update()
    {
        if (!HandManager.IsHandPresent(HandType.Left)) return;

        AimingCheck();

		base.Update();
    }

    /// <summary>
    /// Casts ray from a specified position in a specified direction.
    /// </summary>
    /// <param name="from">Position of tip of the index finger.</param>
    /// <param name="direction">Direction in which the index finger is pointing.</param>
    private void CastRay(Vector3 from, Vector3 direction)
    {
        // Ignore player's collider
		int layerMask = 1 << LayerMask.NameToLayer("Player"); 
		layerMask = ~layerMask;

		RaycastHit hit;
		if (Physics.Raycast(from, direction, out hit, Mathf.Infinity, layerMask))
        {
			Target = hit.point;
            AimedOn = hit.collider.gameObject;
        }
        else
        {
			AimedOn = null;
            Target = Vector3.zero;
        }
	}

    /// <summary>
    /// Check if currently aiming. 
    /// If so, draw the line showing the direction.
    /// </summary>
    private void AimingCheck()
    {
        if (IsHandPointingWithErrorToleration(HandType.Left))
        {
            Vector3 tipPosition = HandManager.GetTipPosition(HandType.Left, 1);
            Vector3 pointingDirection = HandManager.GetPointingDirection(HandType.Left);
			AimingDirection = pointingDirection.normalized;

			CastRay(tipPosition, pointingDirection);

            if (AimedOn != null)
                DrawAim(tipPosition);
            else
				LineRenderer.enabled = false;
			return;
        }

		LineRenderer.enabled = false;
    }

    /// <summary>
    /// Used to determine if the hand is pointing, but considering a common error, 
    /// when the middle finger sometimes extends with the index finger even when it shouldn't.
    /// </summary>
    /// <param name="handeness">Hand to check.</param>
    /// <returns>True if pointing, even with the possible error.</returns>
    private bool IsHandPointingWithErrorToleration(HandType handeness)
    {
        if (HandManager.IsHandPointing(handeness)) return true;

        int[] toleratedFingers = { 1, 2 };
        int[] notToleratedFingers = {0, 3, 4};

        foreach(var i in toleratedFingers)
            if (!HandManager.IsFingerExtended(handeness, i)) return false;

        foreach(var i in notToleratedFingers)
            if(HandManager.IsFingerExtended(handeness, i)) return false;

        return true;
    }


}
