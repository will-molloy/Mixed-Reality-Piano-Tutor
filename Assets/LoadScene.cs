using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour {

	public string sceneToLoad;

	void Start () {
		Debug.Log("Loading scene: " + sceneToLoad);
		SceneManager.LoadScene(sceneToLoad);	
	}
	
}
