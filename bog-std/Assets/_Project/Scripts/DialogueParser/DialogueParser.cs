using System;
using System.Collections.Generic;
using System.IO;
using Assets._Project.Scripts.DialogueData;
using UnityEditor;
using UnityEngine;

//a parser that goes through a text file extracting info
//special chars used so far are :
// (Dx) (Cx) where x is an int
// ~x where x is a number indicating the layer to be increased
// ~~xy where x is the layer and y is the score
// /n and /t are used but just for readability purposes


    public class DialogueParser : MonoBehaviour
    {
        private static readonly char[] charsToTrim = { '*', '~', '@', '(', ')','1','2','3','4','5','6','7','8','9'};

        public static IEnumerable<Dialogue> GetDialogue()
        {
            var dialogue = ReadStringNEW("Assets/_Project/Resources/sample.txt");
            Debug.Log(dialogue);

            return dialogue;
        }

        public static List<Dialogue> ReadString(string path)
        {
            // Read the text directly from the test.txt file
            var reader = new StreamReader(path);

            var script = new List<Dialogue>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                switch(line[0]){
                        
                    case('@'): // dialogue
                        script.Add(new Dialogue()
                        {
                            hasChoice = true,
                            line = line.Trim(charsToTrim)
                        });
                        
                        break;

                    case('\t'): // choice
                        var dialogue = new Dialogue()
                        {
                            hasChoice = true
                        };

                        switch(line[1]){
                            case('1'):
                                Choice ch = new Choice();
                                ch.choice = line.Trim(charsToTrim);

                                if(line[2] == '~'){
                                    ch.layerIndex = (int) char.GetNumericValue(line[3]);
                                }

                                Debug.Log(dialogue);
                                Debug.Log(dialogue.choices);
                                Debug.Log(ch);

                                dialogue.choices.Add(ch); 
                                break;

                            case('2'):
                                Choice ch2 = new Choice();
                                ch2.choice = line.Trim(charsToTrim);

                                if(line[2] == '~'){
                                    ch2.layerIndex = (int) char.GetNumericValue(line[3]);
                                }
                                dialogue.choices.Add(ch2);
                                break;

                        }						
                        break;

                    case('*'):
                        dialogue = new Dialogue();
                        break;

                    default:
                        Debug.Log("Err");
                        break;
                }
            }

            reader.Close();

            return script;
        }

        public static IEnumerable<Dialogue> ReadString3(string path)
        {
            var script = new List<Dialogue>();

            // Read the text directly from the test.txt file
            var reader = new StreamReader(path);

            var dialogue = new Dialogue();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                switch(line[0]){
                        
                    case('@'): // dialogue
                        dialogue.hasChoice = false;
                        dialogue.line = line.Trim(charsToTrim);

                        break;

                    case('\t'): // choice
                        dialogue.hasChoice = true;

                        switch(line[1]){
                            case('1'):
                                Choice ch = new Choice();
                                ch.choice = line.Trim(charsToTrim);

                                if(line[2] == '~'){
                                    ch.layerIndex = (int) char.GetNumericValue(line[3]);
                                }

                                dialogue.choices.Add(ch); 
                                break;

                            case('2'):
                                Choice ch2 = new Choice();
                                ch2.choice = line.Trim(charsToTrim);

                                if(line[2] == '~'){
                                    ch2.layerIndex = (int) char.GetNumericValue(line[3]);
                                }
                                dialogue.choices.Add(ch2);
                                break;

                        }						
                        break;

                    case('*'):
                        script.Add(dialogue);
                        dialogue = new Dialogue();
                        break;

                    default:
                        Debug.Log("Err");
                        break;
                }

            }



            return script;
        }

        public static IEnumerable<Dialogue> ReadString2(string path)
        {
            // Read the text directly from the test.txt file
            var reader = new StreamReader(path);

            var script = new List<Dialogue>();

            var str = reader.ReadToEnd();
            var blocks = str.Split('*');
            reader.Close();

            foreach(var block in blocks)
            {
                using (var sr = new StringReader(block))
                {
                    var dialogue = new Dialogue();
                    var line = sr.ReadLine();
                    while (line?.Length > 0)
                    {
                        Debug.Log(line);

                        switch (line[0])
                        {
                            case ('@'): // dialogue
                                dialogue.hasChoice = false;
                                dialogue.line = line.Trim(charsToTrim);
                                break;

                            case ('\t'): // choice
                                dialogue.hasChoice = true;

                                switch (line[1])
                                {
                                    case ('1'):
                                        Choice ch = new Choice();
                                        ch.choice = line.Trim(charsToTrim);

                                        if (line[2] == '~')
                                        {
                                            ch.layerIndex = (int) char.GetNumericValue(line[3]);
                                        }

                                        Debug.Log(dialogue);
                                        Debug.Log(dialogue.choices);
                                        Debug.Log(ch);

                                        dialogue.choices.Add(ch);
                                        break;

                                    case ('2'):
                                        Choice ch2 = new Choice();
                                        ch2.choice = line.Trim(charsToTrim);

                                        if (line[2] == '~')
                                        {
                                            ch2.layerIndex = (int) char.GetNumericValue(line[3]);
                                        }

                                        dialogue.choices.Add(ch2);
                                        break;

                                }

                                break;
                        }

                        line = sr.ReadLine();
                    }

                    script.Add(dialogue);
                }
            }


            return script;
        }

        public static IEnumerable<Dialogue> ReadStringNEW(string path)
        {
            // Read the text directly from the test.txt file
            var reader = new StreamReader(path);

            var script = new List<Dialogue>();

            while (!reader.EndOfStream)
            {
                var str = reader.ReadLine();
                
                var dialogue = new Dialogue();
                var split = str.Split(':');
                Debug.Log(str + " " + split.Length);

                if (split.Length != 2)
                {
                    var splitCmd = split[0]
                        .Replace("<<", string.Empty)
                        .Replace(">>", string.Empty)
                        .Split(' ');
                    if (splitCmd[0] == "wait")
                    {
                        dialogue.hasCommand = true;
                        dialogue.commandValue = Convert.ToInt32(splitCmd[1]);
                    }
                    continue;
                }
                else
                {
                    var name = split[0];
                    var line = split[1];

                    dialogue.line = line.TrimStart();
                    if (name[0] == '?')
                    {
                        // Choice
                        dialogue.hasChoice = true;
                        dialogue.name = name.Substring(2); // Truncate the "? "

                        AddChoices(dialogue, reader);
                    }
                    else
                    {
                        // Dialogue
                        dialogue.hasChoice = false;
                        dialogue.name = name;
                    }
                }

                

                script.Add(dialogue);
            }

            return script;
        }

        private static void AddChoices(Dialogue dialogue, StreamReader reader)
        {
            while (reader.Peek() == '\t')
            {
                var splitLine = reader.ReadLine().Substring(3).Split(':');
                var choice = new Choice()
                {
                    choiceOption = splitLine[0],
                    choiceResponse = splitLine[1].TrimStart()
                };
                dialogue.choices.Add(choice);
            }
        }

    }
