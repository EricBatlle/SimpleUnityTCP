using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{    
    public string ipAddress = "127.0.0.1";
    public int port = 54010;
    [Space()]
    public Button getQuoteButton;
    public Text ClientLogger = null;

    private TcpClient client;
    private string receivedMessage;
    private byte[] buf = new byte[49152];

    private void ClientLog(string msg)
    {
        ClientLogger.text += '\n'+"- " + msg;
        Debug.Log("Client: "+msg);
    }

    private void Start()
    {
        getQuoteButton.interactable = false;
    }

    public void StartClient()
    {
        client = new TcpClient();
        try
        {
            client.Connect(ipAddress, port);
            ClientLog("Client Started");
            getQuoteButton.interactable = true;
        }
        catch (SocketException exception)
        {
            ClientLog("Socket Error: Start Server first");
        }        
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(receivedMessage))
        {
            ClientLog("ReceivedMessage on Client" + receivedMessage);
            receivedMessage = "";

            getQuoteButton.interactable = false;
        }
    }

    //Button event to read/send to server
    public void GetQuote() 
    {
        if (!client.Connected) return; //early Out

        getQuoteButton.interactable = false;
        receivedMessage = "";

        //Set up async Read
        var stream = client.GetStream();
        stream.BeginRead(buf, 0, buf.Length, MessageReceived, null);
        ClientLog("Setted async Read, begin send msg");        
        // send message
        byte[] msg = Encoding.ASCII.GetBytes("QUOTE");
        stream.Write(msg, 0, msg.Length);
        ClientLog("Sended msg QUOTE to SV");
    }

    void MessageReceived(IAsyncResult res)
    {
        if (res.IsCompleted && client.Connected)
        {
            var stream = client.GetStream();
            int bytesIn = stream.EndRead(res);

            receivedMessage = Encoding.ASCII.GetString(buf, 0, bytesIn);
            if (receivedMessage == "QUOTE")
            {
                client.Close();
                getQuoteButton.interactable = false;
            }
        }
    }

    void OnDestroy()
    {
        if (client.Connected)
        {
            client.Close();
        }
    }

    

    
}
