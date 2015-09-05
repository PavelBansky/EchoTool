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
    /// Implements Udp Echo Client
    /// </summary>
    public class UdpEchoClient : IDisposable
    {
        #region Fields
        Thread mainThread = null;
        bool clientRunning = false;
        UdpClient udpClient;
        #endregion

        #region Constructors
        public UdpEchoClient()
        {
            this.RemotePort = 7;
            this.LocalPort = 0;
            this.ResponseTimeout = 5;
            this.RepeatCount = 5;
            this.EchoPattern = Encoding.ASCII.GetBytes(string.Format("UDP echo from {0}", Dns.GetHostName()));            
        }

        public UdpEchoClient(string hostName, int remotePort, int localPort)  : this()
        {
            this.HostName = hostName;
            this.RemotePort = remotePort;
            this.LocalPort = localPort;
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
            if (mainThread == null)
            {
                mainThread = new Thread(new ThreadStart(ClientThread));
                clientRunning = true;
                mainThread.Start();
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
            byte[] responseData = null;
            int loopCount = (RepeatCount == 0) ? 1 : RepeatCount - 1;

            // Resolve server IP end point
            clientRunning = GetHostnameEndPoint(out serverEndPoint);

            #region Main loop
            while (clientRunning && loopCount >= 0)
            {
                try
                { 
                    // Do we have local port assigned?
                    udpClient = (LocalPort > 0) ? new UdpClient(LocalPort) : new UdpClient();

                    // Send data
                    udpClient.Send(EchoPattern, EchoPattern.Length, serverEndPoint);

                    // Get the start time
                    DateTime echoStart = DateTime.Now;
                    udpClient.Client.ReceiveTimeout = ResponseTimeout * 1000;
                    responseData = udpClient.Receive(ref responseEndPoint);

                    // Raise event if registered
                    if (OnEchoResponse != null)
                    {
                        TimeSpan echoTime = DateTime.Now - echoStart;
                        OnEchoResponse(responseEndPoint, echoTime, Utils.CompareByteArrays(EchoPattern, responseData));
                    }
                }
                catch (SocketException socketException)
                {
                    // Raise event if registered
                    if (OnSocketException != null)
                        OnSocketException(socketException);
                }
                finally
                {
                    udpClient.Close();
                }

                // Infinite test
                if (RepeatCount > 0)
                    loopCount--;

                Thread.Sleep(100);
            }
            #endregion

            // End up thread legaly
            clientRunning = false;
            EndClientThread();
        }

        /// <summary>
        /// Ends up client thread in legal way
        /// </summary>
        private void EndClientThread()
        {
            bool abort = false;

            if (mainThread != null && clientRunning)
            {
                abort = true;
                clientRunning = false;
                mainThread.Abort();                
            }
            
            mainThread = null;

            // Raise event if registered
            if (OnFinish != null)
                OnFinish(abort);
        }

        /// <summary>
        /// Resolves hostname to IP Address
        /// </summary>
        /// <param name="serverEndPoint">Server IP end point</param>
        /// <returns>True if resolve is successfull</returns>
        private bool GetHostnameEndPoint(out IPEndPoint serverEndPoint)
        {
            serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
            bool resolveSuccess = false;

            try
            {
                // Find suitable IP address
                IPAddress[] addressList = Dns.GetHostAddresses(HostName);               
                foreach(IPAddress ipAddress in addressList)
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        serverEndPoint.Address = ipAddress;
                        break;
                    }

                serverEndPoint.Port = RemotePort;
                resolveSuccess = true;

                // Raise event if registered
                if (OnHostnameResolved != null)
                    OnHostnameResolved(serverEndPoint);
            }
            catch (SocketException socketException)
            {
                // Raise event if registered
                if (OnSocketException != null)
                    OnSocketException(socketException);

                resolveSuccess = false;
            }

            return resolveSuccess;
        }
        #endregion

        #region Events & Delegates
        public delegate void SocketExceptionDelegate(SocketException socketException);
        public delegate void EchoResponseDelegate(IPEndPoint responseIPEndPoint, TimeSpan echoTime, bool dataComplete);
        public delegate void HostnameResolvedDelegate(IPEndPoint hostnameIPEndPoint);
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