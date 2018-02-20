using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : RaycastController {

	[Header("Controller 2D")]
	public LayerMask collisionMask;

	public float maxClimbAngle = 75f;
	public float maxDescendAngle = 75f;

	public CollisionInfo collisions;

	[HideInInspector]
	public Vector2 playerInput;

	public override void Awake()
	{
		base.Awake ();
	}

	public override void Start()
	{
		base.Start ();

		collisions.faceDir = 1;
	}

	public void Move (Vector3 velocity, bool standingOnPlatform)
	{
		Move (velocity, Vector2.zero, standingOnPlatform);
	}

	public void Move (Vector3 velocity, Vector2 input, bool standingOnPlatform = false)
	{
		UpdateRaycastOrigins ();
		collisions.Reset ();
		collisions.velocityOld = velocity;
		playerInput = input;

		if (velocity.x != 0)
			collisions.faceDir = (int)(Mathf.Sign (velocity.x));

		if (velocity.y < 0)
			DescendSlope (ref velocity);
		
		HorizontalCollisions (ref velocity);

		if (velocity.y != 0)
			VerticalCollisions (ref velocity);

		if (standingOnPlatform)
			collisions.below = true;

		transform.Translate (velocity);
	}

	void HorizontalCollisions (ref Vector3 velocity)
	{
		float dirX = collisions.faceDir;
		float rayLength = Mathf.Abs (velocity.x) + skinWidth;

		if (Mathf.Abs(velocity.x) < skinWidth) {
			rayLength = 2f * skinWidth;
		}

		for (int i = 0; i < horizontalRayCount; i++) {

			Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);

			//Debug.DrawRay (rayOrigin, Vector2.right * dirX * rayLength, Color.red);

			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * dirX, rayLength, collisionMask);

			if (hit) {

				if (hit.collider.tag == "Through")
					continue;

				if (hit.distance == 0)
					continue;

				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);

				//climbing slope detection
				if (i == 0 && slopeAngle <= maxClimbAngle) {

					if (collisions.desendingSlope) {

						velocity = collisions.velocityOld;
						collisions.desendingSlope = false;
					}

					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleOld) {
						distanceToSlopeStart = hit.distance - skinWidth;
						velocity.x -= distanceToSlopeStart * dirX;
					}

					ClimbSlope (ref velocity, slopeAngle);

					velocity.x += distanceToSlopeStart * dirX;
				}

				if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) {

					velocity.x = (hit.distance - skinWidth) * dirX;
					rayLength = hit.distance;

					if (collisions.climbingSlope) {
						velocity.y = Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
					}

					collisions.left = dirX == -1;
					collisions.right = dirX == 1;
				}
			}
		}
	}

	void VerticalCollisions (ref Vector3 velocity)
	{
		float dirY = Mathf.Sign (velocity.y);
		float rayLength = Mathf.Abs (velocity.y) + skinWidth;

		for (int i = 0; i < verticalRayCount; i++) {

			Vector2 rayOrigin = (dirY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);

			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * dirY, rayLength, collisionMask);

			//Debug.DrawRay (rayOrigin, Vector2.up * dirY * rayLength, Color.red); 

			if (hit) {

				//ignores collisions with through objects
				if (hit.collider.tag == "Through") {
					if (dirY == 1 || hit.distance == 0)
						continue;

					if (collisions.fallingThroughPlatform)
						continue;

					if (playerInput.y == -1 || hit.distance == 0) {
						collisions.fallingThroughPlatform = true;
						Invoke ("ResetFallingThroughPlatform", .25f);
						continue;
					}
				}

				velocity.y = (hit.distance - skinWidth) * dirY;
				rayLength = hit.distance;

				if (collisions.climbingSlope) {

					velocity.x = velocity.y / Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign (velocity.x);
				}

				collisions.below = dirY == -1;
				collisions.above = dirY == 1;
			}
		}

		//Checking for new slope after movement
		if (collisions.climbingSlope) {
			float directionX = Mathf.Sign (velocity.x);
			rayLength = Mathf.Abs (velocity.x) + skinWidth;

			Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);

				//checking for hitting new slope
				if (slopeAngle != collisions.slopeAngle) {
					velocity.x = (hit.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}

	void ClimbSlope (ref Vector3 velocity, float slopeAngle)
	{
		float moveDistance = Mathf.Abs (velocity.x);
		float climbVelocityY =  Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (!(velocity.y > climbVelocityY)) {
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);

			collisions.slopeAngle = slopeAngle;
			collisions.below = true;
			collisions.climbingSlope = true;
		}
	}

	void DescendSlope (ref Vector3 velocity)
	{
		float directionX = Mathf.Sign (velocity.x);

		Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

		if (hit) {

			float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);

			if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) {

				if (Mathf.Sign (hit.normal.x) == directionX) {
					
					if (hit.distance - skinWidth <= Mathf.Tan (slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (velocity.x)) {
						float moveDistance = Mathf.Abs (velocity.x);
						float descendVelocityY =  Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

						velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * directionX;
						velocity.y -= descendVelocityY;

						collisions.slopeAngle = slopeAngle;
						collisions.below = true;
						collisions.desendingSlope = true;
					}
				}
			}
		}
	}

	void ResetFallingThroughPlatform ()
	{
		collisions.fallingThroughPlatform = false;
	}

	public struct CollisionInfo {
		public bool above, below;
		public bool left, right;
		public bool fallingThroughPlatform;

		public bool climbingSlope, desendingSlope;
		public float slopeAngle, slopeAngleOld;

		public int faceDir;

		public Vector2 velocityOld;

		public void Reset()
		{
			above = below = left = right = climbingSlope = desendingSlope = false;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}
