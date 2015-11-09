using UnityEngine;
using UnityEngine.UI;
using Facebook.Unity;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class Login : MonoBehaviour {

	public GameObject firstPanel, secondPanel;
	public Text successMessage;


	// Use this for initialization
	void Start () {
		if (PlayerPrefs.HasKey ("UserId")) {
			Application.LoadLevel("Opening");
		} else {
			FB.Init ();
			firstPanel.SetActive(true);
		}
	}

	public void ShowLogin() {
		FB.ActivateApp();
		FB.LogInWithReadPermissions (
			new List<string>(){"public_profile", "email", "user_friends"}, LoginCallback);
	}

	private void LoginCallback(ILoginResult result) {
		if (FB.IsLoggedIn) {
			AccessToken token = Facebook.Unity.AccessToken.CurrentAccessToken;
			Debug.Log("Login correct with user id " + token.UserId + " and token " + token.TokenString);
			StartCoroutine(VerifyAccount(token.TokenString));
		} else {
			Debug.Log("User cancelled login");
		}
	}

	IEnumerator VerifyAccount (string token){
		Debug.Log ("http://www.mastercava.it/digitalquest/signup.php?t=" + token);
		WWW www = new WWW ("http://www.mastercava.it/digitalquest/signup.php?t=" + token);
		while (!www.isDone)
			yield return null;
		Debug.Log (www.text);
		firstPanel.SetActive (false);

		JSONNode user = JSON.Parse (www.text);
		PlayerPrefs.SetInt ("UserId", user["id"].AsInt);
		PlayerPrefs.SetString ("UserName", user["name"].Value);
		successMessage.text = "Welcome to Digital Quest, " + user["name"].Value + "! Have fun with this incredible game :D!!";

		secondPanel.SetActive (true);
	}

	public void StartGame() {
		Application.LoadLevel ("Opening");
	}
}
