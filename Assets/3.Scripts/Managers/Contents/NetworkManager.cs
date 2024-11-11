using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
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

    public async Task GetTask()
    {
        string id = "admin";
        string pw = "1234";
        HttpContent content = new StringContent($"{id}&{pw}");
        using HttpResponseMessage response = await client.PostAsync("http://localhost:3333/login", content);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        
        Debug.Log(responseBody);

        if (responseBody == "success")
        {
            _ipEndPoint = new IPEndPoint(_ipAddr, 6666);
            _connector.Connect(_ipEndPoint,
                () => { return _session; });
        }
        else
            Debug.Log("fail to login");
    }
    
    void OnApplicationQuit()
    {
        _connector.Disconnect();
    }
}
