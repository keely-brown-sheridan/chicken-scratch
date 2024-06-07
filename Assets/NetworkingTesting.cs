using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkingTesting : MonoBehaviour
{
    public void HostTestLobby()
    {
        CSNetworkManager.singleton.StartHost();
        gameObject.SetActive(false);
    }

    public void JoinTestLobby()
    {
        CSNetworkManager.singleton.StartClient();
        gameObject.SetActive(false);
    }
}
