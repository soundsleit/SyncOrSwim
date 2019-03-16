using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class grooveImageShift : MonoBehaviour {

	float now=0;
	float lastTime = 0;
	int counter = 0;

	public GameObject GameManager;

	public Sprite[] SyncFaceWinner;


	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {

		//move time
		now += Time.deltaTime;

		if ((now - lastTime) > GameManager.GetComponent<GameManager>().TempoBase/1000F) {
			lastTime = now;
			counter++;
			StartCoroutine(LightShift());
		}
	}

	IEnumerator LightShift () {

		this.GetComponent<Image> ().sprite = SyncFaceWinner [counter%4];
		yield return new WaitForSeconds (GameManager.GetComponent<GameManager>().TempoBase/1000F);
	}
}