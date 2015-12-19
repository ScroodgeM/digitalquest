using UnityEngine;
using System.Collections;

public class CollectObject : MonoBehaviour {

	public int objectId;
	public GameObject mapObject;

	void Start() {
		StartCoroutine ("SetPosition");
	}

	IEnumerator SetPosition() {
		yield return new WaitForSeconds(0.1f);
		Debug.Log ("Updating position");
		transform.position = new Vector3 (mapObject.transform.position.x * GameController.mapScale, 0.5f /* 1.5f */, mapObject.transform.position.z * GameController.mapScale);
	}

	void OnMouseDown() {
		if (CheckDistance ())
			StartCoroutine ("ObjectSelected");
		else
			GameController.Instance.SetNotification ("Too far! Try to get closer");
	}

	IEnumerator ObjectSelected() {
		Debug.Log ("Object selected!!");
		Object animation = Resources.Load ("GrabAnimation");
		GameObject.Instantiate (animation, transform.position, Quaternion.identity);
		GameController.Instance.PlayAudio ("Item3");
		Handheld.Vibrate();

		//Take a screenshot
		ServerConnection.Instance.UploadScreenShot(objectId);

		StartCoroutine (ActivateQuest());
		yield return null;
	}

	IEnumerator ActivateQuest() {
		yield return new WaitForSeconds(0.5f);
		QuestManager.Instance.ObjectActivated (this);
	}

	public bool CheckDistance() {
		float distance = Vector3.Distance (GameController.Instance.GetPlayerTransform ().position, transform.position);
		Debug.Log (distance);
		if (distance < 10f)
			return true;
		return false;
	}

	public void DestroyItems() {
		Destroy (mapObject);
		Destroy (gameObject);
	}

}
