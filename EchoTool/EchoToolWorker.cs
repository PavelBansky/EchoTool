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
using EchoTool.Modes;

namespace EchoTool
{
    /// <summary>
    /// Implements main EchoToolCMD functionality
    /// </summary>
    public class EchoToolWorker
    {
        readonly Arguments _arguments;

        /// <summary>
        /// Creates new echo tool instance
        /// </summary>
        /// <param name="args"></param>
        public EchoToolWorker(string[] args)
        {
            _arguments = new Arguments(args);
        }

        /// <summary>
        /// Runs the echo tool
        /// </summary>
        public void Run()
        {
            Console.WriteLine();

            if (!DoWork(_arguments))
            {
                ShowHelp();
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Parse the arguments and starts the show
        /// </summary>
        /// <param name="arguments">Commandline arguments</param>
        /// <returns>False if show can not start</returns>
        private bool DoWork(Arguments arguments)
        {
            IEchoMode mainMode;

            // Parse protocol
            var strProtocol = arguments.Get("/p", string.Empty).ToLower();
            bool isProtocolUdp;

            if (strProtocol == "udp")
                isProtocolUdp = true;
            else if (strProtocol == "tcp")
                isProtocolUdp = false;
            else
                return false;

            // Should we run server mode
            if (arguments.Exists("/s"))
            {
                // UDP server or TCP mode
                if (isProtocolUdp)
                    mainMode = new UdpServerMode(arguments);
                else
                    mainMode = new TcpServerMode(arguments);
            }
            // or client mode
            else if (!string.IsNullOrEmpty(arguments.FirstArgument) && !arguments.FirstArgument.StartsWith("/"))
            {
                // UDP client or TCP mode
                if (isProtocolUdp)
                    mainMode = new UdpClientMode(arguments);
                else
                    mainMode = new TcpClientMode(arguments);
            }
            // no no, read the help
            else
                return false;

            if (mainMode.ParseArguments())
                mainMode.Run();
            else
                return false;

            return true;
        }

        /// <summary>
        /// Writes help screen
        /// </summary>
        private void ShowHelp()
        {
            Console.WriteLine("EchoTool for Dicom testing. Please use from commandline with the following Parameters!");
            Console.WriteLine("Usage: echotool [target_name] [/p protocol] [/s listen_port] [/r remote_port]\n[/l local_port] [/n count] [/t timeout] [/d echo_pattern] [/s [listen_port]]\n");
            Console.WriteLine("Options:");            
            Console.WriteLine("\t/p protocol\t tcp or udp");
            Console.WriteLine("\t/s [port]\t Server mode on specified port");
            Console.WriteLine("\t/r port\t\t Remote port on the echo server");
            Console.WriteLine("\t/l port\t\t Local port for client");
            Console.WriteLine("\t/n count\t Nummber of echo requests to send. 0 = infinite");
            Console.WriteLine("\t/t timeout\t Timeout in seconds");
            Console.WriteLine("\t/d pattern\t Pattern to be sent for echo");            
        }
    }
}
