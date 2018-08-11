using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

public class PlayModeSelectionUIService : MonoBehaviour
{
    [SerializeField] private string midiFolderPath = "Assets/MIDI";

    [SerializeField] private string PlayModeSceneName;

    [SerializeField] private GameObject rowEntryObj;

    [SerializeField] private Transform scrollViewParent;

    [SerializeField] private GameObject nameField;

    [SerializeField] private GameObject canvas;

    [SerializeField] private GameObject rowHeader;

    private const double SCORE_TO_PASS = 0.5d;
    private const int TEXT_INDEX = 0;
    private const int NAME_INDEX = 0;
    private const int DIFFICULTY_INDEX = 1;
    private const int BEST_SCORE_INDEX = 2;
    private const int OVERALL_SCORE_INDEX = 3;

    private List<GameObject> scrollViewRows;

    void Start()
    {
        scrollViewRows = new List<GameObject>();
        scrollViewRows.Add(rowHeader);
        processFolder(midiFolderPath);
    }

    void Update()
    {
        var canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
        scrollViewRows.ForEach(x =>
        {
            var rect = x.transform.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(canvasWidth, rect.rect.height);
        });
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

        // Add to list to fix sizing
        scrollViewRows.Add(row);
    }

    private void setText(string text, int childIndex, GameObject rowObj)
    {
        var textObj = rowObj.transform.GetChild(childIndex).transform.GetChild(TEXT_INDEX).GetComponent<UnityEngine.UI.Text>();
        textObj.text = text;
    }

    private void playButtonEvent(string midiPath)
    {
        var name = nameField.GetComponent<UnityEngine.UI.InputField>().text;
        if (!name.Equals(""))
        {
            RuntimeSettings.CURRENT_USER = name;
        }
        RuntimeSettings.MIDI_FILE_NAME = midiPath;
        SceneManager.LoadScene(PlayModeSceneName);
    }

}
