using UnityEngine;
using Facebook.Unity;
using System.Collections;
using System.Collections.Generic;

public class Login : MonoBehaviour {

	// Use this for initialization
	void Start () {
		FB.Init (ShowLogin);
	}

	private void ShowLogin() {
		FB.ActivateApp();
		FB.LogInWithReadPermissions (
			new List<string>(){"public_profile", "email", "user_friends"}, LoginCallback);
	}

	private void LoginCallback(ILoginResult result) {
		//Debug.Log (result.AccessToken);
		Application.LoadLevel ("Opening");

	}

	public void SkipLogin() {
		Application.LoadLevel ("Opening");
	}
}
