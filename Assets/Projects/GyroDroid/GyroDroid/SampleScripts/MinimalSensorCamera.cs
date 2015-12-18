// prefrontal cortex -- http://prefrontalcortex.de
// Full Android Sensor Access for Unity3D
// Contact:
// 		contact@prefrontalcortex.de

using UnityEngine;
using System.Collections;

public class MinimalSensorCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
		// you can use the API directly:
		// Sensor.Activate(Sensor.Type.RotationVector);
		
		// or you can use the SensorHelper, which has built-in fallback to less accurate but more common sensors:
		SensorHelper.ActivateRotation();

		useGUILayout = false;
	}
	
	// Update is called once per frame
	void Update () {
		// direct Sensor usage:
		#if UNITY_IOS
			transform.rotation = Input.gyro.attitude;
			transform.Rotate( 0f, 0f, 180f, Space.Self ); // Swap "handedness" of quaternion from gyro.
			transform.Rotate( 90f, 180f, 0f, Space.World );
		#else
			// transform.rotation = Sensor.rotationQuaternion; --- is the same as Sensor.QuaternionFromRotationVector(Sensor.rotationVector);
		
			// Helper with fallback:
			#if (!UNITY_EDITOR)
				transform.rotation = SensorHelper.rotation;
			#endif
		#endif
	}
}