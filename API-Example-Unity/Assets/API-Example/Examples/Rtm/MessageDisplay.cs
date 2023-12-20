using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace io.agora.rtm.demo
{
    public class MessageDisplay : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] int maxMessages = 25;
        [SerializeField] GameObject chatPanel, textPrefab;
        [SerializeField] MessageColorStruct MessageColors;
        [SerializeField] List<Message> messageList = new List<Message>();
        //[SerializeField] RawImage ImageDisplay;
#pragma warning restore 0649

        private void Awake()
        {
            //ImageDisplay.gameObject.SetActive(false);
        }
        public class MessageData
        {
            public Message.MessageType messageType;

            public string message;

            public MessageData(Message.MessageType messageType, string message)
            {
                this.messageType = messageType;
                this.message = message;
            }
        }
        Queue<MessageData> messageDatas = new Queue<MessageData>();
        public void AddMessage(string message, Message.MessageType messageType)
        {
            messageDatas.Enqueue(new MessageData(messageType, message));
        }

        private bool _needScrollToBottom = false;
        void Update()
        {
            if (messageDatas.Count > 0)
            {
                MessageData messageData = messageDatas.Dequeue();
                AddTextToDisplay(messageData.message, messageData.messageType);
                _needScrollToBottom = true;

            }
        }


        private void LateUpdate()
        {
            if (_needScrollToBottom)
            {
                this.Invoke("FixSelfPositionToBottom", 0.1f);
                _needScrollToBottom = false;
            }
        }

        private void FixSelfPositionToBottom()
        {
            (this.transform.parent.parent.GetComponent<ScrollRect>()).verticalNormalizedPosition = 0;
        }

        private void AddTextToDisplay(string text, Message.MessageType messageType)
        {
            if (messageList.Count >= maxMessages)
            {
                Destroy(messageList[0].textObj.gameObject);
                messageList.Remove(messageList[0]);
            }

            Message newMessage = new Message();
            newMessage.text = text;

            GameObject newText = Instantiate(textPrefab, chatPanel.transform);
            RectTransform tran = (RectTransform)newText.transform;
            tran.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ((RectTransform)chatPanel.transform).rect.width);
            newMessage.textObj = newText.GetComponent<Text>();
            newMessage.textObj.text = "\n" + newMessage.text;
            newMessage.textObj.color = MessageTypeColor(messageType);
            messageList.Add(newMessage);
        }

        public void AddImageToDisplay(byte[] bytes)
        {
            // Create a texture. Texture size does not matter, since
            // LoadImage will replace with with incoming image size.
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            //ImageDisplay.texture = texture;
            //ImageDisplay.gameObject.SetActive(true);
        }

        public void Clear()
        {
            //ImageDisplay.texture = null;
            //ImageDisplay.gameObject.SetActive(false);
            foreach (Message msg in messageList)
            {
                Destroy(msg.textObj.gameObject);
            }
            messageList.Clear();
            messageDatas.Clear();
        }

        Color MessageTypeColor(Message.MessageType messageType)
        {
            Color color = MessageColors.infoColor;

            switch (messageType)
            {
                case Message.MessageType.PlayerMessage:
                    color = MessageColors.playerColor;
                    break;
                case Message.MessageType.TopicMessage:
                    color = MessageColors.channelColor;
                    break;
                case Message.MessageType.PeerMessage:
                    color = MessageColors.peerColor;
                    break;
                case Message.MessageType.Error:
                    color = MessageColors.errorColor;
                    break;
            }

            return color;
        }
    }


    [System.Serializable]
    public class Message
    {
        public string text;
        public Text textObj;
        public MessageType messageType;

        public enum MessageType
        {
            Info,
            Error,
            PlayerMessage,
            TopicMessage,
            PeerMessage
        }
    }

    [System.Serializable]
    public struct MessageColorStruct
    {
        public Color infoColor, errorColor, playerColor, peerColor, channelColor;
    }
}
