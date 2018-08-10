using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

public class PlayModeSelectionUIService : MonoBehaviour
{
    [SerializeField] private GameObject backButton;

    [SerializeField] private string midiFolderPath = "Assets/MIDI";

    [SerializeField] private GameObject rowEntryObj;

    [SerializeField] private Transform scrollViewParent;

    private const double SCORE_TO_PASS = 0.5d;
    private const int TEXT_INDEX = 0;
    private const int NAME_INDEX = 0;
    private const int DIFFICULTY_INDEX = 1;
    private const int BEST_SCORE_INDEX = 2;
    private const int OVERALL_SCORE_INDEX = 3;

    void Start()
    {
        // setButton(playButton, "PlayStandardMode");
        setBackButton(backButton, "MainUI");
        processFolder(midiFolderPath);
    }

    private void setBackButton(GameObject buttonObj, string sceneToload)
    {
        var button = buttonObj.GetComponent<UnityEngine.UI.Button>();
        button.onClick.AddListener(delegate { backButtonEvent(sceneToload); });
    }

    private void backButtonEvent(string sceneName)
    {
        Debug.Log("loading: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    private void processFolder(string midiDir)
    {
        Debug.Log("Reading MIDI directory: " + midiDir);
        Directory.GetFiles(midiDir)
        .Where(x => x.EndsWith(".mid")).ToList()
        .ForEach(x => processSessionsAndPlaceUiEntry(x));
    }

    private void processSessionsAndPlaceUiEntry(string midiPath)
    {
        var sessions = MidiSessionController.getMidiSessions(midiPath);
        var row = Instantiate(rowEntryObj);
        row.transform.SetParent(scrollViewParent);
        var rowRect = row.transform.GetComponent<RectTransform>();
        rowRect.localScale = Vector3.one;
        rowRect.localPosition = Vector3.zero;

        // Load UI contents
        var head = new MidiSessionDto(midiPath);
        var bestScore = 0d;
        var passes = 0;
        if (sessions.Count > 0)
        {
            head = sessions.First();
            bestScore = sessions.OrderByDescending(x => x.Accuracy).First().Accuracy;
            passes = sessions.Where(x => x.Accuracy >= SCORE_TO_PASS).Count();
        }
        setText(head.FormattedTrackName, NAME_INDEX, row);
        setText(head.TrackDifficulty.ToString(), DIFFICULTY_INDEX, row);
        setText(bestScore * 100 + "%", BEST_SCORE_INDEX, row);
        setText(passes + "/" + sessions.Count(), OVERALL_SCORE_INDEX, row);

        // Setup button
        row.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { playButtonEvent(midiPath); });
    }

    private void setText(string text, int childIndex, GameObject rowObj)
    {
        var textObj = rowObj.transform.GetChild(childIndex).transform.GetChild(TEXT_INDEX).GetComponent<UnityEngine.UI.Text>();
        textObj.text = text;
    }

    private void playButtonEvent(string midiPath)
    {
        RuntimeSettings.MIDI_FILE_NAME = midiPath;
        SceneManager.LoadScene("PlayStandardMode");
    }

}
