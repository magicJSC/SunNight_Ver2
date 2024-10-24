using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    ServerSession _session = new ServerSession();
    Connector _connector = new Connector();
    IPAddress _ipAddr;
    IPEndPoint _ipEndPoint;

    static HttpClient client = new HttpClient();

    public void Init(int port)
    {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        for (int i = 0; ipHost.AddressList.Length > i; i++)
        {
            if (ipHost.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
            {
                _ipAddr = ipHost.AddressList[i];
                break;
            }
        }

        //_ipEndPoint = new IPEndPoint(_ipAddr, port);

        //_connector.Connect(_ipEndPoint,
        //    () => { return _session; });

        GetTask();
    }

    static async Task GetTask()
    {
        using HttpResponseMessage response = await client.GetAsync("http://localhost:3333/login");
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
      
        Debug.Log(responseBody);
    }

    void OnApplicationQuit()
    {
        _connector.Disconnect();
    }
}
