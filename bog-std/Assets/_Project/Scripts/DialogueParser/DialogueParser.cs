using System;
using System.Collections.Generic;
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
//
// NOTE: the parser now supports empty lines, so we can separate blocks of dialogue,
//       and we can make comments by beginning the line with '#' that will not be parsed

    public class DialogueParser : MonoBehaviour
    {
        public static IEnumerable<Dialogue> GetDialogue(TextAsset script)
        {
            var dialogue = ReadString(script);
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

            while (true)
            {
                var dialogue = new Dialogue();
                var line = reader.ReadLine();

                if (line == null) break;

                // skip empty lines and comments
                if (line == string.Empty || line[0] == '#') continue;

                var split = line.Split(':');

                Debug.Log(split.Length);

                if (split.Length == 1)
                {
                    Debug.Log("Length 1: " + split[0]);
                    continue;
                }
                
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
                        goto case "+";
                    case "+":
                        dialogue.tag = split[0];
                        foreach (char layer in split[2])
                            if (layerMap.ContainsKey(layer))
                                dialogue.layers.Add(layerMap[layer]);
                        if (split[1] == "+")
                        {
                            dialogue.magnitude = Int32.Parse(split[3]);
                            dialogue.command = Command.Increment;
                        }
                        else
                        {
                            dialogue.command = Command.Set;
                            dialogue.name = split[3];
                        }
                        break;
                    case "W":
                        dialogue.tag = split[0];
                        dialogue.magnitude = Int32.Parse(split[2]);
                        dialogue.command = Command.Wait;
                        if (split.Length > 3) dialogue.name = split[3];
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
