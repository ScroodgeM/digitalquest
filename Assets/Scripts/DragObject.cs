using UnityEngine;
using System.Collections;

public class DragObject : MonoBehaviour {

	public GameObject worldObject;

	private Vector3 screenPoint;
	private Vector3 offset;

	// Use this for initialization
	void Start () {
	
	}
	
	void OnMouseDown() {
		screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
		offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
	}

	void OnMouseDrag() {
		Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
		Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
		transform.position = new Vector3(curPosition.x, transform.position.y, curPosition.z);
		if (worldObject != null) {
			worldObject.transform.position = new Vector3(curPosition.x * GameController.mapScale, worldObject.transform.position.y, curPosition.z * GameController.mapScale);
		}
	}
}
