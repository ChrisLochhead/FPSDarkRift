﻿using DarkRift;
using DarkRift.Client;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{

    public static LobbyManager Instance;

    [Header("References")]
    public Transform RoomListContainerTransform;

    [Header("Prefabs")]
    public GameObject RoomListPrefab;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GlobalManager.Instance.Client.MessageReceived += OnMessage;
        RefreshRooms(GlobalManager.Instance.LastRecievedLobbyInfoData);
    }


    void OnDestroy()
    {
        GlobalManager.Instance.Client.MessageReceived -= OnMessage;
    }


    private void OnMessage(object sender, MessageReceivedEventArgs e)
    {
        using (Message m = e.GetMessage())
        {
            switch ((Tags)m.Tag)
            {
                case Tags.LobbyJoinRoomDenied:
                    OnRoomJoinDenied(m.Deserialize<LobbyInfoData>());
                    break;
                case Tags.LobbyJoinRoomAccepted:
                    OnRoomJoinAcepted();
                    break;
            }
        }
    }



    public void RefreshRooms(LobbyInfoData data)
    {
        RoomListObject[] roomObjects = RoomListContainerTransform.GetComponentsInChildren<RoomListObject>();
        for (int i = 0; i < data.Rooms.Length; i++)
        {
            RoomData d = data.Rooms[i];
            if (i < roomObjects.Length)
            {
                roomObjects[i].Set(d);
            }
            else
            {
                GameObject go = Instantiate(RoomListPrefab, RoomListContainerTransform);
                go.GetComponent<RoomListObject>().Set(d);
            }
        }
    }

    public void SendJoinRoomRequest(string roomName)
    {
        using (Message m = Message.Create((ushort)Tags.LobbyJoinRoomRequest, new JoinRoomRequest(roomName)))
        {
            GlobalManager.Instance.Client.SendMessage(m, SendMode.Reliable);
        }
    }

    public void OnRoomJoinDenied(LobbyInfoData data)
    {
        RefreshRooms(data);
    }

    public void OnRoomJoinAcepted()
    {
        SceneManager.LoadScene("Game");
    }

}