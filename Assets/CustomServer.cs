using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This Server inheritated class acts like Server but using UI elements like buttons and input fields.
/// </summary>
public class CustomServer : Server
{
	[Header("UI References")]
	[SerializeField] private Button m_StartServerButton = null;
	[SerializeField] private Button m_SendToClientButton = null;
	[SerializeField] private InputField m_SendToClientInputField = null;
	[SerializeField] private Button m_CloseServerButton = null;
	[SerializeField] private ScrollRect m_ServerLoggerScrollRect = null;

	private bool serverConnected = false;
	private bool clientConnected = false;
	private RectTransform m_ServerLoggerRectTransform = null;
	private Text m_ServerLoggerText = null;

	//Set UI interactable properties
	protected virtual void Awake()
	{
		//Start Server
		m_StartServerButton.interactable = true;  //Enable button to let users start the server
		m_StartServerButton.onClick.AddListener(StartServer);

		//Send to Client
		m_SendToClientButton.interactable = false;
		m_SendToClientButton.onClick.AddListener(SendMessageToClient);

		//Close Server
		m_CloseServerButton.interactable = false; //Disable button until the server is started
		m_CloseServerButton.onClick.AddListener(CloseServer);

		//Populate Server delegates
		OnClientConnected = () => { clientConnected = true; };
		OnClientDisconnected = () => { clientConnected = false; };
		OnServerClosed = () => { serverConnected = false; };
		OnServerStarted = () => { serverConnected = true; };

		//UI References
		m_ServerLoggerRectTransform = m_ServerLoggerScrollRect.GetComponent<RectTransform>();
		m_ServerLoggerText = m_ServerLoggerScrollRect.content.gameObject.GetComponent<Text>();
	}

	protected override void Update()
	{
		base.Update();

		//Interactables needs to be setted on Update case can be called from a non-main thread
		m_StartServerButton.interactable = !serverConnected;
		m_CloseServerButton.interactable = serverConnected;
		m_SendToClientButton.interactable = clientConnected;
	}

	//Get input field text and send it to client
	private void SendMessageToClient()
	{
		string newMsg = m_SendToClientInputField.text;
		if(string.IsNullOrEmpty(newMsg))
		{
			m_ServerLoggerText.text += $"\n- Enter message";
			return;
		}
		base.SendMessageToClient(newMsg);
	}

	//Custom Server Log
	#region ServerLog
	//With Text Color
	protected override void ServerLog(string msg)
	{
		base.ServerLog(msg);
		m_ServerLoggerText.text += $"\n- {msg}";

		//Ensure ScrollBar shows last message
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_ServerLoggerRectTransform);
		m_ServerLoggerScrollRect.verticalNormalizedPosition = 0f;
	}
	//Without Text Color
	protected override void ServerLog(string msg, Color color)
	{
		base.ServerLog(msg, color);
		m_ServerLoggerText.text += $"\n<color=#{ColorUtility.ToHtmlStringRGBA(color)}>- {msg}</color>";

		//Ensure ScrollBar shows last message
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_ServerLoggerRectTransform);
		m_ServerLoggerScrollRect.verticalNormalizedPosition = 0f;
	}
	#endregion
}