using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VR.WSA;

namespace HoloKit
{
    /// <summary>
    /// GazeManager determines the location of the user's gaze, hit position and normals.
    /// </summary>
    public partial class GazeManager : Singleton<GazeManager>
    {
        public bool IsDisabled;

        public float MaxGazeDistance = 15.0f;

        public LayerMask RaycastLayerMask = Physics.DefaultRaycastLayers;

        /// <summary>
        /// Physics.Raycast result is true if it hits a hologram.
        /// </summary>
        public bool Hit { get; private set; }

        /// <summary>
        /// HitInfo property gives access
        /// to RaycastHit public members.
        /// </summary>
        public RaycastHit HitInfo { get; private set; }

        /// <summary>
        /// Position of the intersection of the user's gaze and the holograms in the scene.
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// RaycastHit Normal direction.
        /// </summary>
        public Vector3 Normal { get; private set; }

        /// <summary>
        /// Object currently being focused on.
        /// </summary>
        public GameObject FocusedObject { get; private set; }

        public List<GameObject> FocusedObjects { get; private set; }

        public bool SetStabilizationPlane = true;

        private float lastHitDistance = 15.0f;

        private void Start()
        {
            FocusedObjects = new List<GameObject>();
        }

        private void Update()
        {
            UpdateRaycast();
            UpdateStabilizationPlane();
        }

        public void SetFocusedObject(GameObject gameObject)
        {
            FocusedObject = gameObject;
        }

        /// <summary>
        /// Calculates the Raycast hit position and normal.
        /// </summary>
        private void UpdateRaycast()
        {
            if (IsDisabled)
            {
                return;
            }

            Vector3 gazeOrigin = Camera.main.transform.position;
            Vector3 gazeDirection = Camera.main.transform.forward;
            var hits = Physics.RaycastAll(gazeOrigin, gazeDirection, MaxGazeDistance, RaycastLayerMask);
            hits = hits.OrderByDescending(m => (gazeOrigin - m.point).sqrMagnitude).ToArray();
            Hit = hits.Count() > 0;

            var oldFocusedObjects = new List<GameObject>(FocusedObjects);
            FocusedObjects.Clear();

            if (hits.Count() > 0)
            {
                for (int i = 0; i < hits.Count(); i++)
                {
                    FocusedObjects.Add(hits[i].collider.gameObject);

                    HitInfo = hits[i];

                    Position = HitInfo.point;
                    Normal = HitInfo.normal;
                    lastHitDistance = HitInfo.distance;
                    FocusedObject = HitInfo.collider.gameObject;

                    // Check if the currently hit object has changed
                    if (!oldFocusedObjects.Contains(FocusedObject))
                    {
                        IGazeable gazeObject = (IGazeable)FocusedObject.GetComponent(typeof(IGazeable));
                        if (gazeObject != null)
                        {
                            gazeObject.OnGazeEnter();
                        }
                    }
                }
            }
            else
            {
                // If the raycast does not hit a hologram, default the position to last hit distance in front of the user,
                // and the normal to face the user.
                Position = gazeOrigin + (gazeDirection * lastHitDistance);
                Normal = -gazeDirection;
                FocusedObject = null;
            }

            foreach (var oldFocuesdObject in oldFocusedObjects)
            {
                if (FocusedObjects.Contains(oldFocuesdObject) == false)
                {
                    IGazeable gazeObject = (IGazeable)oldFocuesdObject.GetComponent(typeof(IGazeable));
                    if (gazeObject != null)
                    {
                        gazeObject.OnGazeExit();
                    }
                }
            }
        }

        /// <summary>
        /// Adds the stabilization plane modifier if it's enabled and if it doesn't exist yet.
        /// </summary>
        private void UpdateStabilizationPlane()
        {
            // We want to use the stabilization logic.
            if (SetStabilizationPlane)
            {
                // Check if it exists in the scene.
                if (StabilizationPlaneModifier.Instance == null)
                {
                    // If not, add it to us.
                    gameObject.AddComponent<StabilizationPlaneModifier>();
                }
            }

            if (StabilizationPlaneModifier.Instance)
            {
                StabilizationPlaneModifier.Instance.SetStabilizationPlane = SetStabilizationPlane;
            }
        }
    }
}