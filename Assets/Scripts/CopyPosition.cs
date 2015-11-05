using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class CopyPosition : MonoBehaviour {

	public GameObject mapObject;
	//public UnityStandardAssets.CrossPlatformInput.Joystick joypad;

	private GameController gameController;

	private Transform mapTransform;
	private float mapScale = GameController.mapScale;

	private float movementSpeed = 6f;

	void Start() {
		mapTransform = mapObject.transform;
		gameController = GameController.Instance;
	}
	
	// Update is called once per frame
	void Update () {
		if (gameController.IsGameStarted () && !gameController.IsJoypadEnabled ()) {
			transform.position = new Vector3 (mapTransform.position.x * mapScale, transform.position.y, mapTransform.position.z * mapScale);
			#if (UNITY_EDITOR)
			transform.rotation = new Quaternion (0f, mapTransform.rotation.y, 0f, mapTransform.rotation.w);
			#endif
		} else if (gameController.IsJoypadEnabled ()) {
			Vector3 offset = transform.forward * Time.deltaTime * movementSpeed * CrossPlatformInputManager.GetAxis("Vertical");
			transform.position += new Vector3(offset.x, 0f, offset.y);
		}
	}
}
