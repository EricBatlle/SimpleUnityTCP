using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeautyClient : Client
{
    [Header("UI References")]
    public Button sendCloseButton;
    public Button startClientButton;
    public Text ClientLogger = null;

    //Set UI interactable properties
    private void Awake()
    {
        sendCloseButton.interactable = false;
        sendCloseButton.onClick.AddListener(SendCloseToServer);
        startClientButton.onClick.AddListener(StartClient);

        //Populate Client delegates
        OnClientStarted = ()=> 
        {
            //Set UI interactable properties        
            sendCloseButton.interactable = true;
            startClientButton.interactable = false;
        };

        OnClientClosed = ()=> 
        {
            //Set UI interactable properties        
            startClientButton.interactable = true;
            sendCloseButton.interactable = false;
        };
    }

    //Check if the client has been recived something
    private void Update()
    {
        //If there is something received
        if (!string.IsNullOrEmpty(m_receivedMessage))
        {
            ClientLog("Msg recived on Client: " + "<b>" + m_receivedMessage + "</b>", Color.green);
            m_receivedMessage = "";
            //Set UI interactable properties
            sendCloseButton.interactable = false;

            //Close message has to be there, as UI calls can't be called on no-main threads
            ClientLog("Close Connection with Server", Color.red);
        }
    }

    private void SendCloseToServer()
    {
        base.SendMessageToServer("Close");
        //Set UI interactable properties        
        sendCloseButton.interactable = false;
    }    

    //Custom Server Log
    #region ClientLog
    protected override void ClientLog(string msg, Color color)
    {
        base.ClientLog(msg, color);
        ClientLogger.text += '\n' + "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">- " + msg + "</color>";
    }
    protected override void ClientLog(string msg)
    {
        base.ClientLog(msg);
        ClientLogger.text += '\n' + "- " + msg;
    }
    #endregion
}
