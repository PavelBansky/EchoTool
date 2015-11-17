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
using System.Threading;

namespace EchoTool.Protocols
{
    /// <summary>
    /// Implements Udp Echo Client
    /// </summary>
    public class UdpEchoClient : IDisposable
    {
        #region Fields
        Thread _mainThread;
        bool _clientRunning;
        UdpClient _udpClient;
        #endregion

        #region Constructors
        public UdpEchoClient()
        {
            RemotePort = 7;
            LocalPort = 0;
            ResponseTimeout = 5;
            RepeatCount = 5;
            EchoPattern = Encoding.ASCII.GetBytes($"UDP echo from {Dns.GetHostName()}");            
        }

        public UdpEchoClient(string hostName, int remotePort, int localPort)  : this()
        {
            HostName = hostName;
            RemotePort = remotePort;
            LocalPort = localPort;
        }

        public UdpEchoClient(string hostName, int remotePort)
            : this(hostName, remotePort, 0)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Start the echoing thread
        /// </summary>
        public void Start()
        {
            if (_mainThread == null)
            {
                _mainThread = new Thread(ClientThread);
                _clientRunning = true;
                _mainThread.Start();
            }
            else
                throw new Exception("Echo client thread is already running.");
        }

        /// <summary>
        /// Stop the echoing thread
        /// </summary>
        public void Stop()
        {           
            EndClientThread();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Main thread method
        /// </summary>
        private void ClientThread()
        {                        
            IPEndPoint responseEndPoint = null;
            IPEndPoint serverEndPoint;
            var loopCount = (RepeatCount == 0) ? 1 : RepeatCount - 1;

            // Resolve server IP end point
            _clientRunning = GetHostnameEndPoint(out serverEndPoint);

            #region Main loop
            while (_clientRunning && loopCount >= 0)
            {
                try
                { 
                    // Do we have local port assigned?
                    _udpClient = (LocalPort > 0) ? new UdpClient(LocalPort) : new UdpClient();

                    // Send data
                    _udpClient.Send(EchoPattern, EchoPattern.Length, serverEndPoint);

                    // Get the start time
                    var echoStart = DateTime.Now;
                    _udpClient.Client.ReceiveTimeout = ResponseTimeout * 1000;
                    var responseData = _udpClient.Receive(ref responseEndPoint);

                    // Raise event if registered
                    if (OnEchoResponse != null)
                    {
                        var echoTime = DateTime.Now - echoStart;
                        OnEchoResponse(responseEndPoint, echoTime, Utils.CompareByteArrays(EchoPattern, responseData));
                    }
                }
                catch (SocketException socketException)
                {
                    // Raise event if registered
                    OnSocketException?.Invoke(socketException);
                }
                finally
                {
                    _udpClient.Close();
                }

                // Infinite test
                if (RepeatCount > 0)
                    loopCount--;

                Thread.Sleep(100);
            }
            #endregion

            // End up thread legaly
            _clientRunning = false;
            EndClientThread();
        }

        /// <summary>
        /// Ends up client thread in legal way
        /// </summary>
        private void EndClientThread()
        {
            var abort = false;

            if (_mainThread != null && _clientRunning)
            {
                abort = true;
                _clientRunning = false;
                _mainThread.Abort();                
            }
            
            _mainThread = null;

            // Raise event if registered
            OnFinish?.Invoke(abort);
        }

        /// <summary>
        /// Resolves hostname to IP Address
        /// </summary>
        /// <param name="serverEndPoint">Server IP end point</param>
        /// <returns>True if resolve is successfull</returns>
        private bool GetHostnameEndPoint(out IPEndPoint serverEndPoint)
        {
            serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
            bool resolveSuccess;

            try
            {
                // Find suitable IP address
                var addressList = Dns.GetHostAddresses(HostName);               
                foreach(var ipAddress in addressList)
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        serverEndPoint.Address = ipAddress;
                        break;
                    }

                serverEndPoint.Port = RemotePort;
                resolveSuccess = true;

                // Raise event if registered
                OnHostnameResolved?.Invoke(serverEndPoint);
            }
            catch (SocketException socketException)
            {
                // Raise event if registered
                OnSocketException?.Invoke(socketException);

                resolveSuccess = false;
            }

            return resolveSuccess;
        }
        #endregion

        #region Events & Delegates
        public delegate void SocketExceptionDelegate(SocketException socketException);
        public delegate void EchoResponseDelegate(IPEndPoint responseIpEndPoint, TimeSpan echoTime, bool dataComplete);
        public delegate void HostnameResolvedDelegate(IPEndPoint hostnameIpEndPoint);
        public delegate void FinishDelegate(bool abort);

        /// <summary>
        /// Occures when socket exceptions is thrown
        /// </summary>
        public event SocketExceptionDelegate OnSocketException;
        /// <summary>
        /// Occures when response from server arrives
        /// </summary>
        public event EchoResponseDelegate OnEchoResponse;
        /// <summary>
        /// Occures when echoing is ended
        /// </summary>
        public event FinishDelegate OnFinish;
        /// <summary>
        /// Occures when hostanme is resolved to IP
        /// </summary>
        public event HostnameResolvedDelegate OnHostnameResolved;
        #endregion

        #region Properties
        /// <summary>
        /// Remote port
        /// </summary>
        public int RemotePort { get; set; }

        /// <summary>
        /// Local to port to use.
        /// Value 0 will use the port assigned by winsock
        /// </summary>
        public int LocalPort { get; set; }

        /// <summary>
        /// Timeout to wait for server response in seconds
        /// </summary>
        public int ResponseTimeout { get; set; }

        /// <summary>
        /// Times to repeat the transaction
        /// </summary>
        public int RepeatCount { get; set; }

        /// <summary>
        /// Echo server hostname
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Pattern for echo
        /// </summary>
        public byte[] EchoPattern { get; set; }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}