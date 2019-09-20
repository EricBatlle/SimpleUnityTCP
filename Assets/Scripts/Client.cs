using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Client : MonoBehaviour
{
    #region Public Variables
    [Header("Network")]
    public string ipAddress = "127.0.0.1";
    public int port = 54010;
    public float waitingMessagesFrequency = 2;
    #endregion

    #region Network m_Variables
    private TcpClient m_client;
    private NetworkStream m_netStream = null;
    private byte[] m_buffer = new byte[49152];
    private int m_bytesReceived = 0;
    private string m_receivedMessage = "";
    private IEnumerator m_ListenServerMsgsCoroutine = null;
    #endregion

    protected Action OnClientStarted = null;    //Delegate triggered when client start
    protected Action OnClientClosed = null;    //Delegate triggered when client close

    //Start client and stablish connection with server
    protected void StartClient()
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
            OnClientStarted?.Invoke();

            //Start Listening Messages coroutine
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
    private IEnumerator ListenServerMessages()
    {
        //early out if there is nothing connected       
        if (!m_client.Connected)        
            yield break;                

        //Stablish Client NetworkStream information
        m_netStream = m_client.GetStream();

        //Start Async Reading from Server and manage the response on MessageReceived function
        do
        {
            ClientLog("Client is listening server msg...", Color.yellow);
            //Start Async Reading from Server and manage the response on MessageReceived function
            m_netStream.BeginRead(m_buffer, 0, m_buffer.Length, MessageReceived, null);

            yield return new WaitForSeconds(waitingMessagesFrequency);

        }while(m_bytesReceived >= 0 && m_netStream != null);        

    }

    //What to do with the received message on client
    protected virtual void OnMessageReceived(string receivedMessage)
    {
        ClientLog("Msg recived on Client: " + "<b>" + receivedMessage + "</b>", Color.green);
        switch (m_receivedMessage)
        {
            case "Close":
                CloseClient();
                break;
            default:
                ClientLog("Received message " + receivedMessage + ", has no special behaviuor", Color.red);
                break;
        }
        this.m_receivedMessage = null;
    }

    //Send custom string msg to server
    protected void SendMessageToServer(string sendMsg)
    {
        if (!m_client.Connected) return; //early out if there is nothing connected       

        //Build message to server
        byte[] msg = Encoding.ASCII.GetBytes(sendMsg);
        //Start Sync Writing
        m_netStream.Write(msg, 0, msg.Length);
        ClientLog("Msg sended to Server: " + "<b>"+sendMsg+"</b>", Color.blue);
    }

    //AsyncCallback called when "BeginRead" is ended, waiting the message response from server
    private void MessageReceived(IAsyncResult result)
    {
        if (result.IsCompleted && m_client.Connected)
        {
            //build message received from server
            m_bytesReceived = m_netStream.EndRead(result);
            m_receivedMessage = Encoding.ASCII.GetString(m_buffer, 0, m_bytesReceived);

            OnMessageReceived(m_receivedMessage);         
        }
    }
    #endregion

    #region Close Client
    //Close client connection
    private void CloseClient()
    {
        ClientLog("Client Closed", Color.red);

        //Reset everything to defaults        
        if (m_client.Connected)        
            m_client.Close();

        if(m_client != null)
            m_client = null;

        OnClientClosed?.Invoke();
    }
    #endregion

    //Custom Server Log
    #region ClientLog
    protected virtual void ClientLog(string msg, Color color)
    {
        Debug.Log("Client: " + msg);
    }
    protected virtual void ClientLog(string msg)
    {
        Debug.Log("Client: " + msg);
    }
    #endregion

}
