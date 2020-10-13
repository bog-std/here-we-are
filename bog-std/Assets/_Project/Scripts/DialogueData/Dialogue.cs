using System.Collections.Generic;

namespace Assets._Project.Scripts.DialogueData
{
    public class Dialogue
    {
        public Dialogue()
        {
            choices = new List<Choice>();
        }

        public string name; // name of the speaker
        public string line = string.Empty; // the dialog message
        
        public bool hasChoice; //if True == choice if False == dialog no choices
        public List<Choice> choices;
    }
}