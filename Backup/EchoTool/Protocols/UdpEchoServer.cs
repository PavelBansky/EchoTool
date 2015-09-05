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
using System.Text;

namespace EchoTool.Protocols
{
    /// <summary>
    /// Implements UDP echo server
    /// </summary>
    public class UdpEchoServer : IDisposable
    {
        #region Field
        Thread mainThread = null;        
        bool serverRunning = false;        
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
            if (mainThread == null)
            {
                mainThread = new Thread(new ThreadStart(ServerThread));
                serverRunning = true;
                mainThread.Start();
            }
            else
                throw new Exception("Echo server thread is already running.");
        }

        /// <summary>
        /// Stops the UDP echo server thread
        /// </summary>
        public void Stop()
        {
            if (serverRunning)
            {
                serverRunning = false;
                mainThread.Abort();
                mainThread = null;
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
                UdpState udpState = new UdpState();
                udpState.IPEndPoint = new IPEndPoint(IPAddress.Any, ListenPort);
                udpState.UdpClient = new UdpClient(udpState.IPEndPoint);
                udpState.UdpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpState);

                // Main loop
                while (serverRunning) { }
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
            UdpState udpState = (UdpState)ar.AsyncState;
            UdpClient udpClient = udpState.UdpClient;
            IPEndPoint endPoint = udpState.IPEndPoint;

            byte[] data = udpClient.EndReceive(ar, ref endPoint);            

            // Send the data back
            udpClient.Connect(endPoint);
            udpClient.Send(data, data.Length);
            udpClient.Close();

            // Rise the event
            if (OnDataReceived != null)
                OnDataReceived(endPoint, data);

            udpState.IPEndPoint = new IPEndPoint(IPAddress.Any, ListenPort);
            udpState.UdpClient = new UdpClient(udpState.IPEndPoint);
            udpState.UdpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpState);
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
