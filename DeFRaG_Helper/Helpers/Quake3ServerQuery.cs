using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DeFRaG_Helper
{
    public class Quake3ServerQuery
    {
        private Socket _serverConnection;
        private IPEndPoint _remoteIpEndPoint;
        private readonly int _timeout = 5000; // Timeout in milliseconds

        public Quake3ServerQuery(string serverAddress, int serverPort)
        {
            Connect(serverAddress, serverPort);
        }

        protected void Connect(string host, int port)
        {
            _serverConnection = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _serverConnection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _timeout);

            IPAddress ip;
            try
            {
                ip = IPAddress.Parse(host);
            }
            catch (FormatException)
            {
                ip = Dns.GetHostEntry(host).AddressList[0];
            }
            _remoteIpEndPoint = new IPEndPoint(ip, port);
        }

        // In Quake3ServerQuery.cs
        public async Task<(bool Success, string Response)> QueryServerAsync()
        {
            // Recreate the socket for each query
            using (var serverConnection = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                serverConnection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _timeout);

                byte[] message = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x67, 0x65, 0x74, 0x73, 0x74, 0x61, 0x74, 0x75, 0x73 };
                try
                {
                    // Connect moved inside try to ensure disposal even if it fails
                    serverConnection.Connect(_remoteIpEndPoint);

                    await serverConnection.SendToAsync(new ArraySegment<byte>(message, 0, message.Length), SocketFlags.None, _remoteIpEndPoint);
                    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[4096]);
                    SocketReceiveFromResult result = await serverConnection.ReceiveFromAsync(buffer, SocketFlags.None, _remoteIpEndPoint);
                    string responseMessage = Encoding.ASCII.GetString(buffer.Array, 0, result.ReceivedBytes);
                    return (true, $"Received: {responseMessage}");
                }
                catch (Exception ex)
                {
                    return (false, $"Error: {ex.Message}");
                }
                // No need for finally to close the socket, using statement handles it
            }
        }





    }
}
