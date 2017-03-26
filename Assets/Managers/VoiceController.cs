using System;
using System.Collections.Generic;
using UnityEngine;

namespace HoloKit
{ 
    public abstract class VoiceController : MonoBehaviour
    {
        public Dictionary<string, Action> VoiceCommands;

        public void Awake()
        {
            VoiceCommands = new Dictionary<string, Action>();
        }
    }
}