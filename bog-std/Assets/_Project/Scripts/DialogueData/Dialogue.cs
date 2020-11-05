using System;
using System.Collections.Generic;

public enum Command : ushort
{
    None,
    Skip,
    Wait,
    Increment
};

namespace Assets._Project.Scripts.DialogueData
{
    public class Dialogue
    {
        public Dialogue()
        {
            choices = new List<Choice>();
        }

        public string name = string.Empty; // name of the speaker
        public string line = string.Empty; // the dialog message
        public string tag = String.Empty; // the dialogue tag (for branching)
        public readonly List<Choice> choices; // check if length > 0 to see if we have choices
        public Command command;
        public List<LayerName> layers;
        public int magnitude;

        // public bool hasChoice; //if True == choice if False == dialog no choices

        // public bool hasCommand;
        // public int commandValue;




    }
}