using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : LivingEntity {

	Controller2D controller;

	void Awake()
	{
		controller = GetComponent<Controller2D> ();
	}

	public override void Start()
	{
		base.Start ();
	}
		
}
