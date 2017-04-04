using UnityEngine;

namespace HoloKit
{
    [RequireComponent(typeof(GestureManipulator))]
    public class Movable : MonoBehaviour, ISelectable, IGazeable
    {
        public Color SelectedColor = Color.red;
        public Color OnGazeEnterColor = Color.cyan;

        private bool _moving;
        GestureManipulator _manipulator;

        private Color[] oldColor;
        private Renderer[] renderers;

        void Awake()
        {
            _manipulator = GetComponent<GestureManipulator>();

            renderers = GetComponentsInChildren<MeshRenderer>();
            oldColor = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                oldColor[i] = renderers[i].material.color;
            }
        }

        public void OnSelect()
        {
            if (!enabled)
            {
                return;
            }

            _moving = !_moving;

            if (_moving)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].material.color = new Color(SelectedColor.r, SelectedColor.g, SelectedColor.b, renderers[i].material.color.a);
                }
            }
            else
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].material.color = new Color(oldColor[i].r, oldColor[i].g, oldColor[i].b, renderers[i].material.color.a);
                }
            }

            Cursor.Instance.SetActive(!_moving);
            _manipulator.TogglePlacing();
        }

        public void OnHold()
        {
            _manipulator.ToggleManipulating();
        }

        public void OnRelease()
        {
            _manipulator.ToggleManipulating();
        }

        public void OnGazeEnter()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = new Color(OnGazeEnterColor.r, OnGazeEnterColor.g, OnGazeEnterColor.b, renderers[i].material.color.a);
            }
        }

        public void OnGazeExit()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = new Color(oldColor[i].r, oldColor[i].g, oldColor[i].b, renderers[i].material.color.a);
            }
        }
    }
}