using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class SpaceCraftControl : MonoBehaviour
    {
        public static readonly float twopirad = 2 * Mathf.PI;

        public readonly int MAX_DESOTRY_STAGE = 5;

        [SerializeField] private GameObject destoryableLeftEngine;

        [SerializeField] private GameObject destoryableLeftWingObj1;

        [SerializeField] private GameObject destoryableLeftWingObj2;

        [SerializeField] private GameObject destoryableLeftWingObj3;

        [SerializeField] private GameObject destoryableLeftWingObj4;

        [SerializeField] private GameObject destoryableLeftWingObj5;

        [SerializeField] private GameObject destoryableMiddleEngine;

        [SerializeField] private GameObject destoryableRightEngine;

        [SerializeField] private GameObject destoryableRightWingObj1;

        [SerializeField] private GameObject destoryableRightWingObj2;

        [SerializeField] private GameObject destoryableRightWingObj3;

        [SerializeField] private GameObject destoryableRightWingObj4;

        [SerializeField] private GameObject destoryableRightWingObj5;

        private int destoryStage;

        [SerializeField] private GameObject fire;

        private List<GameObject> fireObjs;
        private float floatingVar;

        [SerializeField] private Image hpBarFill;

        private List<GameObject> lefts;

        [SerializeField] private GameObject normalExplosionEffect;

        [SerializeField] private GameObject plasmaExplosionEffect;

        private List<GameObject> rights;
        private float savedY;

        [SerializeField] private GameObject smoke;

        private List<GameObject> smokeObjs;

        [SerializeField] private GameObject whereToPutSmoke1;

        [SerializeField] private GameObject whereToPutSmoke2;

        // Use this for initialization
        private void Start()
        {
            smokeObjs = new List<GameObject>();
            fireObjs = new List<GameObject>();
            lefts = new List<GameObject>();
            rights = new List<GameObject>();

            lefts.Add(destoryableLeftWingObj1);
            lefts.Add(destoryableLeftWingObj2);
            lefts.Add(destoryableLeftWingObj3);
            lefts.Add(destoryableLeftWingObj4);
            lefts.Add(destoryableLeftWingObj5);

            rights.Add(destoryableRightWingObj1);
            rights.Add(destoryableRightWingObj2);
            rights.Add(destoryableRightWingObj3);
            rights.Add(destoryableRightWingObj4);
            rights.Add(destoryableRightWingObj5);

            destoryStage = MAX_DESOTRY_STAGE;
            savedY = transform.localPosition.y;
        }

        // Update is called once per frame
        private void Update()
        {
            var floatY = Mathf.Sin(floatingVar) * 0.01f;
            transform.localPosition = new Vector3(transform.localPosition.x, savedY + floatY, transform.localPosition.z);
            floatingVar += 0.1f;
            if (floatingVar > twopirad) floatingVar = 0f;
        }

        public bool isDestoryed()
        {
            return destoryStage == 0;
        }

        private void SetHpBarAt(float percentage)
        {
            hpBarFill.fillAmount = percentage;
        }

        private void PlaceFireAt(GameObject go)
        {
            var obj = Instantiate(fire);
            obj.GetComponent<ParticleSystem>().enableEmission = true;
            obj.transform.SetParent(go.transform);
            obj.transform.localPosition = Vector3.zero;
            fireObjs.Add(obj);
        }

        private void PlaceSmokeAt(GameObject go)
        {
            var obj = Instantiate(smoke);
            obj.GetComponent<ParticleSystem>().enableEmission = true;
            obj.transform.SetParent(go.transform);
            obj.transform.localPosition = Vector3.zero;
            smokeObjs.Add(obj);
        }

        public void Explode(bool plasma = false)
        {
            var obj = Instantiate(plasma ? plasmaExplosionEffect : normalExplosionEffect);
            obj.GetComponent<ParticleSystem>().enableEmission = true;
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;
            StartCoroutine(SetDelayedDestory(obj, 2));
        }

        private IEnumerator SetDelayedDestory(GameObject go, float time)
        {
            yield return new WaitForSeconds(time);
            Destroy(go);
        }

        public void DestoryLeftWing()
        {
            lefts.ForEach(e => e.GetComponent<Renderer>().enabled = false);
        }

        public void DestoryRightWing()
        {
            rights.ForEach(e => e.GetComponent<Renderer>().enabled = false);
        }

        public void DestoryLeftEngine()
        {
            DestoryEngine(destoryableLeftEngine);
        }

        public void DestoryRightEngine()
        {
            DestoryEngine(destoryableRightEngine);
        }

        public void DestoryMidEngine()
        {
            DestoryEngine(destoryableMiddleEngine);
        }

        private void RestoreEngine(GameObject which)
        {
            which.GetComponent<Renderer>().enabled = true;
            which.GetComponentInChildren<ParticleSystem>().enableEmission = true;
        }

        private void DestoryEngine(GameObject which)
        {
            which.GetComponent<Renderer>().enabled = false;
            which.GetComponentInChildren<ParticleSystem>().enableEmission = false;
        }

        public void StagedDestory()
        {
            switch (destoryStage)
            {
                case 5:
                    DestoryLeftWing();
                    Explode();
                    SetHpBarAt(0.8f);
                    break;
                case 4:
                    DestoryLeftEngine();
                    Explode(true);
                    PlaceSmokeAt(whereToPutSmoke1);
                    SetHpBarAt(0.65f);
                    break;
                case 3:
                    DestoryRightEngine();
                    Explode();
                    SetHpBarAt(0.40f);
                    break;
                case 2:
                    DestoryRightWing();
                    Explode(true);
                    PlaceFireAt(whereToPutSmoke2);
                    SetHpBarAt(0.2f);
                    break;
                case 1:
                    DestoryMidEngine();
                    Explode(true);
                    Explode(false);
                    SetHpBarAt(0f);
                    break;
            }

            destoryStage--;
        }

        public void RestoreAll()
        {
            destoryStage = MAX_DESOTRY_STAGE;
            SetHpBarAt(1f);
            RestoreEngine(destoryableLeftEngine);
            RestoreEngine(destoryableRightEngine);
            RestoreEngine(destoryableMiddleEngine);

            lefts.ForEach(e => e.GetComponent<Renderer>().enabled = true);
            rights.ForEach(e => e.GetComponent<Renderer>().enabled = true);
            smokeObjs.ForEach(e => Destroy(e));
            fireObjs.ForEach(e => Destroy(e));
            smokeObjs.Clear();
            fireObjs.Clear();
        }
    }
}