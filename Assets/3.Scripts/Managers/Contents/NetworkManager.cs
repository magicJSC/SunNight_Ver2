using Google.Protobuf;
using Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    ServerSession _session = new ServerSession();
    Connector _connector = new Connector();
    IPAddress _ipAddr;
    IPEndPoint _ipEndPoint;

    static HttpClient client = new HttpClient();
    readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
    bool _stopping;

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

        _ = GetTask();
    }

    public async Task GetTask()
    {
        string id = "admin";
        string pw = "1234";
        using HttpContent content = new StringContent($"{id}&{pw}");
        try
        {
            using HttpResponseMessage response = await client.PostAsync("http://localhost:3333/login", content, _lifetime.Token);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            if (_stopping) return;


            if (responseBody != "fail")
            {
                _ipEndPoint = new IPEndPoint(_ipAddr, 6666);
                _connector.Connect(_ipEndPoint,
                    () => { return _session; });
                Debug.Log(responseBody);
            }
            else
                Debug.Log("fail to login");
        }
        catch (System.OperationCanceledException) when (_stopping) { }
        catch (HttpRequestException e)
        {
            Debug.Log($"Request Error : {e.Message}");
        }
    }

    public void Send(IMessage packet, PacketId id)
    {
        _session.Send(packet, id);
    }

    void OnApplicationQuit()
    {
        StopNetwork();
    }

    void OnDestroy()
    {
        StopNetwork();
        _lifetime.Dispose();
    }

    void StopNetwork()
    {
        if (_stopping) return;
        _stopping = true;
        _lifetime.Cancel();
        _connector.Disconnect();
    }
}
