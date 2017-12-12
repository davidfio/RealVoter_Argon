using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;



public class ServerBehaviour : NetworkBehaviour
{
    public List<ClientsClass> clientsList = new List<ClientsClass>();
    
    //public Dictionary<int, List<ClientsClass>> dictVoters = new Dictionary<int, List<ClientsClass>>();

    public List<GameSessionClass> gameSession = new List<GameSessionClass>();

    // Contatore della domanda corrente per il dictVoters
    public int currentQuestion = 0;

    [SyncVar]
    public float timerCounter = 0f;
    [SyncVar]
    public float finalTimer = 0f;
    [SyncVar]
    public string questionString;

    public Text feedbackText, questionText, timerText;
    private GraphLogic refGL;

    public GameObject buttonStart;
    public GameObject buttonStop;
    public bool noMoreQuestion;

#region ParserArea
    public TextAsset csvFile; // file CSV da leggere

    private char lineSeparater = '\n';
    private char fieldSeparator = ',';

    public int startCSVIndex = 0;
    private int endCSVIndex = 0;

    List<string> readedData = new List<string>();

#endregion

    // Lista delle risposte da mandare ai Clients
	public List<AnswerClass> answerStringList = new List<AnswerClass>();

    // Lista di tutti i Clients
    public List<GameObject> playerList = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(AddPlayerCO());
       
        refGL = FindObjectOfType<GraphLogic>();
        
        //Legge i dati da file
        readedData.AddRange(csvFile.text.Split(lineSeparater));
        
        //Metodo che passa il testo della domanda da file
        SetDataFromCSV();

        Invoke("CreateNewGameSession", 1f);
    }


    //Controlla se ci sono giocatori che hanno abbandonato la sessione e li rimuove dalla playerlist
    private void CleanPlayerList ()
    {

        Debug.Log("CLEAN");

        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i] == null)
                playerList.RemoveAt(i);
        }

        if(playerList.Count == 0)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    public void SetDataFromCSV()
    {
        string endLine = "--";
        string endFile = "Fine";
        Debug.Log("ReadCSV");
        //legge e imposta la domanda
        questionString = readedData[startCSVIndex];
        questionText.text = questionString;

        //DAVID DEVE SISTEMARE QUESTA COSA DEL 1000
        for (int i = endCSVIndex; i < 1000; i++)
        {
            if (!readedData[i].Contains(endLine))
            {
                endCSVIndex++;
            }
            else
            {
                if (readedData[i+1].Contains(endFile))
                {
                    Debug.Log("DOMANDE FINITE");
                    noMoreQuestion = true;
                    break;
                } else
                {
                    Debug.Log("FINITO DI LEGGERE LE RISPOSTE");
                    break;
                } 
            }
            Debug.LogWarning(endCSVIndex);
        }

        //legge la cella in ordine di posizione lungo la prima colonna
        int j = -1;
        startCSVIndex += 1;
        for (int i = startCSVIndex; i < endCSVIndex; i++)
        {
            j++;

            AnswerClass answerReadClass = null;

            //Controlliamo se la stringa ha il carattere $ e in caso lo escludiamo
            if (readedData[i].Contains("$"))
            {
                //Qui dobbiamo fare in modo che questa risposta venga segnata come corretta con un bool da qualche parte!!!
                //Answerstringlist credo debba diventare una classe che abbia come proprietà il bool corretta
                //oltre alla stringa di testo della risposta stessa! E poi riadattare tutti i metodi di conseguenza.


                readedData[i] = readedData[i].Replace("$", "");
                Debug.Log("TROVATO $: " + readedData[i].ToString());
                answerReadClass = new AnswerClass(true, readedData[i]);

            }
            else
            {
                answerReadClass = new AnswerClass(false, readedData[i]);
            }

            answerStringList.Insert(j, answerReadClass);
            Debug.Log(answerStringList[j]);
        }
    }

    //Esegue le funzionalità del bottone Next
    public void NextQuestion()
    {
        //inserire qui la condizione in base al fatto se ci sono ancora domande o se il televoting è finito
        if (!noMoreQuestion)
        {
            StartCoroutine(CallResetOnClient());
            answerStringList.Clear();
            currentQuestion++;
            endCSVIndex++;
            startCSVIndex = endCSVIndex;
            timerText.text = ("Tempo");
            SetDataFromCSV();
            CreateNewGameSession();
            refGL.RestartGraph();
            buttonStart.SetActive(true);
        } else
        {
            StartCoroutine(CallResetOnClient());
            questionText.text = ("Fine del questionario. Grazie per aver partecipato!");
            Debug.Log("TELEVOTING FINITO");
        }
       
    }

    //Crea la nuova GameSession e ci aggiunge la domanda corrente
    private void CreateNewGameSession()
    {
        gameSession.Add(new GameSessionClass());
        gameSession[currentQuestion].questionStringText = questionString;
    }

    // Quando il timer finisce lo resetto e disattivo tutti i bottoni sui client e mi faccio mandare la risposta scelta
    public IEnumerator TimerCO()
    {
        timerCounter = 10f;
        Debug.LogError("Il timer è partito e parte da: " + timerCounter);
        while (timerCounter > finalTimer)
        {
            yield return new WaitForSecondsRealtime(1f);
            timerCounter--;
            timerText.text = (timerCounter / 60).ToString("00") + ":" + (timerCounter % 60).ToString("00");
        }

        //timerCounter = 0;
        timerText.text = ("Tempo scaduto!");

        CleanPlayerList();

        foreach (var player in playerList)
        {
            player.GetComponent<PlayerBehaviour>().RpcDeactiveButton();
            player.GetComponent<PlayerBehaviour>().RpcSelectedAnswer();
        }

        buttonStop.SetActive(false);
        StartCoroutine(refGL.CallGraphSetup());
        
        yield break;
    }

    public void StopTimer ()
    {
        timerCounter = 0;
    }

    //Termina la fase in cui si può rispondere. Disattiva tutti i bottoni sul client e manda le risposte al graph
    public void StopQuestion ()
    {
        CleanPlayerList();

        foreach (var player in playerList)
        {
            player.GetComponent<PlayerBehaviour>().RpcDeactiveButton();
            player.GetComponent<PlayerBehaviour>().RpcSelectedAnswer();
        }

        buttonStop.SetActive(false);
        StartCoroutine(refGL.CallGraphSetup());
    }

    // Mette tutti i giocatori nella lista PlayerList
    public IEnumerator AddPlayerCO()
    {
        yield return new WaitForSeconds(0.5f);
        // lista di player (clients)
        playerList.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        
        Debug.Log("Ho fatto la ricerca dei giocatori");
        Debug.Log("I giocatori ora in gioco sono: " + playerList.Count);

        CleanPlayerList();

        foreach (var player in playerList)
        {
            Debug.Log(player.GetComponentInChildren<PlayerBehaviour>().name);
        }
    }

    //Setta tutto il necessario sui client
    public void SetupClient()
    {
        SetQuestionOnClient(questionString);

        CreateButtonOnClient(answerStringList.Count);

        SetAnswerOnClient();

        //StartCoroutine(TimerCO());
    }


    //Chiama il reset del client
    public IEnumerator CallResetOnClient()
    {

        CleanPlayerList();

        foreach (var player in playerList)
        {
            player.GetComponent<PlayerBehaviour>().RpcResetClient(noMoreQuestion);
        }
        yield return new WaitForSeconds(0.1f);
    }

    // Crea i pulsanti premibili dai clients
    public void CreateButtonOnClient(int _numberOfString)
    {

        CleanPlayerList();

        foreach (var player in playerList)
        {
            player.GetComponent<PlayerBehaviour>().RpcCreateUIButton(_numberOfString);
        }
    }

    // Setta la domanda a tutti i clients
    public void SetQuestionOnClient(string _question)
    {

        CleanPlayerList();

        foreach (var player in playerList)
        {
            player.GetComponent<PlayerBehaviour>().RpcSetQuestion(_question);
        }
        Debug.Log("Ho inviato la domanda a tutti i giocaotri");
    }

    // Mette il testo delle risposte dentro i pulsanti
    public void SetAnswerOnClient()
    {
        for (int i = 0; i < answerStringList.Count; i++)
        {
            CleanPlayerList();
            foreach (var player in playerList)
            {
                player.GetComponent<PlayerBehaviour>().DoRpcSetAnswer(answerStringList[i].answerChoose.ToString(), i);
            }
        }
        Debug.Log("Ho inviato le risposte in tutti i giocatori");
    }
    
    // Prendo la risposta scelta dai clients
    public void SetAnswerOnServer(string _nameSender, int _answerIndex)
    {
        feedbackText.text = "E' stata scelta la risposta " + _answerIndex + " da " + _nameSender;
        Debug.Log("E' stata scelta la risposta " + _answerIndex + " da " + _nameSender);

        
        SetupGameSession(_nameSender, _answerIndex);

    }

    // Aggiunge le domande e le risposte date da ogni Clients alla Game Session corrente
    public void SetupGameSession(string _nameSender, int _answerIndex)
    {
        string _answerStringTextToPass;
        //estrapolo il testo della risposta in base all'indice passato
        if (_answerIndex != -1)
        {
            _answerStringTextToPass = answerStringList[_answerIndex].answerChoose;
        } else
        {
            _answerStringTextToPass = "Astenuto";
        }

        ClientsClass newClient = new ClientsClass(_nameSender, _answerIndex, _answerStringTextToPass);
        Debug.Log(newClient);
        gameSession[currentQuestion].clientClassArch.Add(newClient);

        Debug.Log("ANSWER INDEX: " + _answerIndex + " e ANSWER STRING: " +_answerStringTextToPass);
        Debug.Log(clientsList.Count);
    }

    // Salva i dati nel file CSV
    public void SaveFileGameSessionData()
    {
        //Qui prende il path dove salvare il file
        string filePath = getPath();

        //Questo è il writer che scrive nel path che gl iabbiamo dato
        StreamWriter writer = new StreamWriter(filePath);

        //Qui descriviamo i titoli delle colonne cioè i nomi delle categorie che appariranno nel file
        writer.WriteLine("Domanda, Utente, Risposta");

        //Qui scorriamo le game session e per ognuna scorriamo la lista di client class estraendo i dati che ci servono
        for (int i = 0; i < gameSession.Count; i++)
        {
            for (int j = 0; j < gameSession[i].clientClassArch.Count; j++)
            {
                //Scrive nel file i dati
                writer.WriteLine(gameSession[i].questionStringText.ToString() +
                "," + gameSession[i].clientClassArch[j].nameClient.ToString() +
                "," + gameSession[i].clientClassArch[j].answerChoose.ToString());
            }
            
        }

        writer.Flush();

        //Chiudiamo il file
        writer.Close();

    }

    // Metodo che prende il path in base alla piattaforma
    private static string getPath()
    {
#if UNITY_EDITOR
        Directory.CreateDirectory(Application.persistentDataPath + "/LogData");
        return Application.persistentDataPath + "/LogData/" + "Log_Televoting.csv";
#elif UNITY_STANDALONE_WIN
        Directory.CreateDirectory(Application.persistentDataPath + "/LogData");
        return Application.persistentDataPath + "/LogData/" + "Log_Televoting.csv";
#elif UNITY_STANDALONE_MAC
        Directory.CreateDirectory(Application.persistentDataPath + "/LogData");
        return Application.persistentDataPath + "/LogData/" + "Log_Televoting.csv";
#elif UNITY_ANDROID
		return Application.persistentDataPath;// +fileName;
#elif UNITY_IPHONE
		return GetiPhoneDocumentsPath();// +"/"+fileName;
#else
		return Application.dataPath;// +"/"+ fileName;
#endif
    }

    // Metodo che prende il path sui device iOS (in caso ci serva)
    private static string GetiPhoneDocumentsPath()
    {
        string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
        path = path.Substring(0, path.LastIndexOf('/'));
        return path + "/Documents";
    }
}

