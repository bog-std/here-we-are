using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

//a parser that goes through a text file extracting info
//special chars used so far are :
// (Dx) (Cx) where x is an int
// ~x where x is a number indicating the layer to be increased
// ~~xy where x is the layer and y is the score
// /n and /t are used but just for readability purposes

public class Test : MonoBehaviour
{

	struct choicee
	{
		public string choice;
		public double layernumber;
	}

	struct dialogg
	{
		public bool choice; //if True == choice if False == dialog no choices
		public string message; // the dialog massege
		public List<choicee> choices;
	}

	[MenuItem("Tools/Read file")]
	static void ReadString()
	{
		string path = "Assets/Resources/test.txt";

		//Read the text from directly from the test.txt file
		StreamReader reader = new StreamReader(path);
		dialogg dialog = new dialogg();
		dialog.choices = new List<choicee>();
		char[] charsToTrim = { '*', '~', '@', '(', ')','1','2','3','4','5','6','7','8','9'};

		while (!reader.EndOfStream)
		{
			string line = reader.ReadLine();

				switch(line[0]){
					
					case('@'): //dialog
						dialog.choice = false;
						dialog.message = line.Trim(charsToTrim);
						break;

					case('\t'): //choise
						dialog.choice = true;

						switch(line[1]){
							case('1'):
							choicee Ch = new choicee();
								Ch.choice = line.Trim(charsToTrim);

								if(line[2] == '~'){
									Ch.layernumber = char.GetNumericValue(line[3]);
								}

								dialog.choices.Add(Ch); 
								break;

							case('2'):
								choicee Ch2 = new choicee();
								Ch2.choice = line.Trim(charsToTrim);

								if(line[2] == '~'){
									Ch2.layernumber = char.GetNumericValue(line[3]);
								}
								dialog.choices.Add(Ch2);
								break;

						}						
						break;

					case('*'):
						dialog = new dialogg();
						break;

					default:
						Debug.Log("Err");
						break;
				}
		}

		reader.Close();
	}

}