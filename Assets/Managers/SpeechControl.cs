using System;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace HoloKit
{ 
    public class SpeechControl : Singleton<SpeechControl>
    {
        private DictationRecognizer dictationRecognizer;
        // Using an empty string specifies the default microphone. 
        private static readonly string deviceName = string.Empty;
        private int samplingRate;

        private const int RECORD_MAX_TIME_IN_SECONDS = 20;

        new void Awake()
        {
            base.Awake();

            InitializeDictation();
            int unused;
            Microphone.GetDeviceCaps(deviceName, out unused, out samplingRate);
        }

        void OnDestroy()
        {
            dictationRecognizer.DictationHypothesis -= DictationRecognizer_DictationHypothesis;
            dictationRecognizer.DictationResult -= DictationRecognizer_DictationResult;
            dictationRecognizer.DictationComplete -= DictationRecognizer_DictationComplete;
            dictationRecognizer.DictationError -= DictationRecognizer_DictationError;
            dictationRecognizer = null;
        }

        private void InitializeDictation()
        {
            dictationRecognizer = new DictationRecognizer();
            dictationRecognizer.DictationHypothesis += DictationRecognizer_DictationHypothesis;
            dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;
            dictationRecognizer.DictationComplete += DictationRecognizer_DictationComplete;
            dictationRecognizer.DictationError += DictationRecognizer_DictationError;
        }

        public AudioClip StartRecording(DictationRecognizer.DictationResultDelegate dictationResultDelegate)
        {
            if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running)
            {
                PhraseRecognitionSystem.Shutdown();
            }

            dictationRecognizer.DictationResult += dictationResultDelegate;
            dictationRecognizer.Start();
            return Microphone.Start(deviceName, false, RECORD_MAX_TIME_IN_SECONDS, samplingRate);
        }

        public void StopRecording(DictationRecognizer.DictationResultDelegate dictationResultDelegate)
        {
            Microphone.End(deviceName);
            dictationRecognizer.Stop();
            dictationRecognizer.DictationResult -= dictationResultDelegate;
            PhraseRecognitionSystem.Restart();
        }

        private void DictationRecognizer_DictationHypothesis(string text)
        {

        }

        private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
        {

        }

        private void DictationRecognizer_DictationComplete(DictationCompletionCause cause)
        {
            // If Timeout occurs, the user has been silent for too long.
            // With dictation, the default timeout after a recognition is 20 seconds.
            // The default timeout with initial silence is 5 seconds.

            StopRecording(null);
        }

        private void DictationRecognizer_DictationError(string error, int hresult)
        {

        }
    }
}