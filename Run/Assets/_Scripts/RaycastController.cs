using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {

	[Header ("Raycast Controller")]
	public float maxRaySpacing = .1f;

	public int horizontalRayCount = 0;
	public int verticalRayCount = 0;

	public const float skinWidth = .015f;
	[HideInInspector]
	public float horizontalRaySpacing;
	[HideInInspector]
	public float verticalRaySpacing;

	[HideInInspector]
	public BoxCollider2D collider;
	[HideInInspector]
	public RaycastOrigins raycastOrigins;

	public virtual void Awake()
	{
		collider = GetComponent<BoxCollider2D> ();
	}

	public virtual void Start()
	{
		CalculateRaySpacing ();
	}

	public void CalculateRaySpacing()
	{
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2f);

		horizontalRayCount = Mathf.CeilToInt (bounds.size.y / maxRaySpacing);
		verticalRayCount = Mathf.CeilToInt (bounds.size.x / maxRaySpacing);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	public void UpdateRaycastOrigins()
	{
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2f);

		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}

	public struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}
