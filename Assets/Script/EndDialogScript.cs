using UnityEngine;
using System.Collections;

public class EndDialogScript : MonoBehaviour {
	TextMesh goodtext = null, misstext = null;
	// Use this for initialization
	void Start () {

	}

	public void SetScore(int good, int miss){
		if (goodtext == null) {
			foreach (Transform child in transform) {
				if (child.name == "GoodScore")
					goodtext = child.gameObject.GetComponent<TextMesh> ();
				else if (child.name == "MissScore")
					misstext = child.gameObject.GetComponent<TextMesh> ();
			}
		}
		goodtext.text = good.ToString();
		misstext.text = miss.ToString();
	}

	// Update is called once per frame
	void Update () {
		// not yet implemented
	}
}
