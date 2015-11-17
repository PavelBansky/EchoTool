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
    /// Implements functionality for UDP server UI
    /// </summary>
    public class UdpServerMode : IEchoMode
    {
        const int ListenPort = 7;

        #region Fields

        readonly Arguments _arguments;
        int _serverPort = ListenPort;
        #endregion

        /// <summary>
        /// Creates new UdpServerMode instance
        /// </summary>
        /// <param name="arguments">Arguments to server</param>
        public UdpServerMode(Arguments arguments)
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

            if (Utils.IsNumber(strServerPort))
                _serverPort = Convert.ToInt32(strServerPort);
            else
                return false;
                
            if (_serverPort > 65535)
                return false;

            return true;
        }

        /// <summary>
        /// Runs the UDP server mode
        /// </summary>
        public void Run()
        {
            var echoServer = new UdpEchoServer(_serverPort);
            echoServer.OnDataReceived += echoServer_OnDataReceived;
            echoServer.OnSocketException += echoServer_OnSocketException;
            
            Console.WriteLine(Messages.UDPServerCaption, _serverPort);

            echoServer.Start();
            Console.ReadKey(true);
            echoServer.Stop();
        }

        /// <summary>
        /// Event handler for socket errors
        /// </summary>
        /// <param name="socketException">Socket exception</param>
        private void echoServer_OnSocketException(SocketException socketException)
        {
            Console.Write(Messages.ServerError + ": ");
            Console.WriteLine(socketException.SocketErrorCode == SocketError.AddressAlreadyInUse
                ? Messages.AddressAlreadyInUse
                : socketException.Message);
        }

        /// <summary>
        /// Event handler for received data from client
        /// </summary>
        /// <param name="clientIpEndPoint">Client IP end point</param>
        /// <param name="receivedData">Data from client</param>
        private void echoServer_OnDataReceived(IPEndPoint clientIpEndPoint, byte[] receivedData)
        {
            var dataString = Encoding.ASCII.GetString(receivedData);
            Console.WriteLine(Messages.UDPServerDataReceived, DateTime.Now.ToLongTimeString(), clientIpEndPoint, dataString);            
        }
    }
}
