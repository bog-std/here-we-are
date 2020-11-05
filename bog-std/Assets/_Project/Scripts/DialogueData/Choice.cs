using System.Collections.Generic;

namespace Assets._Project.Scripts.DialogueData
{
    public class Choice
    {
        public Choice()
        {
            layers = new List<LayerName>();
        }

        public string choiceOption = string.Empty;
        public string choiceResponse = string.Empty;

        public string choice;
        public List<LayerName> layers;
        public int magnitude;
        public string target = string.Empty;
    }
}
