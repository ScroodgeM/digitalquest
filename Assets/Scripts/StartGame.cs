using UnityEngine;
using System.Collections;

public class StartGame : MonoBehaviour {

	void OnMouseDown() {
		GameController.Instance.StartGame ();
	}
}
