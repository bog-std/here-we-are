﻿using System;
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
        //private static readonly char[] charsToTrim = { '*', '~', '@', '(', ')','1','2','3','4','5','6','7','8','9'};

        public static IEnumerable<Dialogue> GetDialogue(TextAsset script)
        {
            var dialogue = ReadStringNEW2(script);
            // Debug.Log(dialogue);

            return dialogue;
        }

        static private Dictionary<char, LayerName> layerMap = new Dictionary<char, LayerName>()
        {
            {'\0', LayerName.None},
            {'R', LayerName.RelationshipSelfless},
            {'r', LayerName.RelationshipSelfish},
            {'E', LayerName.ExistentialismSelfless},
            {'e', LayerName.ExistentialismSelfish},
            {'G', LayerName.GriefSelfless},
            {'g', LayerName.GriefSelfish}
        };

        public static IEnumerable<Dialogue> ReadStringNEW2(TextAsset txt)
        {
            // Read the text directly from the test.txt file
            var reader = new StringReader(txt.ToString());

            var script = new List<Dialogue>();

            while (true)
            {
                var dialogue = new Dialogue();
                var line = reader.ReadLine();

                if (line == null) break;

                // skip empty lines 
                if (line == string.Empty) continue;

                var split = line.Split(':');

                Debug.Log(split.Length);
                
                switch (split[1])
                {
                    case "?":
                        goto case "D";
                    case "D":
                        switch (split.Length)
                        {
                            case 5:
                                goto case 4;
                            case 4:
                                dialogue.line = split[3].Trim();
                                goto case 3;
                            case 3:
                                dialogue.name = split[2];
                                goto case 2;
                            case 2:
                                dialogue.tag = split[0];
                                break;
                        }
                        if (split[1] == "?") AddChoices2(ref dialogue, reader);
                        break;
                        
                    case ">":
                        dialogue.command = Command.Skip;
                        dialogue.tag = split[2];
                        break;
                    case "+":
                        dialogue.command = Command.Increment;
                        dialogue.tag = split[0];
                        foreach (char layer in split[2])
                            if (layerMap.ContainsKey(layer))
                                dialogue.layers.Add(layerMap[layer]);
                        dialogue.magnitude = Int32.Parse(split[3]);
                        break;
                    case "@":
                        dialogue.command = Command.Scene;
                        dialogue.tag = split[0];
                        dialogue.magnitude = Int32.Parse(split[2]);
                        break;
                    case "A":
                        dialogue.tag = split[0];
                        dialogue.command = Command.SetAudio;
                        dialogue.magnitude = Int32.Parse(split[2]);
                        break;
                }

                script.Add(dialogue);
            }

            return script;
        }

           
        private static void AddChoices2(ref Dialogue dialogue, StringReader reader)
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
                        choice.magnitude = Int32.Parse(split[3]);
                        goto case 3;
                    case 3:
                        foreach (char layer in split[2])
                            if (layerMap.ContainsKey(layer))
                                choice.layers.Add(layerMap[layer]);
                        goto case 2;
                    case 2:
                        choice.target = split[1];
                        goto case 1;
                    case 1:
                        choice.choiceOption = split[0].Trim();
                        break;
                }
                
                
                Debug.Log("Adding choice " + choice.choiceOption);
                dialogue.choices.Add(choice);
            }
        }        

    }
