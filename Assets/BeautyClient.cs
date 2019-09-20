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
    protected virtual void Awake()
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
