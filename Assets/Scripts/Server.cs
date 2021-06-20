using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/// <summary>
/// Server class shows how to implement and use TcpListener in Unity.
/// </summary>
public class Server : MonoBehaviour
{
	#region Public Variables
	[Header("Network")]
	public string ipAdress = "127.0.0.1";
	public int port = 54010;
	[Min(0)] public float waitingMessagesFrequency = 1f;
	[Tooltip("0 means time out is disabled")]
	[Min(0)] public float receivingTimeOut = 0f;
	#endregion

	#region  Private m_Variables
	private TcpListener m_Server = null;
	private TcpClient m_Client = null;
	private NetworkStream m_NetStream = null;
	private byte[] m_Buffer = new byte[49152];
	private int m_BytesReceived = 0;
	private float m_EllapsedTime = 0f;
	private bool m_TimeOutReached = false;
	//ToDo: Check if I can do the same without this property
	[SerializeField] [TextArea] private string m_ReceivedMessage = "";
	protected string ReceivedMessage
	{
		get { return m_ReceivedMessage; }
		set
		{
			m_ReceivedMessage = value;
			OnRecivedMessageChange();
		}
	}
	private IEnumerator m_ListenClientMsgsCoroutine = null;

	#endregion

	#region Delegate Variables
	protected Action OnServerStarted = null;        //Delegate triggered when server start
	protected Action OnServerClosed = null;         //Delegate triggered when server close
	protected Action OnClientConnected = null;      //Delegate triggered when the server stablish connection with client
	protected Action OnClientDisconnected = null;   //Delegate triggered when the server stablish connection with client
	protected Action OnUnsuccessfulStart = null;    //Delegate triggered when the server fails the start
	#endregion

	protected virtual void OnRecivedMessageChange() { }

	//Start server and wait for clients
	protected virtual void StartServer()
	{
		try
		{
			//Set and enable Server 
			IPAddress ip = IPAddress.Parse(ipAdress);
			m_Server = new TcpListener(ip, port);
			m_Server.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
			m_Server.Start();
			ServerLog($"Server Started on {ipAdress}::{port}", Color.green);
			//Wait for async client connection 
			m_Server.BeginAcceptTcpClient(ClientConnected, null);
			OnServerStarted?.Invoke();
		}
		catch (Exception)
		{
			OnUnsuccessfulStart?.Invoke();
			ServerLog("Socket or Format Exception: Port or IP not available", Color.red);
		}
	}

	//Check if any client trys to connect
	protected virtual void Update()
	{
		//If some client stablish connection
		if (m_Client != null && m_ListenClientMsgsCoroutine == null)
		{
			//Start Listening Client Messages coroutine
			m_ListenClientMsgsCoroutine = ListenClientMessages();
			StartCoroutine(m_ListenClientMsgsCoroutine);
		}
	}

	//Callback called when "BeginAcceptTcpClient" detects new client connection
	private void ClientConnected(IAsyncResult res)
	{
		//set the client reference
		m_Client = m_Server.EndAcceptTcpClient(res);
		OnClientConnected?.Invoke();
	}

	#region Communication Server<->Client
	//Coroutine waiting client messages while client is connected to the server
	private IEnumerator ListenClientMessages()
	{
		//Restart values in case there are more than one client connections
		m_BytesReceived = 0;
		m_Buffer = new byte[49152];

		//Stablish Client NetworkStream information
		m_NetStream = m_Client.GetStream();
		m_EllapsedTime = 0f;
		m_TimeOutReached = false;

		//While there is a connection with the client, await for messages
		do
		{
			ServerLog("Server is listening client msg...", Color.yellow);
			//Start Async Reading from Client and manage the response on MessageReceived function
			m_NetStream.BeginRead(m_Buffer, 0, m_Buffer.Length, MessageReceived, m_NetStream);

			//If there is any msg, do something
			if (m_BytesReceived > 0)
			{
				OnMessageReceived(m_ReceivedMessage);
				m_BytesReceived = 0;
			}

			yield return new WaitForSeconds(waitingMessagesFrequency);
			//Check TimeOut
			m_EllapsedTime += waitingMessagesFrequency;
			if (m_EllapsedTime >= receivingTimeOut && receivingTimeOut != 0)
			{
				ServerLog("Receiving Messages TimeOut", Color.red);
				ServerLog("Remember to close Client!", Color.black);
				CloseClientConnection();
				m_TimeOutReached = true;
			}
		} while ((m_BytesReceived >= 0 && m_NetStream != null && m_Client != null) && (!m_TimeOutReached));
		//Communication is over
	}

	//What to do with the received message on server
	protected virtual void OnMessageReceived(string receivedMessage)
	{
		ServerLog($"Msg recived on Server: <b>{receivedMessage}</b>", Color.green);
		switch (receivedMessage)
		{
			case "Close":
				//Close client connection
				CloseClientConnection();
				break;
			default:
				ServerLog($"Received message <b>{receivedMessage}</b>, has no special behaviuor", Color.red);
				break;
		}
	}

	//Send custom string msg to client
	protected void SendMessageToClient(string messageToSend)
	{
		//early out if there is nothing connected
		if (m_NetStream == null)
		{
			ServerLog("Socket Error: Start at least one client first", Color.red);
			return;
		}

		//Build message to client
		byte[] encodedMessage = Encoding.ASCII.GetBytes(messageToSend); //Encode message as bytes

		//Start Sync Writing
		m_NetStream.Write(encodedMessage, 0, encodedMessage.Length);
		ServerLog($"Msg sended to Client: <b>{messageToSend}</b>", Color.blue);
	}

	//AsyncCallback called when "BeginRead" is ended, waiting the message response from client
	private void MessageReceived(IAsyncResult result)
	{
		if (result.IsCompleted && m_Client.Connected)
		{
			//build message received from client
			m_BytesReceived = m_NetStream.EndRead(result); //End async reading
			ReceivedMessage = Encoding.ASCII.GetString(m_Buffer, 0, m_BytesReceived); //De-encode message as string
		}
	}
	#endregion

	#region Close Server/ClientConnection
	//Close client connection and disables the server
	protected virtual void CloseServer()
	{
		ServerLog("Server Closed", Color.red);
		//Close client connection
		if (m_Client != null)
		{
			ServerLog("Remember to close Client!", Color.black);
			m_NetStream?.Close();
			m_NetStream = null;
			m_Client.Close();
			m_Client = null;
			OnClientDisconnected?.Invoke();
		}
		//Close server connection
		if (m_Server != null)
		{
			m_Server.Stop();
			m_Server = null;
		}

		if (m_ListenClientMsgsCoroutine != null)
		{
			StopCoroutine(m_ListenClientMsgsCoroutine);
			m_ListenClientMsgsCoroutine = null;
		}

		OnServerClosed?.Invoke();
	}

	//Close connection with the client
	protected virtual void CloseClientConnection()
	{
		ServerLog("Close Connection with Client", Color.red);
		//Reset everything to defaults
		if (m_ListenClientMsgsCoroutine != null)
		{
			StopCoroutine(m_ListenClientMsgsCoroutine);
			m_ListenClientMsgsCoroutine = null;
		}

		m_Client.Close();
		m_Client = null;

		OnClientDisconnected?.Invoke();

		//Waiting to Accept a new Client
		m_Server.BeginAcceptTcpClient(ClientConnected, null);
	}
	#endregion

	#region ServerLog
	//Custom Server Log - With Text Color
	protected virtual void ServerLog(string msg, Color color)
	{
		Debug.Log($"<b>Server:</b> {msg}");
	}
	//Custom Server Log - Without Text Color
	protected virtual void ServerLog(string msg)
	{
		Debug.Log($"<b>Server:</b> {msg}");
	}
	#endregion

}