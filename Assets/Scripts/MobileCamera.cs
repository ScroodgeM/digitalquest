using UnityEngine;
using System.Collections;
using OpenCVForUnity;

public class MobileCamera : MonoBehaviour {

	public Camera camera;
	public bool openCv = false;
	private WebCamTexture cameraTexture;

	//OpenCV
	Mat rgbaMat;
	Color32[] colors;

	// Use this for initialization
	void Start () {

		float quadHeight = camera.orthographicSize * 2.0f;
		float quadWidth = quadHeight * Screen.width / Screen.height;
		transform.localScale = new Vector3(quadWidth, quadHeight, 1f);

		WebCamDevice[] devices = WebCamTexture.devices;
		string backCamName="";
		for( int i = 0 ; i < devices.Length ; i++ ) {
			Debug.Log("Device:"+devices[i].name+ "IS FRONT FACING:"+devices[i].isFrontFacing);

			#if UNITY_EDITOR
				backCamName = devices[i].name;
			#else
				if (!devices[i].isFrontFacing) {
					backCamName = devices[i].name;
				}
			#endif
		}

		//cameraTexture = new WebCamTexture(backCamName,960,540,15);
		cameraTexture = new WebCamTexture(backCamName,480,270,10);
		cameraTexture.Play();

		GetComponent<Renderer>().material.mainTexture = cameraTexture;

	}

	void LateUpdate() {

		if (openCv) {
			#if UNITY_IOS && !UNITY_EDITOR && (UNITY_4_6_3 || UNITY_4_6_4 || UNITY_5_0_0 || UNITY_5_0_1)
			if (webCamTexture.width > 16 && webCamTexture.height > 16) {
			#else
			if (cameraTexture.didUpdateThisFrame) {
				#endif

				colors = new Color32[cameraTexture.width * cameraTexture.height];
				rgbaMat = new Mat (cameraTexture.height, cameraTexture.width, CvType.CV_8UC3);
				Texture2D texture = new Texture2D (cameraTexture.width, cameraTexture.height, TextureFormat.RGBA32, false);

				Utils.webCamTextureToMat (cameraTexture, rgbaMat, colors);
				
				
				Mat grayMat = new Mat ();
				Imgproc.cvtColor (rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);
				

				Utils.matToTexture2D (grayMat, texture, colors);
				GetComponent<Renderer> ().material.mainTexture = texture;

			}

		}
	}

}
