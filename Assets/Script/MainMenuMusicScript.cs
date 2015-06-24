using UnityEngine;
using System.Collections;

public class MainMenuMusicScript : MonoBehaviour {

	public static MainMenuMusicScript instance = null;

	void Awake() {
		if (instance != null && instance != this) {
			Destroy(this.gameObject);
			return;
		} else {
			instance = this;
		}
		DontDestroyOnLoad(this.gameObject);
	}

	public AudioSource music;
	// Use this for initialization
	void Start () {
		music = GetComponent<AudioSource> ();
		music.Play ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}


