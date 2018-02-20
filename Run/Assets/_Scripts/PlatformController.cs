using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController {

	[Header ("Platform Controller")]
	public LayerMask passengerMask;

	public bool cyclic;

	public float speed;
	public float waitTime;
	[Range(0,2)]
	public float easeAmmount;
	float percentBetweenWaypoints;
	float nextMoveTime;

	int fromWaypointIndex;

	public Vector3[] waypoints;
	Vector3[] globalWaypoints;

	List<PassengerMovement> passengerMovement;
	Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

	public override void Start()
	{
		base.Start ();

		globalWaypoints = new Vector3[waypoints.Length];

		for (int i = 0; i < globalWaypoints.Length; i++) {
			globalWaypoints [i] = waypoints [i] + transform.position;
		}
	}

	void Update()
	{
		UpdateRaycastOrigins ();

		Vector3 velocity = CalculatePlatformMovement ();

		CalculatePassengerMovement (velocity);

		MovePassengers (true);
		transform.Translate (velocity);
		MovePassengers (false);
	}

	float Ease (float x)
	{
		float a = easeAmmount + 1;
		return (Mathf.Pow (x, a) / (Mathf.Pow(x, a) + Mathf.Pow (1f - x, a)));
	}

	Vector3 CalculatePlatformMovement ()
	{
		if (Time.time < nextMoveTime) {
			return Vector3.zero;
		}

		fromWaypointIndex %= globalWaypoints.Length;
		int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
		float distanceBetweenWaypoints = Vector3.Distance (globalWaypoints [fromWaypointIndex], globalWaypoints [toWaypointIndex]);
		percentBetweenWaypoints += Time.deltaTime * speed/distanceBetweenWaypoints;
		percentBetweenWaypoints = Mathf.Clamp01 (percentBetweenWaypoints);
		float easedPercentBetweenWaypoints = Ease (percentBetweenWaypoints);

		Vector3 newPos = Vector3.Lerp (globalWaypoints [fromWaypointIndex], globalWaypoints [toWaypointIndex], easedPercentBetweenWaypoints);

		if (percentBetweenWaypoints >= 1) {
			percentBetweenWaypoints = 0;
			fromWaypointIndex++;

			if (!cyclic) {
				//back track through waypoints if reach end of array
				if (fromWaypointIndex >= globalWaypoints.Length - 1) {

					fromWaypointIndex = 0;
					System.Array.Reverse (globalWaypoints);
				}
			}

			nextMoveTime = Time.time + waitTime;
		}

		return newPos - transform.position;
	}

	void MovePassengers(bool beforeMovePlatform)
	{
		foreach (PassengerMovement passenger in passengerMovement) {

			if (!passengerDictionary.ContainsKey (passenger.transform)) {
				passengerDictionary.Add (passenger.transform, passenger.transform.GetComponent<Controller2D> ());
			}

			if (passenger.moveBeforePlatform == beforeMovePlatform) {

				passengerDictionary [passenger.transform].Move (passenger.velocity, passenger.standingOnPlatform);
			}
		}
	}

	void CalculatePassengerMovement (Vector3 velocity)
	{
		HashSet<Transform> movedPassengers = new HashSet<Transform> ();
		passengerMovement = new List<PassengerMovement> ();

		float directionX = Mathf.Sign (velocity.x);
		float directionY = Mathf.Sign (velocity.y);

		//horizontally moving
		if (velocity.x != 0) {
			float rayLength = Mathf.Abs (velocity.x) + skinWidth;

			for (int i = 0; i < horizontalRayCount; i++) {

				Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
				rayOrigin += Vector2.up * (horizontalRaySpacing * i);

				RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

				if (hit && hit.distance != 0) {

					if (!movedPassengers.Contains (hit.transform)) {
						movedPassengers.Add (hit.transform);

						float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
						float pushY = -skinWidth;

						passengerMovement.Add (new PassengerMovement (hit.transform, new Vector3 (pushX, pushY), false, true)); 
					}
				}
			}
		}

		//vertically moving
		if (velocity.y != 0) {
			float rayLength = Mathf.Abs (velocity.y) + skinWidth;

			for (int i = 0; i < verticalRayCount; i++) {

				Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
				rayOrigin += Vector2.right * (verticalRaySpacing * i);
		
				RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

				if (hit && hit.distance != 0) {

					if (!movedPassengers.Contains (hit.transform)) {
						movedPassengers.Add (hit.transform);

						float pushX = (directionY == 1) ? velocity.x : 0;
						float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

						passengerMovement.Add (new PassengerMovement (hit.transform, new Vector3 (pushX, pushY), directionY == 1, true));
					}
				}
			}
		}

		//Passenger on top of platform
		if (directionY == -1 || velocity.y == 0 && velocity.x != 0) {
			float rayLength = 2f * skinWidth;

			for (int i = 0; i < verticalRayCount; i++) {

				Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);

				RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up, rayLength, passengerMask);

				if (hit && hit.distance != 0) {

					if (!movedPassengers.Contains (hit.transform)) {
						movedPassengers.Add (hit.transform);

						float pushX = velocity.x;
						float pushY = velocity.y;

						passengerMovement.Add (new PassengerMovement (hit.transform, new Vector3 (pushX, pushY), true, false)); 
					}
				}
			}
		}
	}

	void OnDrawGizmos()
	{
		if (waypoints.Length > 0) {
			float size = .3f;
			Gizmos.color = Color.red;

			for (int i = 0; i < waypoints.Length; i++) {
				Vector3 globalPos = (Application.isPlaying)?globalWaypoints [i]:waypoints[i] + transform.position;

				Gizmos.DrawCube (globalPos, Vector3.one * size);
			}
		}
	}

	struct PassengerMovement {

		public Transform transform;
		public Vector3 velocity;
		public bool standingOnPlatform;
		public bool moveBeforePlatform;

		public PassengerMovement (Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
		{
			transform = _transform;
			velocity = _velocity;
			standingOnPlatform = _standingOnPlatform;
			moveBeforePlatform = _moveBeforePlatform;
		}
	}
}
