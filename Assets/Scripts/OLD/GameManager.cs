using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public List<GameObject> listPlayer = new List<GameObject>();

    public void Start()
    {
        StartCoroutine(ListPlayerCO());
        Debug.Log("Ho fatto partire la ListPlayerCO");
    }

    // Cambia il nome ai player
    private IEnumerator ListPlayerCO()
    {       
        yield return new WaitForSeconds(0.1f);

        listPlayer.AddRange(GameObject.FindGameObjectsWithTag("Player"));


        for (int i = 0; i < listPlayer.Count; i++)
        {
            listPlayer[i].name = "Giocatore " + (i + 1);
            Debug.Log(listPlayer[i].name);
        }

        Debug.Log("Ho aggiunto i giocatori all'arrayPlayer");
    }
}
