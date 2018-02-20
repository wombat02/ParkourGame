using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : LivingEntity {

	public float moveSpeed = 3f;
	public float moveSmoothingGround = .1f;
	public float moveSmoothingAir = .5f;
	public float maxJumpHeight = 2f;
	public float minJumpHeight = 1f;

	float maxJumpVelocity = 0;
	float minJumpVelocity = 0;
	float gravity = -9.81f;


	[Header ("Wall climbing")]
	public float wallSlideSpeedMax = 1;
	public float wallStickTime = .25f;
	float timeToWallUnstick;


	public Vector2 wallJumpClimb;
	public Vector2 wallJumpLeap;
	public Vector2 wallJumpOff;

	Vector3 velocity;
	float currentXSmoothing;

	Controller2D controller;

	void Awake()
	{
		controller = GetComponent<Controller2D> ();
	}

	public override void Start()
	{
		maxJumpVelocity = Mathf.Sqrt (Mathf.Abs(2f * gravity) * maxJumpHeight);
		minJumpVelocity = Mathf.Sqrt (Mathf.Abs (2f * gravity) * minJumpHeight);

		base.Start ();
	}

	void Update()
	{
		HandleMovement ();
	}

	void HandleMovement ()
	{
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		int wallDirX = (controller.collisions.left) ? -1 : 1;

		velocity.x = Mathf.SmoothDamp (velocity.x, input.x * moveSpeed, ref currentXSmoothing, (controller.collisions.below)?moveSmoothingGround:moveSmoothingAir);
		velocity.y += gravity * Time.deltaTime;

		bool wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below) {

			if (velocity.y > 0)
				velocity.y = 0;
			
			wallSliding = true;

			if (velocity.y < -wallSlideSpeedMax)
				velocity.y = -wallSlideSpeedMax;

			if (timeToWallUnstick > 0) {
				
				currentXSmoothing = 0;
				velocity.x = 0;

				if (input.x != wallDirX && input.x != 0)
					timeToWallUnstick -= Time.deltaTime;
				else
					timeToWallUnstick = wallStickTime;
			} else
				timeToWallUnstick = wallStickTime;
		}

		if (Input.GetKeyDown (KeyCode.Space)) {

			if (wallSliding) {
				if (input.x == 0) {
					//wall jump off

					velocity.x = -wallDirX * wallJumpOff.x;
					velocity.y = wallJumpOff.y;

				} else if (input.x == wallDirX) {
					//wall climb jump

					velocity.x = -wallDirX * wallJumpClimb.x;
					velocity.y = wallJumpClimb.y;

				} else if (input.x == -wallDirX) {
					//wall leap

					velocity.x = -wallDirX * wallJumpLeap.x;
					velocity.y = wallJumpLeap.y;
				}
			}

			if (controller.collisions.below) {
				velocity.y = maxJumpVelocity;
			}
		}

		if (Input.GetKeyUp (KeyCode.Space)) {

			if (velocity.y > minJumpVelocity)
				velocity.y = minJumpVelocity;
		}

		controller.Move (velocity * Time.deltaTime, input);

		if (controller.collisions.above || controller.collisions.below)
			velocity.y = 0;
	}
		
}
