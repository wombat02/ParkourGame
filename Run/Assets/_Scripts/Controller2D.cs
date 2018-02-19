using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour {

	const float skinWidth = .015f;
	float horizontalRaySpacing;
	float verticalRaySpacing;

	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;


	BoxCollider2D collider;
	RaycastOrigins raycastOrigins;

	void Start()
	{
		collider = GetComponent<BoxCollider2D> ();
	}

	void Update()
	{
		UpdateRaycastOrigins ();
		CalculateRaySpacing ();

		for (int i = 0; i < verticalRayCount; i++) {

			Debug.DrawRay (raycastOrigins.bottomLeft + Vector2.right * verticalRaySpacing * i, Vector2.down, Color.red); 
		}
	}

	void CalculateRaySpacing()
	{
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2f);

		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		verticalRaySpacing = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	void UpdateRaycastOrigins()
	{
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2f);

		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}

	struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}
