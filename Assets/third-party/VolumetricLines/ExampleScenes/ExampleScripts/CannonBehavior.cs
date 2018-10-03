using UnityEngine;

public class CannonBehavior : MonoBehaviour
{
    public Transform m_cannonRot;
    public Texture2D m_guiTexture;
    public Transform m_muzzle;
    public GameObject m_shotPrefab;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow)) m_cannonRot.transform.Rotate(Vector3.up, -Time.deltaTime * 100f);
        if (Input.GetKey(KeyCode.RightArrow)) m_cannonRot.transform.Rotate(Vector3.up, Time.deltaTime * 100f);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var go = Instantiate(m_shotPrefab, m_muzzle.position, m_muzzle.rotation);
            Destroy(go, 3f);
        }
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(0f, 0f, m_guiTexture.width / 2, m_guiTexture.height / 2), m_guiTexture);
    }
}