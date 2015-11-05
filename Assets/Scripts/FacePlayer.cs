using UnityEngine;
using System.Collections;

public class FacePlayer : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		Vector3 playerPosition = GameController.Instance.GetPlayerTransform ().position;
		Vector3 target = new Vector3 (playerPosition.x, transform.position.y, playerPosition.z);
		transform.LookAt (target);
		transform.Rotate (0f, 180f, 0f);
	}
}
