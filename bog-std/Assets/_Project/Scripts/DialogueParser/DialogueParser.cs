using System;
using System.Collections.Generic;
using System.IO;
using _Project.Scripts.Scene;
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
            private static Dictionary<LayerName, List<string>> _availableImages = new Dictionary<LayerName, List<string>>();

            private static List<string> _availableAudio = new List<string>();
            
            private static Dictionary<char, LayerName> _layerMap = new Dictionary<char, LayerName>()
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
                                if (split.Length > 3)
                                {
                                    dialogue.name = split[3];
                                    dialogue.magnitude = Convert.ToInt32(split[4]);
                                }
                                break;

                            // Set layer to image
                            case "=":
                                dialogue.command = Command.Set;
                                dialogue.tag = split[0];
                                if (split[2].Length > 0) foreach (char layer in split[2])
                                    if (_layerMap.ContainsKey(layer))
                                    {
                                        dialogue.layers.Add(_layerMap[layer]);
                                        CheckImage(_layerMap[layer], split[3]);
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
                                    if (_layerMap.ContainsKey(layer)) dialogue.layers.Add(_layerMap[layer]);
                                    else throw new Exception(split[2] + " is not a layer");
                                else throw new Exception("No layers");
                                dialogue.magnitude = Int32.Parse(split[3]);
                                break;
                        
                            // Wait
                            case "W":
                                dialogue.command = Command.Wait;
                                dialogue.tag = split[0];
                                dialogue.magnitude = Convert.ToSingle(split[2]);
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

                            // Set Fact
                            case "~":
                                dialogue.command = Command.Fact;
                                dialogue.tag = split[0];
                                dialogue.name = split[2]; // Scene # 
                                dialogue.magnitude = Convert.ToUInt16(split[3]);
                                break;
                            
                            case "#":
                                dialogue.command = Command.Messages;
                                dialogue.tag = split[0];
                                break;
                            
                            case "M":
                                dialogue.command = Command.Menu;
                                dialogue.tag = split[0];
                                break;
                            
                            case "A":
                                dialogue.command = Command.Audio;
                                dialogue.tag = split[0];
                                dialogue.name = split[2];
                                dialogue.magnitude = Convert.ToSingle(split[3]);
                                CheckAudio(dialogue.name);
                                break;

                            case "!":
                                dialogue.command = Command.Notification;
                                dialogue.tag = split[0];
                                break;

                            case "$":
                                dialogue.command = Command.CarCrash;
                                dialogue.tag = split[0];
                                break;

                            case "_":
                                dialogue.command = Command.RipJordan;
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

            private static bool CheckAudio(string name)
            {
                // Get audio tracks from LayeredScene
                if (_availableAudio.Count == 0)
                {
                    _availableAudio = FindObjectOfType<LayeredScene>().GetComponent<LayeredScene>().audioTrackNames;
                    _availableAudio.Add("all");
                }

                // See if audio track is listed and log error if not
                bool hasTrack = _availableAudio.Contains(name);
                if (!hasTrack) Debug.LogError("Audio track '" + name + "' not found");
                return hasTrack;
            }
            
            
            private static bool CheckImage(LayerName layerName, string levelName)
            {
                // Get image options from LayeredScene if we do not have them yet
                if (_availableImages.Count == 0) _availableImages = FindObjectOfType<LayeredScene>().GetComponent<LayeredScene>().GetImageOptions();
            
                // See if corresponding image exists in this layer and warn if not
                bool hasImage = _availableImages[layerName].Contains(levelName);
                if (!hasImage) Debug.LogWarning("Layer "+ layerName + " does not contain image '" + levelName + "'.");
                return hasImage;
            }
            
        }
    }
    