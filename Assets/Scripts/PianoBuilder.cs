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
    private List<GameObject> spaceCraftObj = new List<GameObject>();
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
    private int totalNumberOfSpeaceShips = 0;

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
                Debug.LogWarning("No sequencer component.");
            }
        }
        if (Input.GetKeyDown(KeyCode.H)) // Hide virtual piano keys
        {
            pianoKeys.Values.ToList().ForEach(o => o.GetComponent<MeshRenderer>().enabled = hidden);
            hidden = !hidden;
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            FillUp(0.55f);
        }
    }

    public void FillUp(float percent)
    {
        Debug.Log("Fill up " + percent);
        var newPercent = fillUpPercent + percent;
        if (newPercent > 1.0f)
        {
            var didDestroy = false;
            for (int i = 0; i < spaceCraftObj.Count; i++)
            {
                var o = spaceCraftObj[i].GetComponent<SpaceCraftControl>();
                if (o.isDestoryed())
                {
                    continue;
                }
                else
                {
                    o.StagedDestory();
                    didDestroy = true;
                    break;
                }
            }
            if (!didDestroy)
            {
                PlaceSpacecraft();
            }
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
            if (RuntimeSettings.IS_PLAY_MODE)
            {
                spawnGameElements();
            }
            DrawAuxillaryLines();
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
        DrawPulser();
        PlaceParticleSystems();
        PlaceSpacecraft(true);
    }

    private void PlaceSpacecraft(bool drawBar = false)
    {
        var obj = Instantiate(spaceCraft);
        var midKey = pianoKeys[PianoKeys.GetKeyFor(CENTRE)];
        var lmr = GetLMRAwayVectorsForKey(PianoKeys.GetKeyFor(CENTRE), 0.1f);
        if (spaceCraftObj.Count == 0)
        {
            obj.transform.position = lmr.away + new Vector3(0f, 0.5f, 0f);
        }
        else if (spaceCraftObj.Count == 1)
        {
            spaceCraftObj.ForEach(e => e.GetComponent<SpaceCraftControl>().RestoreAll());
            obj.transform.position = lmr.away + new Vector3(0.2f, 0.5f, 0.0f);
        }
        else if (spaceCraftObj.Count == 2)
        {
            spaceCraftObj.ForEach(e => e.GetComponent<SpaceCraftControl>().RestoreAll());
            obj.transform.position = lmr.away + new Vector3(-0.2f, 0.5f, 0.0f);
        }
        else
        {
            spaceCraftObj.ForEach(e => e.GetComponent<SpaceCraftControl>().RestoreAll());
        }
        var rotation = Quaternion.LookRotation(lmr.centre - lmr.away);
        obj.transform.rotation = rotation;
        obj.transform.Rotate(-90f, 180f, 0f);
        var dummy = new GameObject();
        dummy.transform.SetParent(this.transform);
        obj.transform.SetParent(dummy.transform);
        spaceCraftObj.Add(obj);

        if (drawBar)
        {
            energyBarObj = Instantiate(energyBar);
            energyBarObj.transform.position = lmr.away + new Vector3(0.0f, -0.1f, -0.7f);
            energyBarObj.transform.Rotate(-45f, 0, 0);
            //energyBarObj.transform.rotation = rotation;
            energyBarObj.transform.SetParent(dummy.transform);
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
        Debug.Log("Total: " + total + ", misses: " + totalmiss);

        if (!RuntimeSettings.IS_PLAY_MODE)
        {
            return;
        }
        var missPercentage = totalmiss / (double)total;

        if (missPercentage < 0.15d)
        {
            showText("Perfection!");
            FillUp(0.5f);
        }
        else if (missPercentage < 0.3d)
        {
            showText("Godlike");
            FillUp(0.3f);
        }
        else if (missPercentage < 0.4d)
        {
            showText("Impressive");
            FillUp(0.2f);
        }
        else if (missPercentage < 0.55d)
        {
            showText("Great");
            FillUp(0.15f);
        }
        else if (missPercentage < 0.8d)
        {
            showText("Good");
            FillUp(0.1f);
        }
        else
        {
            showText("Decent");
            FillUp(0.05f);
        }
    }

    public void showText(string text, int fontSize = 100, bool destroy = true)
    {
        var obj = Instantiate(textObj);
        var midKey = pianoKeys[PianoKeys.GetKeyFor(CENTRE)];
        obj.transform.SetParent(midKey.transform);
        obj.name = "Instant feedback text";
        obj.GetComponent<TextMesh>().text = text;
        obj.GetComponent<TextMesh>().fontSize = 500;
        obj.transform.localScale = Vector3.one * 0.1f;
        obj.transform.localEulerAngles = Vector3.zero;
        obj.transform.localPosition = new Vector3(0, 10, 1);

        if (destroy)
        {
            StartCoroutine(SetDelayedDestory(obj, 0.25f));
        }
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
            line.GetComponent<MeshRenderer>().material.color = Sequencer.colorDict[item.Key];
            var di = Instantiate(disk);
            di.transform.position = v.centre;
            di.transform.Rotate(-45f, 0, 0);
            diskDict[item.Key] = di;
            var dummy = new GameObject();
            dummy.transform.SetParent(item.Value.transform);
            line.transform.SetParent(dummy.transform);
            di.transform.SetParent(dummy.transform);
        }
    }

    public void UpdateDiskColor(PianoKey key, Color color)
    {
        diskDict[key].GetComponent<MeshRenderer>().material.color = color;
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
        return refV + (transform.forward * adj * magnitude + transform.up * opposite * magnitude);
    }

    private Vector3 MakeAwayVector(Transform transform)
    {
        return transform.position + (transform.forward * adj + transform.up * opposite);
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
