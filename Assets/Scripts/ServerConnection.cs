using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ServerConnection : MonoBehaviour {

	public GameObject connectionErrorWindow;
	public Button retryConnectionButton;
	public AudioSource audioSource;

	private static ServerConnection instance;

	private int userId, questId, stepId = 0;

	private ServerConnection() {}
	public static ServerConnection Instance {
		get {
			if (instance == null)
			{
				instance = new ServerConnection();
			}
			return instance;
		}
	}	

	void Awake () {
		instance = this;
		userId = PlayerPrefs.GetInt ("UserId");
		deviceIdentifier = SystemInfo.deviceUniqueIdentifier;
		baseUrl = "http://www.mastercava.it/digitalquest/";
		questId = PlayerPrefs.GetInt ("QuestId");
	}

	private string deviceIdentifier;
	private string baseUrl;

	// Use this for initialization
	void Start () {
		InvokeRepeating ("RefreshConnection", 10f, 5f);
	}

	public void LoadQuestGameData() {
		StartCoroutine ("RetrieveGameDatabaseCoroutine");
	}
	
	private void RefreshConnection() {
		Debug.Log (baseUrl + "update.php?q=" + questId + "&id=" + userId + "&p=" + QuestManager.Instance.GetPoints() + "&lat=" + Input.location.lastData.latitude + "&lon=" + Input.location.lastData.longitude + "&acc=" + Input.location.lastData.horizontalAccuracy + "&t=" + PlayerPrefs.GetString("Team"));
		WWW www = new WWW(baseUrl + "update.php?q=" + questId + "&id=" + userId + "&p=" + QuestManager.Instance.GetPoints() + "&lat=" + Input.location.lastData.latitude + "&lon=" + Input.location.lastData.longitude + "&acc=" + Input.location.lastData.horizontalAccuracy + "&t=" + PlayerPrefs.GetString("Team"));
	}

	public void NextStep(int passedId, int gainedPoints) {
		WWW www = new WWW(baseUrl + "step.php?q=" + questId + "&id=" + userId + "&s=" + passedId + "&p=" + gainedPoints + "&lat=" + Input.location.lastData.latitude + "&lon=" + Input.location.lastData.longitude + "&acc=" + Input.location.lastData.horizontalAccuracy);
		stepId = passedId;
	}

	public void UploadScreenShot(int objectId) {
		StartCoroutine (UploadScreenshotCoroutine(objectId));
	}

	private IEnumerator UploadScreenshotCoroutine(int objectId) {
		yield return new WaitForEndOfFrame();
		string screenShotURL = baseUrl + "screenshot.php";

		// Create a texture the size of the screen, RGB24 format
		int width = Screen.width;
		int height = Screen.height;
		Texture2D tex = new Texture2D( width, height, TextureFormat.RGB24, false );
		// Read screen contents into the texture
		tex.ReadPixels(new Rect(0, 0, width, height), 0, 0 );
		tex.Apply();
		// Encode texture into PNG
		//byte[] bytes = tex.EncodeToPNG();
		byte[] bytes = tex.EncodeToJPG(70);
		Destroy( tex );
		// Create a Web Form
		WWWForm form = new WWWForm();
		form.AddField("frameCount", Time.frameCount.ToString());
		form.AddField("userId", userId);
		form.AddField("deviceId", deviceIdentifier);
		form.AddField("questId", questId);
		//form.AddField("stepId", stepId);
		form.AddField("stepId", objectId);
		form.AddBinaryData("file", bytes, "screenshot.jpg", "image/jpeg");
		Debug.Log ("Screenshot uploading to server...");
		// Upload to a cgi script
		WWW w = new WWW(screenShotURL, form);
		while(!w.isDone) yield return w;
		if (w.error != null){
			Debug.Log(w.error);

		}  
		else{
			Debug.Log("Finished Uploading Screenshot: " + w.text);
		}
	}

	private IEnumerator RetrieveGameDatabaseCoroutine() {
		Debug.Log ("Gathering database data...");
		WWW www = new WWW (baseUrl + "quest.php?id=" + questId);
		Debug.Log (baseUrl + "quest.php?id=" + questId);
		while (!www.isDone)
			yield return www.progress;
		Debug.Log (www.text);
		if (www.error != null) {
			ConnectionError(delegate {
				LoadQuestGameData();
			});
		} else {
			if(www.text != "") QuestManager.Instance.LoadJsonDatabase (www.text);
			//Error
			//else GameController.Instance.ShowError("","");
		}
	}

	private IEnumerator RetrieveGameDataCoroutine() {
		Debug.Log ("Gathering saved data...");
		WWW www = new WWW(baseUrl + "load.php?q=" + PlayerPrefs.GetInt("QuestId") + "&id=" + userId);
		while (!www.isDone)
			yield return www.progress;
		Debug.Log (www.text);
		if (www.error != null) {
			ConnectionError(delegate {
				RetrieveGameData();
			});
		} else {
			QuestManager.Instance.LoadGame (www.text);
		}
	}

	public void RetrieveGameData() {
		StartCoroutine ("RetrieveGameDataCoroutine");
	}

	public void ResetQuestData() {
		StartCoroutine ("ResetQuestDataCoroutine");
	}

	private IEnumerator ResetQuestDataCoroutine() {
		Debug.Log ("Eliminating saved data...");
		WWW www = new WWW(baseUrl + "reset.php?q=" + PlayerPrefs.GetInt("QuestId") + "&id=" + userId);
		while (!www.isDone)
			yield return www.progress;
		if (www.error != null) {
			ConnectionError(delegate {
				ResetQuestData();
			});
		}
	}

	public void StreamAudioFromURL (string url) {
		StartCoroutine (StreamAudioFromURLCoroutine(url));
	}

	private IEnumerator StreamAudioFromURLCoroutine(string url) {
		WWW www = new WWW(baseUrl + "audio/" + url);
		Debug.Log (baseUrl + "audio/" + url);
		GameController.Instance.SetNotification ("Audioclip loading");
		while (!www.isDone)
			yield return www.progress;
		AudioClip stream = audioSource.clip = www.GetAudioClip (false, true);
		while (!(stream.loadState == AudioDataLoadState.Loaded))
			yield return www.progress;
		Debug.Log ("Audio stream ready to play");
		audioSource.clip = stream;
		audioSource.Play ();
		GameController.Instance.DisableNotification ();
	}

	private void ConnectionError(UnityEngine.Events.UnityAction call) {
		GameController.Instance.PlayAudio ("Cancel2");
		connectionErrorWindow.SetActive (true);
		retryConnectionButton.onClick.RemoveAllListeners ();
		retryConnectionButton.onClick.AddListener (call);
	}

	public void CloseErrorWindow() {
		connectionErrorWindow.SetActive (false);
		GameController.Instance.PlayAudio ("Cancel1");
	}
}
