/* 
 *  Name:           Echo Tool
 *  
 *  Description:    Application provides functionality of the echo server and client.
 *                  Designed according to RFC 862 http://www.ietf.org/rfc/rfc0862.txt?number=862
 *                  
 *  Author:         Pavel Bansky
 *  Contact:        pavel@bansky.net
 *  Website:        http://bansky.net/echotool
 * 
 */
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

using EchoToolCMD.Protocols;
using EchoToolCMD.Resources;

namespace EchoToolCMD.Modes
{
    /// <summary>
    /// Implements functionality for TCP server UI
    /// </summary>
    public class TcpServerMode : IEchoMode
    {
        const int LISTEN_PORT = 7;
        const int TIMEOUT = 300;

        #region Fields
        Arguments arguments;
        int serverPort = LISTEN_PORT;
        int connTimeout = TIMEOUT;
        #endregion

        /// <summary>
        /// Creates new TcpServerMode instance
        /// </summary>
        /// <param name="arguments">Arguments to server</param>
        public TcpServerMode(Arguments arguments)
        {
            this.arguments = arguments;
        }

        /// <summary>
        /// Checks whether everything is ready to start server
        /// </summary>
        /// <returns>Returns true if server is ready</returns>
        public bool ParseArguments()
        {
            string strServerPort = arguments.Get("/s", LISTEN_PORT.ToString());
            string strTimeout = arguments.GetNotExists("/t", TIMEOUT.ToString());

            if (Utils.IsNumber(strServerPort))
                serverPort = Convert.ToInt32(strServerPort);
            else
                return false;

            if (Utils.IsNumber(strTimeout))
                connTimeout = Convert.ToInt32(strTimeout);
            else
                return false;

            if (serverPort > 65535)
                return false;

            return true;
        }

        /// <summary>
        /// Runs the TCP server mode
        /// </summary>
        public void Run()
        {
            TcpEchoServer echoServer = new TcpEchoServer(serverPort);
            echoServer.ConnectionTimeout = connTimeout;
            echoServer.OnConnect += new TcpEchoServer.OnConnectDelegate(echoServer_OnConnect);
            echoServer.OnDisconnect += new TcpEchoServer.OnDisconnectDelegate(echoServer_OnDisconnect);
            echoServer.OnDataReceived += new TcpEchoServer.DataReceivedDelegate(echoServer_OnDataReceived);
            echoServer.OnSocketException += new TcpEchoServer.SocketExceptionDelegate(echoServer_OnSocketException);
            
            Console.WriteLine(string.Format(Messages.TCPServerCaption, serverPort));

            echoServer.Start();            
            Console.ReadKey(true);
            echoServer.Stop();
        }

        /// <summary>
        /// Event handler for client connect event
        /// </summary>
        /// <param name="clientEndPoint">Connected client end point</param>
        private void echoServer_OnConnect(EndPoint clientEndPoint)
        {
            Console.WriteLine();
            Console.WriteLine(string.Format(Messages.TCPServerConnect, clientEndPoint, DateTime.Now.ToLongTimeString()));            
        }

        /// <summary>
        /// Event handler for client disconnect event
        /// </summary>
        /// <param name="timeout">True if connection was timeouted</param>
        private void echoServer_OnDisconnect(bool timeout)
        {
            Console.WriteLine();

            if (timeout)
                Console.WriteLine(Messages.TCPSessionTimeout);
            else
                Console.WriteLine(Messages.TCPSessionClosedByPeer);

            Console.WriteLine(string.Format(Messages.TCPServerCaption, serverPort));
        }

        /// <summary>
        /// Event handler for socket errors
        /// </summary>
        /// <param name="socketException">Socket exception</param>
        private void echoServer_OnSocketException(SocketException socketException)
        {
            Console.Write(Messages.ServerError + ": ");
            if (socketException.SocketErrorCode == SocketError.AccessDenied)
                Console.WriteLine(Messages.AddressAlreadyInUse);
            else
                Console.WriteLine(socketException.Message);
        }

        /// <summary>
        /// Event handler for received data from client
        /// </summary>
        /// <param name="receivedData">Data from client</param>
        private void echoServer_OnDataReceived(byte[] receivedData)
        {
            string dataString = Encoding.ASCII.GetString(receivedData);
            Console.WriteLine(string.Format(Messages.TCPServerDataReceived, DateTime.Now.ToLongTimeString(), dataString));
        }
    }
}
