using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUIButtonSender : MonoBehaviour
{

    [SerializeField] private GameObject playModeButton;

    [SerializeField] private GameObject practiceModeButton;

    [SerializeField] private string PlayModeMidiDir;

    [SerializeField] private string PracticeModeMidiDir;

	[SerializeField] private string PlayModeSessions;

    [SerializeField] private string PracticeModeSessions;

    void Start()
    {
        playModeButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { playModeMessage(); });
        practiceModeButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { practiceModeMessage(); });
    }

    private void playModeMessage()
    {
		Debug.Log("Setting up play mode");
        RuntimeSettings.MIDI_DIR = PlayModeMidiDir;
		RuntimeSettings.MIDI_SESSIONS_PATH = PlayModeSessions;
        RuntimeSettings.IS_PLAY_MODE = true;
    }

    private void practiceModeMessage()
    {
		Debug.Log("Setting up practice mode");
        RuntimeSettings.MIDI_DIR = PracticeModeMidiDir;
		RuntimeSettings.MIDI_SESSIONS_PATH = PracticeModeSessions;
        RuntimeSettings.IS_PLAY_MODE = false;
    }

}
