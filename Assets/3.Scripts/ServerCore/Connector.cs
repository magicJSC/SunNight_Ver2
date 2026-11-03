using Protocol;
using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Connector
{
    Func<Session> _sessionFactory;
    readonly object _lifetimeLock = new object();
    Session _connectedSession;
    bool _stopping;

    public void Connect(IPEndPoint ipEndPoint, Func<Session> sessionFactory, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory = sessionFactory;

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectCompleted;
            args.RemoteEndPoint = ipEndPoint;
            args.UserToken = sock;

            RegisterConnect(args);
        }
    }

    void RegisterConnect(SocketAsyncEventArgs args)
    {
        Socket sock = args.UserToken as Socket;
        if (sock == null)
            return;

        bool pending = sock.ConnectAsync(args);
        if (pending == false)
            OnConnectCompleted(null, args);
    }

    void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
    {
        lock (_lifetimeLock)
        {
            if (_stopping)
            {
                (args.UserToken as Socket)?.Dispose();
                args.Dispose();
                return;
            }
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.OnConnected(args.RemoteEndPoint);
                session.Start(args.ConnectSocket);
                _connectedSession = session;

                REQUEST_ENTER_GAME packet = new();
                Managers.Network.Send(packet, PacketId.PktRequestEnter);
            }
            else
            {
                Debug.Log($"OnConnectCompleted Fail: {args.SocketError}");
                (args.UserToken as Socket)?.Dispose();
            }
            args.Dispose();
        }
    }

    public void Disconnect()
    {
        lock (_lifetimeLock)
        {
            _stopping = true;
            _connectedSession?.Disconnect();
            _connectedSession = null;
        }
    }
}
