using Senparc.WebSocket;
using System.Threading.Tasks;

namespace WechatMessageSample
{
    public class CustomWebSocketMessageHandler : WebSocketMessageHandler
    {
        public override async Task OnMessageReceiced(WebSocketHelper webSocketHandler, 
            ReceivedMessage message, string originalData)
        {
            await webSocketHandler.SendMessage($"您发送了消息：{message.Message}",
                webSocketHandler.WebSocket.Clients.All);

            //...
        }
    }
}
