using UnityEngine;
using System.Collections;

public class PuppetBehavior : MonoBehaviour {
	public FaceInput input;
	private PXCMSizeI32 imgSize;
	private PlayerScript player;
	// Use this for initialization
	void Start () {
		imgSize.width = imgSize.height = 0;
		input.OnFaceData += OnFaceData;
		input.OnColorImage += OnColorImage;
		player = this.GetComponent<PlayerScript> ();
	}

	public float yawThreshold;
	public float mouthThreshold;
	public float rollThreshold;


	float lastYawFilter = 0, lastMouthOpenFilter = 0;
	public float alpha;
	float yrotation = 30;

	void OnFaceData(PXCMFaceData.PoseEulerAngles headAngles, float mouthOpen){
		float currentYawFilter = 0, currentMouthOpenFilter, ry = 0; int yw = 0;

		mouthOpen = mouthOpen / imgSize.height * 100;

		//filtering, moving average

		currentMouthOpenFilter = alpha * mouthOpen + (1 - alpha) * lastMouthOpenFilter;

		if (imgSize.width == 0 || imgSize.height == 0) return;

		if (headAngles != null) {
			currentYawFilter = alpha * headAngles.yaw + (1 - alpha) * lastYawFilter;
			//Debug.Log(currentYawFilter);
			float rx = headAngles.pitch < -rollThreshold ? -30 : 0;

			if (currentYawFilter < -yawThreshold) {
				ry = yrotation;
				yw = 1;
			} else if (currentYawFilter > yawThreshold) {
				ry = -yrotation;
				yw = -1;
			} else {
				ry = 0;
				yw = 0;
			}

			transform.rotation = Quaternion.Euler (new Vector3 (rx, ry, 0));
		}

		if (currentMouthOpenFilter > mouthThreshold) {
			player.openMouth();
		}else{
			player.closeMouth();
		}

		player.SetStatus (player.isMouthOpen (), yw);

		lastYawFilter = currentYawFilter;
		lastMouthOpenFilter = currentMouthOpenFilter;

		//Debug.Log ("PYR: " + headAngles.pitch + " " + headAngles.yaw + " " + headAngles.roll);
	}
	
	void OnColorImage(PXCMImage data){
		imgSize.width = data.info.width;
		imgSize.height = data.info.height;
	}


	// Update is called once per frame
	void Update () {
	
	}
}
