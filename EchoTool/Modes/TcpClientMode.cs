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
    /// Implements for TCP client UI
    /// </summary>
    public class TcpClientMode : IEchoMode
    {
        const int RemotePort = 7;
        const int LocalPort = 0;
        const int RepeatCount = 5;
        const int Timeout = 5;

        #region Fields

        readonly Arguments _arguments;
        int _remotePort = RemotePort;
        int _localPort = LocalPort;
        int _repeatCount = RepeatCount;
        int _responseTimeout = Timeout;
        string _echoPattern = string.Empty;

        int _echoReceived, _echoCorrupted;
        #endregion

        /// <summary>
        /// Creates new UdpClientMode instance
        /// </summary>
        /// <param name="arguments">Arguments to server</param>
        public TcpClientMode(Arguments arguments)
        {
            _arguments = arguments;
        }

        /// <summary>
        /// Checks whether everything is ready to start echoing
        /// </summary>
        /// <returns>Returns true if client is ready</returns>
        public bool ParseArguments()
        {
            var strRemotePort = _arguments.GetNotExists("/r", RemotePort.ToString());
            var strLocalPort = _arguments.GetNotExists("/l", LocalPort.ToString());
            var strRepeatCount = _arguments.GetNotExists("/n", RepeatCount.ToString());
            var strTimeout = _arguments.GetNotExists("/t", Timeout.ToString());

            _echoPattern = _arguments.Get("/d", string.Empty);

            if (Utils.IsNumber(strRemotePort))
                _remotePort = Convert.ToInt32(strRemotePort);
            else
                return false;

            if (Utils.IsNumber(strLocalPort))
                _localPort = Convert.ToInt32(strLocalPort);
            else
                return false;

            if (Utils.IsNumber(strRepeatCount))
                _repeatCount = Convert.ToInt32(strRepeatCount);
            else
                return false;

            if (Utils.IsNumber(strTimeout))
                _responseTimeout = Convert.ToInt32(strTimeout);
            else
                return false;

            if (_remotePort > 65535 || _localPort > 65535)
                return false;

            return true;
        }

        /// <summary>
        /// Runs TCP client mode
        /// </summary>
        public void Run()
        {
            _echoReceived = _echoCorrupted = 0;

            var echoClient = new TcpEchoClient(_arguments.FirstArgument, _remotePort)
            {
                RepeatCount = _repeatCount,
                LocalPort = _localPort,
                ResponseTimeout = _responseTimeout
            };

            if (! string.IsNullOrEmpty(_echoPattern))
                echoClient.EchoPattern = Encoding.ASCII.GetBytes(_echoPattern);


            echoClient.OnHostnameResolved += echoClient_OnHostnameResolved;
            echoClient.OnEchoResponse += echoClient_OnEchoResponse;
            echoClient.OnSocketException += echoClient_OnSocketException;
            echoClient.OnFinish += echoClient_OnFinish;
            echoClient.Start();            
        }

        /// <summary>
        /// Event handler for host resolving
        /// </summary>
        /// <param name="hostnameIpEndPoint">server IP end point</param>
        private void echoClient_OnHostnameResolved(IPEndPoint hostnameIpEndPoint)
        {
            Console.WriteLine(Messages.HostnameResolved, _arguments.FirstArgument, hostnameIpEndPoint.Address);
            Console.WriteLine();
        }

        /// <summary>
        /// Event handler for ending the echoing
        /// </summary>
        /// <param name="abort">True if client was aborted befor end</param>
        private void echoClient_OnFinish(bool abort)
        {
            Console.WriteLine();            
            Console.WriteLine(Messages.TCPClientStatistics, _echoReceived, _echoCorrupted);
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
        /// <param name="responseIpEndPoint"></param>
        /// <param name="echoTime">Time to get echo back</param>
        /// <param name="dataComplete">True if echo arrived complete</param>
        private void echoClient_OnEchoResponse(IPEndPoint responseIpEndPoint, TimeSpan echoTime, bool dataComplete)
        {
            string state;
            if (dataComplete)
                state = Messages.ResponseOK;
            else
            {
                state = Messages.ResponseCorrupt;
                _echoCorrupted++;
            }

            Console.WriteLine(Messages.ClientResponse, responseIpEndPoint, echoTime.Milliseconds, state);

            _echoReceived++;
        }
    }
}
