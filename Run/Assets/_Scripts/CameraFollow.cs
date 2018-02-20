using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Controller2D target;
	public Vector2 focusAreaSize;

	public float verticalOffset;
	public float lookAheadDistanceX;
	public float smoothTimeX;
	public float smoothTimeY;

	float currentLookAheadX;
	float targetLookAheadX;
	float lookAheadDirX;
	float smoothLookVelocityX;
	float smoothLookVelocityY;
	float screenHeight;

	bool lookAheadStopped;

	FocusArea focusArea;

	void Start()
	{
		if (target != null)
			focusArea = new FocusArea (target.collider.bounds, focusAreaSize);

		screenHeight = Camera.main.orthographicSize / Camera.main.aspect;
	}

	void LateUpdate()
	{
		focusArea.Update (target.collider.bounds);

		Vector2 focusPosition = focusArea.centre + Vector2.up * verticalOffset;

		if (focusArea.velocity.x != 0) {
			lookAheadDirX = Mathf.Sign (focusArea.velocity.x);

			if (Mathf.Sign (target.playerInput.x) == Mathf.Sign (focusArea.velocity.x) && target.playerInput.x != 0) {
				lookAheadStopped = false;

				targetLookAheadX = lookAheadDirX * lookAheadDistanceX;
				currentLookAheadX = Mathf.SmoothDamp (currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, smoothTimeX);
			} else {
				
				if (!lookAheadStopped) {
					lookAheadStopped = true;
					targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDistanceX - currentLookAheadX) / 4f;
				}
			}
		}

		focusPosition += Vector2.right * currentLookAheadX;
		focusPosition.y = Mathf.SmoothDamp (transform.position.y, focusPosition.y, ref smoothLookVelocityY, smoothTimeY);
		focusPosition.y = Mathf.Clamp (focusPosition.y, focusArea.centre.y + verticalOffset - screenHeight/2, focusArea.centre.y - verticalOffset + screenHeight/2);

		transform.position = (Vector3)focusPosition + Vector3.forward * -10;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = new Color (1, 0, 0, .5f);
		Gizmos.DrawCube (focusArea.centre, focusAreaSize);
	}

	struct FocusArea {
		
		public Vector2 velocity;
		public Vector2 centre;
		float left, right;
		float top, bottom;

		public FocusArea (Bounds targetBounds, Vector2 size)
		{
			left = targetBounds.center.x - size.x / 2f;
			right = targetBounds.center.x + size.x / 2f;
			bottom = targetBounds.min.y;
			top = targetBounds.min.y + size.y;

			centre = new Vector2((left + right)/2f, (top + bottom)/2f);
			velocity = Vector2.zero;
		}

		public void Update (Bounds targetBounds)
		{
			float shiftX = 0;

			if (targetBounds.min.x < left) {
				shiftX = targetBounds.min.x - left;
			} else if (targetBounds.max.x > right) {
				shiftX = targetBounds.max.x - right;
			}

			left += shiftX;
			right += shiftX;

			float shiftY = 0;

			if (targetBounds.min.y < bottom) {
				shiftY = targetBounds.min.y - bottom;
			} else if (targetBounds.max.y > top) {
				shiftY = targetBounds.max.y - top;
			}

			top += shiftY;
			bottom += shiftY;

			velocity = new Vector2 (shiftX, shiftY);
			centre = new Vector2((left + right)/2f, (top + bottom)/2f);
		}
	}

}
