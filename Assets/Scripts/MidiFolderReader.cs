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
public class MidiFolderReader : MonoBehaviour
{

    [SerializeField]
    private string midiFolderPath; // should probably read from config file or use ~/Documents

    // UI components
    [SerializeField]
    private GameObject rowPlaceholderObj;
    [SerializeField]
    private GameObject statsObj;
    [SerializeField]
    private GameObject nameObj;
    [SerializeField]
    private GameObject playButtonObj;
    private const int buttonSpacing = 10;
    private Rect thisRect;

    private const double SCORE_TO_PASS = 0.5d;

    void Start()
    {
        thisRect = this.GetComponent<RectTransform>().rect;
        processFolder(midiFolderPath);
        this.transform.parent.transform.parent.GetChild(1);
    }

    private void processFolder(string midiDir)
    {
        Debug.Log("Reading MIDI directory: " + midiDir);
        Directory.GetFiles(midiDir)
        .Where(x => x.EndsWith(".mid")).ToList()
        .ForEach(x => processFile(x));
    }

    ///<summary>
    /// Can read database etc. for each tracks scores, difficulty etc.
    ///</summary>
    private void processFile(string midiPath)
    {
        var sessions = MidiSessionController.getMidiSessions(midiPath);
        var head = new MidiSessionDto(midiPath); // one with no score etc.
        if (sessions.Count > 0)
        {
            head = sessions.First();
        }   
        var parentTransform = Instantiate(rowPlaceholderObj).transform;
        parentTransform.SetParent(this.transform);    

        placeName(head.FormattedTrackName, parentTransform);
        placeDifficulty(head.TrackDifficulty, parentTransform);
        var bestScore = sessions.OrderByDescending(x => x.Accuracy).First().Accuracy;
        placeBestAccuracy(bestScore, parentTransform);
        var passes = sessions.Where(x => x.Accuracy >= SCORE_TO_PASS).Count();
        placeOverallPassAttempts(passes, sessions.Count, parentTransform);
    }

    private void placeName(string name, Transform parent)
    {
        var nameObj = Instantiate(this.nameObj);
        nameObj.transform.SetParent(parent);
        var textObj = nameObj.GetComponent<UnityEngine.UI.Text>();
        textObj.text = name;
    }

    private void placeDifficulty(MidiSessionDto.Difficulty difficulty, Transform parent)
    {
        var statsObj = Instantiate(this.statsObj);
        statsObj.transform.SetParent(parent);
        var textObj = nameObj.GetComponent<UnityEngine.UI.Text>();
        textObj.text = difficulty + "";
    }

    private void placeBestAccuracy(double accuracy, Transform parent)
    {
        var statsObj = Instantiate(this.statsObj);
        statsObj.transform.SetParent(parent);
        var textObj = nameObj.GetComponent<UnityEngine.UI.Text>();
        textObj.text = accuracy + "";
    }

    private void placeOverallPassAttempts(int passes, int attempts, Transform parent)
    {
        var statsObj = Instantiate(this.statsObj);
        statsObj.transform.SetParent(parent);
        var textObj = nameObj.GetComponent<UnityEngine.UI.Text>();
        textObj.text = passes + "/" + attempts;
    }

    // private void placeButton(string midiPath)
    // {
    //     var button = Instantiate(scrollButtonObj);
    //     button.transform.SetParent(this.transform);
    //     var buttonScript = button.GetComponent<UnityEngine.UI.Button>();
    //     buttonScript.onClick.AddListener((delegate { buttonEvent(midiPath); }));
    //     var buttonRect = button.GetComponent<RectTransform>();
    //     buttonRect.localPosition = Vector3.zero;
    //     buttonRect.localScale = Vector3.one;

    //     var text = Instantiate(nameObj);
    //     text.transform.SetParent(button.transform);
    //     text.GetComponent<UnityEngine.UI.Text>().text = formatForUi(midiPath);
    //     var textRect = text.GetComponent<RectTransform>();
    //     textRect.localPosition = Vector3.zero;
    //     textRect.localScale = Vector3.one;
    //     textRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, buttonRect.rect.width);
    //     textRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, buttonRect.rect.height);
    // }

    private void buttonEvent(string midiPath)
    {
        RuntimeSettings.MIDI_FILE_NAME = midiPath;
        SceneManager.LoadScene(RuntimeSettings.GAME_MODE);	
    }

}
