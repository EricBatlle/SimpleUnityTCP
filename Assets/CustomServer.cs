using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This Server inheritated class acts like Server using UI elements like buttons and input fields.
/// </summary>
public class CustomServer : Server
{
    [Header("UI References")]
    [SerializeField] private Button m_StartServerButton = null;
    [SerializeField] private Button m_SendToClientButton = null;
    [SerializeField] private InputField m_SendToClientInputField = null;
    [SerializeField] private Button m_CloseServerButton = null;
    [SerializeField] private Text m_ServerLogger = null;

    //Set UI interactable properties
    protected virtual void Awake()
    {
        //Start Server
        m_StartServerButton.interactable = true;  //Enable button to let users start the server
        m_StartServerButton.onClick.AddListener(StartServer);

        //Send to Client
        m_SendToClientButton.interactable = false;
        m_SendToClientButton.onClick.AddListener(SendMessageToClient);

        //Close Server
        m_CloseServerButton.interactable = false; //Disable button until the server is started
        m_CloseServerButton.onClick.AddListener(CloseServer);

        //Populate Server delegates
        OnClientConnected = () =>
        {
            m_SendToClientButton.interactable = true;
        };
    }

    //Start server and wait for clients
    protected override void StartServer()
    {
        base.StartServer();
        //Set UI interactable properties
        m_StartServerButton.interactable = false; //Disable button to avoid initilize more than one server
    }

    //Get input field text and send it to client
    private void SendMessageToClient()
    {
        string newMsg = m_SendToClientInputField.text;
        base.SendMessageToClient(newMsg);
    }

    //Close connection with the client
    protected override void CloseClientConnection()
    {
        base.CloseClientConnection();
        //Set UI interactable properties
        m_CloseServerButton.interactable = true;  //Enable button to let users close the server
    }

    //Close client connection and disables the server
    protected override void CloseServer()
    {
        base.CloseServer();
        m_StartServerButton.interactable = true;
        m_CloseServerButton.interactable = false;
    }

    //Custom Server Log
    #region ServerLog
    //With Text Color
    protected override void ServerLog(string msg)
    {
        base.ServerLog(msg);
        m_ServerLogger.text += '\n' + "- " + msg;
    }
    //Without Text Color
    protected override void ServerLog(string msg, Color color)
    {
        base.ServerLog(msg, color);
        m_ServerLogger.text += '\n' + "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">- " + msg + "</color>";
    }
    #endregion
}