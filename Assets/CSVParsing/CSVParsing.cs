using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;

public class CSVParsing : MonoBehaviour 
{
	public TextAsset csvFile; // file CSV da leggere
	

	private char lineSeparater = '\n'; 
	private char fieldSeparator = ','; 

    public int inizio = 0;
    public int limite = 9;

    string[] rows;
    public Text [] answers;
    

    void Start () 
	{
		ReadCSV();
	}

    public void NextQuestion()
    {
        inizio += 10;
        limite += 10;
        ReadCSV();
    }
	
    public void ReadCSV()
    {
        string[] rows = csvFile.text.Split(lineSeparater);
        string[] data = new string[rows.Length];
        //legge la cella in ordine di posizione lungo la prima colonna
        int j = 0;
        for (int i = inizio; i < limite; i++)
        {
            
            data[i] = rows[i];
            answers[j].text = data[i];
           
            j++;

        }
        
    }
   

}