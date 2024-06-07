using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    ServerSession _session = new ServerSession();
    Connector _connector = new Connector();
    IPAddress _ipAddr;
    IPEndPoint _ipEndPoint;

    public void Init()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        for (int i = 0; ipHost.AddressList.Length > i; i++)
        {
            _ipAddr = ipHost.AddressList[i];
        }

        _ipEndPoint = new IPEndPoint(_ipAddr, 7777);

        _connector.Connect(_ipEndPoint,
            () => { return _session; });
    }

    void OnApplicationQuit()
    {
        _connector.Disconnect();
    }
}
