using UnityEngine;
using Unity.Netcode;
using System.Collections;


public class TestNB : NetworkBehaviour
{

    private void Start()
    {
        StartCoroutine(ExecuteAfterSeconds(60, "Called from Start"));
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {

        Debug.Log($"[OnNetworkSpawn] IsOwner={IsOwner} OwnerClientId={OwnerClientId} LocalClientId={NetworkManager.Singleton.LocalClientId}");


        if (IsClient)
        {
            Debug.Log("I am client");
        }

        if (IsOwner)
        {
            Debug.Log("I am Owner");
        }

        if (IsServer)
        {
            Debug.Log($"I am Server");
        }

        StartCoroutine(ExecuteAfterSeconds(45, "Called from OnNetworkSpawn"));
    }

    [ClientRpc]
    private void TestClientRPC()
    {
        Debug.Log("RPC CLIENT RECIEVED");
    }

    [ServerRpc]
    private void TestServerRPC()
    {
        Debug.Log("RPC SERVER RECIEVED"); 
    }


    private IEnumerator ExecuteAfterSeconds(float  seconds, string text)
    {
        yield return new WaitForSeconds(seconds);

        Debug.Log($"{text}");

        TestClientRPC();

        if (IsOwner)
            TestServerRPC();

    }

}
