using UnityEngine;

namespace User_Interface
{
    /// <summary>
    ///     - Useful for destroying objects like the ZED-M prefab when changing scenes
    /// </summary>
    public class DestroyObjectsOnSceneChange : MonoBehaviour
    {
        [SerializeField] private GameObject[] destroyees;

        private void OnDestroy()
        {
            Debug.Log("Scene changed, destroying VR prefabs");
            foreach (var i in destroyees) DestroyImmediate(i);
        }
    }
}