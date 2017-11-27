using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.SceneManagement;

public class PlayerBehaviour : NetworkBehaviour
{
    public Text questionText, identifierText;
    public GameObject panelAnswer, answerButton, cancelButton;

    private ServerBehaviour refSB;
    private GameObject lastClicked;
    private GameObject inputField;

    public List<GameObject> listButton = new List<GameObject>();

    private int answerCounter;

    // Indice generale per capire quale risposta è stata scelta e quale inviare al server
    private int indexerAnswer = -1;

    [SyncVar]
    public string playerIdentifier;

    private void Start()
    {
        inputField = GameObject.FindGameObjectWithTag("NewName");
        StartCoroutine(SearchSBCO());
        StartCoroutine(DisableOtherClientCO());
        Debug.Log("Sono nello Start di PlayerBehaviour");
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

    // Disabilita gli spawn degli altri client sul Player locale
    public IEnumerator DisableOtherClientCO()
    {
        yield return new WaitForSeconds(1f);
        playerIdentifier = SystemInfo.deviceUniqueIdentifier;
        this.gameObject.name = playerIdentifier;
        ChangeClientName();
        List<GameObject> playerList = new List<GameObject>();
        playerList.AddRange(GameObject.FindGameObjectsWithTag("Player"));

        for (int i = 0; i < playerList.Count; i++)
        {
            if (!playerList[i].GetComponent<NetworkIdentity>().hasAuthority)
            {
                playerList[i].SetActive(false);
            }
        }
        identifierText.text = this.gameObject.name;
    }

    public void ChangeClientName()
    {
        if(inputField.GetComponent<Text>().text == "")
        {
            string clientNewName = inputField.GetComponent<Text>().text;
            if (clientNewName != "")
            {
                this.gameObject.name = clientNewName;
            }
        }
    }

    // Setta il testo alla domanda
    [ClientRpc]
    public void RpcSetQuestion(string _question)
    {
        questionText.text = _question;
        Debug.Log("Io Client mi sono settato la domanda");
    }

    // Crea il numero di bottoni che il Server gli passa
    [ClientRpc]
    public void RpcCreateUIButton(int _answerIndex)
    {
        for (int i = 0; i < _answerIndex; i++)
        {
            GameObject newPlayerBase = Instantiate(answerButton);
            newPlayerBase.gameObject.transform.SetParent(panelAnswer.transform);
            newPlayerBase.GetComponent<RectTransform>().localScale = Vector3.one;
            newPlayerBase.name = "Answer " + i;
            newPlayerBase.GetComponent<Button>().onClick.AddListener(DoSelectAnswer);
            listButton.Add(newPlayerBase);
            Debug.Log("Io Client mi sono creato il bottone " + newPlayerBase.name);
        }        
    }

    // Chiama RpcSetAnswer
    public void DoRpcSetAnswer(string _answerText, int _answerIndex)
    {
        RpcSetAnswer(_answerText, _answerIndex);
    }

    // Setta il testo dentro ogni bottone creato
    [ClientRpc]
    public void RpcSetAnswer(string _answerText, int _answerIndex)
    {
        answerCounter = _answerIndex + 1;
        listButton[_answerIndex].GetComponentInChildren<Text>().text = (/*answerCounter +". " +*/ _answerText);
        Debug.Log("Io Client mi sono settato la risposta del bottone " + listButton[_answerIndex]);
    }

    // Seleziona la risposta, la rende verde e poi rende tutti non interattivi
    public void DoSelectAnswer ()
    {
        lastClicked = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        lastClicked.GetComponent<Image>().color = Color.green;
        lastClicked.GetComponent<Button>().Select();
        
        for (int i = 0; i < listButton.Count; i++)
        {

            if (listButton[i].GetComponent<Button>() != lastClicked.GetComponent<Button>())
            {
                listButton[i].GetComponent<Button>().interactable = false;
                Debug.Log("Settati tutti gli altri bottoni non interagibili ");
            }

            else
            {
                indexerAnswer = i;
                lastClicked.GetComponent<Button>().interactable = false;
            }                       
        }
    }

    // Disattiva tutti i bottoni
    [ClientRpc]
    public void RpcDeactiveButton()
    {
        cancelButton.GetComponent<Button>().interactable = false;
        for (int i = 0; i < listButton.Count; i++)
        {
            listButton[i].GetComponent<Button>().interactable = false;
        }
    }
    
    // Chiama il CmdSelectAnswer
    [ClientRpc]
    public void RpcSelectedAnswer()
    {
        CmdSelectedAnswer(this.gameObject.name, indexerAnswer);
    }

    // Per inviare al server la risposta scelta inviando il nome del Giocatore e l'indice
    [Command]
    public void CmdSelectedAnswer(string _playerName, int _index)
    {
        refSB.SetAnswerOnServer(_playerName, _index); 
    }

    // Metodo per resettare Indexeranswer a -1 e mette i bottoni bianchi e li riattiva
    public void CancelButton()
    {
        for (int i = 0; i < listButton.Count; i++)
        {
            listButton[i].GetComponent<Button>().interactable = true;
            lastClicked.GetComponent<Image>().color = Color.white;
        }
        indexerAnswer = -1;
    }


    // Resetta lo stato del client
    [ClientRpc]
    public void RpcResetClient(bool noMoreQuestion)
    {
        Debug.Log("ResetClient");
        indexerAnswer = -1;
        for (int i = 0; i < listButton.Count; i++)
        {
            Destroy(listButton[i]);
        }
        listButton.Clear();

        //if non ci sono più domande allora metti come stringa "Grazie per aver partecipato!"
        if (noMoreQuestion)
        {
            Debug.Log("GRAZIE PER AVER PARTECIPATO: " + noMoreQuestion);
            questionText.text = "Fine del questionario. Grazie per aver partecipato!";
            //cancelButton.GetComponent<Button>().interactable = false;
            cancelButton.gameObject.SetActive(false);
            StartCoroutine(ChangeScene());
        } else
        {
            Debug.Log("RESETTATO LA DOMANDA: " + noMoreQuestion);
            questionText.text = "Attendi la domanda...";
            cancelButton.GetComponent<Button>().interactable = true;
        }
    }

    public IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(0);
    }
}
