using System;
using DarkRift;
using DarkRift.Client;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using TMPro;
public class LoginManager : MonoBehaviour
{

    public static LoginManager Instance;

    [Header("References")]
    public GameObject LoginWindow;
    public TMP_InputField NameInput;
    public Button SubmitLoginButton;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoginWindow.SetActive(false);
        SubmitLoginButton.onClick.AddListener(OnSubmitLogin);

        GlobalManager.Instance.Client.MessageReceived += OnMessage;
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
                case Tags.LoginRequestDenied:
                    OnLoginDecline();
                    break;
                case Tags.LoginRequestAccepted:
                    OnLoginAccept(m.Deserialize<LoginInfoData>());
                    break;
            }
        }
    }

    public void OnLoginDecline()
    {
        LoginWindow.SetActive(true);
    }

    public void OnLoginAccept(LoginInfoData data)
    {
        GlobalManager.Instance.PlayerId = data.Id;
        GlobalManager.Instance.LastRecievedLobbyInfoData = data.Data;
        SceneManager.LoadScene("Lobby");
    }

    public void StartLoginProcess()
    {
        LoginWindow.SetActive(true);
    }

    public void OnSubmitLogin()
    {
        if (NameInput.text != "")
        {
            LoginWindow.SetActive(false);

            using (Message m = Message.Create((ushort)Tags.LoginRequest, new LoginRequestData(NameInput.text)))
            {
                GlobalManager.Instance.Client.SendMessage(m, SendMode.Reliable);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
