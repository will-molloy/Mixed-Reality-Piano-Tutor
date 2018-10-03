using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Midi_Session;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace User_Interface
{
    /// <summary>
    ///     - Loads the MIDI folder and sessions for the MIDI selection UIs
    /// </summary>
    public class MidiSelectionUIService : MonoBehaviour
    {
        private const double SCORE_TO_PASS = 0.5d;
        private const int TEXT_INDEX = 0;
        private const int NAME_INDEX = 0;
        private const int DIFFICULTY_INDEX = 1;
        private const int BEST_SCORE_INDEX = 2;
        private const int OVERALL_SCORE_INDEX = 3;

        [SerializeField] private GameObject canvas;

        [SerializeField] private Text headerText;

        private MidiSessionController MidiSessionController;

        [SerializeField] private GameObject nameField;
        [SerializeField] private string PlayModeSceneName;

        [SerializeField] private Text progressField;

        [SerializeField] private GameObject rowEntryObj;

        [SerializeField] private GameObject rowHeader;

        [SerializeField] private Transform scrollViewParent;

        private List<GameObject> scrollViewRows;

        [SerializeField] private GameObject speedField;

        [SerializeField] private Text speedFieldPlaceHolderText;

        private int totalTracks;

        private int tracksCompleted;

        private void Start()
        {
            MidiSessionController = new MidiSessionController();
            scrollViewRows = new List<GameObject>();
            scrollViewRows.Add(rowHeader);
            processFolder(RuntimeSettings.MIDI_DIR);
            headerText.text = RuntimeSettings.IS_PLAY_MODE ? "Track" : "Exercise";
            speedFieldPlaceHolderText.text = "Game speed: " + RuntimeSettings.GAME_SPEED.ToString("0.00");
        }

        private void Update()
        {
            // Update width when screen size changes
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
            //var best = string.Format("%.2f\\%", bestScore * 100);
            setText(head.FormattedTrackName, NAME_INDEX, row);
            setText((bestScore * 100).ToString("F2") + "%", BEST_SCORE_INDEX, row);
            setText(passes + "/" + sessions.Count(), OVERALL_SCORE_INDEX, row);

            var difficulty = MidiSessionController.GetDifficultyFor(midiPath);
            setText(difficulty.ToString(), DIFFICULTY_INDEX, row);

            // Setup button
            row.GetComponent<Button>().onClick.AddListener(delegate { playButtonEvent(midiPath, difficulty); });

            if (passes > 0) tracksCompleted++;
            totalTracks++;

            // Add to list to fix sizing in update()
            scrollViewRows.Add(row);
        }

        private void setText(string text, int childIndex, GameObject rowObj)
        {
            var textObj = rowObj.transform.GetChild(childIndex).transform.GetChild(TEXT_INDEX).GetComponent<Text>();
            textObj.text = text;
        }

        private void playButtonEvent(string midiPath, MidiDifficultyDto.Difficulty difficulty)
        {
            var name = nameField.GetComponent<InputField>().text;
            if (!name.Equals("")) RuntimeSettings.USER = name;
            var speed = speedField.GetComponent<InputField>().text;
            if (!speed.Equals(""))
                RuntimeSettings.GAME_SPEED = float.Parse(speed, CultureInfo.InvariantCulture.NumberFormat);
            RuntimeSettings.MIDI_FILE_NAME = midiPath;
            RuntimeSettings.DIFFICULTY = difficulty;
            SceneManager.LoadScene(PlayModeSceneName);
        }
    }
}