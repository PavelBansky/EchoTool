EchoTool - Echo client and server
=================================

Command line echo server and client for Windows. This tool is designed according to [RFC 862 specification]([http://www.ietf.org/rfc/rfc0862.txt?number=862]) for Echo protocol. It can operate as a echo server that sends back every incoming data. In client mode, it sends data to the server and checks whether they came back. This is a useful debugging tool for application development or network throughput checks. Application is written in C# and source codes is provided.

- Server mode
- Client mode
- TCP and UDP protocol support
- Selectable destination and source port
- Selectable timeout
- Selectable echo pattern
- Just one file

For server mode listening on UDP port 4578 run following command
                
	C:\EchoTool> echotool /p udp /s 4578
				
On client machine run this

	C:\EchoTool> echotool server.to-test.com /p udp /r 4578
				
You can specify outgoing local port by /l switch

	C:\EchoTool> echotool server.to-test.com /p udp /r 4578 /l 8976
				
Number of attempts and timeouts can be set by /n and /t switch

	C:\EchoTool> echotool server.to-test.com /p udp /r 4578 /l 8976 /n 100 /t 10

Use your own echo pattern with /d switch

	C:\EchoTool> echotool server.to-test.com /p udp /r 4578 /d Hello
                

Echo server mode
![server mode image](http://bansky.net/echotool/echo_server.png)

Echo client mode
![client mode image](http://bansky.net/echotool/echo_client.png)

## Download  ##
Stand alone executable
[http://bansky.net/echotool/echotool.exe](http://bansky.net/echotool/echotool.exe) [30 Kb]