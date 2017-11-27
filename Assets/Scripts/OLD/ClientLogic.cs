using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ClientLogic : NetworkBehaviour
{
    [SyncVar]
    public int value;

    public Text valueTextClient;

    private void Start()
    {

        if (!isServer)
        {
            StartCoroutine(RandomValueCO());
        }


    }

    private void Update()
    {
        valueTextClient.text = value.ToString();
    }

    public IEnumerator RandomValueCO()
    {
        yield return new WaitForSeconds(0.6f);
        if (!isServer)
        {
            value = Random.Range(1, 10);
            Debug.LogError("Ho randomizzato value su " + this.gameObject.name);
        }
    }

    [Command]
    public void CmdIncrementer()
    {
        value++;
        Debug.LogError("In Server il valore è: " + value);
    }

    public void DoCmdIncrementer()
    {
        if (!isServer)
        {
            CmdIncrementer();
            Debug.LogError("In Client il valore è: " + value);
        }

    }
}
