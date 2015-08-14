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
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

using EchoTool.Protocols;
using EchoTool.Resources;

namespace EchoTool.Modes
{
    /// <summary>
    /// Implements functionality for UDP server UI
    /// </summary>
    public class UdpServerMode : IEchoMode
    {
        const int LISTEN_PORT = 7;

        #region Fields
        Arguments arguments;
        int serverPort = LISTEN_PORT;
        #endregion

        /// <summary>
        /// Creates new UdpServerMode instance
        /// </summary>
        /// <param name="arguments">Arguments to server</param>
        public UdpServerMode(Arguments arguments)
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

            if (Utils.IsNumber(strServerPort))
                serverPort = Convert.ToInt32(strServerPort);
            else
                return false;
                
            if (serverPort > 65535)
                return false;

            return true;
        }

        /// <summary>
        /// Runs the UDP server mode
        /// </summary>
        public void Run()
        {
            UdpEchoServer echoServer = new UdpEchoServer(serverPort);
            echoServer.OnDataReceived += new UdpEchoServer.DataReceivedDelegate(echoServer_OnDataReceived);
            echoServer.OnSocketException += new UdpEchoServer.SocketExceptionDelegate(echoServer_OnSocketException);
            
            Console.WriteLine(string.Format(Messages.UDPServerCaption, serverPort));

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
            if (socketException.SocketErrorCode == SocketError.AddressAlreadyInUse)
                Console.WriteLine(Messages.AddressAlreadyInUse);
            else
                Console.WriteLine(socketException.Message);
        }

        /// <summary>
        /// Event handler for received data from client
        /// </summary>
        /// <param name="clientIpEndPoint">Client IP end point</param>
        /// <param name="receivedData">Data from client</param>
        private void echoServer_OnDataReceived(System.Net.IPEndPoint clientIpEndPoint, byte[] receivedData)
        {
            string dataString = Encoding.ASCII.GetString(receivedData);
            Console.WriteLine(string.Format(Messages.UDPServerDataReceived, DateTime.Now.ToLongTimeString(), clientIpEndPoint.ToString(), dataString));            
        }
    }
}
