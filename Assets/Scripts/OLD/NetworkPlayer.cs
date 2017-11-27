using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkPlayer : NetworkBehaviour {

    public Text myNumber;

    int conteggio = 0;
    

    [SyncVar(hook = "OnHealthChanged")]
    public int m_health;

    public override void OnStartClient()
    {
        OnHealthChanged(m_health);
    }

    public override void OnStartLocalPlayer()
    {
        
        CmdSetHealth(0);
    }
    
    public void IIncreese () {

        if (isLocalPlayer)
            CmdSetHealth(m_health + 1);
      
	}

    [Command]
    void CmdSetHealth(int health)
    {
        m_health = health;
    }

    void OnHealthChanged(int newHealth)
    {
        m_health = newHealth;
        myNumber.text = m_health.ToString();
    }

    public void DoDoSfuff()
    {
        RpcDoStuff();
    }

    [ClientRpc]
    public void RpcDoStuff()
    {

        CmdSetHealth(m_health + 100);
    }
}
