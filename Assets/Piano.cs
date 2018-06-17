using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sanford.Multimedia.Midi;
using Sanford.Multimedia;

/// <summary>  
/// Initialises piano and holds state.false
/// Builds piano along x axis assumes VR is setup perpendicular.
/// </summary>  
public class Piano : MonoBehaviour
{
    public static bool isInit;
    public GameObject BlackKey;
    public GameObject WhiteKey;

    // private static readonly Vector3 defaultWhiteScale = new Vector3(0.0)
    private static readonly Color activationColor = Color.red;
    private static Dictionary<PianoKey, GameObject> pianoKeys = new Dictionary<PianoKey, GameObject>();
    private static bool isKeyPickedUp = true;


    public static Piano instance;
    void Start()
    {
        instance = this;
    }

    private Vector3 fixY(Vector3 v, float y){
        return new Vector3(v.x, y, v.z);
    }

    public void clearPiano(){
        Debug.Log("CLEARING PIANO");
        foreach(var item in pianoKeys.Values){
            Object.Destroy(item);
        }
        isInit = false;
    }

    public void BuildPiano(GameObject leftMarker, GameObject rightMarker) 
    {
        clearPiano();
        var leftPos = leftMarker.transform.position;
        var rightPos = rightMarker.transform.position;

        Debug.Log("BUILDING PIANO");
        var y = (leftPos + rightPos).y / 2;

        // var width = (fixY(leftMarker, y) - fixY(rightMarker, y));
        var width = (fixY(rightPos, y) - fixY(leftPos, y));
        var whiteKeyWidth = fixY(width / 36, y);
        var defaultWhiteScale = WhiteKey.transform.localScale;
        var defaultBlackScale = BlackKey.transform.localScale;
        var whiteKeyScale = new Vector3(whiteKeyWidth.x, defaultWhiteScale.y, defaultWhiteScale.z);
        var blackKeyScale = new Vector3(whiteKeyWidth.x / 2, defaultBlackScale.y, defaultBlackScale.z);
        var rotation = new Quaternion(0, leftMarker.transform.rotation.y, 0,1);
        var blackKeyOffSet = new Vector3(whiteKeyWidth.x / 2 , 0.001f, 0.025f);
        var nextPos = leftPos - whiteKeyWidth / 2;
        foreach(int i in Enumerable.Range(36,61))
        {
            // Debug.Log("Key: " + i + ", pos: " +  nextPos);
            GameObject keyObject;
            var pianoKey = PianoKeys.GetKeyFor(i);
            if(pianoKey.color == KeyColor.White){
                keyObject = Instantiate(WhiteKey);
                nextPos += whiteKeyWidth;
                keyObject.transform.localPosition = fixY(nextPos, y);
                keyObject.transform.localScale = whiteKeyScale;
            } else {
                keyObject = Instantiate(BlackKey);
                keyObject.transform.localPosition = fixY(nextPos, y) + blackKeyOffSet;
                keyObject.transform.localScale = blackKeyScale;
            }
            // keyObject.transform.rotation = Quaternion.RotateTowards(leftMarker.transform.rotation, rightMarker.transform.rotation, 0);
            keyObject.transform.rotation = rotation;
            
            // Create and hide key marker
            var marker = Instantiate(WhiteKey);
            marker.transform.position = new Vector3(keyObject.transform.position.x, keyObject.transform.position.y + 0.002f, keyObject.transform.position.z);
            var render = marker.GetComponent<MeshRenderer>();
            render.material.color = Color.blue;
            marker.SetActive(false);
            pianoKey.marker = marker;

            // Store key object in dictionary
            pianoKeys[pianoKey] = keyObject;
        }
        isInit = true;
        Sequencer.instance.SpawnNotes();
    }

    public static void ActivateKey(int keyNum)
    {
        if (!isInit)
        {
            Debug.Log("Piano not setup.");
        }
        else
        {
            var pianoKey = PianoKeys.GetKeyFor(keyNum);
            GameObject gameObject;
            if (pianoKeys.TryGetValue(pianoKey, out gameObject))
            {
                var render = gameObject.GetComponent<MeshRenderer>();
                render.material.color = activationColor;
            }
        }
    }

    public void GetOffsetForKeyNum(int keyNum, out float a, out float b)
    {
        if (!isInit)
        {
            Debug.Log("Piano not setup.");
            a = 0;
            b = 0;
        }
        else
        {
            var key = PianoKeys.GetKeyFor(keyNum);
            var pos = pianoKeys[key].transform.position;
            a = pos.x;
            b = pos.z;
        }
    }


    public static void DeactivateKey(int keyNum)
    {
        if (!isInit)
        {
            Debug.Log("Piano not setup.");
        }
        else
        {
            var pianoKey = PianoKeys.GetKeyFor(keyNum);
            GameObject gameObject;
            if (pianoKeys.TryGetValue(pianoKey, out gameObject))
            {
                var render = gameObject.GetComponent<MeshRenderer>();
                render.material.color = pianoKey.color == KeyColor.White ? Color.white : Color.black;
            }
        }
    }

    public static void MarkKeys(List<int> keyNums)
    {
        foreach (var i in pianoKeys.Keys)
        {
            if (keyNums.Contains(i.keyNum))
            {
                //Debug.Log("Marking keynum: " + i.keyNum);
                i.Mark();
            }
            else
            {
                i.Unmark();
            }
        }
    }

}
