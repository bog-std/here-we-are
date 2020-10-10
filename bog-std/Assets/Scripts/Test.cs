using UnityEngine;
using UnityEditor;
using System.IO;

public class Test : MonoBehaviour
{

    public TextAsset textAsset;

    [MenuItem("Tools/Write file")]
    static void WriteString()
    {
        string path = "Assets/Resources/test.txt";
        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(path); 
        TextAsset asset = Resources.Load("test") as TextAsset;

        //Print the text from the file
        Debug.Log(asset.text);
    }

}