using System.Collections.Generic;
using Midi_Session;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace User_Interface
{
    /// <summary>
    ///     - Loads the MIDI sessions for the history UI
    /// </summary>
    public class HistoryModeUIService : MonoBehaviour
    {
        private const int TEXT_INDEX = 0;
        private const int NAME_INDEX = 0;
        private const int DATE_INDEX = 1;
        private const int SCORE_INDEX = 2;

        [SerializeField] private GameObject canvas;
        private MidiSessionController MidiSessionController;
        [SerializeField] private string PlayModeSceneName;

        [SerializeField] private GameObject rowEntryObj;

        [SerializeField] private GameObject rowHeader;

        [SerializeField] private Transform scrollViewParent;

        private List<GameObject> scrollViewRows;

        private void Start()
        {
            MidiSessionController = new MidiSessionController();
            scrollViewRows = new List<GameObject>();
            scrollViewRows.Add(rowHeader);
            processSessions();
        }

        private void Update()
        {
            // Update width when screen size changes
            var canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
            scrollViewRows.ForEach(row =>
            {
                var rect = row.transform.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(canvasWidth, rect.rect.height);
            });
        }

        private void processSessions()
        {
            var sessions = MidiSessionController.getAllSessions();
            sessions.ForEach(session =>
            {
                var row = Instantiate(rowEntryObj);

                // Add to scroll view
                row.transform.SetParent(scrollViewParent);
                var rowRect = row.transform.GetComponent<RectTransform>();
                rowRect.localScale = Vector3.one;
                rowRect.localPosition = Vector3.zero;

                // Set text fields
                setText(session.FormattedTrackName, NAME_INDEX, row);
                setText(session.SessionDateTime.ToShortTimeString() + " " + session.SessionDateTime.ToShortDateString(),
                    DATE_INDEX, row);
                var score = (int) session.Accuracy * 100;
                setText(score + "%", SCORE_INDEX, row);

                // Setup button
                row.GetComponent<Button>().onClick.AddListener(delegate { playButtonEvent(session); });

                // For variable width
                scrollViewRows.Add(row);
            });
        }

        private void setText(string text, int childIndex, GameObject rowObj)
        {
            var textObj = rowObj.transform.GetChild(childIndex).transform.GetChild(TEXT_INDEX).GetComponent<Text>();
            textObj.text = text;
        }

        private void playButtonEvent(MidiSessionDto session)
        {
            RuntimeSettings.CACHED_SESSION = session;
            RuntimeSettings.LOAD_SAVED_SESSION_AT_STARTUP = true;
            SceneManager.LoadScene(PlayModeSceneName);
        }
    }
}