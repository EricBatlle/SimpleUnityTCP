using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomBeautyServer : BeautyServer
{
    [SerializeField] private Button sendToClientButton = null;
    [SerializeField] private InputField sendToClientInputField = null;

    protected override void Awake()
    {
        base.Awake();
        sendToClientButton.onClick.AddListener(SendMessageToClient);
    }

    private void SendMessageToClient()
    {
        string newMsg = sendToClientInputField.text;
        SendMessageToClient(newMsg);
    }
}
