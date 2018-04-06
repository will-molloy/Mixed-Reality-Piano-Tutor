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
    public static PianoKey leftMostKey;
    public static Vector3 leftThumbPos;
    public static PianoKey rightMostKey;
    public static bool isInit;
    public GameObject BlackKey;
    public GameObject WhiteKey;
    private const float blackKeyXOffset = 0.01f;
    private const float blackKeyYOffset = 0.001f;
    private const float blackKeyZOffset = 0.025f;
    private const float whiteKeyWidth = 0.02f;
    private static readonly Color activationColor = Color.red;
    private static Dictionary<PianoKey, GameObject> pianoKeys = new Dictionary<PianoKey, GameObject>();

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (leftMostKey != null && rightMostKey != null && !isInit)
        {
            // Init piano
            Debug.Log("Left key = " + leftMostKey.keyNum + "Right key = " + rightMostKey.keyNum + "Num keys = " + (rightMostKey.keyNum - leftMostKey.keyNum + 1));
            var currentX = 0f; // Maintain state of current piano width
            for (var i = leftMostKey.keyNum; i <= rightMostKey.keyNum; i++)
            {
                var pianoKey = PianoKeys.GetKeyFor(i);
                GameObject keyObject;
                Vector3 nextPos;
                if (pianoKey.color == KeyColor.White)
                {
                    keyObject = Instantiate(WhiteKey);
                    currentX += whiteKeyWidth;
                    nextPos = new Vector3(leftThumbPos.x + currentX, leftThumbPos.y, leftThumbPos.z);
                }
                else
                {
                    keyObject = Instantiate(BlackKey);
                    nextPos = new Vector3(leftThumbPos.x + currentX + blackKeyXOffset, leftThumbPos.y + blackKeyYOffset, leftThumbPos.z + blackKeyZOffset);
                }
                keyObject.transform.localPosition = nextPos;
                pianoKeys[pianoKey] = keyObject;
            }
            isInit = true;
        }
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

}
