using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : LivingEntity {

	public float moveSpeed = 3f;
	public float moveSmoothingGround = .1f;
	public float moveSmoothingAir = .5f;
	public float jumpHeight = 2f;
	float jumpVelocity = 0;
	float gravity = -9.81f;

	Vector3 velocity;
	float currentXSmoothing;

	Controller2D controller;

	void Awake()
	{
		controller = GetComponent<Controller2D> ();
	}

	public override void Start()
	{
		jumpVelocity = Mathf.Sqrt (Mathf.Abs(2f * gravity) * jumpHeight);

		base.Start ();
	}

	void Update()
	{
		if (controller.collisions.above || controller.collisions.below)
			velocity.y = 0;

		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

		if (Input.GetKeyDown (KeyCode.Space) && controller.collisions.below)
			velocity.y = jumpVelocity;

		velocity.x = Mathf.SmoothDamp (velocity.x, input.x * moveSpeed, ref currentXSmoothing, (controller.collisions.below)?moveSmoothingGround:moveSmoothingAir);
		velocity.y += gravity * Time.deltaTime;

		controller.Move (velocity * Time.deltaTime);
	}
		
}
