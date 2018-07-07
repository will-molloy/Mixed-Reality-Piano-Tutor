using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PianoBuilder : MonoBehaviour
{

    [SerializeField]
    private GameObject whiteKey;
    [SerializeField]
    private GameObject blackKey;
    [SerializeField]
    private GameObject pulser;
    [SerializeField]
    private GameObject lockedText;
    [SerializeField]
    private GameObject spaceCraft;
    public static readonly int CENTRE = (PianoKeys.GetLastKey().keyNum + PianoKeys.GetFirstKey().keyNum) / 2;
    internal Dictionary<PianoKey, GameObject> pianoKeys;
    internal static readonly float yOffset = 0.001f;
    internal bool locked = false;
    internal bool pianoIsBuilt = false;
    private bool hidden = false;
    internal GameObject lockedTextObj;
    public static PianoBuilder instance;
    internal Sequencer sequencer;
    private readonly float opposite = 1f;
    private readonly float adj = 5f;
    private List<GameObject> auxLines;
    private List<GameObject> pulsers;
    private Dictionary<PianoKey, GameObject> particleSystems;

    [SerializeField]
    private GameObject particleSystem;

    void Start()
    {
        instance = this;
        pianoKeys = new Dictionary<PianoKey, GameObject>();
        auxLines = new List<GameObject>();
        sequencer = GetComponent<Sequencer>();
        pulsers = new List<GameObject>();
        particleSystems = new Dictionary<PianoKey, GameObject>();

    }

    public void PlacePianoInfrontOfTransform(Transform trf)
    {
        if (!pianoIsBuilt)
        {
            Debug.Log("Building Piano.");
            PlacePianoAt(trf.position + trf.forward * 0.5f);
        }
        else
        {
            Debug.Log("Piano already built.");
        }
    }

    private void PlaceParticleSystems()
    {
        foreach (var item in pianoKeys)
        {
            var lmraway = GetLMRAwayVectorsForKey(item.Key);
            var obj = Instantiate(particleSystem);
            obj.transform.position = lmraway.centre;
            obj.GetComponent<ParticleSystem>().enableEmission = false;
            particleSystems.Add(item.Key, obj);
        }
    }

    private void DestoryParticleSystems()
    {
        foreach (var item in particleSystems)
        {
            Destroy(item.Value);
        }
        particleSystems.Clear();
    }

    public void SetParticleSystemStatusForKey(PianoKey key, bool status)
    {
        var o = particleSystems[key];
        if (o != null)
        {
            var ps = o.GetComponent<ParticleSystem>();
            ps.enableEmission = status;
        }

    }

    public void SetPosition(Vector3 location)
    {
        this.transform.position = location;
    }

    private void PlacePianoAt(Vector3 location)
    {
        SetPosition(location);
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
        var l = pianoKeys[lastkey];
        var up = o.transform.position + pianoKeys[firstkey].transform.forward * 0.1f;
        var front = o.transform.position + pianoKeys[firstkey].transform.up * 0.1f;
        var lookat = MakeAwayVector(o.transform);
        Debug.DrawLine(o.transform.position, front, color: Color.red, duration: 99999f, depthTest: false);
        Debug.DrawLine(o.transform.position, up, color: Color.red, duration: 99999f, depthTest: false);
        Debug.DrawLine(o.transform.position, lookat.normalized, color: Color.green, duration: 99999f, depthTest: false);
    }

    private void DrawPulser()
    {
        var firstkey = PianoKeys.GetFirstKey();
        var lastkey = PianoKeys.GetLastKey();
        var left = this.pianoKeys[firstkey].transform;
        var right = this.pianoKeys[lastkey].transform;
        var leftPulserPos = left.transform.position - left.transform.right * 0.1f;
        var rightPulserPos = right.transform.position + right.transform.right * 0.1f;
        var lp = Instantiate(pulser);
        var rp = Instantiate(pulser);
        lp.transform.position = leftPulserPos;
        rp.transform.position = rightPulserPos;
        this.pulsers.Add(lp);
        this.pulsers.Add(rp);
    }

    private void DeletePulser()
    {
        foreach (var item in this.pulsers)
        {
            Destroy(item);
        }

        pulsers.Clear();

    }

    public void Pulse()
    {
        foreach (var pulser in this.pulsers)
        {
            var p = pulser.GetComponent<Pulser>();
            p.Pulse();
        }
    }

    private void DrawAuxillaryLines()
    {
        foreach (var item in pianoKeys)
        {
            var lmraway = GetLMRAwayVectorsForKey(item.Key, 10);
            this.auxLines.Add(DrawLine(lmraway.centre, lmraway.away, Color.grey));
        }
    }

    private void DeleteAuxillaryLines()
    {
        this.auxLines.ForEach(e => Destroy(e));
        this.auxLines.Clear();
    }

    public PianoKeyVectors GetLMRAwayVectorsForKey(PianoKey key, float magnitude = 1f)
    {
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
        var lookat = MakeAwayVector(mid, this.transform, magnitude);
        Destroy(empty);
        return new PianoKeyVectors(corners[0], corners[1], corners[2], lookat, go.transform.forward, go.transform.up);
    }


    public static GameObject DrawLine(Vector3 start, Vector3 end, Color color)
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
        return myLine;
    }

    private Vector3 MakeAwayVector(Vector3 refV, Transform transform, float magnitude = 1f)
    {
        var lookat = refV + (transform.forward * adj * magnitude + transform.up * opposite * magnitude);
        return lookat;

    }

    private Vector3 MakeAwayVector(Transform transform, float magnitude)
    {
        var lookat = transform.position + (transform.forward * adj * magnitude + transform.up * opposite * magnitude);
        return lookat;
    }

    private Vector3 MakeAwayVector(Transform transform)
    {
        var lookat = transform.position + (transform.forward * adj + transform.up * opposite);
        return lookat;
    }


    public Vector3 GetLeftEdge()
    {
        var obj = pianoKeys[PianoKeys.GetFirstKey()];
        var corners = Corners(obj);
        return corners[1];
    }

    public Vector3 GetRightEdge()
    {
        var obj = pianoKeys[PianoKeys.GetLastKey()];
        var corners = Corners(obj);
        return corners[0];
    }

    private static List<Vector3> Corners(GameObject go)
    {
        float width = go.GetComponent<Renderer>().bounds.size.x;
        float height = go.GetComponent<Renderer>().bounds.size.z;

        var scale = go.transform.lossyScale;

        Vector3 topRight = go.transform.position, topLeft = go.transform.position, topMid = go.transform.position;

        topRight += go.transform.forward * scale.z * 0.5f;
        topRight += go.transform.up * scale.y * 0.5f;
        topRight += go.transform.right * scale.x * 0.5f;


        topLeft += go.transform.forward * scale.z * 0.5f;
        topLeft += go.transform.up * scale.y * 0.5f;
        topLeft -= go.transform.right * scale.x * 0.5f;

        topMid += go.transform.forward * scale.z * 0.5f;
        topMid += go.transform.up * scale.y * 0.5f;

        List<Vector3> cor_temp = new List<Vector3>();
        cor_temp.Add(topLeft);
        cor_temp.Add(topRight);
        cor_temp.Add(topMid);

        return cor_temp;
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
        if (Input.GetKeyDown(KeyCode.Space)) // Locking the piano in place
        {
            if (lockedTextObj != null)
            {
                GameObject.Destroy(lockedTextObj);
            }
            if (locked)
            {
                locked = false;
                DeleteAuxillaryLines();
                DeletePulser();
                DestoryParticleSystems();
            }
            else
            {
                locked = true;
                DrawAuxillaryLines();
                DrawPulser();
                PlaceParticleSystems();
                var pulser = GameObject.FindGameObjectWithTag("Pulser");
                var p = pulser.GetComponent<SimpleSonarShader_Object>();
                p.StartSonarRing(p.transform.position, 10f);
                lockedTextObj = Instantiate(lockedText);
                lockedTextObj.transform.SetParent(this.transform);
                lockedTextObj.transform.localPosition = pianoKeys[PianoKeys.GetKeyFor(CENTRE)].transform.localPosition + new Vector3(0f, 0.1f, 0f);
            }
        }
        if (Input.GetKeyDown(KeyCode.Return)) // Restarting MIDI file sequencer
        {
            if (sequencer)
            {
                sequencer.LoadMidiFile();
                sequencer.SpawnNotes();
            }
            else
            {
                Debug.LogWarning("No sequencer component, you must be in calibration mode.");
            }
        }
        if (Input.GetKeyDown(KeyCode.H)) // Hide virtual piano keys
        {
            pianoKeys.Values.ToList().ForEach(o => o.GetComponent<MeshRenderer>().enabled = hidden);
            hidden = !hidden;
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            SetParticleSystemStatusForKey(PianoKeys.GetKeyFor(52), true);
        }

    }

    public void ActivateKey(int keyNum, Color color, float durationSeconds = -1f)
    {
        if (!pianoIsBuilt)
        {
            Debug.LogWarning("Piano not setup.");
        }
        else
        {
            var pianoKey = PianoKeys.GetKeyFor(keyNum);
            GameObject gameObject;
            if (pianoKeys.TryGetValue(pianoKey, out gameObject))
            {
                // Debug.Log("Activating key: " + pianoKey.keyNum + ", for: " + durationSeconds + "s");
                var render = gameObject.GetComponent<MeshRenderer>();
                render.material.color = color;
                if (durationSeconds > 0)
                {
                    StartCoroutine(DeactivateKey(gameObject, pianoKey, durationSeconds));
                }
            }
        }
    }

    private IEnumerator DeactivateKey(GameObject keyObj, PianoKey pianoKey, float duration)
    {
        yield return new WaitForSeconds(duration);
        var render = keyObj.GetComponent<MeshRenderer>();
        render.material.color = pianoKey.color == KeyColor.White ? Color.white : Color.black;
    }

    public void DeactivateKey(int keyNum)
    {
        ActivateKey(keyNum, PianoKeys.GetKeyFor(keyNum).color == KeyColor.White ? Color.white : Color.black);
    }

}

public struct PianoKeyVectors
{
    public PianoKeyVectors(Vector3 topLeft, Vector3 topRight, Vector3 topMid, Vector3 away, Vector3 forward, Vector3 up)
    {
        this.topLeft = topLeft;
        this.topRight = topRight;
        this.centre = topMid;
        this.away = away;
        this.forward = forward;
        this.up = up;
    }

    public Vector3 topLeft { get; }
    public Vector3 topRight { get; }
    public Vector3 centre { get; }
    public Vector3 away { get; }
    public Vector3 forward { get; }
    public Vector3 up { get; }

}
