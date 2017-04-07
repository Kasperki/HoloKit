using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloKit
{
    public enum PivotAxis
    {
        XYZ,
        X,
        Y,
        Z
    }

    public class Billboard : MonoBehaviour
    {
        public PivotAxis PivotAxis = PivotAxis.XYZ;

        /// <summary>
        /// Overrides the cached value of the GameObject's default rotation.
        /// </summary>
        public Quaternion DefaultRotation { get; private set; }

        public bool Smoothed = true;
        public float SmoothingSpeed = 2.0f;

        private Vector3 rot;

        private void OnEnable()
        {
            DefaultRotation = gameObject.transform.rotation;
        }

        /// <summary>
        /// Keeps the object facing the camera.
        /// </summary>
        private void Update()
        {
            switch (PivotAxis)
            {
                case PivotAxis.X:
                    rot = new Vector3(Camera.main.transform.rotation.eulerAngles.x, 0, 0);
                    break;

                case PivotAxis.Y:
                    rot = new Vector3(0, Camera.main.transform.rotation.eulerAngles.y, 0);
                    break;

                case PivotAxis.Z:
                    rot = new Vector3(0, 0, Camera.main.transform.rotation.eulerAngles.z);
                    break;

                case PivotAxis.XYZ:
                default:
                    rot = Camera.main.transform.rotation.eulerAngles;
                    break;
            }

            var t = Smoothed ? Time.deltaTime * SmoothingSpeed : 1;
            var targetRotation = Quaternion.Euler(rot);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, t);
        }
    }
}