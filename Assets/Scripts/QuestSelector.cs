using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SimpleJSON;

public class QuestSelector : MonoBehaviour {

	public GameObject questList;
	public InputField teamName;

	public Text errorMessage;

	public GameObject loadingBar;
	public UILoadingBar loadingScript;

	private string url;
	private AudioSource audio;

	private int availableQuests;
	private JSONArray quests;

	void Awake() {
		if(PlayerPrefs.GetInt ("QuestId") > 0 && PlayerPrefs.GetString ("Team") != "") {
			Application.LoadLevel(1);
		}
	}


	// Use this for initialization
	void Start () {
		url = "http://www.mastercava.it/digitalquest/";
		StartCoroutine("RefreshQuests");
		teamName.text = PlayerPrefs.GetString ("Team");
		audio = GetComponent<AudioSource> ();
		SensorHelper.ActivateRotation();
		Invoke ("CheckIMU", 2f);
	}

	private IEnumerator RefreshQuests() {
		WWW www = new WWW(url + "quests.php");
		while(!www.isDone) yield return www.progress;
		for (int i=0; i<questList.transform.childCount; i++) {
			Destroy (questList.transform.GetChild(i).gameObject);
		}
		//Debug.Log (www.text);
		if (www.error != null) {
			errorMessage.text = "Network not available. Connect to internet and refresh";
		} else {
			JSONNode json = JSON.Parse (www.text);
			availableQuests = json ["elements"].AsInt;
			quests = json ["quests"].AsArray;
			for (int i=0; i<availableQuests; i++) {
				Object resource = Resources.Load ("QuestSelector");
				GameObject questObject = (GameObject)GameObject.Instantiate (resource);
				questObject.transform.parent = questList.transform;
				questObject.transform.localScale = Vector3.one;
				//Set name
				Text text = questObject.transform.GetChild (1).GetComponent<Text> ();
				text.text = quests [i] ["name"].Value;
				//Button
				GameObject button = questObject.transform.GetChild (2).gameObject;
				Button script = button.GetComponent<Button> ();
				int toBeLoaded = quests [i] ["id"].AsInt;
				LinkButtonToQuest(script, toBeLoaded);
				/*
				script.onClick.AddListener (delegate {
					LoadQuest (toBeLoaded);
				});
				*/
			}
		}
	}

	private void LinkButtonToQuest(Button script, int questId) {
		script.onClick.AddListener (delegate {
			LoadQuest (questId);
		});
	}

	public void ForceRefresh() {
		PlayAudio ("Load");
		StartCoroutine("RefreshQuests");
		errorMessage.text = "";
		Invoke ("CheckIMU", 0f);
	}

	public void PlayAudio(string name) {
		AudioClip clip = (AudioClip)Resources.Load ("Audio/" + name);
		audio.clip = clip;
		audio.Play ();
	}

	private void LoadQuest(int questId) {
		if (teamName.text != "") {
			Debug.Log("Loading quest #" + questId);
			PlayerPrefs.SetInt ("QuestId", questId);
			PlayerPrefs.SetString ("Team", teamName.text);
			loadingBar.SetActive(true);
			PlayAudio("Up3");
			StartCoroutine("AsyncLoading");
		}
	}

	private IEnumerator AsyncLoading() {
		AsyncOperation async = Application.LoadLevelAsync (1);
		while(!async.isDone) {
			loadingScript.SetValue(async.progress);
			yield return async;
		}
	}

	private void CheckIMU() {
		Quaternion rot = SensorHelper.rotation;
		if (rot.x == 0 || rot.y == 0 || rot.z == 0) {
			errorMessage.text = "Your device does not support some functionalities";
		}
	}
	
}
