using UnityEngine;
using UnityEngine.SceneManagement;

///<summary>
/// Controls Main-UI Canvas
///</summary>
public class MainUiService : MonoBehaviour
{

    void Start()
    {
        setButton("MIDISelection", 1);
        setButton("Calibration", 2);
    }

    private void setButton(string sceneToload, int childIndex)
    {
        var buttonTransform = this.transform.GetChild(childIndex).transform;
        var buttonScript = buttonTransform.GetComponent<UnityEngine.UI.Button>();
        buttonScript.onClick.AddListener(delegate { buttonEvent(sceneToload); });
    }

    private void buttonEvent(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}
