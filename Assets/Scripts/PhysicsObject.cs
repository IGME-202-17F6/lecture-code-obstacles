using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * PhsyicsObject 
 * This component tracks position and movement information for a GameObject
 * It knows how to ApplyForce (but some other script will call that)
 * It runs its own BounceCheck method (to keep the object in-bounds)
 * Movement happens in LateUpdate, so that some other script can call
 *   ApplyForce in its Update, and we can be sure all Forces have been
 *   applied before moving the PhysicsObject.
 */
public class PhysicsObject : MonoBehaviour {

	// These guys should be familiar by now!
	Vector3 acceleration;
	Vector3 position;
	Vector3 velocity;

	// We'll use mu for our friction multiplier.
	float mu = 0.01f;

	// You can tweak these in the inspector.
	public float mass = 1f; // higher mass - more force needed to get moving
	public float elasticity = 0.9f; // percentage of energy conserved during a wall bounce
	public float radius = 1f;
	public float speedLimit = 0.125f;

	void Start () {
		// get our position from the game object
		position = new Vector3 (transform.position.x, transform.position.y, 0f);

		// no forces to start off
		velocity = Vector3.zero;
		acceleration = Vector3.zero;

		transform.localScale = new Vector3 (0.27f * radius, 0.27f * radius, 0.27f * radius);
	}

	/*
	 * ApplyForce
	 * Takes in a force and adjusts acceleration according to mass.
	 */
	public void ApplyForce(Vector3 force) {
		// Time.deltaTime is the number of seconds elapsed since the last frame.
		// We multiply by delta because:
		// - it lets us think of our units as per-second adjustments
		// - it smooths out inconsistencies in the framerate
		acceleration += force / mass * Time.deltaTime;

		// Think about it...

		// Regular 2 frames per second:
		//   a += 4 / 2 * 0.5;
		//   a += 4 / 2 * 0.5;
		//   total acceleration 2


		// Irregular 4 frames per second:
		//   a += 4 / 2 * 0.27;
		//   a += 4 / 2 * 0.23;
		//   a += 4 / 2 * 0.26;
		//   a += 4 / 2 * 0.24;
		//   total acceleration 2
	}

	/*
	 * BounceCheck
	 * Inverts a component of velocity when a wall is passed,
	 *   which "bounces" the PhysicsObject.
	 * 
	 * (-velocity * elasticity) lets us lose some energy to the bounce.
	 */
	void BounceCheck() {
		// calculate this once, instead of every if statement
		Vector3 screenPos = Camera.main.WorldToScreenPoint (position);

		// if we're off screen in some direction
		if (0 > screenPos.y) {
			// bottom collision
			position = Camera.main.ScreenToWorldPoint (new Vector3 (screenPos.x, 0, 0));
			velocity.y = -velocity.y * elasticity;
		} else if (Screen.height < screenPos.y) {
			// top collision
			position = Camera.main.ScreenToWorldPoint (new Vector3 (screenPos.x, Screen.height, 0));
			velocity.y = -velocity.y * elasticity;
		}

		// except, if I don't recalculate this between y and x checks,
		// it has a tendency to sink in the corners
		screenPos = Camera.main.WorldToScreenPoint (position);

		if (Screen.width < screenPos.x) {
			// right wrap
			position = Camera.main.ScreenToWorldPoint (new Vector3 (0, Random.Range(0, Screen.height), 0));
		}

		// zero out the z coordinate once for all checks
		position.z = 0;
	}

	void ClampVelocity() {
		float mag = velocity.magnitude;
		if (mag > speedLimit) {
			float scale = speedLimit / mag;
			velocity *= scale;
		}
	}

	/*
	 * LateUpdate
	 * Runs after every other scripts' regular Update()s
	 * In this case, it performs our friction, movement, and bounce checks.
	 */
	void LateUpdate () {
		Vector3 friction = velocity.normalized * (-1f * mu);
		ApplyForce (friction);

		velocity += acceleration;
		ClampVelocity ();
		position += velocity;

		transform.position = position;
		transform.rotation = VectorHelper.QuatFromUnit (velocity.normalized);

		BounceCheck ();

		acceleration = Vector3.zero;
	}

	void OnDrawGizmos() {
		if (ObjectManager.showDebug) {
			Gizmos.DrawWireSphere (position, radius);
		}
	}

	public Vector3 Velocity {
		get { return velocity; }
	}
}
