﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
public class MidiFolderReader : MonoBehaviour
{

    [SerializeField]
    private string midiFolderPath; // should probably read from config file or use ~/Documents
    [SerializeField]
    private GameObject scrollButtonObj;
    [SerializeField]
    private GameObject textObj;
    private const int buttonSpacing = 10;
    private Rect thisRect;

    void Start()
    {
        thisRect = this.GetComponent<RectTransform>().rect;
        processFolder(midiFolderPath);
        this.transform.parent.transform.parent.GetChild(1);
    }

    private void processFolder(string midiDir)
    {
        Debug.Log("Reading MIDI directory: " + midiDir);
        Enumerable.Range(0, 4).ToList().ForEach(_ =>
        { // TODO remove
            Directory.GetFiles(midiDir)
            .Where(x => x.EndsWith(".mid")).ToList()
            .ForEach(x => processFile(x));
        });
    }

    ///<summary>
    /// Can read database etc. for each tracks scores, difficulty etc.
    ///</summary>
    private void processFile(string midiPath)
    {
        var count = this.transform.childCount;
        var button = Instantiate(scrollButtonObj);
        button.transform.SetParent(this.transform);
        var buttonScript = button.GetComponent<UnityEngine.UI.Button>();
        buttonScript.onClick.AddListener((delegate { buttonEvent(midiPath); }));
        var buttonRect = button.GetComponent<RectTransform>();
        buttonRect.localPosition = Vector3.zero;
        buttonRect.localScale = Vector3.one;
        var buttonOffSetLeft = (thisRect.width - buttonRect.rect.width) / 2;
        var buttonOffSetTop = count * (buttonSpacing + buttonRect.rect.height);
        buttonRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, buttonOffSetLeft, buttonRect.rect.width);
        buttonRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, buttonOffSetTop, buttonRect.rect.height);

        var text = Instantiate(textObj);
        text.transform.SetParent(button.transform);
        text.GetComponent<UnityEngine.UI.Text>().text = formatForUi(midiPath);
        var textRect = text.GetComponent<RectTransform>();
        textRect.localPosition = Vector3.zero;
        textRect.localScale = Vector3.one;
        textRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, buttonRect.rect.width);
        textRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, buttonRect.rect.height);
    }

    public void buttonEvent(string midiPath)
    {
        RuntimeSettings.MIDI_FILE_NAME = midiPath;
        SceneManager.LoadScene(RuntimeSettings.GAME_MODE);	
    }

    private string formatForUi(string midiPath)
    {
        return Regex.Replace(midiPath.Replace(midiFolderPath, "").Replace("mid", "").Replace("_", " "), "[^a-zA-Z ]", "");
    }
}
