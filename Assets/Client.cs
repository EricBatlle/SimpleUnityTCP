using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    #region Public Variables
    [Header("Network")]
    public string ipAddress = "127.0.0.1";
    public int port = 54010;
    [Header("UI References")]
    public Button sendCloseButton;
    public Text ClientLogger = null;
    #endregion

    #region Network m_Variables
    private TcpClient m_client;
    private NetworkStream m_netStream = null;
    private byte[] m_buffer = new byte[49152];
    private int m_bytesReceived = 0;
    private string m_receivedMessage = "";
    #endregion

    //Set UI interactable properties
    private void Start()
    {
        sendCloseButton.interactable = false;
    }
    
    //Start client and stablish connection with server
    public void StartClient()
    {
        //Early out
        if(m_client != null)
        {
            ClientLog("There is already a runing client", Color.red);
            return;
        }
        
        try
        {
            //Create new client
            m_client = new TcpClient();
            //Set and enable client
            m_client.Connect(ipAddress, port);
            ClientLog("Client Started", Color.green);
            sendCloseButton.interactable = true;
        }
        catch (SocketException)
        {
            ClientLog("Socket Error: Start Server first", Color.red);
            CloseClient();
        }        
    }

    //Check if the client has been recived something
    private void Update()
    {
        //If there is something received
        if (!string.IsNullOrEmpty(m_receivedMessage))
        {
            ClientLog("Msg recived on Client: " + "<b>"+m_receivedMessage+"</b>", Color.green);
            m_receivedMessage = "";
            //Set UI interactable properties
            sendCloseButton.interactable = false;
            
            //Close message has to be there, as UI calls can't be called on no-main threads
            ClientLog("Close Connection with Server", Color.red);
        }
    }

    //Send "Close" message to the server, and waits the "Close" message response from server
    public void SendCloseToServer() 
    {
        if (!m_client.Connected) return; //early out if there is nothing connected
        
        //Set UI interactable properties        
        sendCloseButton.interactable = false;

        //Stablish Client NetworkStream information
        m_netStream = m_client.GetStream();
        //Start Async Reading
        m_netStream.BeginRead(m_buffer, 0, m_buffer.Length, MessageReceived, null);

        //Build message to server
        string sendMsg = "Close";
        byte[] msg = Encoding.ASCII.GetBytes(sendMsg);
        //Start Sync Writing
        m_netStream.Write(msg, 0, msg.Length);
        ClientLog("Msg sended to Server: "+"<b>Close</b>", Color.blue);
    }

    //Callback called when "BeginRead" is ended
    private void MessageReceived(IAsyncResult result)
    {
        if (result.IsCompleted && m_client.Connected)
        {
            //build message received from server
            m_bytesReceived = m_netStream.EndRead(result);
            m_receivedMessage = Encoding.ASCII.GetString(m_buffer, 0, m_bytesReceived);
            
            //If message recived from server is "Close", close that client
            if (m_receivedMessage == "Close")
            {
                CloseClient();
            }
        }
    }

    //Close client connection
    private void CloseClient()
    {
        if (m_client.Connected)
        {
            //Reset everything to defaults
            m_client.Close();
            m_client = null;
            //Set UI interactable properties        
            sendCloseButton.interactable = false;
        }
    }

    //Custom Server Log
    #region ClientLog
    private void ClientLog(string msg, Color color)
    {
        ClientLogger.text += '\n' + "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">- " + msg + "</color>";
        Debug.Log("Client: " + msg);
    }
    private void ClientLog(string msg)
    {
        ClientLogger.text += '\n' + "- " + msg;
        Debug.Log("Client: " + msg);
    }
    #endregion

}
