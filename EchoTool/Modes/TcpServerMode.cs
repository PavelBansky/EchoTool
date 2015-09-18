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
using System.Net;
using System.Net.Sockets;
using System.Text;
using EchoTool.Protocols;
using EchoTool.Resources;

namespace EchoTool.Modes
{
    /// <summary>
    /// Implements functionality for TCP server UI
    /// </summary>
    public class TcpServerMode : IEchoMode
    {
        const int ListenPort = 7;
        const int Timeout = 300;

        #region Fields

        readonly Arguments _arguments;
        int _serverPort = ListenPort;
        int _connTimeout = Timeout;
        #endregion

        /// <summary>
        /// Creates new TcpServerMode instance
        /// </summary>
        /// <param name="arguments">Arguments to server</param>
        public TcpServerMode(Arguments arguments)
        {
            _arguments = arguments;
        }

        /// <summary>
        /// Checks whether everything is ready to start server
        /// </summary>
        /// <returns>Returns true if server is ready</returns>
        public bool ParseArguments()
        {
            var strServerPort = _arguments.Get("/s", ListenPort.ToString());
            var strTimeout = _arguments.GetNotExists("/t", Timeout.ToString());

            if (Utils.IsNumber(strServerPort))
                _serverPort = Convert.ToInt32(strServerPort);
            else
                return false;

            if (Utils.IsNumber(strTimeout))
                _connTimeout = Convert.ToInt32(strTimeout);
            else
                return false;

            if (_serverPort > 65535)
                return false;

            return true;
        }

        /// <summary>
        /// Runs the TCP server mode
        /// </summary>
        public void Run()
        {
            var echoServer = new TcpEchoServer(_serverPort) { ConnectionTimeout = _connTimeout };
            echoServer.OnConnect += echoServer_OnConnect;
            echoServer.OnDisconnect += echoServer_OnDisconnect;
            echoServer.OnDataReceived += echoServer_OnDataReceived;
            echoServer.OnSocketException += echoServer_OnSocketException;

            Console.WriteLine(Messages.TCPServerCaption, _serverPort);

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
            Console.WriteLine(Messages.TCPServerConnect, clientEndPoint, DateTime.Now.ToLongTimeString());
        }

        /// <summary>
        /// Event handler for client disconnect event
        /// </summary>
        /// <param name="timeout">True if connection was timeouted</param>
        private void echoServer_OnDisconnect(bool timeout)
        {
            Console.WriteLine();

            Console.WriteLine(timeout ? Messages.TCPSessionTimeout : Messages.TCPSessionClosedByPeer);

            Console.WriteLine(Messages.TCPServerCaption, _serverPort);
        }

        /// <summary>
        /// Event handler for socket errors
        /// </summary>
        /// <param name="socketException">Socket exception</param>
        private void echoServer_OnSocketException(SocketException socketException)
        {
            Console.Write(Messages.ServerError + ": ");
            Console.WriteLine(socketException.SocketErrorCode == SocketError.AccessDenied
                ? Messages.AddressAlreadyInUse
                : socketException.Message);
        }

        /// <summary>
        /// Event handler for received data from client
        /// </summary>
        /// <param name="receivedData">Data from client</param>
        private void echoServer_OnDataReceived(byte[] receivedData)
        {
            var dataString = Encoding.ASCII.GetString(receivedData);
            Console.WriteLine(Messages.TCPServerDataReceived, DateTime.Now.ToLongTimeString(), dataString);
        }
    }
}
