using UnityEngine;

public class SteamVR_Teleporter : MonoBehaviour
{
    public enum TeleportType
    {
        TeleportTypeUseTerrain,
        TeleportTypeUseCollider,
        TeleportTypeUseZeroY
    }

    public bool teleportOnClick;
    public TeleportType teleportType = TeleportType.TeleportTypeUseZeroY;

    private Transform reference
    {
        get
        {
            var top = SteamVR_Render.Top();
            return top != null ? top.origin : null;
        }
    }

    private void Start()
    {
        var trackedController = GetComponent<SteamVR_TrackedController>();
        if (trackedController == null) trackedController = gameObject.AddComponent<SteamVR_TrackedController>();

        trackedController.TriggerClicked += DoClick;

        if (teleportType == TeleportType.TeleportTypeUseTerrain)
        {
            // Start the player at the level of the terrain
            var t = reference;
            if (t != null)
                t.position = new Vector3(t.position.x, Terrain.activeTerrain.SampleHeight(t.position), t.position.z);
        }
    }

    private void DoClick(object sender, ClickedEventArgs e)
    {
        if (teleportOnClick)
        {
            // First get the current Transform of the the reference space (i.e. the Play Area, e.g. CameraRig prefab)
            var t = reference;
            if (t == null)
                return;

            // Get the current Y position of the reference space
            var refY = t.position.y;

            // Create a plane at the Y position of the Play Area
            // Then create a Ray from the origin of the controller in the direction that the controller is pointing
            var plane = new Plane(Vector3.up, -refY);
            var ray = new Ray(transform.position, transform.forward);

            // Set defaults
            var hasGroundTarget = false;
            var dist = 0f;
            if (teleportType == TeleportType.TeleportTypeUseTerrain) // If we picked to use the terrain
            {
                RaycastHit hitInfo;
                var tc = Terrain.activeTerrain.GetComponent<TerrainCollider>();
                hasGroundTarget = tc.Raycast(ray, out hitInfo, 1000f);
                dist = hitInfo.distance;
            }
            else if (teleportType == TeleportType.TeleportTypeUseCollider) // If we picked to use the collider
            {
                RaycastHit hitInfo;
                hasGroundTarget = Physics.Raycast(ray, out hitInfo);
                dist = hitInfo.distance;
            }
            else // If we're just staying flat on the current Y axis
            {
                // Intersect a ray with the plane that was created earlier
                // and output the distance along the ray that it intersects
                hasGroundTarget = plane.Raycast(ray, out dist);
            }

            if (hasGroundTarget)
            {
                // Get the current Camera (head) position on the ground relative to the world
                var headPosOnGround = new Vector3(SteamVR_Render.Top().head.position.x, refY,
                    SteamVR_Render.Top().head.position.z);

                // We need to translate the reference space along the same vector
                // that is between the head's position on the ground and the intersection point on the ground
                // i.e. intersectionPoint - headPosOnGround = translateVector
                // currentReferencePosition + translateVector = finalPosition
                t.position = t.position + (ray.origin + ray.direction * dist) - headPosOnGround;
            }
        }
    }
}