using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeautyServer : Server
{
    [Header("UI References")]
    public Button startServerButton;
    public Button closeServerButton;
    public Text ServerLogger = null;

    //Set UI interactable properties
    protected virtual void Awake()
    {
        startServerButton.interactable = true;  //Enable button to let users start the server
        startServerButton.onClick.AddListener(StartServer);

        closeServerButton.interactable = false; //Disable button until the server is started
        closeServerButton.onClick.AddListener(CloseServer);
    }

    //Start server and wait for clients
    protected override void StartServer()
    {
        base.StartServer();
        //Set UI interactable properties
        startServerButton.interactable = false; //Disable button to avoid initilize more than one server
    }

    //Close connection with the client
    protected override void CloseClientConnection()
    {
        base.CloseClientConnection();
        //Set UI interactable properties
        closeServerButton.interactable = true;  //Enable button to let users close the server
    }

    //Close client connection and disables the server
    protected override void CloseServer()
    {
        base.CloseServer();
        startServerButton.interactable = true;
        closeServerButton.interactable = false;
    }

    //Custom Server Log
    #region ServerLog
    //With Text Color
    protected override void ServerLog(string msg)
    {
        base.ServerLog(msg);
        ServerLogger.text += '\n' + "- " + msg;
    }
    //Without Text Color
    protected override void ServerLog(string msg, Color color)
    {
        base.ServerLog(msg, color);
        ServerLogger.text += '\n' + "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">- " + msg + "</color>";
    }
    #endregion
}
