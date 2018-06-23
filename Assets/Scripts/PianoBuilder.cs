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

    private readonly float opposite = 1f;
    private readonly float adj = 5f;

    private List<GameObject> auxLines;

    void Start()
    {
        instance = this;
        pianoKeys = new Dictionary<PianoKey, GameObject>();
        auxLines = new List<GameObject>();
        sequencer = GetComponent<Sequencer>();
        if (!needsCameraHook)
        {
            Debug.Log("Building Piano with saved position. (TODO)");
            // PlacePianoInfrontOfTransform(); -- TODO save transform location to disk 
        }
    }

    public void PlacePianoInfrontOfTransform(Transform trf)
    {
        if (!pianoIsBuilt)
        {
            Debug.Log("Building Piano.");
            PlacePianoAt(trf.position + trf.forward * 0.5f);
            var sequencer = Sequencer.instance;
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
                keyObj.transform.localPosition = new Vector3(xOffset + whitekeyScale.x / 2, yOffset, zOffset);
            }
            pianoKeys[currentKey] = keyObj;
        }
        pianoIsBuilt = true;
        //up.transform.SetParent(pianoKeys[firstkey].transform);
        //front.transform.SetParent(pianoKeys[firstkey].transform);
        var o = pianoKeys[firstkey];
        var l = pianoKeys[lastkey];
        var up = o.transform.position + pianoKeys[firstkey].transform.forward * 0.1f;
        var front = o.transform.position + pianoKeys[firstkey].transform.up * 0.1f;
        var lookat = MakeAwayVector(o.transform);
        Debug.DrawLine(o.transform.position, front, color: Color.red, duration: 99999f, depthTest: false);
        Debug.DrawLine(o.transform.position, up, color: Color.red, duration: 99999f, depthTest: false);
        Debug.DrawLine(o.transform.position, lookat.normalized, color: Color.green, duration: 99999f, depthTest: false);
    }

    private void DrawAuxillaryLines() {
        foreach (var item in pianoKeys)
        {
            var lmraway = GetLMRAwayVectorsForKey(item.Key);
            DrawLine(lmraway.centre, lmraway.away , Color.grey);
        }
    }    

    private void DeleteAuxillaryLines() {
        this.auxLines.ForEach(e => Destroy(e));
        this.auxLines.Clear();
    }

    public PianoKeyVectors GetLMRAwayVectorsForKey(PianoKey key) {
        if (!pianoKeys.ContainsKey(key))
        {
            throw new System.Exception("Invalid request for key");
        }
        var go = pianoKeys[key];
        var corners = Corners(pianoKeys[key]);
        var mid = corners[2];
        var empty = new GameObject();
        empty.transform.SetParent(go.transform);
        empty.transform.position = mid;
        empty.transform.SetParent(this.transform);
        var lookat = MakeAwayVector(empty.transform, 10);
        Destroy(empty);
        return new PianoKeyVectors(corners[0], corners[2], corners[1], lookat);
    }

    public static void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.SetColors(color, color);
        lr.SetWidth(0.001f, 0.001f);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    private Vector3 MakeAwayVector(Transform transform, float magnitude) {
        var lookat = transform.position + (transform.forward * adj * magnitude + transform.up * opposite * magnitude);
        return lookat;
    }

    private Vector3 MakeAwayVector(Transform transform) {
        var lookat = transform.position + (transform.forward * adj + transform.up * opposite);
        return lookat;
    }


    public Vector3 GetLeftEdge() {
        var obj = pianoKeys[PianoKeys.GetFirstKey()];
        var corners = Corners(obj);
        return corners[1];
    }

    public Vector3 GetRightEdge() {
        var obj = pianoKeys[PianoKeys.GetLastKey()];
        var corners = Corners(obj);
        return corners[0];
    }

    private static List<Vector3> Corners(GameObject go)
    {
        float width = go.GetComponent<Renderer>().bounds.size.x;
        float height = go.GetComponent<Renderer>().bounds.size.z;

        Vector3 topRight = go.transform.position, topLeft = go.transform.position, topMid = go.transform.position;

        topRight.x += width / 2;
        topRight.z += height / 2;

        topLeft.x -= width / 2;
        topLeft.z += height / 2;

        topMid.z += height / 2;

        List<Vector3> cor_temp = new List<Vector3>();
        cor_temp.Add(topRight);
        cor_temp.Add(topLeft);
        cor_temp.Add(topMid);

        return cor_temp;
    }

    public Vector3 GetScaleForKey(PianoKey key)
    {
        if (!pianoKeys.ContainsKey(key))
        {
            throw new System.Exception("Invalid request for key");
        }
        return pianoKeys[key].transform.lossyScale;
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
        var lookat = MakeAwayVector(o.transform);
        // Angle = tan(up / forward)
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

    // Update is called once per frame
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
            var forward = this.transform.forward;
            var up = this.transform.up;
            var right = this.transform.right;
            if (Input.GetKey(KeyCode.A))
            {
                position -= this.transform.right * 0.001f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                position += this.transform.right * 0.001f;
            }
            if (Input.GetKey(KeyCode.W))
            {
                position += this.transform.forward * 0.001f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                position -= this.transform.forward * 0.001f;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                position += this.transform.up * 0.001f;
            }
            if (Input.GetKey(KeyCode.E))
            {
                position -= this.transform.up * 0.001f;
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
                // Unlock it
                locked = false;
                DeleteAuxillaryLines();
            }
            else
            {
                // Lock it in place and set a text
                locked = true;
                DrawAuxillaryLines();
                lockedTextObj = Instantiate(lockedText);
                lockedTextObj.transform.SetParent(this.transform);
                lockedTextObj.transform.localPosition = pianoKeys[PianoKeys.GetKeyFor(CENTRE)].transform.localPosition + new Vector3(0f, 0.1f, 0f);
            }
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Spawn 
            sequencer.LoadMidiFile();
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

public struct PianoKeyVectors {
    public PianoKeyVectors(Vector3 topLeft, Vector3 topRight, Vector3 topMid, Vector3 away) {
        this.topLeft = topLeft;
        this.topRight = topRight;
        this.centre = topMid;
        this.away = away;
    }

    public Vector3 topLeft {get;}
    public Vector3 topRight {get;}
    public Vector3 centre {get;}

    public Vector3 away {get;}
}
