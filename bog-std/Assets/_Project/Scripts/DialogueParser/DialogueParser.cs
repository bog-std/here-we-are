using System;
using System.Collections.Generic;
using System.IO;
using Assets._Project.Scripts.DialogueData;
using UnityEngine;

/*
 * Parser that reads through a txt file and converts it to dialogue objects
 * 
 * '#' as the first char of line works as a comment (will not be parsed)
 * 
 * Splits lines on ':'
 * 
 * split[0] is a tag for the line -> use to branch to this line
 * 
 * split[1] is the type token, with the following possible symbols:
 * 
 *    D - dialogue line - speaker name = split[2], dialogue = split[3]
 * 
 *    ? - dialogue line with choices - speaker name = split[2], dialogue = split[3]
 *            choices must be tabbed
 *            split[0] - dialogue
 *            split[1] - branch target (will fall through if no branch target)
 *
 *    > - skip lines -> will skip ahead to line tagged with split[2]
 * 
 *    + - increments layers listed in split[2] (E, ReG, eg, etc..) by magnitude in split[3]
 * 
 *    = - sets layer in split[2] (J) (S) (*J = Jordan*, *S = scene*) to layer level name split[3]
 * 
 *    @ - clears script and loads new script
 *            split[2] - name of new script, or "pop" if we want to return to previous script
 *            split[3] - if set, specifies custom return offset from start of script after pop (if not set, returns to same position it was called in, 0 goes to start of script)
 */

    namespace _Project.Scripts.DialogueParser
    {
        public class DialogueParser : MonoBehaviour
        {
            private static Dictionary<LayerName, List<string>> availableImages = new Dictionary<LayerName, List<string>>();
        
            private static Dictionary<char, LayerName> layerMap = new Dictionary<char, LayerName>()
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
        
            
            public static IEnumerable<Dialogue> GetDialogue(TextAsset script)
            {
                var dialogue = ReadString(script);
                // Debug.Log(dialogue);
                return dialogue;
            }
            

            private static IEnumerable<Dialogue> ReadString(TextAsset txt)
            {
                // Read the text directly from the txt file
                var reader = new StringReader(txt.ToString());

                // Script output
                var script = new List<Dialogue>();

                // Current line
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
                            // Dialogue with prompt
                            case "?":
                                AddChoices2(ref dialogue, reader);
                                goto case "D";
                            
                            // Dialogue    
                            case "D":
                                dialogue.line = split[3].Trim();
                                dialogue.name = split[2];
                                dialogue.tag = split[0];
                                break;
                        
                            // Forward to tag
                            case ">":
                                dialogue.command = Command.Skip;
                                dialogue.tag = split[2];
                                break;

                            // Set layer to image
                            case "=":
                                dialogue.command = Command.Set;
                                dialogue.tag = split[0];
                                if (split[2].Length > 0) foreach (char layer in split[2])
                                    if (layerMap.ContainsKey(layer))
                                    {
                                        dialogue.layers.Add(layerMap[layer]);
                                        CheckImage(layerMap[layer], split[3]);
                                    } 
                                    else throw new Exception(split[2] + " is not a layer");
                                else throw new Exception("No layers ");
                                dialogue.name = split[3];
                                if (split.Length > 4) dialogue.magnitude = Convert.ToInt32(split[4]);
                                break;
                        
                            // Increment layer level
                            case "+":
                                dialogue.command = Command.Increment;
                                dialogue.tag = split[0];
                                if (split[2].Length > 0) foreach (char layer in split[2])
                                    if (layerMap.ContainsKey(layer)) dialogue.layers.Add(layerMap[layer]);
                                    else throw new Exception(split[2] + " is not a layer");
                                else throw new Exception("No layers");
                                dialogue.magnitude = Int32.Parse(split[3]);
                                break;
                        
                            // Wait
                            case "W":
                                dialogue.command = Command.Wait;
                                dialogue.tag = split[0];
                                dialogue.magnitude = Convert.ToInt32(split[2]);
                                if (split.Length > 3) dialogue.name = split[3];
                                break;
                        
                            // Change script
                            case "@":
                                dialogue.command = Command.Script;
                                dialogue.tag = split[0];
                                dialogue.name = split[2];
                                dialogue.magnitude = split.Length > 3 ? Convert.ToInt32(split[3]) : -1;
                                break;
                        
                            // Open phone
                            case "*":
                                dialogue.command = Command.Phone;
                                dialogue.tag = split[0];
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
                var line = String.Empty;
            
                try
                {
                    while (reader.Peek() == '\t')
                    {
                        line = reader.ReadLine();
                        var split = line.Split(':');

                        var choice = new Choice
                        {
                            choiceOption = split[0].Trim(),
                            target = split.Length > 1 ? split[1] : String.Empty
                        };
          
                        dialogue.choices.Add(choice);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Choice parsing error in line: " + line);
                }
            }
            
            
            private static bool CheckImage(LayerName layerName, string levelName)
            {
                // Get image options from LayeredScene if we do not have them yet
                if (availableImages.Count == 0) availableImages = FindObjectOfType<LayeredScene>().GetComponent<LayeredScene>().GetImageOptions();
            
                // See if corresponding image exists in this layer and warn if not
                bool hasImage = availableImages[layerName].Contains(levelName);
                if (!hasImage) Debug.LogWarning("Layer "+ layerName + " does not contain image '" + levelName + "'.");
                return hasImage;
            }
            
        }
    }
    