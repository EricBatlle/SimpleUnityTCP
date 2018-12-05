using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Server : MonoBehaviour
{
    #region Public Variables
    [Header("Network")]
    public string ipAdress = "127.0.0.1";
    public int port = 54010;
    public float waitingMessagesFrequency = 5;
    public string responseMessage = "Close";
    [Header("UI References")]
    public Button startServerButton;
    public Button closeServerButton;
    public Text ServerLogger = null;
    #endregion

    #region  Network m_Variables
    private TcpListener m_server = null;
    private TcpClient m_client = null;
    private NetworkStream m_netStream = null;
    private byte[] m_buffer = new byte[49152];
    private int m_bytesReceived = 0;
    private string m_receivedMessage = "";
    private IEnumerator m_ClientComCoroutine = null;
    #endregion

    //Set UI interactable properties
    private void Start()
    {
        startServerButton.interactable = true;  //Enable button to let users start the server
        closeServerButton.interactable = false; //Disable button until the server is started
    }

    //Start server and wait for clients
    public void StartServer()
    {        
        //Set and enable Server 
        IPAddress ip = IPAddress.Parse(ipAdress);
        m_server = new TcpListener(ip, port);
        m_server.Start();
        ServerLog("Server Started", Color.green);
        //Wait for async client connection 
        m_server.BeginAcceptTcpClient(ClientConnected, null);
        //Set UI interactable properties
        startServerButton.interactable = false; //Disable button to avoid initilize more than one server
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
            //Start Async Reading
            m_netStream.BeginRead(m_buffer, 0, m_buffer.Length, MessageReceived,  m_netStream);

            //If there is any msg
            if (m_bytesReceived > 0)
            {
                ServerLog("Msg recived on Server: " + "<b>" + m_receivedMessage + "</b>", Color.green);
                //If message received from client is "Close", send another "Close" to the client
                if (m_receivedMessage == "Close")
                {
                    //Build message to client
                    string sendMsg = responseMessage;                   //In this case we send "Close" to end the client connection
                    byte[] msgOut = Encoding.ASCII.GetBytes(sendMsg); //Encode message as bytes
                    //Start Sync Writing
                    m_netStream.Write(msgOut, 0, msgOut.Length);      
                    ServerLog("Msg sended to Client: " + "<b>" + sendMsg + "</b>", Color.blue);
                    //Close connection with the client
                    CloseConnection();
                }
            }
            yield return new WaitForSeconds(waitingMessagesFrequency);

        } while (m_bytesReceived >= 0 && m_netStream != null);   
        //The communication is over
        CloseConnection();
    }

    //Callback called when "BeginRead" is ended
    private void MessageReceived(IAsyncResult result)
    {
        if (result.IsCompleted && m_client.Connected)
        {
            //build message received from client
            m_bytesReceived = m_netStream.EndRead(result);                              //End async reading
            m_receivedMessage = Encoding.ASCII.GetString(m_buffer, 0, m_bytesReceived);   //De-encode message as string
        }
    }

    //Callback called when "BeginAcceptTcpClient" detects new client connection
    private void ClientConnected(IAsyncResult res)
    {
        //set the client reference
        m_client = m_server.EndAcceptTcpClient(res); 
    }

    //Close connection with the client
    private void CloseConnection()
    {
        ServerLog("Close Connection with Client", Color.red);
        //Reset everything to defaults
        StopCoroutine(m_ClientComCoroutine);
        m_ClientComCoroutine = null;
        m_client.Close();
        m_client = null;

        //Set UI interactable properties
        closeServerButton.interactable = true;  //Enable button to let users close the server

        //Waiting to Accept a new Client
        m_server.BeginAcceptTcpClient(ClientConnected, null);
    }    

    //Close client connection and disables the server
    public void CloseServer()
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
        if(m_server != null)
        {
            m_server.Stop();
            m_server = null;
            startServerButton.interactable = true;
            closeServerButton.interactable = false;
        }        
    }

    //Custom Server Log
    #region ServerLog
    //With Text Color
    private void ServerLog(string msg, Color color)
    {
        ServerLogger.text += '\n' + "<color=#"+ColorUtility.ToHtmlStringRGBA(color)+">- " + msg + "</color>";
        Debug.Log("Server: " + msg);
    }
    //Without Text Color
    private void ServerLog(string msg)
    {
        ServerLogger.text += '\n' + "- " + msg;
        Debug.Log("Server: " + msg);
    }
    #endregion

}