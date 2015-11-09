using UnityEngine;
using System.Collections;
//using SimpleJSON;
using UnityEngine.UI;
using Facebook.Unity;

public class GameController : MonoBehaviour {

	public GameObject playButton, gameButtons;
	public GameObject questWindow, questList;
	public GameObject settingsWindow, errorWindow;
	public GameObject loadingBar;
	public GameObject notification;
	public GameObject refreshMapButton;
	public Text notificationText;
	public UIUnitFrame_Bar pointsBar;
	public Text accuracyText;
	public GameObject redAlert;
	public Slider fovSlider;

	private static GameController instance;

	private QuestManager questManager;

	public static float mapScale = 100f;

	public Camera mapCamera, worldCamera;
	public GameObject loadingScene;

	public Transform cursorTransform, playerTransform;

	public TextMesh instructions;

	private bool isGeolocationCompleted;
	private bool isGameStarted;
	private bool isJoypadEnabled;

	private AudioSource audio;
	/*
	private FlashlightPlugin flashLightPlugin;
	private bool isFlashlightOn = false;
	*/
	private GameController() {}
	public static GameController Instance {
		get {
			if (instance == null)
			{
				instance = new GameController();
			}
			return instance;
		}
	}	

	// Use this for initialization
	void Awake () {
		instance = this;
		//questManager = GetComponent<QuestManager> ();
	}
	void Start() {
		questManager = QuestManager.Instance;
		gameButtons.SetActive (false);
		audio = GetComponent<AudioSource> ();
		//iPhoneSettings.screenCanDarken = false;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		if (PlayerPrefs.HasKey ("Fov")) {
			worldCamera.fieldOfView = PlayerPrefs.GetFloat("Fov");
			fovSlider.value = PlayerPrefs.GetFloat("Fov");
		}
	}
	

	public void ChangeCamera() {
		if (mapCamera.enabled && !worldCamera.enabled) {
			worldCamera.enabled = true;
			mapCamera.enabled = false;
			refreshMapButton.SetActive(false);
		} else {
			worldCamera.enabled = false;
			mapCamera.enabled = true;
			refreshMapButton.SetActive(true);
		}
		PlayAudio ("Save");
	}

	public void AddObject() {
		//Loading objects
		Object mapResource = Resources.Load ("MapObject");
		GameObject mapObject = (GameObject) GameObject.Instantiate(mapResource,cursorTransform.position, new Quaternion(0f, 0f, 0f, 1f));
		Object worldResource = Resources.Load ("WorldObject");
		GameObject worldObject = (GameObject) GameObject.Instantiate(worldResource,new Vector3(playerTransform.position.x, 0.3f, playerTransform.position.z), new Quaternion(0f, 0f, 0f, 1f));

		//Link between map and world objects
		DragObject script = mapObject.GetComponent<DragObject> ();
		script.worldObject = worldObject;
		PlayAudio ("Save");
	}

	public void StartGame() {
		isGameStarted = true;
		Destroy (loadingScene);
		SetInstructions ("Game started!");
		playButton.SetActive (false);
		gameButtons.SetActive (true);
		PlayAudio ("Save");
	}

	public void SetInstructions(string textToDisplay) {
		instructions.text = textToDisplay;
	}

	public void GeolocationCompleted() {
		isGeolocationCompleted = true;
		questManager.RefreshStatus ();
		SetInstructions ("Geolocation completed!\nReady to start");
		if(!isGameStarted) playButton.SetActive (true);
	}

	public void SwitchJoypad() {
		isJoypadEnabled = !isJoypadEnabled;
	}

	public Transform GetPlayerTransform() {
		return playerTransform;
	}

	public bool IsGameStarted() {
		return isGameStarted;
	}

	public bool IsJoypadEnabled() {
		return isJoypadEnabled;
	}

	public void OpenQuestWindow() {
		questWindow.SetActive (true);
		PlayAudio ("Save");
		//Other
		settingsWindow.SetActive (false);
		//QuestManager.Instance.CloseDetailWindow ();
	}

	public void CloseQuestWindow() {
		questWindow.SetActive (false);
		PlayAudio ("Cancel1");
	}

	public void OpenSettingsWindow() {
		settingsWindow.SetActive (true);
		PlayAudio ("Save");
		//Other
		questWindow.SetActive (false);
		//QuestManager.Instance.CloseDetailWindow ();
	}

	public void ShowError(string title, string text) {
		errorWindow.SetActive (true);
		PlayAudio ("Cancel1");
	}

	public void NoGPS() {
		ShowError ("Geolocation error", "It seems that the application is unable to identify your position. Please check that your GPS services are activated and restart the application.");
	}
	
	public void CloseSettingsWindow() {
		settingsWindow.SetActive (false);
		PlayAudio ("Cancel1");
	}
	
	public void CloseErrorWindow() {
		errorWindow.SetActive (false);
		PlayAudio ("Cancel1");
	}

	public void PlayAudio(string name) {
		AudioClip clip = (AudioClip)Resources.Load ("Audio/" + name);
		audio.clip = clip;
		audio.Play ();
	}

	public void SetLoadingBar(float value) {
		//Debug.Log (value);
		if (value == 1f) {
			loadingBar.SetActive(false);
		} else {
			loadingBar.SetActive(true);
			UILoadingBar script = loadingBar.GetComponent<UILoadingBar> ();
			script.SetValue(value);
		}

	}

	public GameObject AddQuest() {
		Object resource = Resources.Load ("Quest");
		GameObject questObject = (GameObject) GameObject.Instantiate (resource);
		questObject.transform.SetParent(questList.transform);
		questObject.transform.localScale = Vector3.one;
		return questObject;
	}

	public void ResetListElements() {
		for (int i=0; i<questList.transform.childCount; i++) {
			Destroy (questList.transform.GetChild(i).gameObject);
		}
	}


	public void SetPoints(int value) {
		pointsBar.SetValue (value);
	}

	public void SetMaxPoints(int value) {
		pointsBar.SetMaxValue(value);
	}
	public void SetAccuracy(int value) {
		accuracyText.text = value + "m";
		if (value > 10)
			redAlert.SetActive (true);
		else
			redAlert.SetActive (false);
	}
	/*
	public void ToggleFlashLight() {
		if(!isFlashlightOn){
			flashLightPlugin.SetFlashlightOn();
			isFlashlightOn = true;
		}else{
			flashLightPlugin.SetFlashlightOff();
			isFlashlightOn = false;
		}
	}
	*/
	public void SetNotification(string text) {
		notification.SetActive (true);
		notificationText.text = text;
		Invoke ("DisableNotification", 2f);
	}

	public void DisableNotification() {
		notification.SetActive (false);
	}

	public void BackToMainMenu() {
		PlayerPrefs.DeleteKey ("QuestId");
		StartCoroutine ("AsyncLoading");
	}

	private IEnumerator AsyncLoading() {
		AsyncOperation async = Application.LoadLevelAsync ("Opening");
		while(!async.isDone) {
			SetLoadingBar(async.progress);
			yield return async;
		}
	}

	public void SetFieldOfView() {
		worldCamera.fieldOfView = fovSlider.value;
		PlayerPrefs.SetFloat ("Fov", fovSlider.value);
	}

	public void ShowPointOnMap(float lat, float lon) {
		if(!mapCamera.enabled) ChangeCamera ();
		//Loading object
		Object mapResource = Resources.Load ("MapPointer");
		GameObject mapObject = (GameObject) GameObject.Instantiate(mapResource,cursorTransform.position, new Quaternion(0f, 0f, 0f, 1f));//Object on map
		SetGeolocation location = mapObject.AddComponent<SetGeolocation> ();
		location.lat = lat;
		location.lon = lon;
		location.scaleX = 1f;
		location.scaleY = 1f;
		location.scaleZ = 1f;
		location.height = 10035;
		DestroyAfter script = mapObject.AddComponent<DestroyAfter> ();
		script.time = 5f;
		QuestManager.Instance.CloseDetailWindow ();
		CloseQuestWindow ();
		PlayAudio ("Magic1");
	}

	public void Logout() {
		PlayerPrefs.DeleteAll ();
		FB.LogOut ();
		Application.LoadLevelAsync ("Login");
	}

}
