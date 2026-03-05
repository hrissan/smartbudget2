using System;
using System.Net;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace SmartBudgetCommon
{
	public class WebClientChinese
	{
		public static IDispatchOnUIThread dispatch;

		enum State {UNKNOWN, SUCCESS, FAILED};
		private static State direct_state = State.UNKNOWN;
		private static State proxy_state = State.UNKNOWN;
		private static List<WebClientChinese> waiting_chinese = new List<WebClientChinese>();
		private static WebClientChinese check_direct;
		private static WebClientChinese check_proxy;
		private static string check_string;

		static WebClientChinese() {
			RNGCryptoServiceProvider p = new RNGCryptoServiceProvider ();
			byte[] bytes = new byte[9];
			p.GetBytes (bytes);
			check_string = SmartBudgetiOS.Utility.urlsafe_b64encode(bytes);
		}

		const string dhost = "smartbudgetapp2.appspot.com";
		const string phost = "smartbudgetapp.com/cloud2";

		public static void kick_requests()
		{
			bool kick = false;
			if (direct_state == State.SUCCESS || proxy_state == State.SUCCESS)
				kick = true;
			if (direct_state == State.FAILED && proxy_state == State.FAILED)
				kick = true;
			Console.WriteLine("WebClientChinese.kick_requests - direct={0} proxy={1} kick={2}", direct_state, proxy_state, kick);
			if (!kick)
				return;
			//if (check_direct != null) {
			//	check_direct.CancelAsync ();
			//	check_direct = null;
			//}
			//if (check_proxy != null) {
			//	check_proxy.CancelAsync ();
			//	check_proxy = null;
			//}
			List<WebClientChinese> now_waiting = waiting_chinese;
			waiting_chinese = new List<WebClientChinese> ();
			foreach (var ch in now_waiting) {
				ch.UploadValuesAsync(ch.address, ch.method, ch.values);
			}
		}

		public static void start_check_direct(){
			if (direct_state == State.SUCCESS)
				return;
			Console.WriteLine("WebClientChinese - start_check_direct");
			if (check_direct != null) {
				check_direct.CancelAsync ();
				check_direct = null;
			}
			check_direct = new WebClientChinese ();
			check_direct.client.Headers["Accept"] = "*/*";
			check_direct.client.Headers["Pragma"] = "no-cache";
			check_direct.UploadValuesCompleted += (sender, e) => {
				check_direct = null;
				direct_state = State.FAILED;
				try{
					if( e.Result != null && Encoding.UTF8.GetString(e.Result) == check_string ) {
						direct_state = State.SUCCESS;
					}
				}catch(Exception){
					// decoding error, ok
				}
				kick_requests();
			};
			NameValueCollection nvc = new NameValueCollection ();
			nvc ["body"] = check_string;
			check_direct.UploadValuesAsync("https://"+dhost+"/check.php", "POST", nvc, true);
		}
		public static void start_check_proxy(){
			if (proxy_state == State.SUCCESS)
				return;
			Console.WriteLine("WebClientChinese - start_check_proxy");
			if (check_proxy != null) {
				check_proxy.CancelAsync ();
				check_proxy = null;
			}
			check_proxy = new WebClientChinese ();
			check_proxy.client.Headers["Accept"] = "*/*";
			check_proxy.client.Headers["Pragma"] = "no-cache";
			check_proxy.UploadValuesCompleted += (sender, e) => {
				check_proxy = null;
				proxy_state = State.FAILED;
				try{
					if( e.Result != null && Encoding.UTF8.GetString(e.Result) == check_string ) {
						proxy_state = State.SUCCESS;
					}
				}catch(Exception){
					// decoding error, ok
				}
				kick_requests();
			};
			NameValueCollection nvc = new NameValueCollection ();
			nvc ["body"] = check_string;
			check_proxy.UploadValuesAsync("https://"+phost+"/check.php", "POST", nvc, true);
		}
		public class ChineseUploadValuesEventArgs : EventArgs {
			public byte[] Result;
			public ChineseUploadValuesEventArgs(byte[] res)
			{
				this.Result = res;
			}
		};
		public delegate void ChineseUploadValuesHandler (WebClient sender, ChineseUploadValuesEventArgs e);
		public event ChineseUploadValuesHandler UploadValuesCompleted;
		private bool already_invoked;
		//public event UploadDataCompletedEventHandler UploadDataCompleted;
		public WebClient client = new WebClient();
		private string address;
		private string method;
		private NameValueCollection values;
		public WebClientChinese ()
		{
			client.UploadValuesCompleted += (sender, e) => {
				//Console.WriteLine("UploadValuesCompleted 1");
				//Thread.Sleep(2000);
				//Console.WriteLine("UploadValuesCompleted 2");
				dispatch.Invoke( delegate{
					if( !already_invoked ) {
						already_invoked = true;
						//Console.WriteLine("UploadValuesCompleted Invoke Good");
						if( UploadValuesCompleted != null )
							UploadValuesCompleted.Invoke(client, new ChineseUploadValuesEventArgs(e.Cancelled || e.Error != null ? null : e.Result));
					}
				});
			};
		}
		//public void UploadDataAsync (Uri address, string method, byte[] data)
		//{
		//}
		public void UploadValuesAsync (string address, string method, NameValueCollection values)
		{
			UploadValuesAsync(address, method, values, false);
		}
		private void UploadValuesAsync (string address, string method, NameValueCollection values, bool force_direct)
		{
			if (force_direct || direct_state == State.SUCCESS || (direct_state == State.FAILED && proxy_state == State.FAILED) ) {
				this.address = null;
				this.method = null;
				this.values = null;
				//Console.WriteLine("UploadValuesAsync DIRECT");
				client.UploadValuesAsync(new Uri(address), method, values);
				return;
			}
			if (proxy_state == State.SUCCESS) {
				this.address = null;
				this.method = null;
				this.values = null;
				//Console.WriteLine("UploadValuesAsync PROXY");
				client.UploadValuesAsync(new Uri(address.Replace(dhost, phost)), method, values);
				return;
			}
			this.address = address;
			this.method = method;
			this.values = values;
			waiting_chinese.Add (this);
		}
		public void CancelAsync()
		{
			//Console.WriteLine("CancelAsync");
			if( !already_invoked ) {
				already_invoked = true;
				if (UploadValuesCompleted != null) {
					//Console.WriteLine("UploadValuesCompleted Invoke Cancel");
					UploadValuesCompleted.Invoke(client, new ChineseUploadValuesEventArgs(null));
				}
			}
			if (this.address == null) {
				client.CancelAsync ();
				return;
			}
			this.address = null;
			this.method = null;
			this.values = null;
			waiting_chinese.Remove(this);
		}
	}
}

