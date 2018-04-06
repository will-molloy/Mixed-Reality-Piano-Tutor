using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sanford.Multimedia.Midi;
using Sanford.Multimedia;

public class CalibrationScript : MonoBehaviour
{
    public static PianoKey leftKey;
    public static Vector3 leftThumbPos;
    public static PianoKey rightKey;
    public static bool inited = false;
    public GameObject BlackKey;
    public GameObject WhiteKey;

    private float whiteKeyX;

    private float blackKeyX;

    private const float blackKeyXOffset = 0.005f;

    private const float blackKeyYOffset = 0.001f;

    private const float blackKeyZOffset = 0.025f;

    void Start()
    {
        whiteKeyX = WhiteKey.transform.localScale.x;
        blackKeyX = BlackKey.transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (leftKey != null && rightKey != null && !inited)
        {
            // Init keyboard
            Debug.Log("Left key = " + leftKey.keyNum + "Right key = " + rightKey.keyNum + "Num keys = " + (rightKey.keyNum - leftKey.keyNum + 1));
            var currentX = 0f;
            for (var v = 0; v <= rightKey.keyNum - leftKey.keyNum; v++)
            {
                var keyType = PianoKeys.GetKeyFor(v + leftKey.keyNum).blackOrWhite;
                GameObject obj;
                Vector3 nextPos;
                if (keyType == BlackOrWhite.White)
                {
                    obj = Instantiate(WhiteKey);
                    currentX += whiteKeyX;
                    nextPos = new Vector3(leftThumbPos.x + currentX, leftThumbPos.y, leftThumbPos.z);
                }
                else
                {
                    obj = Instantiate(BlackKey);
                    nextPos = new Vector3(leftThumbPos.x + currentX + blackKeyXOffset, leftThumbPos.y + blackKeyYOffset, leftThumbPos.z + blackKeyZOffset);
                }
				obj.transform.localPosition = nextPos;
            }
            inited = true;
        }

        if (inited)
        {
            // MIDI stuff
        }
    }

}
