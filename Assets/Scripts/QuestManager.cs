using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;

public class QuestManager : MonoBehaviour {

	private static QuestManager instance;

	private QuestManager() {}
	public static QuestManager Instance {
		get {
			if (instance == null)
			{
				instance = new QuestManager();
			}
			return instance;
		}
	}	
	
	// Use this for initialization
	void Awake () {
		instance = this;
	}

	//public Transform mapTransform;

	public GameObject detailWindow;
	public GameObject answerForm;
	public GameObject objectsContainer;
	public InputField answer;
	public Button answerButton;
	public GameObject showOnMapButton;
	public Text detailName, detailText;

	public Sprite questType0, questType1, questType2, questType3, questDone;

	private int points = 0, maxPoints = 0;

	private JSONNode objectsDatabase;
	private List<int> solved = new List<int>();
	private List<int> activated = new List<int>();
	private List<int> instantiated = new List<int>();
	private Dictionary<int,JSONNode> quests = new Dictionary<int,JSONNode>();
	private Dictionary<int,GameObject> questElements = new Dictionary<int,GameObject>();

	private bool detailWindowState;

	private CollectObject currentObject;

	void Start() {
		//Load JSON data
		//TextAsset jsonFile = (TextAsset)Resources.Load ("ObjectsDatabase");
		//objectsDatabase = JSON.Parse (jsonFile.text);
		ServerConnection.Instance.LoadQuestGameData ();
	}

	public void LoadJsonDatabase(string jsonString) {
		objectsDatabase = JSON.Parse (jsonString);
		Debug.Log ("Database version '" + objectsDatabase ["version"].Value + "' loaded");
		ServerConnection.Instance.RetrieveGameData();
	}

	public IEnumerator RefreshStatus() {
		foreach (KeyValuePair<int, JSONNode> q in quests) {
			if(CheckRequirements(q.Value["id"].AsInt,q.Value["requires"].Value)) {
				AddObject(q.Value["id"].AsInt);
				UpdateQuestStatus(q.Value["id"].AsInt, q.Value["name"].Value, q.Value["type"].AsInt, false);
			}
		}
		return null;
	}

	private void InitializeQuests() {
		for (int i = 0; i<objectsDatabase["elements"].AsInt; i++) {
			quests.Add(objectsDatabase["objects"][i]["id"].AsInt, objectsDatabase["objects"][i]);
			questElements.Add(objectsDatabase["objects"][i]["id"].AsInt, GameController.Instance.AddQuest());
			maxPoints += objectsDatabase["objects"][i]["points"].AsInt;
		}
		GameController.Instance.SetMaxPoints (maxPoints);
	}

	private void UpdateQuestStatus(int id, string name, int type, bool active) {
		GameObject questElement;
		if (questElements.TryGetValue (id, out questElement)) {
			//Icon
			Image icon = questElement.transform.GetChild(0).GetChild(0).GetComponent<Image>();
			switch(type) {
				case -1: icon.sprite = questDone; break;
				case 0: icon.sprite = questType0; break;
				case 1: icon.sprite = questType1; break;
				case 2: icon.sprite = questType2; break;
				case 3: icon.sprite = questType3; break;
			}
			//Name
			Text text = questElement.transform.GetChild(1).GetComponent<Text>();
			text.text = name;
			//Button
			GameObject button = questElement.transform.GetChild(2).gameObject;
			button.SetActive(active);
			if(active) {
				Button script = button.GetComponent<Button>();
				script.onClick.AddListener(delegate {
					OpenDetailWindow(id);
				});
			}
		}
	}

	private bool IsSolved(int questId) {
		if (solved.IndexOf (questId) != -1) {
			return true;
		}
		return false;
	}

	private bool IsActivated(int questId) {
		if (activated.IndexOf (questId) != -1) {
			return true;
		}
		return false;
	}

	private bool IsInstantiated(int questId) {
		if (instantiated.IndexOf (questId) != -1) {
			return true;
		}
		return false;
	}

	private bool CheckRequirements(int id, string requires) {
		//Debug.Log ("Checking "+id+": "+requires);
		if (IsSolved (id)) {
			return false;
		}
		if (requires == "") {
			return true;
		}
		if (!requires.Contains (",")) {
			if(solved.IndexOf(Int32.Parse(requires))!=-1) return true;
			return false;
		}
		string[] steps = requires.Split (',');
		bool result = true;
		foreach(string s in steps) {
			if(solved.IndexOf(Int32.Parse(s))==-1) result = false;
		}
		return result;
	}

	public void AddObject(int objectId) {
		//Cancel if object already present
		if (IsInstantiated (objectId)) return;

		JSONNode obj = GetObject (objectId);
		if (obj != null) {
			Debug.Log ("Instantiate object!");
			instantiated.Add (objectId);

			//Object on map
			UnityEngine.Object marequiressource = Resources.Load ("MapObject");
			GameObject mapObject = (GameObject)GameObject.Instantiate (marequiressource);
			//mapObject.transform.parent = mapTransform;
			SetGeolocation location = mapObject.AddComponent<SetGeolocation> ();
			location.lat = obj ["lat"].AsFloat;
			location.lon = obj ["lon"].AsFloat;
			location.scaleX = obj ["scale"].AsFloat / 10f;
			location.scaleY = obj ["scale"].AsFloat / 10f;
			location.scaleZ = obj ["scale"].AsFloat / 10f;
			location.height = 10000;

			//Object in world
			UnityEngine.Object worldResource = Resources.Load ("Objects/"+obj["object"]);
			GameObject worldObject = (GameObject) GameObject.Instantiate(worldResource);
			worldObject.transform.position = new Vector3(0f, 1000f, 0f);
			CollectObject script = worldObject.AddComponent<CollectObject>();
			script.mapObject = mapObject;
			script.objectId = obj["id"].AsInt;

			//Set transform parent
			mapObject.transform.SetParent(objectsContainer.transform);
			worldObject.transform.SetParent(objectsContainer.transform);

		}


	}

	private JSONNode GetObject(int id) {
		for(int i=0; i<objectsDatabase["elements"].AsInt; i++) {
			if(objectsDatabase["objects"][i]["id"].AsInt==id) {
				return objectsDatabase["objects"][i];
			}
		}
		return null;
	}

	private JSONNode GetQuest(int id) {
		JSONNode q;
		if(quests.TryGetValue (id, out q)) return q;
		return null;
	}

	public void ObjectActivated(CollectObject obj) {

		currentObject = obj;
		if (!IsActivated (obj.objectId)) QuestActivated (obj.objectId);
		JSONNode quest = GetQuest (obj.objectId);

		//Quest
		if(quest != null && !solved.Contains(obj.objectId)) {

			//Audio
			if(quest["type"].AsInt == 2 && quest["media"].Value != "") {
				ServerConnection.Instance.StreamAudioFromURL(quest["media"].Value);
			}

			//Video
			//Handheld.PlayFullScreenMovie ("Riddle.mp4", Color.black, FullScreenMovieControlMode.Minimal);

			//No answer needed -> Solved!
			if(quest["answer"].Value == "") {
				QuestSolved(obj.objectId, quest["points"].AsInt);
				detailWindow.SetActive(true);
				answerForm.SetActive(false);
				showOnMapButton.SetActive(false);
				detailText.text = quest["pre"].Value;
				detailName.text = quest["name"].Value;
			}
			//Answer needed
			else {

				ActivateDetailWindow(quest);

			}
		}
	}

	private IEnumerator DeferredDestroy() {
		yield return new WaitForSeconds (0.1f);
		currentObject.DestroyItems ();
	}

	private void QuestSolved(int questId, int gainedPoints) {
		solved.Add (questId);
		points += gainedPoints;
		Debug.Log ("Passed: " + questId);
		StartCoroutine("RefreshStatus");
		//Set done icon
		GameObject questElement;
		if (questElements.TryGetValue (questId, out questElement)) {
			Image icon = questElement.transform.GetChild(0).GetChild(0).GetComponent<Image>();
			icon.sprite = questDone;
		}
		if(points >= maxPoints) GameController.Instance.PlayAudio ("Victory1");
		else GameController.Instance.PlayAudio ("Color");
		GameController.Instance.SetPoints(points);

		//Update server + upload screenshot
		ServerConnection.Instance.NextStep (questId, gainedPoints);

		StartCoroutine (DeferredDestroy());
	}

	public void OpenDetailWindow(int id) {
		GameController.Instance.CloseQuestWindow ();
		detailWindow.SetActive (true);
		showOnMapButton.SetActive (false);
		JSONNode quest = GetQuest (id);
		if (IsSolved (id)) {
			if(quest["post"].Value != "") {
				detailText.text = quest["post"].Value;
			}
			else {
				detailText.text = quest["pre"].Value;
			}
		} else {
			showOnMapButton.SetActive (true);
			Button script = showOnMapButton.GetComponent<Button>();
			script.onClick.RemoveAllListeners();
			script.onClick.AddListener(delegate {
				GameController.Instance.ShowPointOnMap(quest["lat"].AsFloat, quest["lon"].AsFloat);
			});
			detailText.text = quest["pre"].Value + (quest["answer"].Value!=""?"\n>> Go back to the portal associated to this quest to insert your answer.":"");
		}
		answerForm.SetActive (false);
		GameController.Instance.PlayAudio ("Save");
		detailWindowState = true;
	}

	private void QuestActivated (int questId) {
		activated.Add (questId);
		GameObject questElement;
		if (questElements.TryGetValue (questId, out questElement)) {
			GameObject button = questElement.transform.GetChild(2).gameObject;
			button.SetActive(true);
			Button script = button.GetComponent<Button>();
			script.onClick.AddListener(delegate {
				OpenDetailWindow(questId);
			});
		}
	}

	public void ActivateDetailWindow(JSONNode quest) {
		detailWindow.SetActive (true);
		showOnMapButton.SetActive (false);

		detailName.text = quest["name"].Value;
		detailText.text = quest["pre"].Value;
		if (quest ["answer"] != "") {
			answerForm.SetActive (true);
		} else {
			answerForm.SetActive (false);
		}
		answerButton.onClick.RemoveAllListeners ();
		answerButton.onClick.AddListener (delegate {
			TrySolution (quest["id"].AsInt);
		});
		detailWindowState = false;
	}

	public void TrySolution(int questId) {
		if (!IsSolved (questId)) {
			JSONNode quest = GetQuest (questId);
			string[] solutions = quest ["answer"].Value.Split(',');
			bool correctAnswer = false;
			for(int i=0; i<solutions.Length; i++) {
				if(solutions[i]!="" && solutions[i].ToLower().Equals(answer.text.ToLower())) {
					correctAnswer = true;
					break;
				}
			}
			if (!correctAnswer) {
				GameController.Instance.PlayAudio ("Cancel2");
			} else {
				answer.text = "";
				if (quest ["post"].Value != "") {
					answerForm.SetActive (false);
					detailText.text = quest ["post"].Value;
				} else {
					CloseDetailWindow ();
				}
				QuestSolved (questId, quest ["points"].AsInt);
			}
		}
	}
	
	public void CloseDetailWindow() {
		if(detailWindowState) GameController.Instance.OpenQuestWindow ();
		detailWindow.SetActive (false);
		GameController.Instance.PlayAudio ("Cancel1");
	}

	public int GetPoints() {
		return points;
	}

	public void LoadGame(string gameData) {
		if (gameData != "") {
			JSONNode data = JSON.Parse (gameData);
			points = data ["points"].AsInt;
			GameController.Instance.SetPoints (points);
			if(data["solved"].Value != "") {
				if(!data["solved"].Value.Contains(",")) {
					solved.Add (Int32.Parse (data["solved"].Value));
					activated.Add (Int32.Parse (data["solved"].Value));
				}
				else {
					string[] steps = data ["solved"].Value.Split (',');
					foreach (string s in steps) {
						solved.Add (Int32.Parse (s));
						activated.Add (Int32.Parse (s));
					}
				}
			}
		}
		InitializeQuests ();
		RestorePreviousQuests ();
	}

	private void RestorePreviousQuests() {
		foreach (int questId in solved) {
			JSONNode q = GetQuest(questId);
			UpdateQuestStatus(q["id"].AsInt, q["name"].Value, -1, true);
		}
	}

	public void ResetData() {
		solved.Clear ();
		activated.Clear ();
		quests.Clear ();
		questElements.Clear ();
		instantiated.Clear ();
		GameController.Instance.ResetListElements ();
		for (int i=0; i<objectsContainer.transform.childCount; i++) {
			Destroy (objectsContainer.transform.GetChild(i).gameObject);
		}
		points = 0;
		maxPoints = 0;
		GameController.Instance.SetPoints (0);
		InitializeQuests ();
		RefreshStatus ();
		ServerConnection.Instance.ResetQuestData ();
		GameController.Instance.CloseSettingsWindow ();
	}

}
