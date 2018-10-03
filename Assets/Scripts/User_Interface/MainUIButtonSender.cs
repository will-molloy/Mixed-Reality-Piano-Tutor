using UnityEngine;
using UnityEngine.UI;

namespace User_Interface
{
    /// <summary>
    ///     - Sets RunTimeSettings to account for the slight differences in play mode and practice mode MIDI selection UIs
    /// </summary>
    public class MainUIButtonSender : MonoBehaviour
    {
        [SerializeField] private GameObject playModeButton;

        [SerializeField] private string PlayModeMidiDir;

        [SerializeField] private GameObject practiceModeButton;

        [SerializeField] private string PracticeModeMidiDir;

        private void Start()
        {
            playModeButton.GetComponent<Button>().onClick.AddListener(delegate { playModeMessage(); });
            practiceModeButton.GetComponent<Button>().onClick.AddListener(delegate { practiceModeMessage(); });
        }

        private void playModeMessage()
        {
            Debug.Log("Setting up play mode");
            RuntimeSettings.MIDI_DIR = PlayModeMidiDir;
            RuntimeSettings.IS_PLAY_MODE = true;
        }

        private void practiceModeMessage()
        {
            Debug.Log("Setting up practice mode");
            RuntimeSettings.MIDI_DIR = PracticeModeMidiDir;
            RuntimeSettings.IS_PLAY_MODE = false;
        }
    }
}