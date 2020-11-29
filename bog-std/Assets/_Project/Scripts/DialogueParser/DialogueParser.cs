﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Assets._Project.Scripts.DialogueData;
using UnityEditor;
using UnityEngine;

// Parser that reads through text file and converts it to dialogue objects
// Splits lines on ':'
// split[0] is a tag for the line -> use to branch to this line
// split[1] is the type token, with the following possible symbols
//    D - dialogue line - speaker name = split[2], dialogue = split[3]
//    ? - dialogue line with choices - speaker name = split[2], dialogue = split[3]
//            choices are tabbed
//            choices have format dialogue = split[0], branch target = split[1], DEPRECIATED split[2] and split[3] (work the same as '+' below)
//            not setting a branch target causes branch to go first line following the choices
//    > - skip lines -> will skip ahead to line tagged with split[2]
//    + - increments layers listed in split[2] (E, ReG, eg, etc..) by magnitude in split[3]
//    = - sets layer in split[2][0] (J) (S) (*J = Jordan*, *S = scene*) to layer level name split[3]
//    @ - clears script and loads new script
//            split[0] = tag to return to after script finishes(MUST BE UNIQUE - use random letters if necessary, name not important), no argument returns to beginning of previous script when script finishes
//            split[2] = name of new script, or "pop" if we want to return to previous script
//
// NOTE: the parser now supports empty lines, so we can separate blocks of dialogue,
//       and we can make comments by beginning the line with '#' that will not be parsed

    public class DialogueParser : MonoBehaviour
    {
        public static IEnumerable<Dialogue> GetDialogue(TextAsset script)
        {
            if (imageOptions.Count == 0)
                imageOptions = FindObjectOfType<LayeredScene>().GetComponent<LayeredScene>().GetImageOptions();
            
            var dialogue = ReadString(script);
            // Debug.Log(dialogue);
            return dialogue;
        }

        static private Dictionary<LayerName, List<string>> imageOptions = new Dictionary<LayerName, List<string>>();

        static private bool CheckImage(LayerName layerName, string levelName)
        {
            bool hasImage = imageOptions[layerName].Contains(levelName);
            if (!hasImage)
                Debug.LogError(layerName + " does not contain image " + levelName);
            return hasImage;
        }

        static private Dictionary<char, LayerName> layerMap = new Dictionary<char, LayerName>()
        {
            {'\0', LayerName.None},
            {'R', LayerName.RelationshipSelfless},
            {'r', LayerName.RelationshipSelfish},
            {'E', LayerName.ExistentialismSelfless},
            {'e', LayerName.ExistentialismSelfish},
            {'G', LayerName.GriefSelfless},
            {'g', LayerName.GriefSelfish},
            {'S', LayerName.Scene},
            {'A', LayerName.Audio},
            {'J', LayerName.Jordan},
            {'1', LayerName.Extra1},
            {'2', LayerName.Extra2},
            {'3', LayerName.Extra3},
        };

        public static IEnumerable<Dialogue> ReadString(TextAsset txt)
        {
            // Read the text directly from the test.txt file
            var reader = new StringReader(txt.ToString());

            var script = new List<Dialogue>();

            string line = String.Empty;

            try
            {
                while (true)
                {
                    var dialogue = new Dialogue();
                    line = reader.ReadLine();

                    if (line == null) break;

                    // skip empty lines and comments
                    if (line == string.Empty || line[0] == '#') continue;

                    var split = line.Split(':');
                    
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

                        case "=":
                            dialogue.command = Command.Set;
                            dialogue.tag = split[0];
                            if (split[2].Length > 0)
                                foreach (char layer in split[2])
                                {
                                    if (layerMap.ContainsKey(layer))
                                    {
                                        dialogue.layers.Add(layerMap[layer]);
                                        CheckImage(layerMap[layer], split[3]);
                                    }
                                }
                                
                            else throw new Exception("No layers ");
                            dialogue.name = split[3];
                            
                            break;
                        
                        case "+":
                            dialogue.command = Command.Increment;
                            dialogue.tag = split[0];
                            if (split[2].Length > 0) foreach (char layer in split[2])
                                if (layerMap.ContainsKey(layer))
                                    dialogue.layers.Add(layerMap[layer]);
                            else throw new Exception("No layers");
                            dialogue.magnitude = Int32.Parse(split[3]);
                            break;
                        
                        case "W":
                            dialogue.command = Command.Wait;
                            dialogue.tag = split[0];
                            dialogue.magnitude = Int32.Parse(split[2]);
                            if (split.Length > 3) dialogue.name = split[3];
                            break;
                        
                        case "@":
                            dialogue.command = Command.LoadScript;
                            dialogue.tag = split[0];
                            dialogue.name = split[2];
                            break;
                        
                        default:
                            throw new Exception("Bad Token");
                    }
                    
                    script.Add(dialogue);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to parse line: " + line);
                Debug.LogException(e);
            }

            return script;
        }

           
        private static void AddChoices2(ref Dialogue dialogue, StringReader reader)
        {
            try
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

                    dialogue.choices.Add(choice);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Choice parsing error");
            }
        }
    }
    