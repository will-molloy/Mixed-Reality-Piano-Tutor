using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EmptyUIScript : MonoBehaviour {

	[SerializeField] private GameObject backButton;

	void Start () {
		setButton(backButton, "MainUI");
	}
	
    private void setButton(GameObject buttonObj, string sceneToload)
    {
        var button = buttonObj.GetComponent<UnityEngine.UI.Button>();
        button.onClick.AddListener(delegate { buttonEvent(sceneToload); });
    }

    private void buttonEvent(string sceneName)
    {
        Debug.Log("loading: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
	
}
