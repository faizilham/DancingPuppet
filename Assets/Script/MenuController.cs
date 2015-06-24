using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if (MainMenuMusicScript.instance != null && MainMenuMusicScript.instance.music != null && !MainMenuMusicScript.instance.music.isPlaying)
			MainMenuMusicScript.instance.music.Play ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void PlayGame(){
		MainMenuMusicScript.instance.music.Stop ();
		Application.LoadLevel ("main");
	}

	public void Calibration(){
		Application.LoadLevel ("calibrate");
	}

	public void ExitGame(){
		Application.Quit ();
	}
}
