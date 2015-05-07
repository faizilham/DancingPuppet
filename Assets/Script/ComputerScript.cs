using UnityEngine;
using System.Collections;

public class ComputerScript : MonoBehaviour {

	// Use this for initialization
	private Transform robotMouth;
	private bool mouthOpened;

	private const float openSize = 0.05f, closeSize = 0.01f;

	void Start () {
		mouthOpened = false;
		foreach (Transform child in transform) {
			if (child.name == "RobotMouth")
				robotMouth = child;
		}
	}

	void Update(){

	}

	float ry = 30;
	public void yawRight(){
		transform.rotation = Quaternion.Euler (new Vector3 (0, -ry, 0));
	}
	
	public void yawLeft(){
		transform.rotation = Quaternion.Euler (new Vector3 (0, ry, 0));
	}

	public void yawNorm(){
		transform.rotation = Quaternion.Euler (new Vector3 (0, 0, 0));
	}

	public void openMouth(){
		if (!mouthOpened) {
			robotMouth.localScale = new Vector3(openSize, robotMouth.localScale.y, robotMouth.localScale.z);
			mouthOpened = true;
		}
	}

	public void closeMouth(){
		if (mouthOpened) {
			robotMouth.localScale = new Vector3(closeSize, robotMouth.localScale.y, robotMouth.localScale.z);
			mouthOpened = false;
		}
	}

	public bool isMouthOpen(){
		return mouthOpened;
	}
}
