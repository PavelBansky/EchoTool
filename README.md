EchoTool - Echo client and server
=================================

Command line echo server and client for Windows. This tool is designed according to [RFC 862 specification]([http://www.ietf.org/rfc/rfc0862.txt?number=862]) for Echo protocol. It can operate as a echo server that sends back every incoming data. In client mode, it sends data to the server and checks whether they came back. This is a useful debugging tool for application development or network throughput checks. Application is written in C# and source codes is provided.

This software needs .NET 4.0 installed to run. It is however 32bit and 64bit compatible. One exe to rule them all.

Double click the echotoolgui.exe to simply start the gui version, or use the cmd version as mentioned below.

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
                


## Download  ##
Stand alone executable Commandline only
[releases/download/v1.7.0-alpha.1/echotool.exe](https://github.com/mitoskalandiel/EchoTool/releases/download/v1.7.0-alpha.1/echotool.exe) [30 Kb]

Stand alone executable GUI (needs the cmd download aswell)
[releases/download/v1.7.0-alpha.1/EchoToolGui.exe](https://github.com/mitoskalandiel/EchoTool/releases/download/v1.7.0-alpha.1/EchoToolGui.exe) [7 Kb]
