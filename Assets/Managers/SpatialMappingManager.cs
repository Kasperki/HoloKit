using UnityEngine;
using UnityEngine.VR.WSA;

namespace HoloKit
{
    public class SpatialMappingManager : Singleton<SpatialMappingManager>
    {
        public SpatialMappingCollider SpatialMappingCollider;
        public SpatialMappingRenderer SpatialMappingRenderer;
        public LayerMask LayerMask;

        [SerializeField]
        private bool visualizeSpatialMesh = false;

        public new void Awake()
        {
            base.Awake();

            if (SpatialMappingCollider == null)
            {
                SpatialMappingCollider = FindObjectOfType<SpatialMappingCollider>();

                if (SpatialMappingCollider == null)
                {
                    SpatialMappingCollider = gameObject.AddComponent<SpatialMappingCollider>();
                }
            }

            if (SpatialMappingRenderer == null)
            {
                SpatialMappingRenderer = FindObjectOfType<SpatialMappingRenderer>();

                if (SpatialMappingRenderer == null)
                {
                    SpatialMappingRenderer = gameObject.AddComponent<SpatialMappingRenderer>();
                }
            }

            VisualizeSpatialMesh = visualizeSpatialMesh;
        }

        public bool VisualizeSpatialMesh
        {
            get { return (SpatialMappingRenderer.enabled == true && visualizeSpatialMesh); }
            set
            {
                if (value == true)
                {
                    SpatialMappingRenderer.renderState = SpatialMappingRenderer.RenderState.Visualization;
                }
                else
                {
                    SpatialMappingRenderer.renderState = SpatialMappingRenderer.RenderState.None;
                }

                visualizeSpatialMesh = value;
            }
        }
	    
    }
}