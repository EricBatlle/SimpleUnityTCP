using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Server : MonoBehaviour
{
    public string ipAdress = "127.0.0.1";
    public int port = 54010;
    public float messagesLapsedTime = 5;
    [Space()]
    public Button startServerButton;
    public Button closeServerButton;
    public Text ServerLogger = null;

    private Quotes quotes = null;
    private IEnumerator ClientComCoroutine = null;

    private TcpListener server = null;
    private TcpClient client = null;
    private NetworkStream netStream = null;
    private byte[] buffer = new byte[49152];
    private int bytesReceived = 0;
    private string receivedMessage = "";
    
    private void ServerLog(string msg)
    {
        ServerLogger.text += '\n' + "- " + msg;
        Debug.Log("Server: " + msg);
    }

    private void Start()
    {
        startServerButton.interactable = true;
        closeServerButton.interactable = false;
    }

    public void StartServer()
    {
        //Load quotes to get something "special" to send
        quotes = new Quotes("wisdom");                      

        //Set and Enable Server 
        IPAddress ip = IPAddress.Parse(ipAdress);
        server = new TcpListener(ip, port);
        server.Start();
        ServerLog("Server Started");
        //Wait for client connection 
        server.BeginAcceptTcpClient(ClientConnected, null);
        startServerButton.interactable = false; //Disable button to avoid initilize more than one server
        closeServerButton.interactable = true;
    }

    //Check if any client trys to connect
    private void Update()
    {   
        //If some client stablish connection
        if (client != null && ClientComCoroutine == null)
        {
            //Start the ClientCommunication coroutine
            ClientComCoroutine = ClientCommunication();
            StartCoroutine(ClientComCoroutine);
        }
    }    

    //Manage client communication while a client is connected to the server
    IEnumerator ClientCommunication()
    {        
        //Restart values in case there are more than one client connections
        bytesReceived = 0;
        buffer = new byte[49152];
        netStream = client.GetStream();
        do
        {
            ServerLog("Server is listening");

            netStream.BeginRead(buffer, 0, buffer.Length, ReadCallback,  netStream);

            if (bytesReceived > 0)
            {
                //string msg = Encoding.ASCII.GetString(buf, 0, bytesReceived);
                ServerLog("Msg recived on SV: " + receivedMessage);

                if (receivedMessage == "QUOTE")
                {
                    //string sendMsg = quotes.RandomQuote;                    
                    string sendMsg = "QUOTE";
                    byte[] quoteOut = Encoding.ASCII.GetBytes(sendMsg);
                    netStream.Write(quoteOut, 0, quoteOut.Length);
                    ServerLog("Message sended to Client: " + sendMsg);

                    CloseConnection();
                }
            }

            //yield return null;
            yield return new WaitForSeconds(messagesLapsedTime);
        } while (bytesReceived >= 0);
        ServerLog("Close doClient coroutine");

        //Reset everything to defaults
        ClientComCoroutine = null;
        client.Close();
        client = null;
        //Waiting to Accept a new Client
        server.BeginAcceptTcpClient(ClientConnected, null);
    }

    private void ReadCallback(IAsyncResult result)
    {
        //var buffer = (byte[])result.AsyncState;
        //var ns = tcpClient.GetStream();
        if (result.IsCompleted)
        {
            bytesReceived = netStream.EndRead(result);

            receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
            //Debug.Log("Server: ReceivedMessage on Server" + receivedMessage);
        }
    }

    private void ClientConnected(IAsyncResult res)
    {
        client = server.EndAcceptTcpClient(res); //set the client reference
    }

    private void CloseConnection()
    {
        ServerLog("Close Connection");
        //Reset everything to defaults
        StopCoroutine(ClientComCoroutine);
        ClientComCoroutine = null;
        client.Close();
        client = null;
        //Waiting to Accept a new Client
        server.BeginAcceptTcpClient(ClientConnected, null);
    }    

    public void CloseServer()
    {
        if (client != null)
        {
            netStream.Close();
            netStream = null;
            client.Close();
            client = null;
        }
        server.Stop();
        server = null;

        startServerButton.interactable = true;
        closeServerButton.interactable = false;
    }
}