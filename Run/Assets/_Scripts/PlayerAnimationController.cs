using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour {

	public float hardLandingVelocityThreshold = 4;

	Animator animator;
	Player player;

	bool hasSetJumping = false;
	bool hasSetFalling = false;
	bool hasSetLand = false;

	void Awake()
	{
		animator = GetComponent<Animator> ();
		player = GetComponentInParent<Player> ();
	}

	void LateUpdate()
	{
		if (player.controller.collisions.below) {
			hasSetJumping = false;
			hasSetFalling = false;

			if (!hasSetLand) {
				hasSetLand = true;
				animator.SetTrigger ("Land");
			}
		}

		//jumping
		if (player.jumped && !hasSetJumping) {
			hasSetJumping = true;
			hasSetLand = false;

			animator.SetBool ("Jumping", true);
		} else {
			animator.SetBool ("Jumping", false);
		}

		//falling
		if (Mathf.Sign (player.Velocity.y) == -1 && !hasSetFalling) {
			hasSetFalling = true;
			animator.SetBool ("Falling", true);
		} else {
			animator.SetBool ("Falling", false);
		}

		animator.SetBool ("HardLanding", Mathf.Abs(player.Velocity.y) > hardLandingVelocityThreshold);

		//movement
		animator.SetBool ("Grounded", player.controller.collisions.below);
		animator.SetBool ("Moving", player.controller.playerInput.x != 0);

		if (animator.GetBool ("Moving")) {
			animator.speed = CalculateSpeedCoefficient ();
		} 	
	}

	float CalculateSpeedCoefficient ()
	{
		if (player.isSprinting) {
			return player.sprintSpeedCoefficient;
		} else
			return 1f;
	}
}
