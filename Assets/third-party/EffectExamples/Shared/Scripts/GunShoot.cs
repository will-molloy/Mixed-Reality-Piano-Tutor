using UnityEngine;

public class GunShoot : MonoBehaviour
{
    private Animator anim;
    public ParticleSystem cartridgeEjection;

    public float fireRate = 0.25f; // Number in seconds which controls how often the player can fire
    public GameObject[] fleshHitEffects;
    private GunAim gunAim;

    public Transform gunEnd;

    public GameObject metalHitEffect;
    public ParticleSystem muzzleFlash;

    private float nextFire; // Float to store the time the player will be allowed to fire again, after firing
    public GameObject sandHitEffect;
    public GameObject stoneHitEffect;
    public GameObject waterLeakEffect;
    public GameObject waterLeakExtinguishEffect;
    public float weaponRange = 20f; // Distance in Unity units over which the player can fire
    public GameObject woodHitEffect;

    private void Start()
    {
        anim = GetComponent<Animator>();
        gunAim = GetComponentInParent<GunAim>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.time > nextFire && !gunAim.GetIsOutOfBounds())
        {
            nextFire = Time.time + fireRate;
            muzzleFlash.Play();
            cartridgeEjection.Play();
            anim.SetTrigger("Fire");

            var rayOrigin = gunEnd.position;
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, gunEnd.forward, out hit, weaponRange)) HandleHit(hit);
        }
    }

    private void HandleHit(RaycastHit hit)
    {
        if (hit.collider.sharedMaterial != null)
        {
            var materialName = hit.collider.sharedMaterial.name;

            switch (materialName)
            {
                case "Metal":
                    SpawnDecal(hit, metalHitEffect);
                    break;
                case "Sand":
                    SpawnDecal(hit, sandHitEffect);
                    break;
                case "Stone":
                    SpawnDecal(hit, stoneHitEffect);
                    break;
                case "WaterFilled":
                    SpawnDecal(hit, waterLeakEffect);
                    SpawnDecal(hit, metalHitEffect);
                    break;
                case "Wood":
                    SpawnDecal(hit, woodHitEffect);
                    break;
                case "Meat":
                    SpawnDecal(hit, fleshHitEffects[Random.Range(0, fleshHitEffects.Length)]);
                    break;
                case "Character":
                    SpawnDecal(hit, fleshHitEffects[Random.Range(0, fleshHitEffects.Length)]);
                    break;
                case "WaterFilledExtinguish":
                    SpawnDecal(hit, waterLeakExtinguishEffect);
                    SpawnDecal(hit, metalHitEffect);
                    break;
            }
        }
    }

    private void SpawnDecal(RaycastHit hit, GameObject prefab)
    {
        var spawnedDecal = Instantiate(prefab, hit.point, Quaternion.LookRotation(hit.normal));
        spawnedDecal.transform.SetParent(hit.collider.transform);
    }
}