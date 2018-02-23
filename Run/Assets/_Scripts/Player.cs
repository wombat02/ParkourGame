using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : LivingEntity {

	[Header("Player")]
	[Header("Movement")]
	public float moveSpeed = 3f;
	public float sprintSpeedCoefficient = 2;
	public float moveSmoothingGround = .1f;
	public float moveSmoothingAir = .5f;
	public float maxJumpHeight = 2f;
	public float minJumpHeight = 1f;

	const float gravity = -9.81f;
	float maxJumpVelocity = 0;
	float minJumpVelocity = 0;
	float currentXSmoothing;

	[HideInInspector]
	public bool isSprinting;
	[HideInInspector]
	public bool jumped;

	public Vector2 Velocity {

		get { return velocity; }
		private set { velocity = value; }
	}

	Vector2 velocity;
	[HideInInspector]
	public Controller2D controller;

	[Header ("Wall Sliding")]
	public float wallSlideSpeedGripping = .2f;
	public float wallSlideSpeedMax = 1;
	public float wallSlideSpeedSmoothing = .8f;
	public float wallStickTime = .25f;
	float timeToWallUnstick;

	float currentWallSlideSpeedSmoothingVelocity;

	int wallDirX;

	public Vector2 wallJumpClimb;
	public Vector2 wallJumpLeap;
	public Vector2 wallJumpOff;
	bool wallSliding = false;

	Transform model;

	void Awake()
	{
		controller = GetComponent<Controller2D> ();
		model = transform.Find ("Model");
	}

	public override void Start()
	{
		maxJumpVelocity = Mathf.Sqrt (Mathf.Abs(2f * gravity) * maxJumpHeight);
		minJumpVelocity = Mathf.Sqrt (Mathf.Abs (2f * gravity) * minJumpHeight);

		base.Start ();
	}

	public void HandleMovement (Vector2 directionalInput)
	{
		model.right = controller.collisions.faceDir * Vector2.right;
		wallDirX = (controller.collisions.left) ? -1 : 1;

		float targetVelocityX = directionalInput.x * moveSpeed;
		if (isSprinting && controller.collisions.below)
			targetVelocityX *= sprintSpeedCoefficient;

		velocity.x = Mathf.SmoothDamp (velocity.x,  targetVelocityX, ref currentXSmoothing, (controller.collisions.below)?moveSmoothingGround:moveSmoothingAir);
		velocity.y += gravity * Time.deltaTime;

		controller.Move (velocity * Time.deltaTime, directionalInput);

		if (controller.collisions.above || controller.collisions.below)
			velocity.y = 0;
	}

	public void HandleWallSliding(Vector2 directionalInput)
	{
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below) {

			if (velocity.y > 0) {
				velocity.y = 0;
				wallSliding = true;
			}

			wallSliding = true;
			if (velocity.y < -wallSlideSpeedMax)
				velocity.y = -wallSlideSpeedMax;

			if (directionalInput.x == wallDirX) {
				velocity.y = Mathf.Lerp (velocity.y, -wallSlideSpeedGripping, wallSlideSpeedSmoothing);
			}

			if (timeToWallUnstick > 0) {
				
				currentXSmoothing = 0;
				velocity.x = 0;

				if (directionalInput.x != wallDirX && directionalInput.x != 0)
					timeToWallUnstick -= Time.deltaTime;
				else
					timeToWallUnstick = wallStickTime;
			} else
				timeToWallUnstick = wallStickTime;
		} else
			wallSliding = false;
	}

	public void OnJumpInputDown (Vector2 directionalInput)
	{
		if (wallSliding) {
			if (directionalInput.x == 0) {
				//wall jump off

				velocity.x = -wallDirX * wallJumpOff.x;
				velocity.y = wallJumpOff.y;

			} else if (directionalInput.x == wallDirX) {
				//wall climb jump

				velocity.x = -wallDirX * wallJumpClimb.x;
				velocity.y = wallJumpClimb.y;

			} else if (directionalInput.x == -wallDirX) {
				//wall leap

				velocity.x = -wallDirX * wallJumpLeap.x;
				velocity.y = wallJumpLeap.y;
			}

			jumped = true;

		} else if (controller.collisions.below) {
			velocity.y = maxJumpVelocity;
			jumped = true;
		}
	}

	public void OnJumpInputUp ()
	{
		if (velocity.y > minJumpVelocity)
			velocity.y = minJumpVelocity;

		jumped = false;
	}

	public void OnSprintInputDown()
	{
		if (controller.collisions.below)
			isSprinting = true;
	}

	public void OnSprintInputUp ()
	{
		isSprinting = false;
	}
}
