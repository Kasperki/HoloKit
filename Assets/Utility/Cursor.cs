using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloKit
{
    public class Cursor : Singleton<Cursor>
    {
        public GameObject CursorOnHolograms;
        public GameObject CursorOffHolograms;

        public float DistanceFromCollision = 0.01f;

        private bool active = true;

        new void Awake()
        {
            base.Awake();

            if (CursorOnHolograms == null || CursorOffHolograms == null)
            {
                return;
            }

            // Hide the Cursors to begin with.
            CursorOnHolograms.SetActive(false);
            CursorOffHolograms.SetActive(false);
        }

        public void SetActive(bool active)
        {
            this.active = active;
        }

        void LateUpdate()
        {
            if (GazeManager.Instance == null || CursorOnHolograms == null || CursorOffHolograms == null)
            {
                return;
            }

            if (GazeManager.Instance.Hit)
            {
                CursorOnHolograms.SetActive(true && active);
                CursorOffHolograms.SetActive(false);
            }
            else
            {
                CursorOffHolograms.SetActive(true && active);
                CursorOnHolograms.SetActive(false);
            }

            // Place the cursor at the calculated position.
            this.gameObject.transform.position = GazeManager.Instance.Position + GazeManager.Instance.Normal * DistanceFromCollision;

            //Orient cursor
            gameObject.transform.up = GazeManager.Instance.Normal;
        }
    }
}