using UnityEngine;
using System.Collections;
using OpenCVForUnity;

public class MobileCamera : MonoBehaviour {

	public Camera camera;
	public bool openCv = false;
	private WebCamTexture cameraTexture;
	private Renderer renderer;

	public DeferredNightVisionEffect cameraEffect;

	//OpenCV
	Mat rgbaMat, grayMat;
	Color32[] colors;

	
	private FaceRecognizer faceRecognizer;
	private MatOfRect faces;
	private CascadeClassifier faceDetector;

	// Use this for initialization
	void Start () {

		renderer = GetComponent<Renderer> ();

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

		renderer.material.mainTexture = cameraTexture;

		//OpenCV
		InitializeClassifiers ();

	}

	void LateUpdate() {

		if (openCv) {
			#if UNITY_IOS && !UNITY_EDITOR && (UNITY_4_6_3 || UNITY_4_6_4 || UNITY_5_0_0 || UNITY_5_0_1)
			if (webCamTexture.width > 16 && webCamTexture.height > 16) {
			#else
			if (cameraTexture.didUpdateThisFrame) {
				#endif

				colors = new Color32[cameraTexture.width * cameraTexture.height];
				rgbaMat = new Mat (cameraTexture.height, cameraTexture.width, CvType.CV_8UC4);
				grayMat = new Mat (cameraTexture.height, cameraTexture.width, CvType.CV_8UC1);
				Texture2D texture = new Texture2D (cameraTexture.width, cameraTexture.height, TextureFormat.RGBA32, false);

				Utils.webCamTextureToMat (cameraTexture, rgbaMat, colors);
				
				Imgproc.cvtColor (rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);
				
				DetectFaces();

				Utils.matToTexture2D (rgbaMat, texture, colors);
				renderer.material.mainTexture = texture;

			}

		}
	}

	void OnDisable () {
		cameraTexture.Stop ();
	}

	public void SwitchMode() {
		if (!openCv) {
			openCv = true;
			cameraEffect.enabled = true;
			
		} else {
			openCv = false;
			renderer.material.mainTexture = cameraTexture;
			cameraEffect.enabled = false;
		}
	}


	private void InitializeClassifiers() {

			faceDetector = new CascadeClassifier (Utils.getFilePath ("lbpcascade_frontalface.xml"));
			faces = new MatOfRect ();
	}


	private OpenCVForUnity.Rect[] DetectFaces() {
		faceDetector.detectMultiScale (grayMat, faces, 1.1, 2, 2, // TODO: objdetect.CV_HAAR_SCALE_IMAGE
		                               new Size (cameraTexture.height * 0.2, cameraTexture.height * 0.2), new Size ());
		
		OpenCVForUnity.Rect[] rects = faces.toArray ();
		for (int i = 0; i < rects.Length; i++) {
			//Debug.Log ("detect faces " + rects [i]);
			
			Core.rectangle (rgbaMat, new Point (rects [i].x, rects [i].y), new Point (rects [i].x + rects [i].width, rects [i].y + rects [i].height), new Scalar (255, 255, 0, 255), 2);
		}
		
		return rects;
	}

}
