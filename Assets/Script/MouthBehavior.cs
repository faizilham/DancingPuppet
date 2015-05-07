using UnityEngine;
using System.Collections;

public class MouthBehavior : MonoBehaviour {

	public float angleSize;
	private Transform upperBeak, lowerBeak;
	private bool mouthOpened;

	private Vector3 rotateUp, rotateDown;
	// Use this for initialization
	void Start () {
		foreach (Transform child in transform) {
			if (child.name == "UpperBeak")
				upperBeak = child;
			else if (child.name == "LowerBeak")
				lowerBeak = child;
		}

		rotateUp = new Vector3 (angleSize, 0, 0);
		rotateDown = new Vector3 (-angleSize, 0, 0);
		mouthOpened = false;
	}
	
	public void openMouth(){
		if (!mouthOpened) {
			upperBeak.transform.Rotate(rotateUp);
			lowerBeak.transform.Rotate(rotateDown);
			mouthOpened = true;
		}
	}

	
	public void closeMouth(){
		if (mouthOpened) {
			upperBeak.transform.Rotate(rotateDown);
			lowerBeak.transform.Rotate(rotateUp);
			mouthOpened = false;
		}
	}

	public bool isMouthOpen(){
		return mouthOpened;
	}
}
