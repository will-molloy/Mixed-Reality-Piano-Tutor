using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    public static readonly int CENTRE = (PianoKeys.GetLastKey().keyNum + PianoKeys.GetFirstKey().keyNum) / 2;

    internal Dictionary<PianoKey, GameObject> pianoKeys;

    internal static readonly float yOffset = 0.001f;
    // Whether the piano has been locked in placed by the user
    internal bool locked = false;
    // Whether the piano has been placed into the initial positon
    internal bool pianoIsBuilt = false;
    internal GameObject lockedTextObj;

    public static PianoBuilder instance;
    internal Sequencer sequencer;

    private readonly Color activationColor = Color.red;

    void Start()
    {
        instance = this;
        pianoKeys = new Dictionary<PianoKey, GameObject>();
        sequencer = GetComponent<Sequencer>();
        if (!needsCameraHook)
        {
            Debug.Log("Building Piano with saved position. (TODO)");
            // PlacePianoInfrontOfTransform(); -- TODO save transform location to RunTimeSettings 
        }
    }

    public void PlacePianoInfrontOfTransform(Transform trf)
    {
        if (!pianoIsBuilt)
        {
            Debug.Log("Building Piano.");
            PlacePianoAt(trf.position + trf.forward * 0.5f);
            if (sequencer)
            {
                sequencer.SpawnNotes();
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

    public void PlacePianoAt(Vector3 location)
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
        var o = pianoKeys[firstkey];
        var up = o.transform.position + pianoKeys[firstkey].transform.forward * 0.1f;
        var front = o.transform.position + pianoKeys[firstkey].transform.up * 0.1f;
        var lookat = o.transform.position + (o.transform.forward * 5f + o.transform.up * 1f);
        Debug.DrawLine(o.transform.position, front, color: Color.red, duration: 99999f, depthTest: false);
        Debug.DrawLine(o.transform.position, up, color: Color.red, duration: 99999f, depthTest: false);
        Debug.DrawLine(o.transform.position, lookat.normalized, color: Color.green, duration: 99999f, depthTest: false);
    }


    public Vector3 GetScaleForKey(PianoKey key)
    {
        if (!pianoKeys.ContainsKey(key))
        {
            throw new System.Exception("Invalid request for key");
        }
        return pianoKeys[key].transform.localScale;
    }

    public Vector3 GetKeyPositionForKey(PianoKey key)
    {
        if (!pianoKeys.ContainsKey(key))
        {
            throw new System.Exception("Invalid request for key");
        }
        return pianoKeys[key].transform.position;

    }

    public Vector3 GetForwardVectorForKey(PianoKey key)
    {
        if (!pianoKeys.ContainsKey(key))
        {
            throw new System.Exception("Invalid request for key");
        }
        return pianoKeys[key].transform.forward;
    }

    public Vector3 GetPointingAwayVectorForKey(PianoKey key)
    {
        if (!pianoKeys.ContainsKey(key))
        {
            throw new System.Exception("Invalid request for key");
        }
        var o = pianoKeys[key];
        var edge = o.transform.position + o.transform.forward * (key.color == KeyColor.White ? whiteKey : blackKey).transform.localScale.z / 2;
        var up = o.transform.position + pianoKeys[key].transform.forward * 0.1f;
        var front = o.transform.position + pianoKeys[key].transform.up * 0.1f;
        var lookat = o.transform.position + (o.transform.forward * 5f + o.transform.up * 1f);
        return (lookat - o.transform.position).normalized;
    }

    public Vector3 GetHorizontalVectorForKey(PianoKey key)
    {
        if (!pianoKeys.ContainsKey(key))
        {
            throw new System.Exception("Invalid request for key");
        }
        return pianoKeys[key].transform.right;
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
            // Locking the piano in place
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
        if (Input.GetKeyDown(KeyCode.Return))
        {
            sequencer.SpawnNotes();
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
}
