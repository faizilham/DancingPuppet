using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

	public class Status{
		public bool mouthOpen;
		public int yaw; //1 left 0 norm -1 right
		public float tstamp;
		public Status(bool open, int y, float tm){
			mouthOpen = open; yaw = y; tstamp = tm;
		}

		public void set(bool open, int y, float tm){
			mouthOpen = open; yaw = y; tstamp = tm;
		}
	}

	private MouthBehavior mouth;

	Status status;
	float basetime;
	// Use this for initialization
	void Start () {
		foreach (Transform child in transform){
			if (child.name == "Mouth"){
				mouth = child.gameObject.GetComponent<MouthBehavior>();
				break;
			}
		}
	}

	public void SetBaseTime(float time){
		basetime = time;
	}

	public void SetStatus(bool mouth, int yaw){
		float time = Time.time - basetime;
		if (status == null) {
			status = new Status(mouth, yaw, time);
			//Debug.Log("status null: " + yaw + " " + time);
		}else if (status.mouthOpen != mouth || status.yaw != yaw){
			//status.set(mouth, yaw, Time.time - basetime);
			status = new Status(mouth, yaw, time);
			//Debug.Log("status: " + yaw +  " " + time);
		}
	}

	public Status GetStatus(){
		Status temp = status; status = null;
		return temp;
	}

	public void openMouth(){
		mouth.openMouth ();
	}

	public void closeMouth(){
		mouth.closeMouth ();
	}

	public bool isMouthOpen(){
		return mouth.isMouthOpen ();
	}

	void Update(){

	}
}
