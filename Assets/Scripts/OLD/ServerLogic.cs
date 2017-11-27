using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ServerLogic : NetworkBehaviour
{
    public List<GameObject> listPlayer = new List<GameObject>();
    //per motivi di test riempiamo questa lista di index risposte con 6 valori. Poi faremo un metodo per prenderli da un xml.
    public List<int> listValue = new List<int>();

    public Canvas canvasServer;
    public Text serverValue;

    public GameObject playerBase;

    //private ClientLogic refCL;
    //private GameManager refGM;

    private void Start()
    {       
        StartCoroutine(CreateListCO());
        StartCoroutine(RenameListPlayerCO());
        StartCoroutine(CreateUIListPlayerCO());

        for (int i = 0; i < listPlayer.Count; i++)
        {
            //Debug.LogError(listPlayer[i].name + " - " + listPlayer[i].GetComponent<ClientLogic>().value);
        }       
    }

    // Crea la lista di player
    public IEnumerator CreateListCO()
    {
        yield return new WaitForSeconds(0.1f);

        listPlayer.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        Debug.LogError("Ho aggiunto i giocatori alla listPlayer");
    }

    // Cambia il nome ai player
    private IEnumerator RenameListPlayerCO()
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < listPlayer.Count; i++)
        {
            listPlayer[i].name = "Giocatore " + (i + 1);
            Debug.LogError(listPlayer[i].name);
        }

        Debug.LogError("Ho cambiato i nomi ai Player");
    }

    // Crea la lista UI
    private IEnumerator CreateUIListPlayerCO()
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < listPlayer.Count; i++)
        {
            GameObject newPlayerBase = Instantiate(playerBase);
            newPlayerBase.gameObject.transform.SetParent(canvasServer.transform.GetChild(1));
            newPlayerBase.GetComponentInChildren<Text>().text = listPlayer[i].name + " - ";
        }

        Debug.LogError("Ho creato la lista in UI dei Player");
        yield break;
    }


    //private void Update()
    //{
    //foreach (var player in refGM.listPlayer)
    //{
    //    serverValue.text = player.GetComponent<ClientLogic>().value.ToString();
    //}
    //serverValue.text = refCL.value.ToString();
    //}

    //private IEnumerator ClientLogicSearchCO()
    //{
    //    while (refCL == null)
    //    {
    //        refCL = FindObjectOfType<ClientLogic>();
    //        yield return null;
    //    }
    //    Debug.Log("Ho trovato Client Logic");
    //    yield break;
    //}


    //private IEnumerator GMSearchCO()
    //{
    //    while (refGM == null)
    //    {
    //        refGM = FindObjectOfType<GameManager>();
    //        yield return null;
    //    }
    //    Debug.Log("Ho trovato GM");
    //    yield break;
    //}

}
