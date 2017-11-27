using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class MyLobbyManager : NetworkLobbyManager
{
    public GameObject playButton;
    public bool allReady;

    //public GameObject[] arrayPlayer;

    private void Awake()
    {
        networkAddress = Network.player.ipAddress;
        Debug.Log(networkAddress);
    }

    public override void OnLobbyServerPlayersReady()
    {      
        Debug.Log("Tutti i giocatori sono pronti a partire");
        CheckPlayPlayer();
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        base.OnServerAddPlayer(conn, playerControllerId);

    }

    public void PlayButton()
    {
        ServerChangeScene("ClientDavid");
        Debug.Log("Il server ha premuto PLAY e cambiato scena");
    }

    private void CheckPlayPlayer()
    {
        for (int i = 0; i < lobbySlots.Length; i++)
        {
            if (!lobbySlots[i]) continue;
            if (!lobbySlots[i].readyToBegin)
            {
                allReady = false;
                break;
            }
            else
            {
                allReady = true;
                break;
            }
        }

        if (allReady)
        {
            playButton.gameObject.SetActive(true);
        }
    }

    //public override void OnLobbyServerSceneChanged(string sceneName)
    //{
    //    arrayPlayer = GameObject.FindGameObjectsWithTag("Player");
    //    Debug.Log("Ho aggiunto i giocatori all'arrayPlayer");

    //    for (int i = 0; i < arrayPlayer.Length; i++)
    //    {
    //        arrayPlayer[i].name = "Giocatore " + (i +1);
    //        Debug.Log(arrayPlayer[i].name);
    //    }
    //}
}
