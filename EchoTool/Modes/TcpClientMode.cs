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

using EchoToolCMD.Protocols;
using EchoToolCMD.Resources;
using System.Net;

namespace EchoToolCMD.Modes
{
    /// <summary>
    /// Implements for TCP client UI
    /// </summary>
    public class TcpClientMode : IEchoMode
    {
        const int REMOTE_PORT = 7;
        const int LOCAL_PORT = 0;
        const int REPEAT_COUNT = 5;
        const int TIMEOUT = 5;

        #region Fields
        Arguments arguments;
        int remotePort = REMOTE_PORT;
        int localPort = LOCAL_PORT;
        int repeatCount = REPEAT_COUNT;
        int responseTimeout = TIMEOUT;
        string echoPattern = string.Empty;

        int echoReceived, echoCorrupted;
        #endregion

        /// <summary>
        /// Creates new UdpClientMode instance
        /// </summary>
        /// <param name="arguments">Arguments to server</param>
        public TcpClientMode(Arguments arguments)
        {
            this.arguments = arguments;
        }

        /// <summary>
        /// Checks whether everything is ready to start echoing
        /// </summary>
        /// <returns>Returns true if client is ready</returns>
        public bool ParseArguments()
        {
            string strRemotePort = arguments.GetNotExists("/r", REMOTE_PORT.ToString());
            string strLocalPort = arguments.GetNotExists("/l", LOCAL_PORT.ToString());
            string strRepeatCount = arguments.GetNotExists("/n", REPEAT_COUNT.ToString());
            string strTimeout = arguments.GetNotExists("/t", TIMEOUT.ToString());

            echoPattern = arguments.Get("/d", string.Empty);

            if (Utils.IsNumber(strRemotePort))
                remotePort = Convert.ToInt32(strRemotePort);
            else
                return false;

            if (Utils.IsNumber(strLocalPort))
                localPort = Convert.ToInt32(strLocalPort);
            else
                return false;

            if (Utils.IsNumber(strRepeatCount))
                repeatCount = Convert.ToInt32(strRepeatCount);
            else
                return false;

            if (Utils.IsNumber(strTimeout))
                responseTimeout = Convert.ToInt32(strTimeout);
            else
                return false;

            if (remotePort > 65535 || localPort > 65535)
                return false;

            return true;
        }

        /// <summary>
        /// Runs TCP client mode
        /// </summary>
        public void Run()
        {
            echoReceived = echoCorrupted = 0;

            TcpEchoClient echoClient = new TcpEchoClient(arguments.FirstArgument, remotePort);
            echoClient.RepeatCount = repeatCount;
            echoClient.LocalPort = localPort;            
            echoClient.ResponseTimeout = responseTimeout;
            if (! string.IsNullOrEmpty(echoPattern))
                echoClient.EchoPattern = Encoding.ASCII.GetBytes(echoPattern);


            echoClient.OnHostnameResolved += new TcpEchoClient.HostnameResolvedDelegate(echoClient_OnHostnameResolved);
            echoClient.OnEchoResponse += new TcpEchoClient.EchoResponseDelegate(echoClient_OnEchoResponse);
            echoClient.OnSocketException += new TcpEchoClient.SocketExceptionDelegate(echoClient_OnSocketException);
            echoClient.OnFinish += new TcpEchoClient.FinishDelegate(echoClient_OnFinish);
            echoClient.Start();            
        }

        /// <summary>
        /// Event handler for host resolving
        /// </summary>
        /// <param name="hostnameIPEndPoint">server IP end point</param>
        private void echoClient_OnHostnameResolved(System.Net.IPEndPoint hostnameIPEndPoint)
        {
            Console.WriteLine(string.Format(Messages.HostnameResolved, arguments.FirstArgument, hostnameIPEndPoint.Address.ToString()));
            Console.WriteLine();
        }

        /// <summary>
        /// Event handler for ending the echoing
        /// </summary>
        /// <param name="abort">True if client was aborted befor end</param>
        private void echoClient_OnFinish(bool abort)
        {
            Console.WriteLine();            
            Console.WriteLine(string.Format(Messages.TCPClientStatistics, echoReceived, echoCorrupted));
        }

        /// <summary>
        /// Event handler for socket errors
        /// </summary>
        /// <param name="socketException">Socket exception</param>
        private void echoClient_OnSocketException(SocketException socketException)
        {
            if (socketException.SocketErrorCode == SocketError.HostNotFound)
                Console.WriteLine(Messages.HostNotFound);
            else if (socketException.SocketErrorCode == SocketError.HostUnreachable)
                Console.WriteLine(Messages.HostUnreachable);
            else if (socketException.SocketErrorCode == SocketError.ConnectionRefused)
                Console.WriteLine(Messages.CanNotConnectToServer);
            else if (socketException.SocketErrorCode == SocketError.ConnectionAborted)
                Console.WriteLine(Messages.ClosedByRemoteParty);
            else if (socketException.SocketErrorCode == SocketError.TimedOut)
                Console.WriteLine(Messages.ResponseTimeout);
            else
                Console.WriteLine(socketException.Message);
        }

        /// <summary>
        /// Event handler for echo response
        /// </summary>
        /// <param name="echoTime">Time to get echo back</param>
        /// <param name="dataComplete">True if echo arrived complete</param>
        private void echoClient_OnEchoResponse(IPEndPoint responseIPEndPoint, TimeSpan echoTime, bool dataComplete)
        {
            string state = string.Empty;
            if (dataComplete)
                state = Messages.ResponseOK;
            else
            {
                state = Messages.ResponseCorrupt;
                echoCorrupted++;
            }

            Console.WriteLine(string.Format(Messages.ClientResponse, responseIPEndPoint, echoTime.Milliseconds, state));

            echoReceived++;
        }
    }
}
