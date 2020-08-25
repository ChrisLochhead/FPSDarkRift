using System;
using DarkRift;
using DarkRift.Client;
using UnityEngine;
using UnityEngine.UI;
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
