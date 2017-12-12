using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphLogic : MonoBehaviour {

    public int nAnswers;
    public float graphIncr, finalPerc;
    public List<GameObject> answerBarList = new List<GameObject>();
    public GameObject barPrefab;

    public List<Color32> answerColors = new List<Color32>();

    private ServerBehaviour refSB;

    //Lista di valori risposti
    public List<int> clientAnswersList = new List<int>();

    //Lista delle classi client contenenti anche le risposte
    public List<ClientsClass> clientList = new List<ClientsClass>();

    public Text noVoterText;
    public GameObject nextQuestionButton;

    IEnumerator Start ()
    {
        StartCoroutine(SearchSBCO());
        yield return new WaitForSeconds(0.5f);
        //Passa al graph il valore estratto dal file. Questo metodo verrà chiamato dal server allo scadere del tempo.
        CreateGraph();
	}

    // Cerco il referimento al Server
    public IEnumerator SearchSBCO()
    {
        while (refSB == null)
        {
            refSB = FindObjectOfType<ServerBehaviour>();
            yield return null;
        }
        Debug.Log("Ho trovato Server Behaviour");
        yield break;
    }

    public void RestartGraph()
    {
        Debug.Log("RestartGraph");
        for (int i = 0; i < answerBarList.Count; i++)
        {
            Destroy(answerBarList[i]);
        }
        answerBarList.Clear();
        clientList.Clear();
        clientAnswersList.Clear();
        CreateGraph();
    }

    //Crea il graph instanziando tante barre quante sono le risposte per quella specifica domanda
    public void CreateGraph()
    {
        //Allo start deve prendere il numero di risposte daL file in base al contatore della domanda corrente 
        nAnswers = refSB.answerStringList.Count;
        noVoterText.text = ("Astenuti: 0%");
        Debug.Log("CreateGraph");
        for (int i = 0; i < nAnswers; i++)
        {
            GameObject newBar = Instantiate(barPrefab);
            newBar.gameObject.transform.SetParent(this.gameObject.transform);
            newBar.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
            newBar.gameObject.GetComponent<Image>().color = answerColors[i];
            answerBarList.Add(newBar);

            //Qua inseriamo il testo e il colore della risposta e della percentuale
            Debug.LogWarning("answer in graph: " + refSB.answerStringList[i]);

            //Fare in modo che appaia come testo della risposta solo il numero tra parentesi così --> (num)
            string answerTextMod = refSB.answerStringList[i];

            int startIndex;
            startIndex = answerTextMod.IndexOf(")");
            answerTextMod = answerTextMod.Remove(startIndex+1);

            answerBarList[i].gameObject.transform.GetChild(1).GetComponent<Text>().text = answerTextMod;
            //answerBarList[i].gameObject.transform.GetChild(1).GetComponent<Text>().color = answerColors[i];
            //answerBarList[i].gameObject.transform.GetChild(0).GetComponent<Text>().color = answerColors[i];
        }
        //ReadData();
        //FillGraph();
    }


    public IEnumerator CallGraphSetup()
    {
        yield return new WaitForSeconds(1f);
        ReadData();
    }

    public void ReadData()
    {
        Debug.Log("CURRENT QUESTION: " + refSB.currentQuestion);
        clientList.AddRange(refSB.gameSession[refSB.currentQuestion].clientClassArch);
        for (int i = 0; i < clientList.Count; i++)
        {
            clientAnswersList.Add(clientList[i].indexAnswerChoose);
        }
        nextQuestionButton.SetActive(true);
        FillGraph();
        refSB.SaveFileGameSessionData();
        
    }

    //Riempie ogni singola barra in base a quanti utenti hanno dato quella specifica risposta
    //Se la risposta corrisponde a i allora aggiungi altrimenti passa avanti
    public void FillGraph()
    { 
        graphIncr = 1f / clientAnswersList.Count;
        //Serve a far comparire il count di quante volte è stata scelta quella domanda
        float sameAnswers = 0f;
        Debug.Log("Graph Incr: " + graphIncr);
        float noVoterCount = 0f;
        float percSingleClient = (100f / clientAnswersList.Count);
        Debug.Log("PercSingleClient: " + percSingleClient);

        for (int i = 0; i < answerBarList.Count; i++)
        {
            sameAnswers = 0;
            for (int j = 0; j < clientAnswersList.Count; j++)
            {
                if(clientAnswersList[j] == i)
                {
                    Debug.Log("ClientAnswers " + j + ": " + clientAnswersList[j]);
                    StartCoroutine(FillGraduateGraphCO(answerBarList[i]));
                    //answerBarList[i].gameObject.GetComponent<Image>().fillAmount += graphIncr;
                    sameAnswers++;
                    
                    Debug.Log("nClient: " + clientAnswersList.Count);
                } 

                finalPerc = sameAnswers * percSingleClient;
                if (finalPerc >= 99f)
                    finalPerc = 100;
                finalPerc = (int)finalPerc;
                StartCoroutine(FillGraduateGraphCO(answerBarList[i]));
                //Invoke(PrintPerc(answerBarList[i]), .1f);
                //answerBarList[i].gameObject.transform.GetChild(0).GetComponent<Text>().text = (finalPerc.ToString() + "%");
            }         
        }


        for (int j = 0; j < clientAnswersList.Count; j++)
        {
            if (clientAnswersList[j] == -1)
            {
                noVoterCount++;
                Debug.Log("No Voter Count: " + noVoterCount);
            }
        }

        float percNoVoters = noVoterCount * percSingleClient;
        if (percNoVoters >= 99f)
            percNoVoters = 100;
        percNoVoters = (int)percNoVoters;
        noVoterText.text = ("Astenuti: " + percNoVoters.ToString() + "%");
    }

    private IEnumerator FillGraduateGraphCO(GameObject _go)
    {
        while (_go.GetComponent<Image>().fillAmount != graphIncr)
        {
            _go.GetComponent<Image>().fillAmount += graphIncr * Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(.5f);
        _go.transform.GetChild(0).GetComponent<Text>().text = (finalPerc.ToString() + "%");
    }
}
