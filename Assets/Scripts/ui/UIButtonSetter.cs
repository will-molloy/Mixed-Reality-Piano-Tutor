using UnityEngine;
using UnityEngine.SceneManagement;

///<summary>
/// Match buttons and scenes to load for UI
///</summary>
public class UIButtonSetter : MonoBehaviour
{
    [SerializeField] private GameObject[] buttons;

    [SerializeField] private string[] sceneNames;

    void Start()
    {
        if (buttons.Length != sceneNames.Length)
        {
            Debug.LogError("Buttons don't match scene names");
            return;
        }
        for (int i = 0; i < buttons.Length; i++)
        {
            setButton(buttons[i], sceneNames[i]);
        }
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
