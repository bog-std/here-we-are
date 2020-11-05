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
            var dialogue = ReadStringNEW2("Assets/_Project/Resources/script.txt");
            // Debug.Log(dialogue);

            return dialogue;
        }       

        // public static IEnumerable<Dialogue> ReadStringNEW(string path)
        // {
        //     // Read the text directly from the test.txt file
        //     var reader = new StreamReader(path);
        //
        //     var script = new List<Dialogue>();
        //
        //     while (!reader.EndOfStream)
        //     {
        //         var str = reader.ReadLine();
        //         
        //         var dialogue = new Dialogue();
        //         var split = str.Split(':');
        //         // Debug.Log(str + " " + split.Length);
        //
        //         if (split.Length != 2)
        //         {
        //             var splitCmd = split[0]
        //                 .Replace("<<", string.Empty)
        //                 .Replace(">>", string.Empty)
        //                 .Split(' ');
        //             if (splitCmd[0] == "wait")
        //             {
        //                 dialogue.hasCommand = true;
        //                 dialogue.commandValue = Convert.ToInt32(splitCmd[1]);
        //             }
        //             continue;
        //         }
        //         else
        //         {
        //             var name = split[0];
        //             var line = split[1];
        //
        //             dialogue.line = line.TrimStart();
        //             if (name[0] == '?')
        //             {
        //                 // Choice
        //                 dialogue.hasChoice = true;
        //                 dialogue.name = name.Substring(2); // Truncate the "? "
        //
        //                 AddChoices(ref dialogue, reader);
        //             }
        //             else
        //             {
        //                 // Dialogue
        //                 dialogue.hasChoice = false;
        //                 dialogue.name = name;
        //             }
        //         }
        //
        //         
        //
        //         script.Add(dialogue);
        //     }
        //
        //     return script;
        // }
        
        public static IEnumerable<Dialogue> ReadStringNEW2(string path)
        {
            // Read the text directly from the test.txt file
            var reader = new StreamReader(path);

            var script = new List<Dialogue>();

            while (!reader.EndOfStream)
            {
                var dialogue = new Dialogue();
                var split = reader.ReadLine().Split(':');

                switch (split[0])
                {
                    case "D":
                        switch (split.Length)
                        {
                            case 5:
                                goto case 4;
                            case 4:
                                dialogue.tag = Int32.Parse(split[3]);
                                goto case 3;
                            case 3:
                                dialogue.line = split[2].Trim();
                                goto case 2;
                            case 2:
                                dialogue.name = split[1];
                                break;
                        }
                        break;
                    
                    case "?":
                        switch (split.Length)
                        {
                            case 5:
                                goto case 4;
                            case 4:
                                dialogue.tag = Int32.Parse(split[3]);
                                goto case 3;
                            case 3:
                                dialogue.line = split[2].Trim();
                                goto case 2;
                            case 2:
                                dialogue.name = split[1];
                                break;
                        }
                        AddChoices2(ref dialogue, reader);
                        break;
                        
                    case ">":
                        dialogue.command = Command.Skip;
                        dialogue.tag = Int32.Parse(split[1]);
                        break;
                }

                script.Add(dialogue);
            }

            return script;
        }
        
        private static void AddChoices2(ref Dialogue dialogue, StreamReader reader)
        {
            while (reader.Peek() == '\t')
            {
                var split = reader.ReadLine().Split(':');
                var choice = new Choice();

                switch (split.Length)
                {
                    case 5:
                        goto case 4;
                    case 4:
                        goto case 3;
                    case 3:
                        goto case 2;
                    case 2:
                        choice.choiceOption = split[1];
                        choice.target = Int32.Parse(split[0]);
                        break;
                }
                
                
                Debug.Log("Adding choice " + choice.choiceOption);
                dialogue.choices.Add(choice);
            }
        }

    }
