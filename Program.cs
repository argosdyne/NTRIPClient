using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ntrip
{
    class Program
    {
		//static string NTRIP_Caster = "RTS2.ngii.go.kr";
		//static string NTRIP_Caster = "gnss.eseoul.go.kr";
		static string NTRIP_Caster = "gnssdata.or.kr";

		static void Main(string[] args)
        {
			string responseData = string.Empty;

			var IP_addresses = Dns.GetHostEntry(NTRIP_Caster);
			var ntripCaster = IP_addresses.AddressList[0];
			var ntripCasterEP = new IPEndPoint(ntripCaster, 2101);

			var ntripSocket = CreateSocket();
			//var requestBytes = CreateRequest("SSR-SSRG", "Argos2018", "ngii");
			var requestBytes = CreateRequest("YYAN-RTCM32", "gnss", "gnss");

			ntripSocket.Connect(ntripCasterEP);

			ntripSocket.Send(requestBytes);

			while (true)
			{
				var nData = ntripSocket.Available;

				Console.WriteLine($"NUM = {nData}");
				if (nData > 0)
                {
					byte[] rxData = new byte[nData];
					ntripSocket.Receive(rxData); //Get response
					responseData += System.Text.Encoding.ASCII.GetString(rxData, 0, nData);
                    Console.WriteLine(responseData);
				}
				else
                {
					System.Threading.Thread.Sleep(100); //Wait for response
				}
			}
		}

		static private byte[] CreateRequest(string strRequest, string username, string password)
		{
			//Build request message
			string msg = "GET /" + strRequest + " HTTP/1.1\r\n";
			//string msg = "GET /" + " HTTP/1.1\r\n";
			msg += "Host: " + NTRIP_Caster + ":2101\r\n";
			msg += "Ntrip-Version: Ntrip/2.0\r\n";
			msg += "User-Agent: NTRIP BirdCom/2023\r\n";
			msg += "Accept: */*\r\n";
			msg += "Connection: close\r\n";
			//If password/username is specified, send login details
			if (string.IsNullOrEmpty(username) == false && string.IsNullOrEmpty(password) == false)
			{
				string auth = ToBase64(username + ":" + password);
				msg += "Authorization: Basic " + auth + "\r\n";
			}

			msg += "\r\n";

			Console.WriteLine(msg);

			return System.Text.Encoding.ASCII.GetBytes(msg);
		}

		static private Socket CreateSocket()
		{
			return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}


		static private string ToBase64(string str)
		{
			Encoding asciiEncoding = Encoding.ASCII;
			byte[] byteArray = new byte[asciiEncoding.GetByteCount(str)];
			byteArray = asciiEncoding.GetBytes(str);
			return Convert.ToBase64String(byteArray, 0, byteArray.Length);
		}
	}
}
