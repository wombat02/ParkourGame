using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour {

	Animator animator;
	Player player;

	void Awake()
	{
		animator = GetComponent<Animator> ();
		player = GetComponentInParent<Player> ();
	}

	void Update()
	{
		animator.SetBool ("Grounded", player.controller.collisions.below);
		animator.SetBool ("Moving", player.controller.playerInput.x != 0);

		if (animator.GetBool ("Moving")) {
			animator.speed = SprintSpeedCoefficient ();
		} else
			animator.speed = 1;
	}

	float SprintSpeedCoefficient ()
	{
		if (player.isSprinting) {
			return player.sprintSpeedCoefficient;
		} else
			return 1f;
	}
}
