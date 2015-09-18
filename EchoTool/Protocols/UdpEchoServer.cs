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
using System.Threading;

namespace EchoTool.Protocols
{
    /// <summary>
    /// Implements UDP echo server
    /// </summary>
    public class UdpEchoServer : IDisposable
    {
        #region Field
        Thread _mainThread;
        bool _serverRunning;
        #endregion

        #region Constructor
        /// <summary>
        /// UDP echo server on default Echo port 7
        /// </summary>
        public UdpEchoServer() : this(7)
        {
        }

        /// <summary>
        /// UDP echo server on specified port
        /// </summary>
        /// <param name="listenPort">port to listen</param>
        public UdpEchoServer(int listenPort)
        {
            ListenPort = listenPort;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts the UDP echo server thread
        /// </summary>
        public void Start()
        {
            if (_mainThread == null)
            {
                _mainThread = new Thread(ServerThread);
                _serverRunning = true;
                _mainThread.Start();
            }
            else
                throw new Exception("Echo server thread is already running.");
        }

        /// <summary>
        /// Stops the UDP echo server thread
        /// </summary>
        public void Stop()
        {
            if (_serverRunning)
            {
                _serverRunning = false;
                _mainThread.Abort();
                _mainThread = null;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Main thread method
        /// </summary>
        private void ServerThread()
        {
            try
            {
                var udpState = new UdpState { IpEndPoint = new IPEndPoint(IPAddress.Any, ListenPort) };
                udpState.UdpClient = new UdpClient(udpState.IpEndPoint);
                udpState.UdpClient.BeginReceive(ReceiveCallback, udpState);

                // Main loop
                while (_serverRunning) { }
            }
            catch (SocketException socketException)
            {
                if (OnSocketException != null)
                {
                    OnSocketException(socketException);
                    Stop();
                }
            }
        }

        /// <summary>
        /// AsyncCallback fro UDP receive
        /// </summary>
        /// <param name="ar">Async result</param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            var udpState = (UdpState)ar.AsyncState;
            var udpClient = udpState.UdpClient;
            var endPoint = udpState.IpEndPoint;

            var data = udpClient.EndReceive(ar, ref endPoint);

            // Send the data back
            udpClient.Connect(endPoint);
            udpClient.Send(data, data.Length);
            udpClient.Close();

            // Rise the event
            OnDataReceived?.Invoke(endPoint, data);

            udpState.IpEndPoint = new IPEndPoint(IPAddress.Any, ListenPort);
            udpState.UdpClient = new UdpClient(udpState.IpEndPoint);
            udpState.UdpClient.BeginReceive(ReceiveCallback, udpState);
        }
        #endregion

        #region Events & Delegates
        public delegate void DataReceivedDelegate(IPEndPoint clientIpEndPoint, byte[] receivedData);
        public delegate void SocketExceptionDelegate(SocketException socketException);

        /// <summary>
        /// Event is raised whenever server receives data for echo
        /// </summary>
        public event DataReceivedDelegate OnDataReceived;
        /// <summary>
        /// Occures when socket exceptions is thrown
        /// </summary>
        public event SocketExceptionDelegate OnSocketException;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets server listen port
        /// </summary>
        public int ListenPort { get; set; }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}
