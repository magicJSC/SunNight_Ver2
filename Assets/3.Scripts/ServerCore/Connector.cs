using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Connector
{
    Func<Session> _sessionFactory;

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
        if (args.SocketError == SocketError.Success)
        {
            Session session = _sessionFactory.Invoke();
            session.OnConnected(args.RemoteEndPoint);
            session.Start(args.ConnectSocket);
        }
        else
        {
            Debug.Log($"OnConnectCompleted Fail: {args.SocketError}");
        }
    }

    public void Disconnect()
    {
        Session session = _sessionFactory.Invoke();
        session.Disconnect();
    }
}
