using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace User_Interface
{
    /// <summary>
    ///     - Match buttons and scenes to load for UI
    ///     - E.g. button[0] loads scene with name sceneName[0]
    /// </summary>
    public class UIButtonSetter : MonoBehaviour
    {
        [SerializeField] private GameObject[] buttons;

        [SerializeField] private string[] sceneNames;

        private void Start()
        {
            if (buttons.Length != sceneNames.Length)
            {
                Debug.LogError("Buttons don't match scene names");
                return;
            }

            for (var i = 0; i < buttons.Length; i++) setButton(buttons[i], sceneNames[i]);
        }

        private void setButton(GameObject buttonObj, string sceneToload)
        {
            var button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(delegate { buttonEvent(sceneToload); });
        }

        private void buttonEvent(string sceneName)
        {
            Debug.Log("loading: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
    }
}