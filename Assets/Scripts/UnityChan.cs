using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]

public class UnityChan : MonoBehaviour {

	private Transform playerTransform;
	private Animator animator;

	private float walkSpeed = 2.5f;
	private float runSpeed = 5f;

	void Start() {
		playerTransform = GameController.Instance.GetPlayerTransform ();
		animator = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
		//Orientation
		Vector3 playerPosition = playerTransform.position;
		Vector3 target = new Vector3 (playerPosition.x, transform.position.y, playerPosition.z);
		transform.LookAt (target);

		//Moving
		float distance = Vector3.Distance (transform.position, playerPosition);
		//Run
		if (distance > 6) {
			//Debug.Log("Run!!");
			animator.SetBool ("Running", true);
			animator.SetBool ("Walking", false);
			transform.position = Vector3.MoveTowards(transform.position, target, runSpeed*Time.deltaTime);
		}
		//Walk
		else if (distance > 3) {
			//Debug.Log("Walk");
			animator.SetBool ("Running", false);
			animator.SetBool ("Walking", true);
			transform.position = Vector3.MoveTowards(transform.position, target, walkSpeed*Time.deltaTime);
		}
		//Actions
		else {
			animator.SetBool ("Walking", false);
			animator.SetBool ("Running", false);
		}
	}
}
