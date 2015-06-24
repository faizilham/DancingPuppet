using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CalibrateController : MonoBehaviour {
	public Slider headSlider;
	public Slider mouthSlider;
	// Use this for initialization
	void Start () {
		headSlider.value = PuppetBehavior.yawThreshold;
		mouthSlider.value = PuppetBehavior.mouthThreshold;
	}

	public void SetHeadThreshold(float yaw){
		PuppetBehavior.yawThreshold = yaw;
	}

	public void SetMouthThreshold(float mouth){
		PuppetBehavior.mouthThreshold = mouth;
	}

	public void MainMenu(){
		Application.LoadLevel ("menu");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
