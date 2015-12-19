using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OpenCVForUnity;
using UnityStandardAssets.ImageEffects;

public class MobileCamera : MonoBehaviour {

	public Camera camera;
	public bool openCv = false;
	private WebCamTexture cameraTexture;
	private Renderer renderer;

	public VignetteAndChromaticAberration cameraEffect;

	//OpenCV
	Mat rgbaMat, grayMat;
	Color32[] colors;

	
	private FaceRecognizer faceRecognizer;
	private MatOfRect faces;
	private CascadeClassifier faceDetector;
	private List<string> faceNames = new List<string>();

	// Use this for initialization
	IEnumerator Start () {

		renderer = GetComponent<Renderer> ();
		cameraEffect.enabled = false;

		float quadHeight = camera.orthographicSize * 2.0f;
		float quadWidth = quadHeight * Screen.width / Screen.height;
		transform.localScale = new Vector3(quadWidth, quadHeight, 1f);

		#if UNITY_IOS
			while(WebCamTexture.devices.Length <= 1) {
				yield return new WaitForSeconds (0.1f);
			}
		#endif

		WebCamDevice[] devices = WebCamTexture.devices;
		Debug.Log (devices.Length + " cameras available");
		if (Application.HasUserAuthorization (UserAuthorization.WebCam)) {
			Debug.Log("Camera user authorization successful");
		}

		string backCamName="";
		for( int i = 0 ; i < devices.Length ; i++ ) {
			Debug.Log("Device:"+devices[i].name+ "IS FRONT FACING:"+devices[i].isFrontFacing);
			/*
			#if UNITY_EDITOR
				backCamName = devices[i].name;
			#else
				if (!devices[i].isFrontFacing) {
					backCamName = devices[i].name;
				}
			#endif
			*/
			if (!devices[i].isFrontFacing) {
				backCamName = devices[i].name;
			}
		}

		//cameraTexture = new WebCamTexture(backCamName,960,540,15);
		cameraTexture = new WebCamTexture(backCamName,480,270,10);
		if (cameraTexture.videoVerticallyMirrored) {
			Debug.Log("Video vertically mirrored!");
		}
		//Mirrored camera on iOS!
		# if UNITY_IOS
				transform.Rotate(0, 0, 180);
		#endif

		cameraTexture.Play();

		renderer.material.mainTexture = cameraTexture;

		//OpenCV
		InitializeClassifiers ();

		yield return null;

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

				#if UNITY_IOS
					Core.flip(rgbaMat,rgbaMat,0);
				#endif
				
				Imgproc.cvtColor (rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);
				
				OpenCVForUnity.Rect[] rects = DetectFaces();
				//If faces detected
				if (rects.Length > 0) {
					int ext = 0;

					Mat faceMat = grayMat.submat(rects[0]);
					Mat scaledFace = new Mat();
					Imgproc.resize(faceMat, scaledFace, new Size(100, 100));

					/*
					//Display face on UI
					Texture2D faceTexture = new Texture2D(faceMat.width(), faceMat.height());
					Color32[] faceColors = new Color32[faceMat.width() * faceMat.height()];
					Utils.matToTexture2D (faceMat, faceTexture, faceColors);
					previewTexture.texture = faceTexture;
					*/
					
					int label = RecognizeFace(scaledFace);


					Core.putText(rgbaMat, faceNames[label], new Point(rects[0].x, rects[0].y - 8), 0, 0.4, new Scalar(0, 255, 255));
				}

				#if UNITY_IOS
					Core.flip(rgbaMat,rgbaMat,0);
				#endif

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

			//Face detector

			faceDetector = new CascadeClassifier (Utils.getFilePath ("lbpcascade_frontalface.xml"));
			faces = new MatOfRect ();

			//Face recognizer
			
			List<Mat> images = new List<Mat> ();
			List<int> labelsList = new List<int> ();
			MatOfInt labels = new MatOfInt ();

			images.Add (Highgui.imread (Utils.getFilePath ("facerec/MarcoCavallo_1.png"), 0));
			images.Add (Highgui.imread (Utils.getFilePath ("facerec/MarcoCavallo_2.png"), 0));
			images.Add (Highgui.imread (Utils.getFilePath ("facerec/MarcoCavallo_3.png"), 0));
			labelsList.Add (0);
			labelsList.Add (0);
			labelsList.Add (0);
			faceNames.Add ("Marco Cavallo");

			images.Add (Highgui.imread (Utils.getFilePath ("facerec/AndreaRottigni_1.png"), 0));
			images.Add (Highgui.imread (Utils.getFilePath ("facerec/AndreaRottigni_2.png"), 0));
			images.Add (Highgui.imread (Utils.getFilePath ("facerec/AndreaRottigni_3.png"), 0));
			labelsList.Add (1);
			labelsList.Add (1);
			labelsList.Add (1);
			faceNames.Add ("Andrea Rottigni");

			images.Add (Highgui.imread (Utils.getFilePath ("facerec/FilippoPellolio_1.png"), 0));
			images.Add (Highgui.imread (Utils.getFilePath ("facerec/FilippoPellolio_2.png"), 0));
			images.Add (Highgui.imread (Utils.getFilePath ("facerec/FilippoPellolio_3.png"), 0));
			labelsList.Add (2);
			labelsList.Add (2);
			labelsList.Add (2);
			faceNames.Add ("Filippo Pellolio");



			labels.fromList (labelsList);
			
			faceRecognizer = FaceRecognizer.createEigenFaceRecognizer ();
			
			faceRecognizer.train (images, labels);
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

	private int RecognizeFace(Mat faceMat) {
		int[] predictedLabel = new int[1];
		double[] predictedConfidence = new double[1];
		
		faceRecognizer.predict (faceMat, predictedLabel, predictedConfidence);
		
		Debug.Log ("Predicted class: <" + faceNames[predictedLabel[0]] + "> with confidence: " + predictedConfidence [0]);

		return predictedLabel [0];
	}

}
