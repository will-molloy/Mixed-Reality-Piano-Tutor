using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

sealed public class PianoBuilder : MonoBehaviour
{
    [SerializeField] private GameObject textObj;
    [SerializeField] private GameObject whiteKey;
    [SerializeField] private GameObject blackKey;
    [SerializeField] private GameObject pulser;
    [SerializeField] private GameObject spaceCraft;
    [SerializeField] private GameObject fineLine;
    [SerializeField] private GameObject energyBar;
    [SerializeField] private GameObject disk;
    public static readonly int CENTRE = (PianoKeys.Last().keyNum + PianoKeys.First().keyNum) / 2;
    internal Dictionary<PianoKey, GameObject> pianoKeys;
    internal static readonly float yOffset = 0.001f;
    internal bool locked;
    private bool hidden;
    private GameObject energyBarObj;
    public static PianoBuilder instance;
    internal Sequencer sequencer;
    private readonly float opposite = 1f;
    private readonly float adj = 5f;
    private GameObject spaceCraftObj;
    private List<GameObject> auxLines = new List<GameObject>();
    private List<GameObject> pulsers = new List<GameObject>();
    private Dictionary<PianoKey, GameObject> particleSystems;
    [SerializeField]
    private GameObject particleSystem;
    [SerializeField]
    public float pianoKeyGap = 0.001f; // 1mm or so
    [SerializeField]
    internal Transform worldAnchor;

    private Dictionary<PianoKey, GameObject> diskDict = new Dictionary<PianoKey, GameObject>();
    private float fillUpPercent = 0f;

    void Start()
    {
        instance = this;
        sequencer = GetComponent<Sequencer>();
        pianoKeys = new Dictionary<PianoKey, GameObject>();
        particleSystems = new Dictionary<PianoKey, GameObject>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) // Restarting MIDI file sequencer
        {
            if (sequencer)
            {
                sequencer.StartGame();
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
        if (Input.GetKeyDown(KeyCode.N))
        {
            FillUp(0.55f);
        }
    }

    public void FillUp(float percent)
    {
        Debug.Log("Fill up" + percent);
        var newPercent = fillUpPercent + percent;
        if (newPercent > 1.0f)
        {
            spaceCraftObj.GetComponent<SpaceCraftControl>().StagedDestory(); // TODO: Fire lasers
            newPercent = newPercent - 1f;
        }
        var obs = energyBarObj.GetComponentsInChildren<Image>();
        foreach (var i in obs)
        {
            if (i.name == "FG")
            {
                i.fillAmount = newPercent;
                break;
            }
        }
        fillUpPercent = newPercent;
    }

    private IEnumerator AnimateScale(float newPercent, float speed, GameObject obj)
    {
        var obs = obj.GetComponentsInChildren<Image>();
        foreach (var i in obs)
        {
            if (i.name == "FG")
            {
                i.fillAmount = newPercent;
                break;
            }
        }
        return null;
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

    private bool isPianoBuilt;

    public void BuildPianoAsChildOfTransform(Transform transform)
    {
        if (isPianoBuilt)
        {
            Debug.LogWarning("Piano already built");
        }
        else
        {
            isPianoBuilt = true;
            Debug.Log("Building Piano.");
            var obj = new GameObject("Piano wrapper");
            obj.transform.position = Vector3.zero;
            BuildPianoAt(obj.transform.position);
            obj.transform.SetParent(transform);
            base.transform.SetParent(obj.transform);
            if (RuntimeSettings.isPlayMode)
            {
                spawnGameElements();
            }
            sequencer.LoadMidiFile();
        }
    }

    private void BuildPianoAt(Vector3 location)
    {
        this.transform.position = location;
        var firstkey = PianoKeys.First();
        var lastkey = PianoKeys.Last();
        var whitekeyScale = whiteKey.transform.localScale;
        var blackKeyScale = blackKey.transform.localScale;
        var currentX = 0f;
        var blackKeyYOffset = (whitekeyScale.y + blackKeyScale.y) / 2;
        var blackKeyZOffset = (whitekeyScale.z - blackKeyScale.z) / 2;
        var rightBlackKeyXOffset = 141f / 550 * (blackKeyScale.x / 2); // http://www.rwgiangiulio.com/construction/manual/
        var leftBlackKeyXOffset = -rightBlackKeyXOffset;
        for (int keyNum = firstkey.keyNum; keyNum <= lastkey.keyNum; keyNum++)
        {
            var currentKey = PianoKeys.GetKeyFor(keyNum);
            GameObject keyObj;
            if (currentKey.color == KeyColor.White)
            {
                keyObj = Instantiate(whiteKey);
                keyObj.transform.SetParent(this.transform);
                keyObj.transform.localPosition = new Vector3(currentX, 0, 0);
                currentX += whitekeyScale.x + pianoKeyGap;
            }
            else
            {
                float xOffset = -(whitekeyScale.x + pianoKeyGap) / 2; // Unity measures from midpoint of white key
                if (PianoKeys.leftOffsetBlackKeyNums.Contains(keyNum))
                {
                    xOffset += leftBlackKeyXOffset;
                }
                else if (PianoKeys.rightOffsetBlackKeyNums.Contains(keyNum))
                {
                    xOffset += rightBlackKeyXOffset;
                }
                keyObj = Instantiate(blackKey);
                keyObj.transform.SetParent(this.transform);
                keyObj.transform.localPosition = new Vector3(currentX + xOffset, blackKeyYOffset, blackKeyZOffset);
            }
            pianoKeys[currentKey] = keyObj;
        }
        var o = pianoKeys[firstkey];
        var l = pianoKeys[lastkey];
        var up = o.transform.position + pianoKeys[firstkey].transform.forward * 0.1f;
        var front = o.transform.position + pianoKeys[firstkey].transform.up * 0.1f;
        var lookat = MakeAwayVector(o.transform);
        Debug.DrawLine(o.transform.position, front, color: Color.red, duration: 99999f, depthTest: false);
        Debug.DrawLine(o.transform.position, up, color: Color.red, duration: 99999f, depthTest: false);
        Debug.DrawLine(o.transform.position, lookat.normalized, color: Color.green, duration: 99999f, depthTest: false);
    }

    private void spawnGameElements()
    {
        DrawAuxillaryLines();
        DrawPulser();
        PlaceParticleSystems();
        PlaceSpacecraft();
    }

    private void DrawEnergyBar()
    {
        energyBarObj = Instantiate(energyBar);
    }

    private void PlaceSpacecraft()
    {
        spaceCraftObj = Instantiate(spaceCraft);
        var midKey = pianoKeys[PianoKeys.GetKeyFor(CENTRE)];
        var lmr = GetLMRAwayVectorsForKey(PianoKeys.GetKeyFor(CENTRE), 0.2f);
        spaceCraftObj.transform.position = lmr.away + new Vector3(0f, 0.5f, 0f);
        var rotation = Quaternion.LookRotation(lmr.centre - lmr.away);
        spaceCraftObj.transform.rotation = rotation;
        spaceCraftObj.transform.Rotate(-90f, 180f, 0f);
        var dummy = new GameObject();
        dummy.transform.SetParent(this.transform);
        spaceCraftObj.transform.SetParent(dummy.transform);

        energyBarObj = Instantiate(energyBar);
        energyBarObj.transform.position = lmr.away + new Vector3(0f, 0.3f, 0.05f);
        energyBarObj.transform.rotation = rotation;
        energyBarObj.transform.SetParent(dummy.transform);
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
            var dummy = new GameObject();
            dummy.transform.SetParent(this.transform);
            obj.transform.SetParent(this.transform);
        }
    }

    private void DrawPulser()
    {
        var firstkey = PianoKeys.First();
        var lastkey = PianoKeys.Last();
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
        var dummy = new GameObject();
        dummy.transform.SetParent(this.transform);
        lp.transform.SetParent(dummy.transform);
        rp.transform.SetParent(dummy.transform);
    }

    public void Pulse()
    {
        foreach (var pulser in this.pulsers)
        {
            var p = pulser.GetComponent<Pulser>();
            p.Pulse();
        }
    }

    public void PutInstantFeedback(int total, int totalmiss, int totalBeats)
    {
        if (!RuntimeSettings.isPlayMode)
        {
            return;
        }
        float missPercentage = totalmiss / (float)total;

        var obj = Instantiate(textObj);
        string text;
        if (missPercentage > 0.9f)
        {
            text = "PURRFECT";
            FillUp(0.45f);
        }
        else if (missPercentage > 0.6f)
        {
            text = "GURRET";
            FillUp(0.25f);
        }
        else if (missPercentage > 0.4f)
        {
            text = "GUUUD";
            FillUp(0.15f);
        }
        else
        {
            text = "SH!T";
        }
        obj.GetComponent<TextMesh>().text = text;
        var midKey = pianoKeys[PianoKeys.GetKeyFor(CENTRE)];
        var lmr = GetLMRAwayVectorsForKey(PianoKeys.GetKeyFor(CENTRE), 0.1f);
        obj.transform.position = lmr.away + new Vector3(0f, 0.05f, 0f);
        var rotation = Quaternion.LookRotation(lmr.centre - lmr.away);
        obj.transform.rotation = rotation;
        obj.transform.Rotate(0, 180f, 0f);

        StartCoroutine(SetDelayedDestory(obj, 0.3f));
    }

    private IEnumerator SetDelayedDestory(GameObject go, float time)
    {
        yield return new WaitForSeconds(time);
        DestroyObject(go);
    }

    public static float calcX(float y)
    {
        return Mathf.Sqrt((y * y) / 26f);
    }
    private void DrawAuxillaryLines()
    {
        foreach (var item in pianoKeys)
        {
            var line = Instantiate(fineLine);
            line.name = "Aux line";
            var v = GetLMRAwayVectorsForKey(item.Key, calcX(2f));
            line.transform.position = v.away;
            var rotation = Quaternion.LookRotation(v.centre - v.away);
            line.transform.rotation = rotation;
            line.transform.Rotate(90f, 0f, 0f);
            this.auxLines.Add(line);
            if (item.Key.color == KeyColor.Black) {
                line.GetComponent<MeshRenderer>().material.color = Color.blue;
            }
            var di = Instantiate(disk);
            di.transform.position = v.centre;
            diskDict[item.Key] = di;
            var dummy = new GameObject();
            dummy.transform.SetParent(item.Value.transform);
            line.transform.SetParent(dummy.transform);
            di.transform.SetParent(dummy.transform);
        }
    }

    public void UpdateDiskColor(PianoKey key, float maxDist) {
        diskDict[key].GetComponent<MeshRenderer>().material.color = Color.HSVToRGB(1f, maxDist, 1f);
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
        var lookat = MakeAwayVector(mid, this.transform, magnitude);
        return new PianoKeyVectors(corners[0], corners[1], corners[2], lookat, go.transform.forward, go.transform.up);
    }

    public GameObject DrawLine(Vector3 start, Vector3 end, Color color)
    {
        var myLine = new GameObject("Auxillary line");
        myLine.transform.localPosition = start;
        myLine.AddComponent<LineRenderer>();
        var lr = myLine.GetComponent<LineRenderer>();
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

    private Vector3 MakeAwayVector(Transform transform)
    {
        var lookat = transform.position + (transform.forward * adj + transform.up * opposite);
        return lookat;
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

    public void ActivateKey(int keyNum, Color color, float durationSeconds = -1f)
    {
        var pianoKey = PianoKeys.GetKeyFor(keyNum);
        GameObject gameObject;
        if (pianoKeys.TryGetValue(pianoKey, out gameObject))
        {
            var render = gameObject.GetComponent<MeshRenderer>();
            render.material.color = color;
            if (durationSeconds > 0)
            {
                StartCoroutine(DeactivateKey(gameObject, pianoKey, durationSeconds));
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

    public GameObject GetKeyObj(PianoKey key)
    {
        return pianoKeys[key];
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
