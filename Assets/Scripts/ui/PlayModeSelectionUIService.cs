using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Globalization;

public class PlayModeSelectionUIService : MonoBehaviour
{
    [SerializeField] private string PlayModeSceneName;

    [SerializeField] private GameObject rowEntryObj;

    [SerializeField] private Transform scrollViewParent;

    [SerializeField] private GameObject nameField;

    [SerializeField] private GameObject speedField;

    [SerializeField] private UnityEngine.UI.Text speedFieldPlaceHolderText;

    [SerializeField] private GameObject canvas;

    [SerializeField] private GameObject rowHeader;

    [SerializeField] private UnityEngine.UI.Text progressField;

    [SerializeField] private UnityEngine.UI.Text headerText;

    private const double SCORE_TO_PASS = 0.5d;
    private const int TEXT_INDEX = 0;
    private const int NAME_INDEX = 0;
    private const int DIFFICULTY_INDEX = 1;
    private const int BEST_SCORE_INDEX = 2;
    private const int OVERALL_SCORE_INDEX = 3;

    private MidiSessionController MidiSessionController;

    private List<GameObject> scrollViewRows;

    void Start()
    {
        MidiSessionController = new MidiSessionController();
        scrollViewRows = new List<GameObject>();
        scrollViewRows.Add(rowHeader);
        processFolder(RuntimeSettings.MIDI_DIR);
        headerText.text = RuntimeSettings.IS_PLAY_MODE ? "Track" : "Exercise";
        speedFieldPlaceHolderText.text = "Game speed: " + RuntimeSettings.GAME_SPEED.ToString("0.00");
    }

    void Update()
    {
        // Update width when screen size changes
        var canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
        scrollViewRows.ForEach(x =>
        {
            var rect = x.transform.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(canvasWidth, rect.rect.height);
        });
    }

    private int tracksCompleted;

    private int totalTracks;

    private void processFolder(string midiDir)
    {
        Debug.Log("Reading MIDI directory: " + midiDir);
        Directory.GetFiles(midiDir)
        .Where(x => x.EndsWith(".mid")).ToList()
        .ForEach(x => processSessionsAndPlaceUiEntry(x));

        // Set progress count
        progressField.text = "Completed: " + tracksCompleted + "/" + totalTracks;
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
        Debug.Log(sessions.Count());
        setText(head.FormattedTrackName, NAME_INDEX, row);
        setText(bestScore * 100 + "%", BEST_SCORE_INDEX, row);
        setText(passes + "/" + sessions.Count(), OVERALL_SCORE_INDEX, row);

        var difficulty = MidiSessionController.GetDifficultyFor(midiPath);
        setText(difficulty.ToString(), DIFFICULTY_INDEX, row);

        // Setup button
        row.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { playButtonEvent(midiPath, difficulty); });

        if (passes > 0)
        {
            this.tracksCompleted++;
        }
        this.totalTracks++;

        // Add to list to fix sizing in update()
        scrollViewRows.Add(row);
    }

    private void setText(string text, int childIndex, GameObject rowObj)
    {
        var textObj = rowObj.transform.GetChild(childIndex).transform.GetChild(TEXT_INDEX).GetComponent<UnityEngine.UI.Text>();
        textObj.text = text;
    }

    private void playButtonEvent(string midiPath, MidiDifficultyDto.Difficulty difficulty)
    {
        var name = nameField.GetComponent<UnityEngine.UI.InputField>().text;
        if (!name.Equals(""))
        {
            RuntimeSettings.USER = name;
        }
        var speed = speedField.GetComponent<UnityEngine.UI.InputField>().text;
        if (!speed.Equals(""))
        {
            RuntimeSettings.GAME_SPEED = float.Parse(speed, CultureInfo.InvariantCulture.NumberFormat);
        }
        RuntimeSettings.MIDI_FILE_NAME = midiPath;
        RuntimeSettings.DIFFICULTY = difficulty;
        SceneManager.LoadScene(PlayModeSceneName);
    }

}
