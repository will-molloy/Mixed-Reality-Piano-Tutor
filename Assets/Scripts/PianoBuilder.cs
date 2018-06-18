using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>  
/// - Builds piano 
/// - Use keyboard to scale/move/rotate piano
/// </summary> 
public class PianoBuilder : MonoBehaviour
{

    [SerializeField]
    private bool needsCameraHook; // determine if spawn straight away or with key specified in CameraHook script
    [SerializeField]
    private GameObject whiteKey;
    [SerializeField]
    private GameObject blackKey;
    [SerializeField]
    private GameObject lockedText;
    private static readonly Color activationColor = Color.red;

    public static readonly int CENTRE = (PianoKeys.GetLastKey().keyNum + PianoKeys.GetFirstKey().keyNum) / 2;

    internal Dictionary<PianoKey, GameObject> pianoKeys;

    internal static readonly float yOffset = 0.001f;
    internal bool locked = false;
    internal bool pianoIsBuilt = false;
    internal GameObject lockedTextObj;

    public static PianoBuilder instance;

    public void PlacePianoInfrontOfTransform(Transform trf)
    {
        if (!pianoIsBuilt)
        {
            Debug.Log("Building Piano.");
            BuildPianoAt(trf.position + trf.forward * 0.5f);
            var sequencer = Sequencer.instance;
            if (sequencer)
            {
                sequencer.spawnNotes();
            }
            else
            {
                Debug.Log("No Sequencer component (you must be in calibration mode)");
            }
        }
        else
        {
            Debug.Log("Piano already built.");
        }
    }

    private void BuildPianoAt(Vector3 location)
    {
        this.transform.position = location;
        var firstkey = PianoKeys.GetFirstKey();
        var lastkey = PianoKeys.GetLastKey();
        var whitekeyScale = whiteKey.transform.localScale;
        var blackkeyScale = blackKey.transform.localScale;
        var xOffset = 0f;
        var zOffset = whitekeyScale.z / 4;

        for (int i = firstkey.keyNum; i <= lastkey.keyNum; i++)
        {
            var currentKey = PianoKeys.GetKeyFor(i);
            GameObject keyObj;
            if (currentKey.color == KeyColor.White)
            {
                keyObj = Instantiate(whiteKey);
                keyObj.transform.SetParent(this.transform);
                keyObj.transform.localPosition = new Vector3(xOffset, 0f, 0);
                xOffset += whitekeyScale.x + PianoKeys.pianoKeyGap;
            }
            else
            {
                keyObj = Instantiate(blackKey);
                keyObj.transform.SetParent(this.transform);
                keyObj.transform.localPosition = new Vector3(xOffset - whitekeyScale.x / 2, yOffset, zOffset);
            }
            pianoKeys[currentKey] = keyObj;
        }
        pianoIsBuilt = true;

    }
    void Start()
    {
        instance = this;
        pianoKeys = new Dictionary<PianoKey, GameObject>();
        if (!needsCameraHook)
        {
            Debug.Log("Building Piano with saved position. (TODO)");
            // PlacePianoInfrontOfTransform(); -- TODO save transform location to disk 
        }
    }

    void Update()
    {
        if (!pianoIsBuilt)
        {
            return;
        }
        if (!locked)
        {
            var position = this.transform.position;
            var scale = this.transform.localScale;
            var angle = this.transform.localEulerAngles;
            if (Input.GetKey(KeyCode.A))
            {
                position.x -= 0.001f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                position.x += 0.001f;
            }
            if (Input.GetKey(KeyCode.W))
            {
                position.y += 0.001f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                position.y -= 0.001f;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                position.z += 0.001f;
            }
            if (Input.GetKey(KeyCode.E))
            {
                position.z -= 0.001f;
            }
            if (Input.GetKey(KeyCode.Z))
            {
                scale += new Vector3(0.001f, 0f, 0.001f);
            }
            if (Input.GetKey(KeyCode.X))
            {
                scale -= new Vector3(0.001f, 0f, 0.001f);
            }
            if (Input.GetKey(KeyCode.C))
            {
                angle += new Vector3(0f, 1f, 0f);
            }
            if (Input.GetKey(KeyCode.V))
            {
                angle -= new Vector3(0f, 1f, 0f);
            }
            this.transform.position = position;
            this.transform.localScale = scale;
            this.transform.localEulerAngles = angle;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (lockedTextObj != null)
            {
                GameObject.Destroy(lockedTextObj);
            }
            if (locked)
            {
                locked = false;
            }
            else
            {
                locked = true;
                lockedTextObj = Instantiate(lockedText);
                lockedTextObj.transform.SetParent(this.transform);
                lockedTextObj.transform.localPosition = pianoKeys[PianoKeys.GetKeyFor(CENTRE)].transform.localPosition + new Vector3(0f, 0.1f, 0f);
            }
        }
    }

    public void ActivateKey(int keyNum)
    {
        if (!pianoIsBuilt)
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

    public void DeactivateKey(int keyNum)
    {
        if (!pianoIsBuilt)
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

    public void GetOffsetForKeyNum(int keyNum, out float a, out float b)
    {
        if (!pianoIsBuilt)
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
}
