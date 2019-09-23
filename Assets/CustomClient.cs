using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This Client inheritated class acts like Client using UI elements like buttons and input fields.
/// </summary>
public class CustomClient : Client
{
    [Header("UI References")]
    [SerializeField] private Button m_StartClientButton = null;
    [SerializeField] private Button m_SendToServerButton = null;
    [SerializeField] private InputField m_SendToServerInputField = null;
    [SerializeField] private Button m_SendCloseButton = null;
    [SerializeField] private Text m_ClientLogger = null;

    //Set UI interactable properties
    private void Awake()
    {
        //Start Client
        m_StartClientButton.onClick.AddListener(base.StartClient);

        //Send to Server
        m_SendToServerButton.interactable = false;
        m_SendToServerButton.onClick.AddListener(SendMessageToServer);

        //SendClose
        m_SendCloseButton.interactable = false;
        m_SendCloseButton.onClick.AddListener(SendCloseToServer);

        //Populate Client delegates
        OnClientStarted = () =>
        {
            //Set UI interactable properties        
            m_SendCloseButton.interactable = true;
            m_SendToServerButton.interactable = true;
            m_StartClientButton.interactable = false;
        };

        OnClientClosed = () =>
        {
            //Set UI interactable properties        
            m_StartClientButton.interactable = true;
            m_SendToServerButton.interactable = false;
            m_SendCloseButton.interactable = false;
        };
    }

    private void SendMessageToServer()
    {
        string newMsg = m_SendToServerInputField.text;
        base.SendMessageToServer(newMsg);
    }

    private void SendCloseToServer()
    {
        base.SendMessageToServer("Close");
        //Set UI interactable properties        
        m_SendCloseButton.interactable = false;
    }

    //Custom Client Log
    #region ClientLog
    protected override void ClientLog(string msg, Color color)
    {
        base.ClientLog(msg, color);
        m_ClientLogger.text += '\n' + "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">- " + msg + "</color>";
    }
    protected override void ClientLog(string msg)
    {
        base.ClientLog(msg);
        m_ClientLogger.text += '\n' + "- " + msg;
    }
    #endregion
}