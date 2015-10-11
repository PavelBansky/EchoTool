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
using System.IO;

namespace EchoTool.Protocols
{
    /// <summary>
    /// Implements TCP echo server
    /// </summary>
    public class TcpEchoServer : IDisposable
    {
        #region Field
        Thread mainThread = null;
        bool serverRunning = false;
        TcpListener tcpListener;
        TcpClient tcpClient;
        #endregion

        #region Constructor
        /// <summary>
        /// TCP echo server on default Echo port 7
        /// </summary>
        public TcpEchoServer()
            : this(7)
        {
        }

        /// <summary>
        /// TCP echo server on specified port
        /// </summary>
        /// <param name="listenPort">port to listen</param>
        public TcpEchoServer(int listenPort)
        {
            ListenPort = listenPort;
            ConnectionTimeout = 300;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts the TCP echo server thread
        /// </summary>
        public void Start()
        {
            if (mainThread == null)
            {
                tcpListener = new TcpListener(IPAddress.Any, ListenPort);              
                mainThread = new Thread(new ThreadStart(ServerThread));
                serverRunning = true;
                mainThread.Start();
            }
            else
                throw new Exception("Echo server thread is already running.");
        }

        /// <summary>
        /// Stops the TCP echo server thread
        /// </summary>
        public void Stop()
        {
            if (serverRunning)
            {
                serverRunning = false;
                tcpListener.Stop();
                mainThread.Abort();
                mainThread = null;
                tcpClient = null;
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
                // Start listening
                tcpListener.Start();

                // Main loop
                while (serverRunning)
                {                    
                    tcpClient = tcpListener.AcceptTcpClient();

                    // Raise event when client is connected
                    if (OnConnect != null)
                        OnConnect(tcpClient.Client.RemoteEndPoint);

                    // Start exchanging the data when client is connected
                    if (tcpClient != null && tcpClient.Client.Connected)
                    {
                        // Init state object
                        TcpState tcpState = new TcpState();
                        tcpState.NetworkStream = new NetworkStream(tcpClient.Client);
                        tcpState.DataBuffer = new byte[tcpClient.ReceiveBufferSize];
                        tcpState.TimeoutWatch = DateTime.Now;

                        tcpState.NetworkStream.BeginRead(tcpState.DataBuffer, 0, tcpState.DataBuffer.Length, new AsyncCallback(ReadCallBack), tcpState);

                        bool timeOut = false;
                        while (tcpClient.Client.Connected && serverRunning && !timeOut)
                        {
                            TimeSpan timeoutSpan = DateTime.Now - tcpState.TimeoutWatch;
                            if (timeoutSpan.Seconds >= ConnectionTimeout)
                                timeOut = true;
                        }
                        
                        // Raise disconnect event
                        if (OnDisconnect != null)
                            OnDisconnect(timeOut);
                        
                        tcpClient.Close();
                    }
                }
            }
            catch (SocketException socketException)
            {
                // Report every error but Interrupted, thrown by tcpListener when stopping the server
                if (socketException.SocketErrorCode != SocketError.Interrupted && OnSocketException != null)
                {
                    OnSocketException(socketException);
                    Stop();             
                }
            }
            
            Stop();
        }

        /// <summary>
        /// Callback method for asynchronous read
        /// </summary>
        /// <param name="ar"></param>
        public void ReadCallBack(IAsyncResult ar)
        {
            TcpState tcpState = (TcpState)ar.AsyncState;
            NetworkStream networkStream = tcpState.NetworkStream;

            try
            {
                int bytesRead = networkStream.EndRead(ar);
                if (bytesRead > 0)
                {
                    // Send back received date
                    byte[] receivedData = new byte[bytesRead];
                    Array.Copy(tcpState.DataBuffer, receivedData, bytesRead);                  
                    networkStream.Write(receivedData, 0, receivedData.Length);

                    // Raise event
                    if (OnDataReceived != null)
                        OnDataReceived(receivedData);

                    // Reset timeouut watch
                    tcpState.TimeoutWatch = DateTime.Now;
                    networkStream.BeginRead(tcpState.DataBuffer, 0, tcpState.DataBuffer.Length, new AsyncCallback(ReadCallBack), tcpState);
                }
                else
                {
                    // We are hooked in CLOSE_WAIT state, so close
                    tcpClient.Client.Close();
                }
            }
            catch (IOException ioException)
            {
                // Rethrow every exception that is not socketException                
                if (ioException.InnerException.GetType() == typeof(SocketException))
                {
                    SocketException socketException = (SocketException)ioException.InnerException;
                    // Report every error but ConnectionReset
                    if (socketException.SocketErrorCode != SocketError.ConnectionReset && OnSocketException != null)
                    {
                        OnSocketException(socketException);
                        Stop();
                    }
                }
                else if (ioException.InnerException.GetType() != typeof(ObjectDisposedException))
                    throw ioException;                
            }
        }
        #endregion

        #region Events & Delegates
        public delegate void DataReceivedDelegate(byte[] data);
        public delegate void OnConnectDelegate(EndPoint clientEndPoint);
        public delegate void OnDisconnectDelegate(bool timeout);
        public delegate void SocketExceptionDelegate(SocketException socketException);

        /// <summary>
        /// Event is raised whenever server receives data for echo
        /// </summary>
        public event DataReceivedDelegate OnDataReceived;
        /// <summary>
        /// Event is raised when client connection is accepted
        /// </summary>
        public event OnConnectDelegate OnConnect;
        /// <summary>
        /// Event is raised when client disconnects session
        /// </summary>
        public event OnDisconnectDelegate OnDisconnect;
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

        /// <summary>
        /// Connection timeout in seconds
        /// </summary>
        public int ConnectionTimeout { get; set; }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (tcpClient != null)
                tcpClient.Close();

            if (tcpListener != null)
                tcpListener.Stop();

            Stop();
        }

        #endregion
    }
}
