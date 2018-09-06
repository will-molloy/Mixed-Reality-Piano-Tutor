using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Globalization;

public class HistoryModeUIService : MonoBehaviour
{
    [SerializeField] private string PlayModeSceneName;

    [SerializeField] private GameObject rowEntryObj;

    [SerializeField] private Transform scrollViewParent;

    [SerializeField] private GameObject canvas;

    [SerializeField] private GameObject rowHeader;

    private const int TEXT_INDEX = 0;
    private const int NAME_INDEX = 0;
    private const int DATE_INDEX = 1;
    private const int SCORE_INDEX = 2;
    private MidiSessionController MidiSessionController;

    private List<GameObject> scrollViewRows;

    void Start()
    {
        MidiSessionController = new MidiSessionController();
        scrollViewRows = new List<GameObject>();
        scrollViewRows.Add(rowHeader);
        processSessions();
    }

    void Update()
    {
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
            setText(session.SessionDateTime.ToShortDateString(), DATE_INDEX, row);
            setText(session.Accuracy * 100 + "%", SCORE_INDEX, row);

            // Setup button
            row.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { playButtonEvent(session); });

            // For variable width
            scrollViewRows.Add(row);
        });
    }

    private void setText(string text, int childIndex, GameObject rowObj)
    {
        var textObj = rowObj.transform.GetChild(childIndex).transform.GetChild(TEXT_INDEX).GetComponent<UnityEngine.UI.Text>();
        textObj.text = text;
    }

    private void playButtonEvent(MidiSessionDto session)
    {
        RuntimeSettings.CACHED_SESSION = session;
        RuntimeSettings.LOAD_SAVED_SESSION_AT_STARTUP = true;
        SceneManager.LoadScene(PlayModeSceneName);
    }

}
