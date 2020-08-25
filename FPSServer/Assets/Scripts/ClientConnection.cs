using UnityEngine;
using DarkRift;
using DarkRift.Server;

[System.Serializable]
public class ClientConnection
{

    [Header("Public Fields")]
    public string Name;
    public IClient Client;

    public ClientConnection(IClient client, LoginRequestData data)
    {
        Client = client;
        Name = data.Name;

        ServerManager.Instance.Players.Add(client.ID, this);
        ServerManager.Instance.PlayersByName.Add(Name, this);

        using (Message m = Message.Create((ushort)Tags.LoginRequestAccepted, new LoginInfoData(client.ID, new LobbyInfoData())))
        {
            client.SendMessage(m, SendMode.Reliable);
        }

    }
}
