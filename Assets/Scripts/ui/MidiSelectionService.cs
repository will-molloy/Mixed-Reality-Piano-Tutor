using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

///<summary>
/// Processes MIDI directory and database for MIDI selection UI
///</summary>
public class MidiSelectionService : MonoBehaviour
{
    [SerializeField]
    private string midiFolderPath; // should probably read from config file or use ~/Documents
    private const double SCORE_TO_PASS = 0.5d;

    [SerializeField]
    private GameObject rowEntryObj;
    private const int TEXT_INDEX = 0;
    private const int NAME_INDEX = 0;
    private const int DIFFICULTY_INDEX = 1;
    private const int BEST_SCORE_INDEX = 2;
    private const int OVERALL_SCORE_INDEX = 3;
    private const int BUTTON_INDEX = 4;

    void Start()
    {
        processFolder(midiFolderPath);
    }

    private void processFolder(string midiDir)
    {
        Debug.Log("Reading MIDI directory: " + midiDir);
        Directory.GetFiles(midiDir)
        .Where(x => x.EndsWith(".mid")).ToList()
        .ForEach(x => processSessionsAndPlaceUiEntry(x));
        // .ForEach(x => MidiSessionController.putDummyMidiSession(x));
    }

    private void processSessionsAndPlaceUiEntry(string midiPath)
    {
        var sessions = MidiSessionController.getMidiSessions(midiPath);
        var head = new MidiSessionDto(midiPath);
        if (sessions.Count > 0)
        {
            head = sessions.First();
        }
        var rowObj = Instantiate(rowEntryObj);
        rowObj.transform.SetParent(this.transform);
        var rowRect = rowObj.transform.GetComponent<RectTransform>();
        rowRect.localScale = Vector3.one;
        rowRect.localPosition = Vector3.zero;

        setText(head.FormattedTrackName, NAME_INDEX, rowObj);
        setText(head.TrackDifficulty.ToString(), DIFFICULTY_INDEX, rowObj);
        var bestScore = 0d;
        var passes = 0;
        if (sessions.Count > 0){
            bestScore = sessions.OrderByDescending(x => x.Accuracy).First().Accuracy;
            passes = sessions.Where(x => x.Accuracy >= SCORE_TO_PASS).Count();
        }
        setText(bestScore * 100 + "%", BEST_SCORE_INDEX, rowObj);
        setText(passes + "/" + sessions.Count(), OVERALL_SCORE_INDEX, rowObj);
        setButton(midiPath, rowObj);
    }

    private void setText(string text, int childIndex, GameObject rowObj)
    {
        var textObj = rowObj.transform.GetChild(childIndex).transform.GetChild(TEXT_INDEX).GetComponent<UnityEngine.UI.Text>();
        textObj.text = text;
    }

    private void setButton(string midiPath, GameObject rowObj)
    {
        var buttonScript = rowObj.transform.GetChild(BUTTON_INDEX).GetComponent<UnityEngine.UI.Button>();
        buttonScript.onClick.AddListener(delegate { buttonEvent(midiPath); });
    }

    private void buttonEvent(string midiPath)
    {
        RuntimeSettings.MIDI_FILE_NAME = midiPath;
        SceneManager.LoadScene(RuntimeSettings.GAME_MODE);
    }

}
