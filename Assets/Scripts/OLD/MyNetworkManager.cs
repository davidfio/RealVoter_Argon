using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MyNetworkManager : NetworkManager
{
    private ServerBehaviour refSB;

    public override void OnStartServer()    
    {
        StartCoroutine(SearchSBCO());     
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        //base.OnServerReady(conn);
        StartCoroutine(refSB.AddPlayerCO());
        //StartCoroutine(refSB.SetupCO());
    }

    private IEnumerator SearchSBCO()
    {
        while (refSB == null)
        {
            Debug.LogError("Non ho trovato refSB");
            refSB = FindObjectOfType<ServerBehaviour>();
            yield return null;
        }
        Debug.LogError("Ho trovato refSB");
        yield break;
    }


}
