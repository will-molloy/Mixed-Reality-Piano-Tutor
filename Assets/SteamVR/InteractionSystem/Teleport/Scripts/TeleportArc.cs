//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Displays the arc lines for teleporting and does the traces
//
//=============================================================================

using UnityEngine;
using UnityEngine.Rendering;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class TeleportArc : MonoBehaviour
    {
        [Tooltip("The amount of time in seconds to predict the motion of the projectile.")]
        public float arcDuration = 3.0f;

        private bool arcInvalid;
        private Transform arcObjectsTransfrom;

        [Tooltip("The speed at which the line segments of the arc move.")]
        public float arcSpeed = 0.2f;

        private float arcTimeOffset;

        //Private data
        private LineRenderer[] lineRenderers;

        public Material material;
        private int prevSegmentCount;
        private float prevThickness;
        private Vector3 projectileVelocity;

        [Tooltip("The amount of time in seconds between each segment of the projectile.")]
        public float segmentBreak = 0.025f;

        public int segmentCount = 60;
        private bool showArc = true;
        private Vector3 startPos;
        public float thickness = 0.01f;

        [HideInInspector] public int traceLayerMask;

        private bool useGravity = true;


        //-------------------------------------------------
        private void Start()
        {
            arcTimeOffset = Time.time;
        }


        //-------------------------------------------------
        private void Update()
        {
            if (thickness != prevThickness || segmentCount != prevSegmentCount)
            {
                CreateLineRendererObjects();
                prevThickness = thickness;
                prevSegmentCount = segmentCount;
            }
        }


        //-------------------------------------------------
        private void CreateLineRendererObjects()
        {
            //Destroy any existing line renderer objects
            if (arcObjectsTransfrom != null) Destroy(arcObjectsTransfrom.gameObject);

            var arcObjectsParent = new GameObject("ArcObjects");
            arcObjectsTransfrom = arcObjectsParent.transform;
            arcObjectsTransfrom.SetParent(transform);

            //Create new line renderer objects
            lineRenderers = new LineRenderer[segmentCount];
            for (var i = 0; i < segmentCount; ++i)
            {
                var newObject = new GameObject("LineRenderer_" + i);
                newObject.transform.SetParent(arcObjectsTransfrom);

                lineRenderers[i] = newObject.AddComponent<LineRenderer>();

                lineRenderers[i].receiveShadows = false;
                lineRenderers[i].reflectionProbeUsage = ReflectionProbeUsage.Off;
                lineRenderers[i].lightProbeUsage = LightProbeUsage.Off;
                lineRenderers[i].shadowCastingMode = ShadowCastingMode.Off;
                lineRenderers[i].material = material;
#if (UNITY_5_4)
				lineRenderers[i].SetWidth( thickness, thickness );
#else
                lineRenderers[i].startWidth = thickness;
                lineRenderers[i].endWidth = thickness;
#endif
                lineRenderers[i].enabled = false;
            }
        }


        //-------------------------------------------------
        public void SetArcData(Vector3 position, Vector3 velocity, bool gravity, bool pointerAtBadAngle)
        {
            startPos = position;
            projectileVelocity = velocity;
            useGravity = gravity;

            if (arcInvalid && !pointerAtBadAngle) arcTimeOffset = Time.time;
            arcInvalid = pointerAtBadAngle;
        }


        //-------------------------------------------------
        public void Show()
        {
            showArc = true;
            if (lineRenderers == null) CreateLineRendererObjects();
        }


        //-------------------------------------------------
        public void Hide()
        {
            //Hide the line segments if they were previously being shown
            if (showArc) HideLineSegments(0, segmentCount);
            showArc = false;
        }


        //-------------------------------------------------
        // Draws each segment of the arc individually
        //-------------------------------------------------
        public bool DrawArc(out RaycastHit hitInfo)
        {
            var timeStep = arcDuration / segmentCount;

            var currentTimeOffset = (Time.time - arcTimeOffset) * arcSpeed;

            //Reset the arc time offset when it has gone beyond a segment length
            if (currentTimeOffset > timeStep + segmentBreak)
            {
                arcTimeOffset = Time.time;
                currentTimeOffset = 0.0f;
            }

            var segmentStartTime = currentTimeOffset;

            var arcHitTime = FindProjectileCollision(out hitInfo);

            if (arcInvalid)
            {
                //Only draw first segment
                lineRenderers[0].enabled = true;
                lineRenderers[0].SetPosition(0, GetArcPositionAtTime(0.0f));
                lineRenderers[0].SetPosition(1, GetArcPositionAtTime(arcHitTime < timeStep ? arcHitTime : timeStep));

                HideLineSegments(1, segmentCount);
            }
            else
            {
                //Draw the first segment outside the loop if needed
                var loopStartSegment = 0;
                if (segmentStartTime > segmentBreak)
                {
                    var firstSegmentEndTime = currentTimeOffset - segmentBreak;
                    if (arcHitTime < firstSegmentEndTime) firstSegmentEndTime = arcHitTime;
                    DrawArcSegment(0, 0.0f, firstSegmentEndTime);

                    loopStartSegment = 1;
                }

                var stopArc = false;
                var currentSegment = 0;
                if (segmentStartTime < arcHitTime)
                    for (currentSegment = loopStartSegment; currentSegment < segmentCount; ++currentSegment)
                    {
                        //Clamp the segment end time to the arc duration
                        var segmentEndTime = segmentStartTime + timeStep;
                        if (segmentEndTime >= arcDuration)
                        {
                            segmentEndTime = arcDuration;
                            stopArc = true;
                        }

                        if (segmentEndTime >= arcHitTime)
                        {
                            segmentEndTime = arcHitTime;
                            stopArc = true;
                        }

                        DrawArcSegment(currentSegment, segmentStartTime, segmentEndTime);

                        segmentStartTime += timeStep + segmentBreak;

                        //If the previous end time or the next start time is beyond the duration then stop the arc
                        if (stopArc || segmentStartTime >= arcDuration || segmentStartTime >= arcHitTime) break;
                    }
                else
                    currentSegment--;

                //Hide the rest of the line segments
                HideLineSegments(currentSegment + 1, segmentCount);
            }

            return arcHitTime != float.MaxValue;
        }


        //-------------------------------------------------
        private void DrawArcSegment(int index, float startTime, float endTime)
        {
            lineRenderers[index].enabled = true;
            lineRenderers[index].SetPosition(0, GetArcPositionAtTime(startTime));
            lineRenderers[index].SetPosition(1, GetArcPositionAtTime(endTime));
        }


        //-------------------------------------------------
        public void SetColor(Color color)
        {
            for (var i = 0; i < segmentCount; ++i)
            {
#if (UNITY_5_4)
				lineRenderers[i].SetColors( color, color );
#else
                lineRenderers[i].startColor = color;
                lineRenderers[i].endColor = color;
#endif
            }
        }


        //-------------------------------------------------
        private float FindProjectileCollision(out RaycastHit hitInfo)
        {
            var timeStep = arcDuration / segmentCount;
            var segmentStartTime = 0.0f;

            hitInfo = new RaycastHit();

            var segmentStartPos = GetArcPositionAtTime(segmentStartTime);
            for (var i = 0; i < segmentCount; ++i)
            {
                var segmentEndTime = segmentStartTime + timeStep;
                var segmentEndPos = GetArcPositionAtTime(segmentEndTime);

                if (Physics.Linecast(segmentStartPos, segmentEndPos, out hitInfo, traceLayerMask))
                    if (hitInfo.collider.GetComponent<IgnoreTeleportTrace>() == null)
                    {
                        Util.DrawCross(hitInfo.point, Color.red, 0.5f);
                        var segmentDistance = Vector3.Distance(segmentStartPos, segmentEndPos);
                        var hitTime = segmentStartTime + timeStep * (hitInfo.distance / segmentDistance);
                        return hitTime;
                    }

                segmentStartTime = segmentEndTime;
                segmentStartPos = segmentEndPos;
            }

            return float.MaxValue;
        }


        //-------------------------------------------------
        public Vector3 GetArcPositionAtTime(float time)
        {
            var gravity = useGravity ? Physics.gravity : Vector3.zero;

            var arcPos = startPos + (projectileVelocity * time + 0.5f * time * time * gravity);
            return arcPos;
        }


        //-------------------------------------------------
        private void HideLineSegments(int startSegment, int endSegment)
        {
            if (lineRenderers != null)
                for (var i = startSegment; i < endSegment; ++i)
                    lineRenderers[i].enabled = false;
        }
    }
}