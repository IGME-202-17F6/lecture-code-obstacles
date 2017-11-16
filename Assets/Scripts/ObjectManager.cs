using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ObjectManager 
 * This component tracks collections of PhysicsObjects
 * It also orchestrates the forces and applies them.
 */
public class ObjectManager : MonoBehaviour {

	// wind and gravity are constant throughout the scene
	// so we'll track them here (instead of on every object)
	Vector3 motivation;

	// for PhysicsObjects that already exist in the scene
	// (populated by dragging them into the inspector)
	public List<Obstacle> obstacles;

	// for PhysicsObjects generated at runtime
	// (populated with the GeneratePOs() call
	List<PhysicsObject> movers = new List<PhysicsObject>();

	// a reference to the prefab used for PhysicsObjects
	// (also populated in the inspector)
	public GameObject moverPrefab;

	public static bool showDebug = false;


	void Start () {
		motivation = new Vector3 (0.1f, 0, 0);

		GenerateMovers ();
	}

	void GenerateMovers() {
		for (int i = 0; i < 1000; i++) {
			Vector3 pos = Camera.main.ScreenToWorldPoint (
				new Vector3 (Random.Range (0, Screen.width), Random.Range (0, Screen.height), 0));
			pos.z = 0;
			GameObject go = Instantiate (moverPrefab, pos, Quaternion.identity);
			PhysicsObject po = go.GetComponent<PhysicsObject> ();
			movers.Add (po);
		}
	}

	/**
	 * Runs obstacle avoidance for a given PO
	 **/
	void AvoidObstacles(PhysicsObject mover) {
		Obstacle closestDangerous = null;
		float minDist = float.MaxValue;

		// iterate over obstacles and find the closest one that is:
		// - in front of the PO
		// - dangerous to the PO
		foreach (Obstacle obstacle in obstacles) {
			if (obstacle.IsInFrontOf (mover) && 
				obstacle.IsDangerous (mover)) {
				float dist = obstacle.ToObstacle (mover).sqrMagnitude;
				if (dist < minDist) {
					minDist = dist;
					closestDangerous = obstacle;
				}
			}
		}

		// if an obstacle was found
		// - apply a turning force based on the sign of the left/right differentiating dot product
		// - slow down!
		if (closestDangerous != null) {
			Vector3 left = VectorHelper.Perpindicularize (mover.Velocity.normalized);
			float dot = Vector3.Dot (left, closestDangerous.ToObstacle (mover));
			float direction = dot < 0 ? 1 : -1;
			mover.ApplyForce (left * mover.speedLimit * direction);
			mover.ApplyForce (mover.Velocity * -2f);
		}
	}

	/**
	 * For a given PO
	 * - have it move to the right (motiviation)
	 * - have it avoid obstacles
	 **/
	void SimulateObject (PhysicsObject mover) {
		mover.ApplyForce (motivation);
		AvoidObstacles (mover);
	}

	void Update () {
		// iterate over all movers
		for (int i = movers.Count - 1; i >= 0; i--) {
			// and apply some forces
			SimulateObject (movers [i]);
		}
	}
}
