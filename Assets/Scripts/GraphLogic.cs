using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphLogic : MonoBehaviour {

    public int nAnswers;
    //public float graphIncr, finalPerc, sameAnswers, percSingleClient;
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
    public int nTryQuestion;

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
            Debug.LogWarning("answer in graph: " + refSB.answerStringList[i].answerText);

            //Fare in modo che appaia come testo della risposta solo il numero tra parentesi così --> (num)
            string answerTextMod = refSB.answerStringList[i].answerText;

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
        float graphIncr = 1f / clientAnswersList.Count;
        //Serve a far comparire il count di quante volte è stata scelta quella domanda
        float sameAnswers = 0f;
        Debug.Log("Graph Incr: " + graphIncr);
        //float noVoterCount = 0f;
        float percSingleClient = (100f / clientAnswersList.Count);
        Debug.Log("PercSingleClient: " + percSingleClient);

        for (int i = 0; i < answerBarList.Count; i++)
        {
            sameAnswers = 0;

            for (int j = 0; j < clientAnswersList.Count; j++)
            {
                if(clientAnswersList[j] == i)
                {            
                    //answerBarList[i].gameObject.GetComponent<Image>().fillAmount += graphIncr;
                    sameAnswers++;
                    float finalPerc = sameAnswers * percSingleClient;
                    if (finalPerc >= 99f)
                        finalPerc = 100;
                    finalPerc = (int)finalPerc;

                    StartCoroutine(FillGraduateGraphCO((answerBarList[i]), graphIncr, sameAnswers));
                    StartCoroutine(CalcAndPrintPercCO((answerBarList[i]), finalPerc));
                    Debug.LogError("SameAnswer: " + sameAnswers);
                    Debug.LogError("FinalPerc: " + finalPerc);

                }
                //answerBarList[i].gameObject.transform.GetChild(0).GetComponent<Text>().text = (finalPerc.ToString() + "%");
            }         
        }

        //for (int j = 0; j < clientAnswersList.Count; j++)
        //{
        //    if (clientAnswersList[j] == -1)
        //    {
        //        noVoterCount++;
        //        Debug.Log("No Voter Count: " + noVoterCount);
        //    }
        //}

        //float percNoVoters = noVoterCount * percSingleClient;
        //if (percNoVoters >= 99f)
        //    percNoVoters = 100;
        //percNoVoters = (int)percNoVoters;
        //noVoterText.text = ("Astenuti: " + percNoVoters.ToString() + "%");
    }

    private IEnumerator FillGraduateGraphCO(GameObject _go, float _graphIncr, float _sameAnswers)
    {
        Debug.LogWarning("DENTRO FILL");
        float graphIncrNew = _graphIncr;
        if(_sameAnswers > 1)
        {
            graphIncrNew = _graphIncr * _sameAnswers;
        }

        while (_go.GetComponent<Image>().fillAmount <= graphIncrNew)
        {
            _go.GetComponent<Image>().fillAmount += graphIncrNew * Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator CalcAndPrintPercCO(GameObject _go, float _finalPerc)
    {
        yield return null; //new WaitForSeconds(1f);
        _go.transform.GetChild(0).GetComponent<Text>().text = (_finalPerc.ToString() + "%");
    }

    public void FinalGraphSetup()
    {
        //Ripuliamo il graph
        for (int i = 0; i < answerBarList.Count; i++)
        {
            Destroy(answerBarList[i]);
        }
        answerBarList.Clear();
        clientList.Clear();
        clientAnswersList.Clear();

        //Creiamo il graph con tante barre quanto il numero dei player
        for (int i = 0; i < refSB.finalGraphPlayerList.Count; i++)
        {
            GameObject newBar = Instantiate(barPrefab);
            newBar.gameObject.transform.SetParent(this.gameObject.transform);
            newBar.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
            newBar.gameObject.GetComponent<Image>().color = answerColors[i];
            answerBarList.Add(newBar);

            //Aumentiamo lo spazio tra le barre
            this.gameObject.GetComponent<HorizontalLayoutGroup>().spacing = 130;

            //Settiamo nel answerText il nome del player
            answerBarList[i].gameObject.transform.GetChild(1).GetComponent<Text>().text = refSB.finalGraphPlayerList[i].namePlayer;

            //Chiamiamo la coroutine che riempie la barra
            StartCoroutine(FillGraduateFinalGraphCO(answerBarList[i], refSB.finalGraphPlayerList[i].counterRightAnswer));

            //Chiamiamo la coroutine che setta il numero di risposte
            //StartCoroutine(SetNumCorrectAnswersFinalGraph(answerBarList[i], refSB.finalGraphPlayerList[i].counterRightAnswer));
        }
    }

    private IEnumerator FillGraduateFinalGraphCO(GameObject _go, int nCorrectAnswers)
    {
        Debug.LogWarning("DENTRO FILL FINAL GRAPH");

        //Incrementiamo la barra di 1/nDomande * nRispCorrette
        int nQuestion = refSB.gameSession.Count - nTryQuestion;

        float graphIncrFinal = 1f / nQuestion;

        float finalIncrValue = graphIncrFinal * nCorrectAnswers;

        while (_go.GetComponent<Image>().fillAmount <= finalIncrValue)
        {
            _go.GetComponent<Image>().fillAmount += graphIncrFinal * Time.deltaTime;
            yield return null;
        }
        // Chiamo qui la coroutine per far stampare il numero in questo modo è indipendente da quanto veloce si riempione le barre
        StartCoroutine(SetNumCorrectAnswersFinalGraph(_go, nCorrectAnswers));
    }

    //Settiamo dentro nAnswerCounter il n° di risposte corrette date dal player
    private IEnumerator SetNumCorrectAnswersFinalGraph(GameObject _go, int nCorrectAnswers)
    {
        yield return new WaitForSeconds(.5f);
        _go.transform.GetChild(0).GetComponent<Text>().text = nCorrectAnswers.ToString();
    }

}
