using UnityEngine;
using UnityEngine.Rendering;

public class AsteroidsManager : MonoBehaviour
{
    public static int amount = 100;

    public GameObject asteroidsType1;
    public GameObject asteroidsType2;
    public Transform center;
    private Camera leftCamera;
    private readonly Matrix4x4[] listPositions = new Matrix4x4[amount];
    private readonly Matrix4x4[] listPositions2 = new Matrix4x4[amount];
    private readonly Matrix4x4[] listPositionsOrigin = new Matrix4x4[amount];

    private readonly Matrix4x4[] listPositionsOrigin2 = new Matrix4x4[amount];
    public ZEDManager manager;
    public float offset;
    public float radius = 1;
    private Camera rightCamera;

    private void Start()
    {
        CreateAsteroids(listPositionsOrigin, listPositions, amount, radius, offset);
        CreateAsteroids(listPositionsOrigin2, listPositions2, amount, radius, offset);
    }

    private void CreateAsteroids(Matrix4x4[] listPositionsOrigin, Matrix4x4[] listPositions, int amount, float radius,
        float offset)
    {
        // Matrix4x4 m2 = Matrix4x4.TRS(center.position, Quaternion.identity, Vector3.one);
        //
        // listPositionsOrigin.Add(m2);
        for (var i = 0; i < amount; ++i)
        {
            // 1. translation: displace along circle with 'radius' in range [-offset, offset]
            var angle = i / (float) amount * 360.0f;
            var displacement = Random.Range(0, 30) % (int) (2 * offset * 100) / 100.0f - offset;
            var x = Mathf.Sin(angle) * radius + displacement;
            displacement = Random.Range(0, 30) % (int) (2 * offset * 100) / 100.0f - offset;
            var y = displacement * 2.0f; // keep height of asteroid field smaller compared to width of x and z
            displacement = Random.Range(0, 30) % (int) (2 * offset * 100) / 100.0f - offset;
            var z = Mathf.Cos(angle) * radius + displacement;
            var position = new Vector3(x, y, z);
            //position = center.TransformPoint(position);
            //// 2. scale: Scale between 0.05 and 0.25f
            var scale = Random.Range(0.1f, 0.3f);
            //model = glm::scale(model, glm::vec3(scale));

            // 3. rotation: add random rotation around a (semi)randomly picked rotation axis vector
            float rotAngle = Random.Range(0, 100000) % 360;


            //Matrix4x4 m = Matrix4x4.TRS(position, Quaternion.identity, new Vector3(scale, scale, scale));
            var m = Matrix4x4.TRS(position, Quaternion.Euler(rotAngle, rotAngle, rotAngle),
                new Vector3(scale, scale, scale));

            listPositionsOrigin[i] = m;
        }
    }


    private void UpdatePosition(Matrix4x4[] listPositionsOrigin, Matrix4x4[] listPositions)
    {
        for (var i = 0; i < listPositionsOrigin.Length; ++i)
        {
            listPositionsOrigin[i] = listPositionsOrigin[i] * Matrix4x4.TRS(Vector3.zero,
                                         Quaternion.Euler(Time.deltaTime * Random.Range(0, 100),
                                             Time.deltaTime * Random.Range(0, 100),
                                             Time.deltaTime * Random.Range(0, 100)),
                                         Vector3.one);
            listPositions[i] = transform.localToWorldMatrix * listPositionsOrigin[i];
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (leftCamera == null)
            if (manager)
                leftCamera = manager.GetLeftCameraTransform().GetComponent<Camera>();
        if (rightCamera == null && ZEDManager.IsStereoRig)
            if (manager)
                rightCamera = manager.GetRightCameraTransform().GetComponent<Camera>();
        //Update positions and draw asteroids of type 1
        UpdatePosition(listPositionsOrigin, listPositions);
        Graphics.DrawMeshInstanced(asteroidsType1.GetComponent<MeshFilter>().sharedMesh,
            0, asteroidsType1.GetComponent<MeshRenderer>().sharedMaterial,
            listPositions,
            listPositions.Length,
            null,
            ShadowCastingMode.Off,
            false,
            8,
            leftCamera);
        if (ZEDManager.IsStereoRig)
            Graphics.DrawMeshInstanced(asteroidsType1.GetComponent<MeshFilter>().sharedMesh,
                0,
                asteroidsType1.GetComponent<MeshRenderer>().sharedMaterial,
                listPositions,
                listPositions.Length,
                null,
                ShadowCastingMode.Off,
                false,
                10,
                rightCamera);
        //Update positions and draw asteroids of type 2
        UpdatePosition(listPositionsOrigin2, listPositions2);
        Graphics.DrawMeshInstanced(asteroidsType2.GetComponent<MeshFilter>().sharedMesh,
            0,
            asteroidsType2.GetComponent<MeshRenderer>().sharedMaterial,
            listPositions2,
            listPositions2.Length,
            null,
            ShadowCastingMode.Off,
            false,
            8,
            leftCamera);
        if (ZEDManager.IsStereoRig)
            Graphics.DrawMeshInstanced(asteroidsType2.GetComponent<MeshFilter>().sharedMesh,
                0,
                asteroidsType2.GetComponent<MeshRenderer>().sharedMaterial,
                listPositions2,
                listPositions2.Length,
                null,
                ShadowCastingMode.Off,
                false,
                10,
                rightCamera);
    }
}