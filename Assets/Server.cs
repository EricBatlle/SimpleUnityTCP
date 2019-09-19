using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Server : MonoBehaviour
{
    #region Public Variables
    [Header("Network")]
    public string ipAdress = "127.0.0.1";
    public int port = 54010;
    public float waitingMessagesFrequency = 5;
    #endregion

    #region  Network m_Variables
    private TcpListener m_server = null;
    private TcpClient m_client = null;
    private NetworkStream m_netStream = null;
    private byte[] m_buffer = new byte[49152];
    private int m_bytesReceived = 0;
    protected string m_receivedMessage = "";
    private IEnumerator m_ClientComCoroutine = null;
    #endregion

    protected Action OnServerStarted = null;    //Delegate triggered when server start
    protected Action OnServerClosed = null;    //Delegate triggered when server close


    //Start server and wait for clients
    protected virtual void StartServer()
    {        
        //Set and enable Server 
        IPAddress ip = IPAddress.Parse(ipAdress);
        m_server = new TcpListener(ip, port);
        m_server.Start();
        ServerLog("Server Started", Color.green);
        //Wait for async client connection 
        m_server.BeginAcceptTcpClient(ClientConnected, null);
        OnServerStarted?.Invoke();
    }

    //Check if any client trys to connect
    private void Update()
    {   
        //If some client stablish connection
        if (m_client != null && m_ClientComCoroutine == null)
        {
            //Start the ClientCommunication coroutine
            m_ClientComCoroutine = ClientCommunication();
            StartCoroutine(m_ClientComCoroutine);
        }
    }

    //Callback called when "BeginAcceptTcpClient" detects new client connection
    private void ClientConnected(IAsyncResult res)
    {
        //set the client reference
        m_client = m_server.EndAcceptTcpClient(res);
    }

    #region Communication Server/Client
    //Coroutine that manage client communication while client is connected to the server
    private IEnumerator ClientCommunication()
    {        
        //Restart values in case there are more than one client connections
        m_bytesReceived = 0;
        m_buffer = new byte[49152];

        //Stablish Client NetworkStream information
        m_netStream = m_client.GetStream();

        //While there is a connection with the client, await for messages
        do
        {
            ServerLog("Server is listening client msg...", Color.yellow);
            //Start Async Reading from Client and manage the response on MessageReceived function
            m_netStream.BeginRead(m_buffer, 0, m_buffer.Length, MessageReceived,  m_netStream);

            //If there is any msg, do something
            if (m_bytesReceived > 0)            
                OnMessageReceived(m_receivedMessage);            

            yield return new WaitForSeconds(waitingMessagesFrequency);

        } while (m_bytesReceived >= 0 && m_netStream != null);   
        //The communication is over
        CloseClientConnection();
    }

    //What to do with the received message on server
    protected virtual void OnMessageReceived(string receivedMessage)
    {
        ServerLog("Msg recived on Server: " + "<b>" + receivedMessage + "</b>", Color.green);
        switch (receivedMessage)
        {
            case "Close":
                //In this case we send "Close" to shut down client
                SendMessageToClient("Close");
                //Close client connection
                CloseClientConnection();
                break;
            default:
                ServerLog("Received message :" + receivedMessage + ", has no special behaviuor", Color.red);
                break;
        }
    }
    protected void SendMessageToClient(string sendMsg)
    {
        //Build message to client        
        byte[] msgOut = Encoding.ASCII.GetBytes(sendMsg); //Encode message as bytes
        //Start Sync Writing
        m_netStream.Write(msgOut, 0, msgOut.Length);
        ServerLog("Msg sended to Client: " + "<b>" + sendMsg + "</b>", Color.blue);
    }

    //Callback called when "BeginRead" is ended
    private void MessageReceived(IAsyncResult result)
    {
        if (result.IsCompleted && m_client.Connected)
        {
            //build message received from client
            m_bytesReceived = m_netStream.EndRead(result);                              //End async reading
            m_receivedMessage = Encoding.ASCII.GetString(m_buffer, 0, m_bytesReceived); //De-encode message as string
        }
    }
    #endregion    

    #region Close Server/ClientConnection
    //Close client connection and disables the server
    protected virtual void CloseServer()
    {
        ServerLog("Server Closed", Color.red);
        //Close client connection
        if (m_client != null)
        {
            m_netStream.Close();
            m_netStream = null;
            m_client.Close();
            m_client = null;
        }
        //Close server connection
        if (m_server != null)
        {
            m_server.Stop();
            m_server = null;
        }

        OnServerClosed?.Invoke();
    }

    //Close connection with the client
    protected virtual void CloseClientConnection()
    {
        ServerLog("Close Connection with Client", Color.red);
        //Reset everything to defaults
        StopCoroutine(m_ClientComCoroutine);
        m_ClientComCoroutine = null;
        m_client.Close();
        m_client = null;

        //Waiting to Accept a new Client
        m_server.BeginAcceptTcpClient(ClientConnected, null);
    }        
    #endregion

    //Custom Server Log
    #region ServerLog
    //With Text Color
    protected virtual void ServerLog(string msg, Color color)
    {
        Debug.Log("Server: " + msg);
    }
    //Without Text Color
    protected virtual void ServerLog(string msg)
    {
        Debug.Log("Server: " + msg);
    }
    #endregion

}