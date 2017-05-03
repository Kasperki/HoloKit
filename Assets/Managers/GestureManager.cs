
using UnityEngine;
using UnityEngine.VR.WSA.Input;
using System.Collections.Generic;
using System;

namespace HoloKit
{
    /// <summary>
    /// GestureManager provides access to several different input gestures, including
    /// Tap and Manipulation.
    /// </summary>
    /// <remarks>
    /// When a tap gesture is detected, GestureManager uses GazeManager to find the game object.
    /// GestureManager then sends a message to that game object.
    /// 
    /// Using Manipulation requires subscribing the the ManipulationStarted events and then querying
    /// information about the manipulation gesture via ManipulationOffset and ManipulationHandPosition
    /// </remarks>
    [RequireComponent(typeof(GazeManager))]
    public partial class GestureManager : Singleton<GestureManager>
    {
        public Vector3 ManipulationHandPosition { get { return interactionRecognizer.ManipulationHandPosition; } }

        public event EventHandler OnTapped;

        private GameObject focusedObject { get { return GazeManager.Instance.FocusedObject; } }

        private BasicGestureRecognizer basicGestureRecognizer;
        private InteractionRecognizer interactionRecognizer;

        new void Awake()
        {
            base.Awake();

            basicGestureRecognizer = new BasicGestureRecognizer(OnTap,OnHold,OnHoldEnd);
            basicGestureRecognizer.Start();
            interactionRecognizer = new InteractionRecognizer();
            interactionRecognizer.Start();
        }

#if UNITY_EDITOR
        public KeyCode EditorSelectKey = KeyCode.Space;
        void LateUpdate()
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(EditorSelectKey))
            {
                OnTap();
            }
            if(Input.GetMouseButtonDown(2))
            {
                OnHold();
            }
        }
#endif

        private void OnEnable()
        {
            basicGestureRecognizer.Start();
            interactionRecognizer.Start();
        }

        private void OnDisable()
        {
            basicGestureRecognizer.Stop();
            interactionRecognizer.Stop();
        }

        void OnDestroy()
        {
            basicGestureRecognizer.Stop();
            interactionRecognizer.Stop();
        }

        private void OnTap()
        {
            Debug.Log("GestureManager_OnTap");

            if (focusedObject != null)
            {
                ISelectable selectable = (ISelectable)focusedObject.GetComponent(typeof(ISelectable));
                if(selectable != null)
                {
                    selectable.OnSelect();
                }                
            }

            if (OnTapped != null)
            {
                OnTapped(this, EventArgs.Empty);
            }
        }
        private void OnHold()
        {
            if (focusedObject != null)
            {
                ISelectable selectable = (ISelectable)focusedObject.GetComponent(typeof(ISelectable));
                if (selectable != null)
                {
                    selectable.OnHold();
                }
            }
        }
        private void OnHoldEnd()
        {
            if (focusedObject != null)
            {
                ISelectable selectable = (ISelectable)focusedObject.GetComponent(typeof(ISelectable));
                if (selectable != null)
                {
                    selectable.OnRelease();
                }
            }
        }
    }

    public abstract class GestureRecognizerBase
    {
        protected GestureRecognizer Recognizer;
        public bool IsActive { get; set; }
        public delegate void TappedHandler(InteractionSourceKind source, int tapCount, Ray headRay);

        public Action<InteractionSourceKind, int, Ray> OnTappedAction { get; set; }

        public GestureRecognizerBase(GestureSettings settings)
        {
            Recognizer = new GestureRecognizer();
            Recognizer.SetRecognizableGestures(settings);
        }

        public void Restart()
        {
            Recognizer.CancelGestures();
            Recognizer.StartCapturingGestures();
        }
        public void Stop()
        {
            Recognizer.CancelGestures();
            Recognizer.StopCapturingGestures();
            DeInitialize();
            IsActive = false;
        }
        protected abstract void Initialize();
        protected abstract void DeInitialize();
        public void Start()
        {
            Initialize();
            Recognizer.StartCapturingGestures();
            IsActive = true;
        }
    }

    public class BasicGestureRecognizer : GestureRecognizerBase
    {
        //public event TappedHandler OnTapped;
        private Action OnTapped;
        private Action OnHold;
        private Action OnHoldEnd;
        public BasicGestureRecognizer(Action onTapped, Action onHoldStart, Action onHoldEnd) :base(GestureSettings.Tap|GestureSettings.Hold)
        {
            OnTapped = onTapped;
            OnHold = onHoldStart;
            OnHoldEnd = onHoldEnd;
        }

        protected override void DeInitialize()
        {
            Recognizer.TappedEvent -= Recognizer_TappedEvent;
            Recognizer.HoldStartedEvent -= Recognizer_HoldStartedEvent;
            Recognizer.HoldCompletedEvent -= Recognizer_HoldCompletedEvent;
        }

        protected override void Initialize()
        {
            Recognizer.TappedEvent += Recognizer_TappedEvent;
            Recognizer.HoldStartedEvent += Recognizer_HoldStartedEvent;
            Recognizer.HoldCanceledEvent += Recognizer_HoldCanceledEvent;
            Recognizer.HoldCompletedEvent += Recognizer_HoldCompletedEvent;
        }

        private void Recognizer_HoldCanceledEvent(InteractionSourceKind source, Ray headRay)
        {
            OnHoldEnd();
        }
        private void Recognizer_HoldCompletedEvent(InteractionSourceKind source, Ray headRay)
        {
            OnHoldEnd();
        }
        private void Recognizer_HoldStartedEvent(InteractionSourceKind source, Ray headRay)
        {
            OnHold();
        }
        private void Recognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
        {
            OnTapped();
        }
    }

    public class InteractionRecognizer
    {
        public Vector3 ManipulationHandPosition
        {
            get
            {
                var handPosition = Vector3.zero;
                currentHandState.properties.location.TryGetPosition(out handPosition);
                return handPosition;
            }
        }

        private bool HandTracked { get { return trackedHand.Count > 0; } }
        private HashSet<uint> trackedHand = new HashSet<uint>();
        private InteractionSourceState currentHandState;
        public InteractionRecognizer()
        {
        }

        public void Start()
        {
            InteractionManager.SourcePressed += InteractionManager_SourcePressed;
            InteractionManager.SourceReleased += InteractionManager_SourceReleased;
            InteractionManager.SourceUpdated += InteractionManager_SourceUpdated;
            InteractionManager.SourceLost += InteractionManager_SourceLost;
        }
        public void Stop()
        {
            InteractionManager.SourcePressed -= InteractionManager_SourcePressed;
            InteractionManager.SourceReleased -= InteractionManager_SourceReleased;
            InteractionManager.SourceUpdated -= InteractionManager_SourceUpdated;
            InteractionManager.SourceLost -= InteractionManager_SourceLost;
        }

        private void InteractionManager_SourcePressed(InteractionSourceState state)
        {
            if (!HandTracked)
            {
                currentHandState = state;
            }

            trackedHand.Add(state.source.id);
        }
        private void InteractionManager_SourceUpdated(InteractionSourceState state)
        {
            if (HandTracked && state.source.id == currentHandState.source.id)
            {
                currentHandState = state;
            }
        }
        private void InteractionManager_SourceReleased(InteractionSourceState state)
        {
            trackedHand.Remove(state.source.id);
        }
        private void InteractionManager_SourceLost(InteractionSourceState state)
        {
            trackedHand.Remove(state.source.id);
        }
    }
}