using UnityEngine;

public class SunCorona : MonoBehaviour
{
    private Material mat;
    private Vector2 offset;
    public Vector2 speed = new Vector2(1, 1);

    private void Start()
    {
        mat = GetComponent<Renderer>().sharedMaterial;
    }

    // Update is called once per frame
    private void Update()
    {
        offset += speed * Time.deltaTime;
        mat.SetTextureOffset("_MainTex", offset);
    }
}