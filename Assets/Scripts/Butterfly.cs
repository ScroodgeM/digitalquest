using UnityEngine;
using System.Collections;

public class Butterfly : MonoBehaviour {

	private float speed = 3f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Transform playerTransform = GameController.Instance.GetPlayerTransform ();
		if(Vector3.Distance(playerTransform.position, transform.position) > 5) {
			transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, speed*Time.deltaTime);
			transform.LookAt (playerTransform);
		}
	}
}
