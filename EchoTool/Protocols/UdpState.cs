﻿/* 
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

using System.Net;
using System.Net.Sockets;

namespace EchoTool.Protocols
{
    /// <summary>
    /// UDP async state class
    /// </summary>
    public class UdpState
    {
        public IPEndPoint IpEndPoint;
        public UdpClient UdpClient;
    }
}
