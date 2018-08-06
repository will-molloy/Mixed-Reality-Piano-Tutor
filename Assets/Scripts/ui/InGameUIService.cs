using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameUIService : MonoBehaviour
{
    [SerializeField] private GameObject quitButton;

    [SerializeField] private GameObject zed;

    void Start()
    {
        setButton(quitButton, "MainUI");
    }

    private void setButton(GameObject buttonObj, string sceneToload)
    {
        var button = buttonObj.GetComponent<UnityEngine.UI.Button>();
        button.onClick.AddListener(delegate { buttonEvent(sceneToload); });
    }

    private void buttonEvent(string sceneName)
    {
        Debug.Log("Killing ZED");
        DestroyImmediate(zed);
        Debug.Log("loading: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

}
