using UnityEngine;
using UnityEngine.SceneManagement;

///<summary>
/// Controls Main-UI Canvas
///</summary>
public class MainUIService : MonoBehaviour
{
    [SerializeField] private GameObject playButton;

    [SerializeField] private GameObject trainButton;

    [SerializeField] private GameObject historyButton;

    void Start()
    {
        setButton(playButton, "TrackAndGameSelectionUI");
        setButton(trainButton, "PracticeModeSelectionUI");
        setButton(historyButton, "HistoryUI");
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
