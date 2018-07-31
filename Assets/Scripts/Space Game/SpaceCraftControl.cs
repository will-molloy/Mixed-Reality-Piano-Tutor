using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SpaceCraftControl : MonoBehaviour {

    [SerializeField]
	private GameObject plasmaExplosionEffect;
    [SerializeField]
	private GameObject normalExplosionEffect;
    [SerializeField]
	private GameObject destoryableLeftWingObj1;
    [SerializeField]
	private GameObject destoryableLeftWingObj2;
    [SerializeField]
	private GameObject destoryableLeftWingObj3;
    [SerializeField]
	private GameObject destoryableLeftWingObj4;
    [SerializeField]
	private GameObject destoryableLeftWingObj5;
    [SerializeField]
	private GameObject destoryableLeftEngine;
    [SerializeField]
	private GameObject destoryableMiddleEngine;
    [SerializeField]
	private GameObject destoryableRightWingObj1;
    [SerializeField]
	private GameObject destoryableRightWingObj2;
    [SerializeField]
	private GameObject destoryableRightWingObj3;
    [SerializeField]
	private GameObject destoryableRightWingObj4;
    [SerializeField]
	private GameObject destoryableRightWingObj5;
    [SerializeField]
	private GameObject destoryableRightEngine;
    [SerializeField]
	private GameObject whereToPutSmoke1;
    [SerializeField]
	private GameObject whereToPutSmoke2;

    [SerializeField]
	private GameObject smoke;
    [SerializeField]
	private GameObject fire;
    [SerializeField]
	private Image hpBarFill;
	private List<GameObject> lefts;
	private List<GameObject> rights;

	private List<GameObject> smokeObjs;
	private List<GameObject> fireObjs;


	public readonly int MAX_DESOTRY_STAGE = 5;
	private int destoryStage;

	// Use this for initialization
	void Start () {
		this.smokeObjs = new List<GameObject>();
		this.fireObjs= new List<GameObject>();
		this.lefts = new List<GameObject>();
		this.rights = new List<GameObject>();

		this.lefts.Add(destoryableLeftWingObj1);
		this.lefts.Add(destoryableLeftWingObj2);
		this.lefts.Add(destoryableLeftWingObj3);
		this.lefts.Add(destoryableLeftWingObj4);
		this.lefts.Add(destoryableLeftWingObj5);

		this.rights.Add(destoryableRightWingObj1);
		this.rights.Add(destoryableRightWingObj2);
		this.rights.Add(destoryableRightWingObj3);
		this.rights.Add(destoryableRightWingObj4);
		this.rights.Add(destoryableRightWingObj5);

		this.destoryStage = MAX_DESOTRY_STAGE;
		
	}
	
	// Update is called once per frame
	void Update () {
	}

	private void SetHpBarAt(float percentage) {
		hpBarFill.fillAmount = percentage;
	}

	private void PlaceFireAt(GameObject go) {
		var obj = Instantiate(fire);
		obj.GetComponent<ParticleSystem>().enableEmission = true;
		obj.transform.SetParent(go.transform);
		obj.transform.localPosition = Vector3.zero;
		this.fireObjs.Add(obj);
	}

	private void PlaceSmokeAt(GameObject go) {
		var obj = Instantiate(smoke);
		obj.GetComponent<ParticleSystem>().enableEmission = true;
		obj.transform.SetParent(go.transform);
		obj.transform.localPosition = Vector3.zero;
		this.smokeObjs.Add(obj);
	}

	public void Explode(bool plasma = false) {
		var obj = Instantiate(plasma ? this.plasmaExplosionEffect : this.normalExplosionEffect);
		obj.GetComponent<ParticleSystem>().enableEmission = true;
		obj.transform.SetParent(this.transform);
		obj.transform.localPosition = Vector3.zero;
		StartCoroutine(SetDelayedDestory(obj, 2));
	}

	private IEnumerator SetDelayedDestory(GameObject go, float time) {
		yield return new WaitForSeconds(time);
		Destroy(go);
	}

	public void DestoryLeftWing() {
		this.lefts.ForEach(e=> e.GetComponent<Renderer>().enabled = false);
	}

	public void DestoryRightWing() {
		this.rights.ForEach(e=> e.GetComponent<Renderer>().enabled = false);
	}

	public void DestoryLeftEngine() {
		DestoryEngine(destoryableLeftEngine);
	}
	public void DestoryRightEngine() {
		DestoryEngine(destoryableRightEngine);
	}

	public void DestoryMidEngine() {
		DestoryEngine(destoryableMiddleEngine);
	}

	private void RestoreEngine(GameObject which) {
		which.GetComponent<Renderer>().enabled = true;
		which.GetComponentInChildren<ParticleSystem>().enableEmission = true;
	}

	private void DestoryEngine(GameObject which) {
		which.GetComponent<Renderer>().enabled = false;
		which.GetComponentInChildren<ParticleSystem>().enableEmission = false;
	}

	public void StagedDestory() {
		switch (this.destoryStage) {
			case 5: 
				this.DestoryLeftWing();
				this.Explode();
				this.SetHpBarAt(0.8f);
				break;
			case 4: 
				this.DestoryLeftEngine();
				this.Explode(true);
				this.PlaceSmokeAt(whereToPutSmoke1);
				this.SetHpBarAt(0.65f);
				break;
			case 3: 
				this.DestoryRightEngine();
				this.Explode();
				this.SetHpBarAt(0.40f);
				break;
			case 2: 
				this.DestoryRightWing();
				this.Explode(true);
				this.PlaceFireAt(whereToPutSmoke2);
				this.SetHpBarAt(0.2f);
				break;
			case 1: 
				this.DestoryMidEngine();
				this.Explode(true);
				this.Explode(false);
				this.SetHpBarAt(0f);
				break;
		}
		this.destoryStage--;


	}

	public void RestoreAll() {
		this.destoryStage = MAX_DESOTRY_STAGE;
		this.SetHpBarAt(1f);
		RestoreEngine(this.destoryableLeftEngine);
		RestoreEngine(this.destoryableRightEngine);
		RestoreEngine(this.destoryableMiddleEngine);
		
		this.lefts.ForEach(e => e.GetComponent<Renderer>().enabled = true);
		this.rights.ForEach(e => e.GetComponent<Renderer>().enabled = true);
		this.smokeObjs.ForEach(e => Destroy(e));
		this.fireObjs.ForEach(e => Destroy(e));
		this.smokeObjs.Clear();
		this.fireObjs.Clear();

	}

}
