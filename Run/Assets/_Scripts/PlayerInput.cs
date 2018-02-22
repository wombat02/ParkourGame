using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour {

	Player player;

	void Awake()
	{
		player = GetComponent<Player> ();
	}

	void Update()
	{
		if (Input.GetKeyDown (KeyCode.Space)) {
			player.OnJumpInputDown (GetDirectionalInput ());
		}

		if (Input.GetKeyUp (KeyCode.Space)) {
			player.OnJumpInputUp ();
		}

		if (Input.GetKeyDown (KeyCode.LeftShift)) {
			player.OnSprintInputDown ();
		}

		if (Input.GetKeyUp (KeyCode.LeftShift)) {
			player.OnSprintInputUp ();
		}

		player.HandleMovement (GetDirectionalInput ());
		player.HandleWallSliding (GetDirectionalInput ());
	}

	public Vector2 GetDirectionalInput()
	{
		return new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
	}
}
