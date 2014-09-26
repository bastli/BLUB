using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using BLUBmotion;
using System.Diagnostics;



// TCP example found in http://tech.pro/tutorial/704/csharp-tutorial-simple-threaded-tcp-server

// In this tutorial I'm going to show you how to build a threaded tcp server with C#. If you've ever worked with Window's sockets, you know how difficult this can sometimes be. However, thanks to the .NET framework, making one is a lot easier than it used to be.
// What we'll be building today is a very simple server that accepts client connections and can send and receive data. The server spawns a thread for each client and can, in theory, accept as many connections as you want (although in practice this is limited because you can only spawn so many threads before Windows will get upset).
// Let's just jump into some code. Below is the basic setup for our TCP server class.

namespace TCPServerTutorial
{
	public class TCPServer
	{

		// listening TCP port
		private int tcp_port = 3000;

		private TcpListener tcpListener;
		private Thread listenThread;

		private bool listen = true;

		public TCPServer()
		{
			this.tcpListener = new TcpListener(IPAddress.Any, tcp_port);
			this.listenThread = new Thread(new ThreadStart(ListenForClients));
			this.listenThread.Start();
		}

		public void Close()
		{
			listen = false;
			this.tcpListener.Stop();
		}

		// So here's a basic server class - without the guts. We've got a TcpListener which does a good job of wrapping up the underlying socket communication, and a Thread which will be listening for client connections. You might have noticed the function ListenForClients that is used for our ThreadStart delegate. Let's see what that looks like.
		private void ListenForClients()
		{
			this.tcpListener.Start();

			while (listen)
			{
				TcpClient client = null;
				//blocks until a client has connected to the server
				try
				{
					System.Diagnostics.Debug.WriteLine("[TCP Server] waiting for client to connect... ");
					client = this.tcpListener.AcceptTcpClient();
					System.Diagnostics.Debug.WriteLine("[TCP Server] Client connected. ");



				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine("[TCP Server] Listener closed:" + ex.Message);
				};

				//create a thread to handle communication 
				//with connected client
				if (client != null)
				{
					Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
					clientThread.Start(client);
				}
			}
		}
		// This function is pretty simple. First it starts our TcpListener and then sits in a loop accepting connections. The call to AcceptTcpClient will block until a client has connected, at which point we fire off a thread to handle communication with our new client. I used a ParameterizedThreadStart delegate so I could pass the TcpClient object returned by the AcceptTcpClient call to our new thread.

		// The function I used for the ParameterizedThreadStart is called HandleClientComm. This function is responsible for reading data from the client. Let's have a look at it.
		private void HandleClientComm(object client)
		{
			TcpClient tcpClient = (TcpClient)client;
			NetworkStream clientStream = tcpClient.GetStream();

			DateTime timer = DateTime.Now;

			byte[] message = new byte[4096];
			int bytesRead;

			// for debugging only
			bool clientSleeping = false; 

			// name for the SourceSwitcher
			string sourcename = null;

			BLUBstate telegram = new BLUBstate();
			BLUBstate lastTelegram = new BLUBstate();

			bool isActive = false;

			while (true)
			{
				bytesRead = 0;
				int ret;
				byte msg = 0;

				for (int i = 0; i < telegram.numValves; i++)
				{
					lastTelegram.setValve(i, telegram.getValve(i));
				}
				for (int i = 0; i < telegram.numLEDs; i++)
				{
					lastTelegram.setLED(i, telegram.getLED(i));
				}

				try
				{

					//bytesRead = clientStream.Read(message, 0, 4096);
					ret = clientStream.ReadByte();

					if (ret != -1)
					{
						msg = (byte)ret;
						//System.Diagnostics.Debug.WriteLine("byte read: " + msg.ToString());
					}
				}
				catch
				{
					//a socket error has occured
					break;
				}

				//if (bytesRead == 0)
				//{
				//	//the client has disconnected from the server
				//	break;
				//}

				//message has successfully been received

				// debug: write received message to console
				//ASCIIEncoding encoder = new ASCIIEncoding();
				//System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead));

				// communication protocol definition:
				// Messages sent from the Client to BLUB consist of a start byte and data bytes, concluded by a newline (TODO check what exactly, CR, LF whatever)
				//
				// Messages in <tags> are bytes, everythin else is ASCII.
				// start bytes can be:
				// c	connect.
				//	syntax: c name_of_your_client - will be shown in the GUI
				// l	set LED N° <nr> to Value <val>.
				//	syntax: l<nr><val>
				// v	set Valve N° <nr> to Value <val>.
				//	<0> and 0 will be interpreted as closed valve, everything else as open.
				//	syntax: v<nr><val>
				// L	set a line of LEDs to Values <val1> to <valn>.
				//	syntax: L<val1><val2><val3>...<valn>
				// V	set a line of valves to Values <val1> to <valn>.
				//	syntax: V<val1><val2><val3>...<valn>
				// x	reserved for (future) multi-byte commands

				// reserved falls Zeit, muss auch noch genau definiert werden...: 
				// 0	set all values to 0
				// r	start ring buffer recording
				// R	end ring buffer recording
				// p	play values from ring buffer
				// P	stop playing values from ring buffer

				// BLUB may send any messages back. For the moment, they shall be ignored. 


				ASCIIEncoding encoder = new ASCIIEncoding();
				//string startbyte = encoder.GetString(message, 0, 1);
				//string databytes = encoder.GetString(message, 1, bytesRead - 1);

				// LeDebug
				//System.Diagnostics.Debug.WriteLine("received message: ");
				//System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead));


				//switch (startbyte) {
				switch (msg.ToString())
				{
					case "99": //"c"
						//System.Diagnostics.Debug.WriteLine("case c found ");
						string name = "";
						do
						{
							ret = clientStream.ReadByte();
							if (ret != -1)
							{
								msg = (byte)ret;
								if (Convert.ToChar(msg) != '\n')
								{
									name += (char)msg;
									//System.Diagnostics.Debug.WriteLine(msg);
								}
								else
								{

									break;
								}

							} // else it was -1.

						} while (true);


						//if(databytes != ""){
						if (name != "")
						{
							System.Diagnostics.Debug.WriteLine("[TCP Server] Client connected with name " + name);
							BLUBmotion.SourceSwitcher.unregisterSource(name);
						}
						telegram.isActive = true;

						// WARNING hack ahead:
						// currently all apps connecting via TCP get the highest priority. 
						// This should be changed in the protocol and then adapted here... 
						sourcename = BLUBmotion.SourceSwitcher.registerSource(name);
						telegram.priority = 1; 
						break;

					case "76": //"L":
						//System.Diagnostics.Debug.WriteLine("case L found. ");
						clientStream.Read(message, 0, telegram.numLEDs);
						for (int i = 0; i < telegram.numLEDs; i++)
						{
							if (message[i] != null)
							{
								telegram.setLED(i, message[i]);
							}
						}
						telegram.isActive = true;


						// debug only
						if (clientSleeping)
							System.Diagnostics.Debug.WriteLine("[TCP Server] client awake. ");


						break;

					//case "l":
					//    System.Diagnostics.Debug.WriteLine("case l found. ");
					//    if ((int)message[1] < telegram.numLEDs)
					//        telegram.setLED((int)message[1], message[2]);
					//    break;

					case "86"://"V":
						//System.Diagnostics.Debug.WriteLine("case V found. ");
						clientStream.Read(message, 0, telegram.numValves);
						for (int i = 0; i < telegram.numValves; i++)
						{
							if (message[i] != null)
								//Check for ASCII as well as binary zero
								if (encoder.GetString(message, i, 1) == "0" || message[i] == 0)
									telegram.setValve(i, false);
								else
									telegram.setValve(i, true);
						}
						telegram.isActive = true;


						// debug only
						if (clientSleeping)
						{
							System.Diagnostics.Debug.WriteLine("[TCP Server] client awake. ");
							clientSleeping = false;
						}




						break;

					case "116"://"t":
						System.Diagnostics.Debug.WriteLine("[TCP Server] client sleeping. ");
						clientSleeping = true; // leDebug
						telegram.isActive = false;
						break;

					//case "v":
					//    System.Diagnostics.Debug.WriteLine("case v found. ");
					//    if ((int)message[1] < telegram.numValves)
					//        //Check for ASCII as well as binary zero
					//        if (encoder.GetString(message, 2, 1) == "0" || message[2] == 0)
					//            telegram.setValve((int)message[1], false);
					//        else
					//            telegram.setValve((int)message[1], true);
					//    break;

				}

		

				

				if (sourcename != null)
				{
					//System.Diagnostics.Debug.WriteLine("Write to: " + sourcename);
					SourceSwitcher.put(sourcename, telegram);
				}

				// leDebug
				//System.Diagnostics.Debug.WriteLine("Data Bytes are: ");
				//System.Diagnostics.Debug.WriteLine(databytes);


				// uncomment next 3 lines to send something back.
				//byte[] buffer = encoder.GetBytes("Hello Client!");
				//clientStream.Write(buffer, 0 , buffer.Length);
				//clientStream.Flush();


			}
			if (sourcename != null)
				SourceSwitcher.unregisterSource(sourcename);
			// TODO your cleanup code here
			tcpClient.Close();
		}

		// The first thing we need to do is cast client as a TcpClient object since the ParameterizedThreadStart delegate can only accept object types. Next, we get the NetworkStream from the TcpClient, which we'll be using to do our reading. After that we simply sit in a while true loop reading information from the client. The Read call will block indefinitely until a message from the client has been received. If you read zero bytes from the client, you know the client has disconnected. Otherwise, a message has been successfully received from the server. In my example code, I simply convert the byte array to a string and push it to the debug console. You will, of course, do something more interesting with the data - I hope. If the socket has an error or the client disconnects, you should call Close on the TcpClient object to free up any resources it was using.

		// Believe it or not, that's pretty much all you need to do to create a threaded server that accepts connections and reads data from clients. 



		// However, a server isn't very useful if it can't send data back, so let's look at how to send data to one of our connected clients.
		//NetworkStream clientStream = tcpClient.GetStream();
		//ASCIIEncoding encoder = new ASCIIEncoding();
		//byte[] buffer = encoder.GetBytes("Hello Client!");

		//clientStream.Write(buffer, 0 , buffer.Length);
		//clientStream.Flush();

		// Do you remember the TcpClient object that was returned from the call AcceptTcpClient? Well, that's the object we'll be using to send data back to that client. That being said, you'll probably want to keep those objects around somewhere in your server. I usually keep a collection of TcpClient objects that I can use later. Sending data to connected clients is very simple. All you have to do is call Write on the the client's NetworkStream object and pass it the byte array you'd like to send.

		// Your TCP server is now finished. The hard part is defining a good protocol to use for sending information between the client and server. Application level protocols are generally unique for application, so I'm not going to go into any details - you'll just have to invent you're own.


		////////////////////////////////////////////////////////////////////////////


		// But what use is a server without a client to connect to it? This tutorial is mainly about the server, but here's a quick piece of code that shows you how to set up a basic TCP connection and send it a piece of data.

		/*
		TcpClient client = new TcpClient();

		IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);

		client.Connect(serverEndPoint);

		NetworkStream clientStream = client.GetStream();

		ASCIIEncoding encoder = new ASCIIEncoding();
		byte[] buffer = encoder.GetBytes("Hello Server!");

		clientStream.Write(buffer, 0 , buffer.Length);
		clientStream.Flush();
		 */

		// The first thing we need to do is get the client connected to the server. We use the TcpClient.Connect method to do this. It needs the IPEndPoint of our server to make the connection - in this case I connect it to localhost on port 3000. I then simply send the server the string "Hello Server!".

		// One very important thing to remember is that one write from the client or server does not always equal one read on the receiving end. For instance, your client could send 10 bytes to the server, but the server may not get all 10 bytes the first time it reads. Using TCP, you're pretty much guaranteed to eventually get all 10 bytes, but it might take more than one read. You should keep that in mind when designing your protocol.

		// That's it! Now get out there and clog the tubes with your fancy new C# TCP servers.
	}

}
