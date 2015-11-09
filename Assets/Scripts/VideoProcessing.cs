using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using OpenCVForUnity;

namespace OpenCVForUnitySample
{
	/// <summary>
	/// WebCamTexture to mat sample.
	/// </summary>
	public class VideoProcessing : MonoBehaviour
	{

		public Camera camera;
		/// <summary>
		/// The web cam texture.
		/// </summary>
		WebCamTexture webCamTexture;
		
		/// <summary>
		/// The web cam device.
		/// </summary>
		WebCamDevice webCamDevice;
		
		/// <summary>
		/// The colors.
		/// </summary>
		Color32[] colors;
		
		/// <summary>
		/// Should use front facing.
		/// </summary>
		public bool shouldUseFrontFacing = false;
		
		/// <summary>
		/// The width.
		/// </summary>
		int width = 480;
		
		/// <summary>
		/// The height.
		/// </summary>
		int height = 270;
		
		/// <summary>
		/// The rgba mat.
		/// </summary>
		Mat rgbaMat;
		Mat grayMat;
		
		/// <summary>
		/// The texture.
		/// </summary>
		Texture2D texture;
		
		/// <summary>
		/// The init done.
		/// </summary>
		bool initDone = false;
		
		/// <summary>
		/// The screenOrientation.
		/// </summary>
		ScreenOrientation screenOrientation = ScreenOrientation.Unknown;
		
		//Custom variables
		public RawImage previewTexture;
		
		private FaceRecognizer faceRecognizer;
		private MatOfRect faces;
		private CascadeClassifier faceDetector;
		
		
		// Use this for initialization
		void Start ()
		{
			float quadHeight = camera.orthographicSize * 2.0f;
			float quadWidth = quadHeight * Screen.width / Screen.height;
			transform.localScale = new Vector3(quadWidth, quadHeight, 1f);

			StartCoroutine (init ());
			
		}
		
		private IEnumerator init ()
		{
			if (webCamTexture != null) {
				webCamTexture.Stop ();
				initDone = false;
				
				rgbaMat.Dispose ();
				grayMat.Dispose();
			}
			
			// Checks how many and which cameras are available on the device
			for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++) {
				
				
				if (WebCamTexture.devices [cameraIndex].isFrontFacing == shouldUseFrontFacing) {
					
					
					Debug.Log (cameraIndex + " name " + WebCamTexture.devices [cameraIndex].name + " isFrontFacing " + WebCamTexture.devices [cameraIndex].isFrontFacing);
					
					webCamDevice = WebCamTexture.devices [cameraIndex];
					
					webCamTexture = new WebCamTexture (webCamDevice.name, width, height);
					
					break;
				}
				
				
			}
			
			if (webCamTexture == null) {
				webCamDevice = WebCamTexture.devices [0];
				webCamTexture = new WebCamTexture (webCamDevice.name, width, height);
			}
			
			Debug.Log ("width " + webCamTexture.width + " height " + webCamTexture.height + " fps " + webCamTexture.requestedFPS);
			
			
			
			// Starts the camera
			webCamTexture.Play ();
			
			
			while (true) {
				//If you want to use webcamTexture.width and webcamTexture.height on iOS, you have to wait until webcamTexture.didUpdateThisFrame == 1, otherwise these two values will be equal to 16. (http://forum.unity3d.com/threads/webcamtexture-and-error-0x0502.123922/)
				#if UNITY_IOS && !UNITY_EDITOR && (UNITY_4_6_3 || UNITY_4_6_4 || UNITY_5_0_0 || UNITY_5_0_1)
				if (webCamTexture.width > 16 && webCamTexture.height > 16) {
					#else
					if (webCamTexture.didUpdateThisFrame) {
						#if UNITY_IOS && !UNITY_EDITOR && UNITY_5_2                                    
						while (webCamTexture.width <= 16) {
							webCamTexture.GetPixels32 ();
							yield return new WaitForEndOfFrame ();
						} 
						#endif
						#endif
						
						Debug.Log ("width " + webCamTexture.width + " height " + webCamTexture.height + " fps " + webCamTexture.requestedFPS);
						Debug.Log ("videoRotationAngle " + webCamTexture.videoRotationAngle + " videoVerticallyMirrored " + webCamTexture.videoVerticallyMirrored + " isFrongFacing " + webCamDevice.isFrontFacing);
						
						colors = new Color32[webCamTexture.width * webCamTexture.height];
						rgbaMat = new Mat (webCamTexture.height, webCamTexture.width, CvType.CV_8UC4);
						grayMat = new Mat (webCamTexture.height, webCamTexture.width, CvType.CV_8UC1);
						texture = new Texture2D (webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);
						
						gameObject.GetComponent<Renderer> ().material.mainTexture = texture;
						
						//TrainClassifier();
						
						screenOrientation = Screen.orientation;
						initDone = true;
						
						break;
					} else {
						yield return 0;
					}
				}
			}
			
			
			// Update is called once per frame
			void Update ()
			{
				if (!initDone)
					return;
				
				#if UNITY_IOS && !UNITY_EDITOR && (UNITY_4_6_3 || UNITY_4_6_4 || UNITY_5_0_0 || UNITY_5_0_1)
				if (webCamTexture.width > 16 && webCamTexture.height > 16) {
					#else
					if (webCamTexture.didUpdateThisFrame) {
						#endif
						
						Utils.webCamTextureToMat (webCamTexture, rgbaMat, colors);
						
						if (webCamDevice.isFrontFacing) {
							if (webCamTexture.videoRotationAngle == 0) {
								Core.flip (rgbaMat, rgbaMat, 1);
							} else if (webCamTexture.videoRotationAngle == 90) {
								Core.flip (rgbaMat, rgbaMat, 0);
							}
							if (webCamTexture.videoRotationAngle == 180) {
								Core.flip (rgbaMat, rgbaMat, 0);
							} else if (webCamTexture.videoRotationAngle == 270) {
								Core.flip (rgbaMat, rgbaMat, 1);
							}
						} else {
							if (webCamTexture.videoRotationAngle == 180) {
								Core.flip (rgbaMat, rgbaMat, -1);
							} else if (webCamTexture.videoRotationAngle == 270) {
								Core.flip (rgbaMat, rgbaMat, -1);
							}
						}
						
						
						Imgproc.cvtColor (rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);
						Imgproc.equalizeHist (grayMat, grayMat);
						
						//OpenCVForUnity.Rect[] rects = DetectFaces();

						/*
						//If faces detected
						if (rects.Length > 0) {
							int ext = 0;
							OpenCVForUnity.Rect faceRect = new OpenCVForUnity.Rect(new Point(rects[0].x, rects[0].y-ext), new Size(rects[0].width, rects[0].height+ext));
							Mat faceMat = grayMat.submat(faceRect);
							
							//Display face on UI
							Texture2D faceTexture = new Texture2D(faceMat.width(), faceMat.height());
							Color32[] faceColors = new Color32[faceMat.width() * faceMat.height()];
							Utils.matToTexture2D (faceMat, faceTexture, faceColors);
							previewTexture.texture = faceTexture;
							
							RecognizeFace(faceMat);
						}
						
						Utils.matToTexture2D (rgbaMat, texture, colors);
						*/
					}
					
				}
				
				void OnDisable () {
					webCamTexture.Stop ();
				}
				
				
				private OpenCVForUnity.Rect[] DetectFaces() {
					faceDetector.detectMultiScale (grayMat, faces, 1.1, 2, 2, // TODO: objdetect.CV_HAAR_SCALE_IMAGE
					                               new Size (webCamTexture.height * 0.2, webCamTexture.height * 0.2), new Size ());
					
					OpenCVForUnity.Rect[] rects = faces.toArray ();
					for (int i = 0; i < rects.Length; i++) {
						//Debug.Log ("detect faces " + rects [i]);
						
						Core.rectangle (rgbaMat, new Point (rects [i].x, rects [i].y), new Point (rects [i].x + rects [i].width, rects [i].y + rects [i].height), new Scalar (255, 255, 0, 255), 2);
					}
					
					return rects;
				}
				
				
				private void TrainClassifier(){
					
					//Face detector
					
					faceDetector = new CascadeClassifier (Utils.getFilePath ("lbpcascade_frontalface.xml"));
					faces = new MatOfRect ();
					
					//Face recognizer
					
					List<Mat> images = new List<Mat> ();
					List<int> labelsList = new List<int> ();
					MatOfInt labels = new MatOfInt ();
					images.Add (Highgui.imread (Utils.getFilePath ("facerec/marco.bmp"), 0));
					images.Add (Highgui.imread (Utils.getFilePath ("facerec/andrea.bmp"), 0));
					images.Add (Highgui.imread (Utils.getFilePath ("facerec/filippo.bmp"), 0));
					labelsList.Add (0);
					labelsList.Add (1);
					labelsList.Add (2);
					labels.fromList (labelsList);
					
					faceRecognizer = FaceRecognizer.createEigenFaceRecognizer ();
					
					faceRecognizer.train (images, labels);
					
				}
				
				private void RecognizeFace(Mat faceMat) {
					int[] predictedLabel = new int[1];
					double[] predictedConfidence = new double[1];
					
					faceRecognizer.predict (faceMat, predictedLabel, predictedConfidence);
					
					Debug.Log ("Predicted class: " + predictedLabel [0] + " with confidence: " + predictedConfidence [0]);
					
				}
				
				private void SaveTextureToFile(Texture2D texture, string fileName){
					byte[] bytes = texture.EncodeToPNG();
					File.WriteAllBytes(Application.dataPath + "/../" + fileName, bytes);
				}
				
			}
		}