using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloKit
{
    public enum PivotAxis
    {
        XYZ,
        X,
        Y
    }

    public class Billboard : MonoBehaviour
    {
        public PivotAxis PivotAxis = PivotAxis.XYZ;

        /// <summary>
        /// Overrides the cached value of the GameObject's default rotation.
        /// </summary>
        public Quaternion DefaultRotation { get; private set; }

        public bool Smoothed = true;

        /// <summary>
        /// Duration after enable to face the camera
        /// </summary>
        public float Duration = 2.0f;

        private float startTime, t;
        private Vector3 startForward, startUp;

        private void OnEnable()
        {
            DefaultRotation = gameObject.transform.rotation;

            startForward = transform.forward;
            startUp = transform.up;
            startTime = Time.time;
        }

        /// <summary>
        /// Keeps the object facing the camera.
        /// </summary>
        private void Update()
        {
            Vector3 forward;
            Vector3 up;

            t = Smoothed ? Mathf.SmoothStep(0, 1, (Time.time - startTime) / Duration) : 1;

            switch (PivotAxis)
            {
                case PivotAxis.X:
                    Vector3 right = transform.right; // Fixed right
                    forward = Vector3.ProjectOnPlane(Camera.main.transform.forward, right).normalized;
                    up = Vector3.Lerp(startUp, Vector3.Cross(forward, right), t);
                    break;

                case PivotAxis.Y:
                    up = transform.up; // Fixed up
                    forward = Vector3.Lerp(startForward, Camera.main.transform.forward, t);
                    break;

                case PivotAxis.XYZ:
                default:
                    forward = Vector3.Lerp(startForward, Camera.main.transform.forward, t);
                    up = Vector3.Lerp(startUp, Camera.main.transform.up, t);
                    break;
            }

            // Calculate and apply the rotation required to reorient the object
            transform.rotation = Quaternion.LookRotation(forward, up);
        }
    }
}