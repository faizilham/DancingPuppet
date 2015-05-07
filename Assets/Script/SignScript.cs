using UnityEngine;
using System.Collections;

public class SignScript : MonoBehaviour {
	float lifetime = 1f;
	// Use this for initialization
	Vector3 to;
	void Start () {
		Destroy (gameObject, lifetime);
		to = transform.position + new Vector3 (0, 1f);

	}
	
	// Update is called once per frame
	void Update () {
		//transform.position += new Vector3 (0, Time.deltaTime * 3);
		transform.position = Vector3.Lerp (transform.position, to, Time.deltaTime);		
	}
}
