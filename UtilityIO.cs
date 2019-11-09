using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml.Serialization;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace LungMorphApp
{
	public static class UtilityIO
	{
		public static string[] getFiles(this string SourceFolder, string Filter, SearchOption searchOption)
		{
			var alFiles = new System.Collections.ArrayList();
			string[] MultipleFilters = Filter.Split('|');
			foreach (string FileFilter in MultipleFilters) {
				alFiles.AddRange(Directory.GetFiles(SourceFolder, FileFilter, searchOption));
			}
			return (string[])alFiles.ToArray(typeof(string));
		}
		//public static T DeepCopy<T>(T other)
		//{
		//	using (var ms = new MemoryStream()) {
		//		var formatter = new BinaryFormatter();
		//		formatter.Serialize(ms, other);
		//		ms.Position=0;
		//		return (T)formatter.Deserialize(ms);
		//	}
		//}

		//public static async Task<bool> Save(this UISettings ui, string filename = "LastSaved.xsetting")
		//{
		//	ui.ShowBusySign("Saving Settings...");
		//	bool result = await Task<bool>.Factory.StartNew(() => {
		//		try {
		//			using (FileStream fsUserSetting = File.Create(filename)) {
		//				var formatter = new XmlSerializer(ui.GetType());
		//				formatter.Serialize(fsUserSetting, ui);
		//			}
		//			return true;
		//		} catch { MessageBox.Show("Failed to save the current settings!", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return false; }
		//	});
		//	ui.StopBusySign(); return result;
		//}
		public static async Task<bool> SaveCrypt(this UISettings ui, string filename = "LastSaved.xsetting")
		{
			ui.ShowBusySign("Saving Settings...");
			try {
				await Task.Factory.StartNew(() => {
					var aUE = new UnicodeEncoding();
					byte[] key = aUE.GetBytes("password");
					var RMCrypto = new RijndaelManaged();
					using (var fs = File.Open(filename, FileMode.Create)) {
						using (var cs = new CryptoStream(fs, RMCrypto.CreateEncryptor(key, key), CryptoStreamMode.Write)) {
							var xml = new XmlSerializer(ui.GetType());
							xml.Serialize(cs, ui);
						}
					}
				});
				return true;
			} catch {
				MessageBox.Show("Failed to save the current settings!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			} finally { ui.StopBusySign(); }
		}
		//public static async Task<bool> Load(this UISettings ui, string filename = "LastSaved.xsetting")
		//{
		//	ui.ShowBusySign("Loading Settings...");
		//	bool result = await Task<bool>.Factory.StartNew(() => {
		//		try {
		//			var deserializer = new XmlSerializer(typeof(UISettings));
		//			using (var reader = new StreamReader(filename)) {
		//				object obj = deserializer.Deserialize(reader);
		//				ui.UpdateAll((UISettings)obj);
		//			}
		//			if (MessageBox.Show($"Yes: Update the project name to the current date. => {DateTime.Now:yyyy-MM-dd}\nNo: Use the previouly saved. => {ui.ProjectName}", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)==MessageBoxResult.Yes) {
		//				ui.ProjectName=$"{DateTime.Now:yyyy-MM-dd}";
		//			}
		//			return true;
		//		} catch { MessageBox.Show("Failed to load the selected settings!", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return false; }
		//	});
		//	ui.StopBusySign(); return result;
		//}
		public static async Task<bool> LoadCrypt(this UISettings ui, string filename = "LastSaved.xsetting")
		{
			ui.ShowBusySign("Loading Settings...");
			try {
				await Task.Factory.StartNew(() => {
					var deserializer = new XmlSerializer(typeof(UISettings));
					using (var fs = new FileStream(filename, FileMode.Open)) {
						using (var sr = new StreamReader(fs)) {
							var aUE = new UnicodeEncoding();
							byte[] key = aUE.GetBytes("password");
							using (var RMCrypto = new RijndaelManaged()) {
								using (var cs = new CryptoStream(fs, RMCrypto.CreateDecryptor(key, key), CryptoStreamMode.Read)) {
									object obj = deserializer.Deserialize(cs);
									ui.UpdateAll((UISettings)obj);
								}
							}
						}
					}
				});
				if (Path.GetFileNameWithoutExtension(filename).Substring(0, 1)=="~") {
					ui.WorkDirectory=Directory.GetCurrentDirectory()+"\\"+Path.GetFileNameWithoutExtension(filename).Substring(1);
					ui.ProjectName=$"{DateTime.Now:yyyy-MM-dd}";
				} else if ((ui.ProjectName!=$"{DateTime.Now:yyyy-MM-dd}")&&(MessageBox.Show($"Yes: Update the project name to the current date. => {DateTime.Now:yyyy-MM-dd}\nNo: Use the previouly saved. => {ui.ProjectName}", "Would you like to update project name to the current date?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)==MessageBoxResult.Yes))
					ui.ProjectName=$"{DateTime.Now:yyyy-MM-dd}";
				return true;
			} catch {
				MessageBox.Show("Failed to load the selected settings!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			} finally { ui.StopBusySign(); }
		}

		//public static async Task<bool> Save(this DockingManager dockingManager, UISettings ui, string filename = "LastSaved.xlayout")
		//{
		//	ui.ShowBusySign("Saving Settings...");
		//	try {
		//		var serializer = new XmlLayoutSerializer(dockingManager);
		//		using (var stream = new StreamWriter(filename)) { serializer.Serialize(stream); }
		//		ui.StopBusySign(); return true;
		//	} catch { ui.StopBusySign(); MessageBox.Show("Failed to save the current layout!", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return false; }
		//}
		public static async Task<bool> SaveCrypt(this DockingManager dockingManager, UISettings ui, string filename = "LastSaved.xlayout")
		{
			ui.ShowBusySign("Saving Settings...");
			try {
				var aUE = new UnicodeEncoding();
				byte[] key = aUE.GetBytes("password");
				var RMCrypto = new RijndaelManaged();
				using (var fs = File.Open(filename, FileMode.Create)) {
					using (var cs = new CryptoStream(fs, RMCrypto.CreateEncryptor(key, key), CryptoStreamMode.Write)) {
						var serializer = new XmlLayoutSerializer(dockingManager);
						serializer.Serialize(cs);
					}
				}
				return true;
			} catch {
				MessageBox.Show("Failed to save the current layout!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			} finally { ui.StopBusySign(); }
		}

		//public static async Task<bool> Load(this DockingManager dockingManager, UISettings ui, string filename = "LastSaved.xlayout")
		//{
		//	ui.ShowBusySign("Saving Settings...");
		//	try {
		//		var serializer = new XmlLayoutSerializer(dockingManager);
		//		using (var stream = new StreamReader(filename)) { serializer.Deserialize(stream); }
		//		ui.StopBusySign(); return true;
		//	} catch { ui.StopBusySign(); MessageBox.Show("Failed to load the selected layout!", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return false; }
		//}
		public static async Task<bool> LoadCrypt(this DockingManager dockingManager, UISettings ui, string filename = "LastSaved.xlayout")
		{
			ui.ShowBusySign("Saving Settings...");
			try {
				var deserializer = new XmlLayoutSerializer(dockingManager);
				using (var fs = new FileStream(filename, FileMode.Open)) {
					using (var sr = new StreamReader(fs)) {
						var aUE = new UnicodeEncoding();
						byte[] key = aUE.GetBytes("password");
						using (var RMCrypto = new RijndaelManaged()) {
							using (var cs = new CryptoStream(fs, RMCrypto.CreateDecryptor(key, key), CryptoStreamMode.Read)) {
								deserializer.Deserialize(cs);
							}
						}
					}
				}
				return true;
			} catch {
				MessageBox.Show("Failed to load the selected layout!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			} finally { ui.StopBusySign(); }
		}

		public static bool ValidateSystemTime(string ExpirationDate)
		{
			try {
				return string.Compare($"{DateTime.Now:yy-MM}", ExpirationDate, StringComparison.Ordinal)<=0;
			} catch {
				//throw new Exception("Could not acccess current system time.");
				return false;
			}
		}
		public static bool ValidateInternetTimeNtp(string ExpirationDate)
		{
			string date = "50-12"; //2050-12
			foreach (string ntpServer in new string[] { "time.nist.gov", "time.windows.com" }) {
				try {
					var ntpData = new byte[48];// NTP message size - 16 bytes of the digest (RFC 2030)    
														//Setting the Leap Indicator, Version Number and Mode values
					ntpData[0]=0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)
					var addresses = Dns.GetHostEntry(ntpServer).AddressList;
					var ipEndPoint = new IPEndPoint(addresses[0], 123); //The UDP port number assigned to NTP is 123    
					var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//NTP uses UDP
					socket.Connect(ipEndPoint);
					socket.ReceiveTimeout=2000; //Stops code hang if NTP is blocked
					socket.Send(ntpData);
					socket.Receive(ntpData);
					socket.Close();
					//Offset to get to the "Transmit Timestamp" field (time at which the reply 
					//departed the server for the client, in 64-bit timestamp format."
					const byte serverReplyTime = 40;
					ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime); //Get the seconds part    
					ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime+4); //Get the seconds fraction
																												//Convert From big-endian to little-endian
					intPart=SwapEndianness(intPart);
					fractPart=SwapEndianness(fractPart);
					var milliseconds = (intPart*1000)+((fractPart*1000)/0x100000000L);
					//**UTC** time
					var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);
					date=networkDateTime.ToLocalTime().ToString("yy-MM");
					return (string.Compare(date, ExpirationDate, StringComparison.Ordinal)<=0);
				} catch { }
			}
			return false;
		}
		static uint SwapEndianness(ulong x)
		{
			 return (uint) (((x & 0x000000ff) << 24) +
								 ((x & 0x0000ff00) << 8) +
								 ((x & 0x00ff0000) >> 8) +
								 ((x & 0xff000000) >> 24));
		}

		
		public static bool ValidateInternetTimeHttp(string ExpirationDate)
		{
			string yyMM = "50-12"; // 2050-12
			try {
				//DateTime dateTime = DateTime.MinValue;
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://nist.time.gov/actualtime.cgi?lzbc=siqm9b");
				request.Method="GET";
				request.Accept="text/html, application/xhtml+xml, */*";
				request.UserAgent="Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
				request.ContentType="application/x-www-form-urlencoded";
				request.CachePolicy=new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore); //No caching
                request.ReadWriteTimeout=2000; //milliseconds
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();
				if (response.StatusCode==HttpStatusCode.OK) {
					StreamReader stream = new StreamReader(response.GetResponseStream());
					string html = stream.ReadToEnd();//<timestamp time=\"1395772696469995\" delay=\"1395772696469995\"/>
					string time = System.Text.RegularExpressions.Regex.Match(html, @"(?<=\btime="")[^""]*").Value;
					double milliseconds = Convert.ToInt64(time)/1000.0;
					yyMM=new DateTime(1970, 1, 1).AddMilliseconds(milliseconds).ToString("yy-MM");
				}
				return string.Compare(yyMM, ExpirationDate, StringComparison.Ordinal)<=0;
			} catch { return false; }
		}
	}


	/// <summary>
	/// TcpClientWithTimeout is used to open a TcpClient connection, with a 
	/// user definable connection timeout in milliseconds (1000=1second)
	/// Use it like this:
	/// TcpClient connection = new TcpClientWithTimeout('127.0.0.1',80,1000).Connect();
	/// </summary>
	public class TcpClientWithTimeout
	{
		protected string _hostname;
		protected int _port;
		protected int _timeout_milliseconds;
		protected TcpClient connection;
		protected bool connected;
		protected Exception exception;

		public TcpClientWithTimeout(string hostname, int port, int timeout_milliseconds)
		{
			_hostname=hostname;
			_port=port;
			_timeout_milliseconds=timeout_milliseconds;
		}
		public TcpClient Connect()
		{
			// kick off the thread that tries to connect
			connected=false;
			exception=null;
			Thread thread = new Thread(new ThreadStart(BeginConnect));
			thread.IsBackground=true; // So that a failed connection attempt 
											  // wont prevent the process from terminating while it does the long timeout
			thread.Start();

			// wait for either the timeout or the thread to finish
			thread.Join(_timeout_milliseconds);

			if (connected==true) {
				// it succeeded, so return the connection
				thread.Abort();
				return connection;
			}
			if (exception!=null) {
				// it crashed, so return the exception to the caller
				thread.Abort();
				throw exception;
			} else {
				// if it gets here, it timed out, so abort the thread and throw an exception
				thread.Abort();
				string message = string.Format("TcpClient connection to {0}:{1} timed out",
				  _hostname, _port);
				throw new TimeoutException(message);
			}
		}
		protected void BeginConnect()
		{
			try {
				connection=new TcpClient(_hostname, _port);
				// record that it succeeded, for the main thread to return to the caller
				connected=true;
			} catch (Exception ex) {
				// record the exception for the main thread to re-throw back to the calling code
				exception=ex;
			}
		}
	}


	[ValueConversion(typeof(bool), typeof(bool))]
	public class InverseBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(bool)value;
		}

	}
}
