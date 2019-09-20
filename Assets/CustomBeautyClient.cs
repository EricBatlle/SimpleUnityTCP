using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomBeautyClient : BeautyClient
{
    [SerializeField] private Button sendToServerButton = null;
    [SerializeField] private InputField sendToServerInputField = null;

    protected override void Awake()
    {
        base.Awake();
        sendToServerButton.interactable = false;
        sendToServerButton.onClick.AddListener(SendMessageToServer);

        OnClientStarted += () => { sendToServerButton.interactable = true; };
        OnClientClosed += () => { sendToServerButton.interactable = false; };
    }

    private void SendMessageToServer()
    {
        string newMsg = sendToServerInputField.text;
        SendMessageToServer(newMsg);
    }
}
