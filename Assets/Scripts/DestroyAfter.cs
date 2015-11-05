using UnityEngine;
using System.Collections;

public class DestroyAfter : MonoBehaviour {

	public float time = 2f;

	// Use this for initialization
	void Start () {
		Invoke ("DestroyGameObject", time);
	}

	void DestroyGameObject() {
		Destroy (gameObject);
	}

}
