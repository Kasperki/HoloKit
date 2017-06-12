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
            hits = hits.OrderBy(m => (gazeOrigin - m.point).sqrMagnitude).ToArray();
            Hit = hits.Count() > 0;

            var oldFocusedObjects = new List<GameObject>(FocusedObjects);
            FocusedObjects.Clear();

            if (hits.Any())
            {
                int order = 0;
                for (int i = 0; i < hits.Count(); i++)
                {
                    FocusedObjects.Add(hits[i].collider.gameObject);
                    FocusedObject = hits[i].collider.gameObject;

                    // Check if the currently hit object has changed
                    if (!oldFocusedObjects.Contains(FocusedObject))
                    {
                        Component[] gazeObjects = FocusedObject.GetComponents(typeof(IGazeable));
                        if (gazeObjects != null)
                        {
                            for (int j = 0; j < gazeObjects.Length; j++)
                            {
                                ((IGazeable)gazeObjects[j]).OnGazeEnter(order);
                            }

                            order++;
                        }
                    }
                }

                Position = hits[0].point;
                Normal = hits[0].normal;
                lastHitDistance = hits[0].distance;
                FocusedObject = hits[0].collider.gameObject;
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
                if (!FocusedObjects.Contains(oldFocuesdObject))
                {
                    Component[] gazeObjects = oldFocuesdObject.GetComponents(typeof(IGazeable));
                    if (gazeObjects != null)
                    {
                        foreach (var gazeObject in gazeObjects)
                        {
                            ((IGazeable)gazeObject).OnGazeExit();
                        }
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