using System;
using System.Collections.Generic;
using _Project.Scripts.Scene;

public enum Command : ushort
{
    None,
    Skip,
    Wait,
    Increment,
    Script,
    Set,
    Phone,
    Fact,
    Messages,
    Menu,
    Audio,
    Notification,
    CarCrash,
    RipJordan
};

namespace Assets._Project.Scripts.DialogueData
{
    public class Dialogue
    {
        public Dialogue()
        {
            choices = new List<Choice>();
            layers = new List<LayerName>();
        }

        public string name = string.Empty; // name of the speaker
        public string line = string.Empty; // the dialog message
        public string tag = String.Empty; // the dialogue tag (for branching)
        public readonly List<Choice> choices; // check if length > 0 to see if we have choices
        public Command command;
        public List<LayerName> layers;
        public float magnitude;
        
    }
}