using System;
using System.Net;
using DarkRift;
using DarkRift.Client.Unity;
using UnityEngine;

[RequireComponent(typeof(UnityClient))]
public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance;

    [Header("Variables")]
    public string IpAdress;
    public int Port;

    [Header("References")]
    public UnityClient Client;

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
        Client.ConnectInBackground(IPAddress.Parse(IpAdress), Port, true, ConnectCallback);

    }

    private void ConnectCallback(Exception exception)
    {
        //Deprecated
        //if (Client.Connected)
        //{
            if (Client.ConnectionState == ConnectionState.Connected)
        {
            LoginManager.Instance.StartLoginProcess();
        }
        else
        {
            Start();
        }
    }

}