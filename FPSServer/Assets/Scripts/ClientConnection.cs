﻿using UnityEngine;
using DarkRift;
using DarkRift.Server;

[System.Serializable]
public class ClientConnection
{

    [Header("Public Fields")]
    public string Name;
    public IClient Client;
    public ServerPlayer Player;
    public Room Room;

    public ClientConnection(IClient client, LoginRequestData data)
    {
        Client = client;
        Name = data.Name;

        ServerManager.Instance.Players.Add(client.ID, this);
        ServerManager.Instance.PlayersByName.Add(Name, this);

        using (Message m = Message.Create((ushort)Tags.LoginRequestAccepted, new LoginInfoData(client.ID, new LobbyInfoData(RoomManager.Instance.GetRoomDataList()))))
        {
            client.SendMessage(m, SendMode.Reliable);
        }

        Client.MessageReceived += OnMessage;

    }

    public void OnClientDisconnect(object sender, ClientDisconnectedEventArgs e)
    {
        if (Room != null)
        {
            Room.RemovePlayerFromRoom(this);
        }

        ServerManager.Instance.Players.Remove(Client.ID);
        ServerManager.Instance.PlayersByName.Remove(Name);
        e.Client.MessageReceived -= OnMessage;
    }

    private void OnMessage(object sender, MessageReceivedEventArgs e)
    {
        IClient client = (IClient)sender;
        using (Message m = e.GetMessage())
        {
            switch ((Tags)m.Tag)
            {
                case Tags.LobbyJoinRoomRequest:
                    RoomManager.Instance.TryJoinRoom(client, m.Deserialize<JoinRoomRequest>());
                    break;
                case Tags.GameJoinRequest:
                    Room.JoinPlayerToGame(this);
                    break;
                case Tags.GamePlayerInput:
                    Player.RecieveInput(m.Deserialize<PlayerInputData>());
                    break;
                case Tags.SpawnDataInfo:
                    Player.SetSpawnPosition(m.Deserialize<SpawnStateInfo>());
                    break;
            }
        }
    }
}
