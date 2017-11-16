using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {

	public float radius = 1f;

	void Start() {
		transform.localScale = new Vector3 (0.27f * radius, 0.27f * radius, 0.27f * radius);
	}

	/**
	 * Helper method for getting the vector from PO to Obstacle
	 **/
	public Vector3 ToObstacle(PhysicsObject po) {
		return transform.position - po.transform.position;
	}

	/**
	 * Uses dot product of (PO's forward unit vector) and (ToObstacle)
	 * to determine if obstacle is in front of PO
	 **/
	public bool IsInFrontOf(PhysicsObject po) {
		Vector3 forward = po.Velocity.normalized;
		float dot = Vector3.Dot (forward, ToObstacle (po));
		return dot > 0;
	}

	/**
	 * Uses dot product of (PO's side unit vector) and (ToObstacle)
	 * Compares it to (Obstacle radius) + (PO radius) + (a little extra spacing)
	 * to determin if PO is on a collision course with Obstacle
	 **/
	public bool IsDangerous(PhysicsObject po) {
		Vector3 left = VectorHelper.Perpindicularize (po.Velocity.normalized);
		float dot = Vector3.Dot (left, ToObstacle (po));
		return Mathf.Abs (dot) < radius + po.radius + 0.125f;
	}

	void OnDrawGizmos() {
		if (ObjectManager.showDebug) {
			Gizmos.DrawWireSphere (transform.position, radius);
		}
	}
}
