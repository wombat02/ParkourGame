using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : Entity {

	[Header ("LivingEntity")]
	public float startingHealth;
	float currentHealth;

	public float Health {

		get { return currentHealth; }
		private set { currentHealth = value; }
	}

	public virtual void Start()
	{
		Health = startingHealth;
	}

	public virtual void TakeDamage (float damage)
	{
		Health -= damage;

		if (Health <= 0)
			Die ();
	}

	protected virtual void Die()
	{
		print (gameObject.name + " -is dead");
	}
}
