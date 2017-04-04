using UnityEngine;

namespace HoloKit
{
    public class GestureManipulator : MonoBehaviour
    {
        [Tooltip("How much to scale each axis of hand movement (camera relative) when manipulating the object")]
        public Vector3 handPositionScale = new Vector3(10.0f, 10.0f,10.0f);  // Default tuning values, expected to be modified per application

        public LayerMask layerMask;

        public bool RotateTowards = true;

        private Vector3 initialHandPosition;
        private Vector3 initialObjectPosition;
        private Interpolator targetInterpolator;
        private bool manipulating { get; set; }
        private bool placing;

        void LateUpdate()
        {
            if (manipulating)
            {
                Manipulate();
            }
            else if (placing)
            {
                Place();
            }
        }

        public void ToggleManipulating()
        {
            Debug.Log("ToggleManipulation");

            placing = false;
            GazeManager.Instance.IsDisabled = !GazeManager.Instance.IsDisabled;
            manipulating = !manipulating;
            if (manipulating)
            {
                targetInterpolator = gameObject.GetComponent<Interpolator>();
                initialHandPosition = Camera.main.transform.InverseTransformPoint(GestureManager.Instance.ManipulationHandPosition);
                initialObjectPosition = Camera.main.transform.InverseTransformPoint(transform.position);
            }
        }

        public void TogglePlacing()
        {
            manipulating = false;
            GazeManager.Instance.IsDisabled = !GazeManager.Instance.IsDisabled;
            placing = !placing;

#if UNITY_EDITOR
            //SpatialMappingManager.Instance.DrawVisualMeshes = placing;
#endif
        }

        private void Place()
        {
            var headPosition = Camera.main.transform.position;
            var gazeDirection = Camera.main.transform.forward;

            RaycastHit hitInfo;
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo, GazeManager.Instance.MaxGazeDistance, SpatialMappingManager.Instance.LayerMask))
            {
                Quaternion toQuat = Quaternion.LookRotation(hitInfo.normal * -1);

                //Hit on floor
                if (hitInfo.normal.y > 0)
                {
                    transform.position = hitInfo.point + new Vector3(0,transform.localScale.y);
                    toQuat = Quaternion.LookRotation(Camera.main.transform.forward);
                }
                //Hit on roof
                else if(hitInfo.normal.y < 0)
                {
                    transform.position = hitInfo.point - new Vector3(0, transform.localScale.y);
                }
                else
                {
                    transform.position = hitInfo.point;
                }

                if (RotateTowards)
                {
                    toQuat.x = 0;
                    toQuat.z = 0;
                    transform.rotation = toQuat;
                }
            }
            else
            {
                transform.position = headPosition + Camera.main.transform.forward.normalized * GazeManager.Instance.MaxGazeDistance;
            }
        }

        private void Manipulate()
        {
            // First step is to figure out the delta between the initial hand position and the current hand position
            Vector3 localHandPosition = Camera.main.transform.InverseTransformPoint(GestureManager.Instance.ManipulationHandPosition);
            Vector3 initialHandToCurrentHand = localHandPosition - initialHandPosition;
            Vector3 scaledLocalHandPositionDelta = Vector3.Scale(initialHandToCurrentHand, handPositionScale);
            Vector3 localObjectPosition = initialObjectPosition + scaledLocalHandPositionDelta;
            Vector3 worldObjectPosition = Camera.main.transform.TransformPoint(localObjectPosition);

            // Rotate this object to face the user.
            Quaternion toQuat = Camera.main.transform.localRotation;
            toQuat.x = 0;
            toQuat.z = 0;
            this.transform.rotation = toQuat;

            // If the object has an interpolator we should use it, otherwise just move the transform directly
            if (targetInterpolator != null)
            {
                targetInterpolator.SetTargetPosition(worldObjectPosition);
            }
            else
            {
                this.transform.position = worldObjectPosition;
            }
        }
    }
}
