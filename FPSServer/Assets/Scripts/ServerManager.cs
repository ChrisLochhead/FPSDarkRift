using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;

public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance;

    public Dictionary<ushort, ClientConnection> Players = new Dictionary<ushort, ClientConnection>();
    public Dictionary<string, ClientConnection> PlayersByName = new Dictionary<string, ClientConnection>();

    [Header("References")]
    public XmlUnityServer XmlServer;

    public DarkRiftServer Server;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        Server = XmlServer.Server;
        Server.ClientManager.ClientConnected += OnClientConnect;
        Server.ClientManager.ClientDisconnected += OnClientDisconnect;
    }

    void OnDestroy()
    {
        Server.ClientManager.ClientConnected -= OnClientConnect;
        Server.ClientManager.ClientDisconnected -= OnClientDisconnect;
    }

    private void OnClientConnect(object sender, ClientConnectedEventArgs e)
    {
        e.Client.MessageReceived += OnMessage;
    }

    private void OnMessage(object sender, MessageReceivedEventArgs e)
    {
        IClient client = (IClient)sender;
        using (Message m = e.GetMessage())
        {
            switch ((Tags)m.Tag)
            {
                case Tags.LoginRequest:
                    OnclientLogin(client, m.Deserialize<LoginRequestData>());
                    break;
            }
        }
    }

    private void OnClientDisconnect(object sender, ClientDisconnectedEventArgs e)
    {
        IClient client = e.Client;
        ClientConnection p;
        if (Players.TryGetValue(client.ID, out p))
        {
            p.OnClientDisconnect(sender, e);
        }
        e.Client.MessageReceived -= OnMessage;
    }

    private void OnclientLogin(IClient client, LoginRequestData data)
    {
        //check if player is already logged in (name already chosen in our case) and if not create a new object to represent a logged in client.

        if (PlayersByName.ContainsKey(data.Name))
        {
            using (Message m = Message.CreateEmpty((ushort)Tags.LoginRequestDenied))
            {
                client.SendMessage(m, SendMode.Reliable);
            }
            return;
        }

        //In the future the ClientConnection will handle its messages
        client.MessageReceived -= OnMessage;

        new ClientConnection(client, data);

    }
}
