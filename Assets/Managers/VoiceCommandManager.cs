using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HoloKit
{ 
    public class VoiceCommandManager : Singleton<VoiceCommandManager>
    {
        public List<VoiceController> VoiceControllers;

        private KeywordRecognizer keywordRecognizer;
        Dictionary<string, Action> keywords;

        void Start ()
        {
            keywords = new Dictionary<string, Action>();

            foreach (var voiceController in VoiceControllers)
            {
                foreach (var voiceCommand in voiceController.VoiceCommands)
                {
                    keywords.Add(voiceCommand.Key, voiceCommand.Value);
                }
            }

            keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
            keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
            keywordRecognizer.Start();
        }

        private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            Action keywordAction;
            if (keywords.TryGetValue(args.text, out keywordAction))
            {
                keywordAction.Invoke();
            }
        }
    }
}