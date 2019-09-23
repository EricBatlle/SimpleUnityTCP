using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/// <summary>
/// Client class shows how to implement and use TcpClient in Unity.
/// </summary>
public class Client : MonoBehaviour
{
    #region Public Variables
    [Header("Network")]
    public string ipAddress = "127.0.0.1";
    public int port = 54010;
    public float waitingMessagesFrequency = 2;
    #endregion

    #region Private m_Variables
    private TcpClient m_Client;
    private NetworkStream m_NetStream = null;
    private byte[] m_Buffer = new byte[49152];
    private int m_BytesReceived = 0;
    private string m_ReceivedMessage = "";
    private IEnumerator m_ListenServerMsgsCoroutine = null;
    #endregion

    #region Delegate Variables
    protected Action OnClientStarted = null;    //Delegate triggered when client start
    protected Action OnClientClosed = null;    //Delegate triggered when client close
    #endregion

    //Start client and stablish connection with server
    protected void StartClient()
    {
        //Early out
        if(m_Client != null)
        {
            ClientLog("There is already a runing client", Color.red);
            return;
        }
        
        try
        {
            //Create new client
            m_Client = new TcpClient();
            //Set and enable client
            m_Client.Connect(ipAddress, port);
            ClientLog("Client Started", Color.green);
            OnClientStarted?.Invoke();

            //Start Listening Server Messages coroutine
            m_ListenServerMsgsCoroutine = ListenServerMessages();
            StartCoroutine(m_ListenServerMsgsCoroutine);
        }
        catch (SocketException)
        {
            ClientLog("Socket Exception: Start Server first", Color.red);
            CloseClient();
        }        
    }

    #region Communication Client<->Server
    //Coroutine waiting server messages
    private IEnumerator ListenServerMessages()
    {
        //early out if there is nothing connected       
        if (!m_Client.Connected)        
            yield break;                

        //Stablish Client NetworkStream information
        m_NetStream = m_Client.GetStream();

        //Start Async Reading from Server and manage the response on MessageReceived function
        do
        {
            ClientLog("Client is listening server msg...", Color.yellow);
            //Start Async Reading from Server and manage the response on MessageReceived function
            m_NetStream.BeginRead(m_Buffer, 0, m_Buffer.Length, MessageReceived, null);

            if(m_BytesReceived > 0)
            {
                OnMessageReceived(m_ReceivedMessage);
                m_BytesReceived = 0;
            }

            yield return new WaitForSeconds(waitingMessagesFrequency);

        }while(m_BytesReceived >= 0 && m_NetStream != null);
        //The communication is over
        //CloseClient();
    }

    //What to do with the received message on client
    protected virtual void OnMessageReceived(string receivedMessage)
    {
        ClientLog("Msg recived on Client: " + "<b>" + receivedMessage + "</b>", Color.green);
        switch (m_ReceivedMessage)
        {
            case "Close":
                CloseClient();
                break;
            default:
                ClientLog("Received message " + receivedMessage + ", has no special behaviuor", Color.red);
                break;
        }
    }

    //Send custom string msg to server
    protected void SendMessageToServer(string sendMsg)
    {
        //early out if there is nothing connected       
        if (!m_Client.Connected)
        {
            ClientLog("Socket Error: Stablish Server connection first", Color.red);
            return;
        }

        //Build message to server
        byte[] msg = Encoding.ASCII.GetBytes(sendMsg); //Encode message as bytes
        //Start Sync Writing
        m_NetStream.Write(msg, 0, msg.Length);
        ClientLog("Msg sended to Server: " + "<b>"+sendMsg+"</b>", Color.blue);
    }

    //AsyncCallback called when "BeginRead" is ended, waiting the message response from server
    private void MessageReceived(IAsyncResult result)
    {
        if (result.IsCompleted && m_Client.Connected)
        {
            //build message received from server
            m_BytesReceived = m_NetStream.EndRead(result);
            m_ReceivedMessage = Encoding.ASCII.GetString(m_Buffer, 0, m_BytesReceived);
        }
    }
    #endregion

    #region Close Client
    //Close client connection
    private void CloseClient()
    {
        ClientLog("Client Closed", Color.red);

        //Reset everything to defaults        
        if (m_Client.Connected)        
            m_Client.Close();

        if(m_Client != null)
            m_Client = null;

        OnClientClosed?.Invoke();
    }
    #endregion

    #region ClientLog
    //Custom Client Log - With Text Color
    protected virtual void ClientLog(string msg, Color color)
    {
        Debug.Log("<b>Client:</b> " + msg);
    }
    //Custom Client Log - Without Text Color
    protected virtual void ClientLog(string msg)
    {
        Debug.Log("<b>Client:</b> " + msg);
    }
    #endregion

}