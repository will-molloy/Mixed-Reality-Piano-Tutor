using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public class MidiFolderReader : MonoBehaviour
{

    [SerializeField]
    private string midiFolderPath;
    [SerializeField]
    private GameObject scrollButtonObj;
    [SerializeField]
    private GameObject textObj;
    private const int buttonSpacing = 10; 
    private float buttonsHeightSpace = 0;
    private Rect thisRect;

    private GameObject scrollBarVertical;

    void Start()
    {
        thisRect = this.GetComponent<RectTransform>().rect;
        processFolder(midiFolderPath);
        this.transform.parent.transform.parent.GetChild(1);
    }

    private void processFolder(string midiDir)
    {
        Debug.Log("Reading MIDI directory: " + midiDir);
        Enumerable.Range(0, 4).ToList().ForEach(_ => { // TODO remove
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
        Debug.Log("Processing: " + midiPath + ", count: " + count);

        var button = Instantiate(scrollButtonObj);
        button.transform.SetParent(this.transform);
        var buttonRect = button.GetComponent<RectTransform>();
        buttonRect.localPosition = Vector3.zero;
        buttonRect.localScale = Vector3.one;
        var buttonOffSetLeft = (thisRect.width - buttonRect.rect.width) / 2;
        var buttonOffSetTop = count * (buttonSpacing + buttonRect.rect.height);
        buttonsHeightSpace += buttonOffSetTop;
        if (buttonsHeightSpace > thisRect.height)
        {
            Debug.Log("Increasing canvas height.");
            // change scrollbar
        }
        buttonRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, buttonOffSetLeft, buttonRect.rect.width);
        buttonRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, buttonOffSetTop, buttonRect.rect.height);


        var text = Instantiate(textObj);
        text.transform.SetParent(button.transform);
        text.GetComponent<UnityEngine.UI.Text>().text = format(midiPath);
        var textRect = text.GetComponent<RectTransform>();
        textRect.localPosition = Vector3.zero;
        textRect.localScale = Vector3.one;
        textRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, buttonRect.rect.width);
        textRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, buttonRect.rect.height);
    }

    private string format(string midiPath)
    {
        return Regex.Replace(midiPath.Replace(midiFolderPath, "").Replace("mid", "").Replace("_", " "), "[^a-zA-Z ]", "");
    }
}
