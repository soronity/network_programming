using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class ChatManager : NetworkBehaviour
{
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private TextMeshProUGUI messagesText;  // Text object to display chat messages
    [SerializeField] private Button sendMessageButton;     

    private void Start()
    {
        ResetChatState();

        if (sendMessageButton != null)
        {
            sendMessageButton.onClick.AddListener(OnSendMessageClicked);
        }
        else
        {
            Debug.LogError("SendMessageButton is not assigned in the Inspector!");
        }

        if (messagesText == null || chatInputField == null)
        {
            Debug.LogError("Ensure ChatInputField and MessagesText are properly assigned in the Inspector.");
        }
    }

    private void ResetChatState()
    {
        if (messagesText != null)
        {
            messagesText.text = ""; // Clear chat display
        }

        if (chatInputField != null)
        {
            chatInputField.text = ""; // Clear input field
        }
    }

    private void OnSendMessageClicked()
    {

        string message = chatInputField.text.Trim();

        if (string.IsNullOrEmpty(message))
        {
            Debug.Log("Message is empty. Not sending.");
            return;
        }

        SendChatMessageServerRpc(message);

        chatInputField.text = "";
        chatInputField.ActivateInputField();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendChatMessageServerRpc(string message, ServerRpcParams serverRpcParams = default)
    {
        // Broadcast the message to all clients
        BroadcastChatMessageClientRpc(message, serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void BroadcastChatMessageClientRpc(string message, ulong senderClientId)
    {
        // Format the message with the sender's ID
        string formattedMessage = $"<b>Player {senderClientId}</b>: {message}";

        // Append the message to the chat display
        AppendMessageToChatDisplay(formattedMessage);
    }

    private void AppendMessageToChatDisplay(string message)
    {
        if (messagesText != null)
        {
            messagesText.text += message + "\n";

            // Optionally, scroll to the bottom of the messages
            Canvas.ForceUpdateCanvases();
        }
        else
        {
            Debug.LogError("MessagesText is not assigned in the Inspector!");
        }
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallback;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedCallback;
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log($"Client connected: ClientId {clientId}");
        // No ownership transfer
    }

    private void OnClientDisconnectedCallback(ulong clientId)
    {
        Debug.Log($"Client disconnected: ClientId {clientId}");
        ResetChatState();
    }
}
