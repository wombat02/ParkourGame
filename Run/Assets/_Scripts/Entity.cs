using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour {

	public Vector3 Position {

		get { return transform.position; }
		set { transform.position = value; }
	}

	public Vector2 Position2D {

		get { return new Vector2 (Position.x, Position.y); }
		set { transform.position = new Vector3 (value.x, value.y, Position.z); }
	}

	public Vector3 EulerAngles {

		get { return transform.eulerAngles; }
		set { transform.eulerAngles = value; }
	}

	public Vector3 DirTo (Entity other)
	{
		return (other.Position - Position).normalized;
	}

	public float DistTo (Entity other)
	{
		return (other.Position - Position).magnitude;
	}
}
