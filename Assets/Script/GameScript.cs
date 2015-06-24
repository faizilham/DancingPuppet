using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameScript : MonoBehaviour {
	public ComputerScript computer;
	public PlayerScript player;
	public Light playerLight, computerLight, mainLight;
	public GameObject missSign, goodSign;
	public GameObject endGameDialogPrefab;
	public GameObject loadingScreen;
	public GameObject canvas;
	public Renderer repeatAfter;
	public Transform signPos;

	AudioSource music;
	float basetime, tmr;

	int currentAction = 0;	
	List<float> timing = new List<float>();
	List<string> code = new List<string>();

	bool playerTurn = false;
	float glowfactor = 0.5f;

	int goodScore, missScore;

	enum GameState {LOAD, INIT, FADEIN, PLAYING, FADEOUT, END};
	GameState state = GameState.LOAD;

	void Start () {
		// read rythm data
		TextAsset mydata = Resources.Load("Rhythm/beats6") as TextAsset;
		StringReader reader = new StringReader (mydata.text);
		string line;
		while ((line = reader.ReadLine()) != null) {
			string[] token = line.Split(" "[0]);
			timing.Add(float.Parse(token[0]));
			code.Add(token[1].Trim());
		}
		repeatAfter.enabled = false;
		computerLight.enabled = false; playerLight.enabled = false;
	}

	public void MainMenu(){
		Application.LoadLevel ("menu");
	}

	public void GameStart(){
		state = GameState.INIT;
		tmr = Time.time + 2;
		repeatAfter.enabled = true;
		//computerLight.enabled = true;
		loadingScreen.SetActive (false);
		goodScore = 0; missScore = 0;

	}

	void StartPlaying(){
		music = GetComponent<AudioSource> ();
		basetime = Time.time; player.SetBaseTime (basetime);
		
		music.Play ();
		state = GameState.PLAYING;
	}

	void notifyOk(){
		//Debug.Log("ok");
		Instantiate (goodSign, signPos.position, Quaternion.identity);
		goodScore += 1;
	}

	void notifyMiss(int yaw){
		//Debug.Log ("miss " + yaw);
		Instantiate (missSign, signPos.position, Quaternion.identity);
		missScore += 1;
	}
	private int lastyaw = -99;
	bool checkPlayer(string action){
		bool mouthOpen = false; int yaw = 0;

		switch (action) {
		case "r":
			yaw = -1;
			break;
		case "l": 
			yaw = 1;
			break;
		case "n": 
			yaw = 0;
			break;
		case "ro":
			yaw = -1;
			mouthOpen = true;
			break;
		case "lo": 
			yaw = 1;
			mouthOpen = true;
			break;
		case "no": 
			yaw = 0;
			mouthOpen = true;
			break;
		case "comp":
		case "play":
		case "end":
			return false;
		break;
		}
		
		PlayerScript.Status stat = player.GetStatus ();

		if (stat == null) {
			return false;
		} else {

			if (timing[currentAction] - stat.tstamp > 1) return false;
			lastyaw = stat.yaw;
			//Debug.Log (yaw + " " + timing[currentAction]);
			if (stat.yaw == yaw && stat.mouthOpen == mouthOpen){
				notifyOk();
				return true;
			}else{
				//notifyMiss(stat.yaw);
				return false;
			}
			return true;
		}
	}

	void executeCom(string action){
		switch (action) {
			case "r":
				computer.yawRight();
				computer.closeMouth();
			break;
			case "l": 
				computer.yawLeft();
				computer.closeMouth();
			break;
			case "n": 
				computer.yawNorm();
				computer.closeMouth();
			break;
			case "ro":
				computer.yawRight();
				computer.openMouth();
				break;
			case "lo": 
				computer.yawLeft();
				computer.openMouth();
				break;
			case "no": 
				computer.yawNorm();
				computer.openMouth();
				break;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (state == GameState.PLAYING){
			float diff;
			while (currentAction < timing.Count) {

				diff = (Time.time - basetime) - timing[currentAction];
				if (code[currentAction] == "comp" && diff > 0f){
					computerLight.enabled = true; playerLight.enabled = false; playerTurn = false;
					currentAction++;
				}else if (code[currentAction] == "play" && diff > 0f){
					computerLight.enabled = false; playerLight.enabled = true; playerTurn = true;
					computer.closeMouth();
					currentAction++;
				}else if (code[currentAction] == "end" && diff > 0f){
					computerLight.enabled = false; playerLight.enabled = false; playerTurn = false;
					computer.closeMouth();
					state = GameState.FADEOUT;
					break;
				}else if (!playerTurn && diff > 0f){
					executeCom(code[currentAction++]); 
				}else if (playerTurn){
					if (Mathf.Abs(diff) < 0.15f){
						if (checkPlayer(code[currentAction])){
							currentAction++;
						}else{
							break;
						}
					}else if (diff > 0f && code[currentAction] != "comp" && code[currentAction] != "play" && code[currentAction] != "end"){
						notifyMiss(lastyaw); currentAction++; lastyaw = -99;
					}else{
						break;
					}
				}else{
					break;
				}
			}
		} else if (state == GameState.INIT && Time.time > tmr){
			repeatAfter.enabled = false;
			state = GameState.FADEIN;
		} else if (state == GameState.FADEIN){
			if (mainLight.intensity <0.3f){
				mainLight.intensity = 0.3f;
				StartPlaying();
			} else {
				mainLight.intensity -= Time.deltaTime * glowfactor;
			}
		}else if (state == GameState.FADEOUT){
			if (mainLight.intensity > 1.5f){
				mainLight.intensity = 1.5f;
				state =  GameState.END;
				EndDialogScript dialog = Instantiate(endGameDialogPrefab).GetComponent<EndDialogScript>();
				dialog.SetScore(goodScore, missScore);
				canvas.SetActive (true);
			} else {
				mainLight.intensity += Time.deltaTime * glowfactor;
			}
		}
	}
}
