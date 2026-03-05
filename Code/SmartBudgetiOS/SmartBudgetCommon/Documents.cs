using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Globalization;
using System.Json;
using System.Text;
using System.Collections.Specialized;
using SmartBudgetiOS;
using Foundation;

namespace SmartBudgetCommon
{
	public class Documents
	{
		public List<Document> documents = new List<Document>();
		public List<string> new_documents = new List<string> ();
		public Document selected_doc { get; private set; }
		public int loading_operations_in_progress;
		private string last_stat_settings_v;
		public string last_stat_settings { get {
				return last_stat_settings_v;
			}
			set{
				last_stat_settings_v = value;
				save_settings ();
			}
		}
		//public List<Stream> load_data_streams = new List<Stream>();
		public class DocumentChangeEventArgs : EventArgs {
			public Object originator;
			public DocumentChangeEventArgs(Object originator)
			{
				this.originator = originator;
			}
		};
		public delegate void DocumentChangedHandler (Documents docs, DocumentChangeEventArgs e);

		private DocumentChangedHandler private_anything_changed;
		private int private_anything_changed_counter;
		public event DocumentChangedHandler anything_changed
		{
			add { private_anything_changed += value; private_anything_changed_counter +=1; }
			remove { private_anything_changed -= value; private_anything_changed_counter -= 1; }
		}
		public event DocumentChangedHandler another_loaded;
		public void send_anything_changed(Object originator)
		{
			Console.WriteLine ("private_anything_changed_counter={0}", private_anything_changed_counter);
			if (private_anything_changed != null)
				private_anything_changed (this, new DocumentChangeEventArgs(originator));
		}
		public void send_another_loaded(Object originator)
		{
			if (another_loaded != null)
				another_loaded (this, new DocumentChangeEventArgs(originator));
		}

		public const string exchange_rates_filename = "exchange.txt";
		public const string settings_filename = "settings.txt";
		public const string loading_tmp_name = "loading.tmp";
		private const string NO_OPEN_DOC_MARKER = "null"; // Never coincides with filename
		private static Random rnd = new Random ();

		private string full_version_receipt; // in urlsafe base64
		private string full_version_r_token;
		public bool full_version { 
			get {
					return !String.IsNullOrEmpty (full_version_receipt);
				}
			}
		public void set_full_version_receipt(byte[] receipt)
		{
			full_version_receipt = Convert.ToBase64String (receipt);
			full_version_r_token = "";
			save_settings ();
		}
		public void switch_fake_full_version() {
			if (full_version)
				set_full_version_receipt(new byte[0]);
			else
				set_full_version_receipt(Encoding.UTF8.GetBytes("MagicReceipt"));
		}
		public const int SHAREWARE_LIMIT = 30;
		public int get_shareware_progress(Document doc)
		{
			if (full_version)
				return 0;
			if (doc == null)
				return 0;
			var pla = doc.get_expenses_for_sheet ("", true, null);
			var notpla = doc.get_expenses_for_sheet ("", false, null);
			return pla.Count + notpla.Count;
		}

		// local_cur can be null for generic english locale
		public Documents (string local_cur)
		{
			AppDelegate.ReportTime("Documents.Documents 1");
			// read settings
			selected_currencies = new List<string> ();
			string last_open_doc = "";
			try {
				string settings_json = File.ReadAllText (get_db_path(settings_filename), System.Text.Encoding.UTF8);
				JsonObject settings = (JsonObject)JsonObject.Parse(settings_json);
				AppDelegate.ReportTime("Documents.Documents 1.1");
				full_version_receipt = JSONHelper.read_string(settings, "full_version_receipt", "");
				full_version_r_token = JSONHelper.read_string(settings, "full_version_r_token", "");
				last_open_doc = JSONHelper.read_string(settings, "last_open_doc", "");
				report_currency = JSONHelper.read_string(settings, "report_currency", "");
				short_balance = JSONHelper.read_bool(settings, "short_balance", short_balance);
				new_expense_date = JSONHelper.read_long(settings, "new_expense_date", 0);
				new_expense_date_set_utc = new DateTime(JSONHelper.read_long(settings, "new_expense_date_set_utc", 0), DateTimeKind.Utc);
				last_stat_settings_v = JSONHelper.read_string(settings, "last_stat_settings", "");

				AppDelegate.ReportTime("Documents.Documents 1.2");
				JsonArray jcur = (JsonArray)settings["selected_currencies"];
				foreach(JsonValue s in jcur)
					selected_currencies.Add(s);
				JsonArray jndoc = (JsonArray)settings["new_documents"];
				foreach(JsonValue s in jndoc)
					new_documents.Add(s);

			}catch(Exception ex) {
				Console.WriteLine ("Settings did not parse {0}",ex.Message);
			}
			AppDelegate.ReportTime("Documents.Documents 1.3");
			// select some currencies on first launch
			if (selected_currencies.Count == 0) {
				if (local_cur != null)
					selected_currencies.Add (local_cur);
				if (selected_currencies.IndexOf ("USD") == -1)
					selected_currencies.Add ("USD");
				if (selected_currencies.IndexOf ("EUR") == -1)
					selected_currencies.Add ("EUR");
			}
			if ( String.IsNullOrEmpty(report_currency))
				report_currency = next_selected_currency ("", "");
			// set up popular currencies
			popular_currencies = new List<string> () { "AUD", "CAD", "CHF", "CNY", "EUR", "GBP", "HKD", "JPY", "USD" };
			if (local_cur != null && popular_currencies.IndexOf (local_cur) == -1) {
				popular_currencies.Add (local_cur);
				popular_currencies.Sort ();
			}
			AppDelegate.ReportTime("Documents.Documents 2");
			foreach(var fileName in System.IO.Directory.EnumerateFiles(get_dbs_path ()))
			{
				System.IO.FileInfo fi = new System.IO.FileInfo (fileName);
				if( fi.Extension == ".db3" )
				{
					documents.Add (new Document( fi.DirectoryName, fi.Name ));
					if (fi.Name == last_open_doc)
						selected_doc = documents [documents.Count-1];
				}
			}
			AppDelegate.ReportTime("Documents.Documents 3");
			update_new_documents ();
			// Load here
			if( documents.Count == 0 && last_open_doc != NO_OPEN_DOC_MARKER ) // We create new doc only on first launch
			{
				string error_text;
				Document dd = create_standalone_doc (out error_text);
				add_standalone_doc (dd);
				selected_doc = dd;
				new_documents.Clear (); // first launch
				save_settings ();
			}
			//if( doc == null && documents.Count > 00 )
			//	doc = documents [0];
			AppDelegate.ReportTime("Documents.Documents 3.5");
			start_exchange_rate_update();
			AppDelegate.ReportTime("Documents.Documents 4");
		}
		void update_new_documents ()
		{
			for (int i = 0; i != new_documents.Count; ++i) {
				string nd = new_documents [i];
				if (documents.FindIndex (d => d.file_name == nd) == -1) {
					new_documents.RemoveAt (i);
					i -= 1;
					continue;
				}
			}
		}
		public void save_settings()
		{
			JsonArray jcur = new JsonArray ();
			foreach (string s in selected_currencies)
				jcur.Add (s);
			JsonArray jndoc = new JsonArray ();
			foreach (string s in new_documents)
				jndoc.Add (s);
			JsonObject jsettings = new JsonObject{
				{"full_version_receipt",full_version_receipt},
				{"full_version_r_token",full_version_r_token},
				{"selected_currencies",jcur},
				{"new_documents",jndoc},
				{"last_open_doc",selected_doc == null ? NO_OPEN_DOC_MARKER : selected_doc.file_name},
				{"report_currency",report_currency},
				{"last_stat_settings",last_stat_settings_v},
				{"short_balance",short_balance},
				{"new_expense_date", new_expense_date}, 
				{"new_expense_date_set_utc", new_expense_date_set_utc.Ticks} };
			string settings_string = jsettings.ToString ();
			try {
				File.WriteAllText (get_db_path(settings_filename), settings_string, System.Text.Encoding.UTF8);
			}catch(Exception ex){
				Console.WriteLine("Failed to write settings {0}",ex.Message);
			}
		}
		public long new_expense_date;
		public DateTime new_expense_date_set_utc;
		public bool recently_selected_new_expense_date() {
			return DateTime.UtcNow.Subtract (new_expense_date_set_utc).TotalHours < 1;
		}
		public static bool planned_soon(long planned_date) {
			if (planned_date == 0)
				return false;
			double days = new DateTime (planned_date, DateTimeKind.Local).Subtract (DateTime.Now).TotalDays;
			return days < 3;
		}
		public Document create_standalone_doc(out string error_text) // null to create empty file
		{
			Document dd = null;
			try {
				FileInfo tmpfi = new FileInfo( Path.Combine(get_dbs_path(), loading_tmp_name));
				tmpfi.Delete ();
				dd = new Document( get_dbs_path(), loading_tmp_name);
				error_text = "";
				return dd;
			}catch(Exception ex) {
				if( dd != null ) {
					dd.conn.Dispose (); // We do not want stuck open databases
					dd.conn = null;
				}
				error_text = ex.Message;
			}
			//error_text = "LOAD_FROM_BACKUP_BAD_FORMAT";
			return null; // Tell user that bad thing happened
		}
		public bool add_standalone_doc(Document dd)
		{
			if( !String.IsNullOrEmpty(dd.a_token) ){
				int ind = documents.FindIndex( d => d.a_token == dd.a_token );
				if( ind != -1 ) { //exists
					// TODO - somehow tell user he has this already
					return false;
				}
			}		
			string rndpart = "";
			while(true)
			{
				string datepart = DateTime.UtcNow.ToString ("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
				if (dd.try_rename_file(get_dbs_path(), datepart + rndpart + ".db3")) {
					documents.Add(dd);
					new_documents.Add (dd.file_name);
					save_settings ();
					dd.need_to_sync (true);
					dd.write_to_icloud (1);
					//send_anything_changed(null);
					return true;
				}
				rndpart = rnd.Next (65536).ToString("X4", CultureInfo.InvariantCulture);
			}
		}
		public bool remove_doc(Document dd, object originator)
		{
//			if (documents.Count < 2)
//				return false;
			int ind = documents.IndexOf (dd);
			if (ind == -1)
				return false;
			string was_name = dd.file_name;
			string del_name = dd.file_name + ".erased";
			try {
				FileInfo delfi = new FileInfo( Path.Combine(get_dbs_path(), del_name));
				delfi.Delete ();
			}catch(Exception ex){
				Console.WriteLine("Failed to remove previously erased file {0}",ex.Message);
				return false;
			}
			if (!dd.try_rename_file(get_dbs_path(), del_name))
				return false;
			documents.RemoveAt (ind);
			new_documents.Remove (was_name);
			dd.write_to_icloud (0);
			if (selected_doc == dd) {
				//if (ind < documents.Count)
				//	select_document (documents [ind], originator);
				//else if (documents.Count != 0)
				//	select_document( documents [documents.Count-1], originator);
				//else
				select_document (null, originator);
			}
			cancel_sync_if_unpublished ();
			//send_anything_changed(originator);
			//send_another_loaded (originator);
			return true;
		}
		public void select_document(Document dd, Object originator)
		{
			if( dd != selected_doc )
			{
				selected_doc = dd;
				if (selected_doc != null)
					new_documents.Remove (selected_doc.file_name);
				//send_another_loaded(originator);
				//send_anything_changed(originator);
				save_settings ();
			}
		}
		public void execute_change(Change ch, Object originator = null)
		{
			ch.get_doc().execute_change_no_event (ch);
			send_anything_changed (originator);
			sync_some_document (false, originator);
		}
		public void arrange_account(Document doc, int from, int to, int mhal, Object originator = null)
		{
			doc.arrange_account_no_event (from, to, mhal);
			send_anything_changed (originator);
			sync_some_document (false, originator);
		}
		public void execute_move_from_sheet(Document doc, string sheet_id, string to_sheet_id, Object originator = null)
		{
			doc.execute_move_from_sheet(sheet_id, to_sheet_id);
			send_anything_changed (originator);
			sync_some_document (false, originator);
		}	
		public void execute_move_from_account(Document doc, string account_id, string to_account_id, Object originator = null)
		{
			doc.execute_move_from_account(account_id, to_account_id);
			send_anything_changed (originator);
			sync_some_document (false, originator);
		}
		public void execute_delete_from_sheet(Document doc, string sheet_id, Object originator = null)
		{
			doc.execute_delete_from_sheet(sheet_id);
			send_anything_changed (originator);
			sync_some_document (false, originator);
		}
		public List<string> selected_currencies;
		public string next_selected_currency(string cur, string skip_cur)
		{
			List<string> sc = new List<string> (selected_currencies);
			sc.Remove (skip_cur);
			if (sc.Count == 0)// Those who require currency other then skip_cur will get some good guess
				return skip_cur != "USD" ? "USD" : "EUR";
			int ind = sc.IndexOf (cur);
			if (ind == -1)
				return sc [0];
			return sc [(ind + 1) % sc.Count];
		}
		private List<string> popular_currencies;
		public List<string> get_popular_currencies()
		{
			// Select all currencies from SBA
			List<string> pc = new List<String> (popular_currencies);
			foreach(var d in documents)
			foreach(string c in d.get_used_currencies())
			{
				if( pc.IndexOf(c) == -1 )
					pc.Add (c);
			}
			pc.Sort ();
			return pc;
		}
		private WebClientChinese exchange_rate_client;
		public bool exchange_rate_update_on(out DateTime date)
		{
			System.IO.FileInfo fi = new System.IO.FileInfo(get_db_path(exchange_rates_filename));
			if (!fi.Exists) {
				date = DateTime.UtcNow;
				return false;
			}
			date = fi.LastWriteTimeUtc;
			return true;
		}
		public void start_exchange_rate_update()
		{
			System.IO.FileInfo fi = new System.IO.FileInfo(get_db_path(exchange_rates_filename));
			if( fi.Exists && Math.Abs( fi.LastWriteTimeUtc.Subtract(DateTime.UtcNow).TotalHours ) < 12 )
			{
				Console.WriteLine("Decided to NOT update exchange rates");
				return;
			}
			if (exchange_rate_client != null)
				return;
			exchange_rate_client = new WebClientChinese ();
			exchange_rate_client.UploadValuesCompleted += (sender, e) => {
				Console.WriteLine("DownloadDataCompleted exchange rates");
				if( e.Result != null )
					parse_and_save_exchange_rates( System.Text.Encoding.UTF8.GetString(e.Result) );
					exchange_rate_client = null;
			};
			NameValueCollection nvc = new NameValueCollection ();
			nvc["base"]="USD";
			exchange_rate_client.UploadValuesAsync ("http://smartbudgetapp2.appspot.com/quotes.php", "POST", nvc);
		}
		private Dictionary<string, decimal> exchange_rates;
		public string report_currency;
		public bool short_balance = true;
		private bool parse_exchange_rates(string str)
		{
			var lines = str.Split ("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			foreach (string line in lines) {
				var comp = line.Split (",".ToCharArray(), StringSplitOptions.None);
				if (comp.Length != 2 || comp [0].Length != 3)
					return false;
				decimal rate = 0;
				if (!Decimal.TryParse (comp[1], NumberStyles.Float, CultureInfo.InvariantCulture, out rate))
					continue;
				exchange_rates [comp[0]] = rate;
			}
			return true;
		}
		private void parse_and_save_exchange_rates(string str)
		{
			exchange_rates = new Dictionary<string, decimal> ();
			//Console.WriteLine("parse_and_save_exchange_rates {0}", str);
			if( parse_exchange_rates (str) )
			{
				try {
					File.WriteAllText (get_db_path(exchange_rates_filename), str, System.Text.Encoding.UTF8);
				}catch(Exception ex){
					Console.WriteLine("Failed to write exchange rates {0}",ex.Message);
				}
			}
		}
		public decimal get_exchange_rate(string iso_symbol)
		{
			if (exchange_rates == null) {
				exchange_rates = new Dictionary<string, decimal> ();
				try {
					string str = File.ReadAllText (get_db_path(exchange_rates_filename), System.Text.Encoding.UTF8);
					parse_exchange_rates (str);
				}catch(Exception ex){
					Console.WriteLine("Failed to read exchange rates {0}",ex.Message);
				}
			}
			decimal result = 0;
			exchange_rates.TryGetValue (iso_symbol, out result);
			return result;
		}
		public bool convert_currency(ref long result_1000, string to, long amount_1000, string from)
		{
			if( from == to )
			{
				result_1000 = amount_1000;
				return true;
			}
			decimal from_e = get_exchange_rate (from);
			decimal to_e = get_exchange_rate (to);
			if( to == null || to_e == 0 || from_e == 0 )
				return false;
			result_1000 = (long)(amount_1000 * to_e / from_e);
			return true;
		}
		public static string get_db_path(string filename)
		{ 
			return Path.Combine (get_dbs_path(), filename);
		}
		public static string get_dbs_path()
		{ 
			#if SILVERLIGHT
			return "";
			#elif __ANDROID__
			return libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); ;
			#else
			// we need to put in /Library/ on iOS5.1 to meet Apple's iCloud terms
			// (they don't want non-user-generated data in Documents)
			string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal); // Documents folder
			string libraryPath = Path.Combine (documentsPath, "..", "Library");
			return libraryPath;
			#endif
		}
		public static string get_caches_path()
		{ 
			#if SILVERLIGHT
			return ???;
			#elif __ANDROID__
			return libraryPath = ???;
			#else
			// we need to put in /Caches/ on iOS5.1 to meet Apple's iCloud terms
			// (they don't want non-user-generated data in Documents)
			string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal); // Documents folder
			string libraryPath = Path.Combine(Path.Combine (documentsPath, "..", "Library"), "Caches");
			return libraryPath;
			#endif
		}
		// Server part
		private WebClientChinese sync_loader;
		private Document sync_doc;
		public void sync_some_document(bool force, Object originator)
		{
			really_sync_some_document (force, originator); // Comment this to test conflict resolutions
		}
		public void cancel_sync_if_unpublished()
		{
			if( sync_doc != null && (!sync_doc.is_published() || documents.IndexOf(sync_doc) == -1) ) {
				sync_loader.CancelAsync ();
				// Delegate will fire and make everything needed
			}
		}
		public Document get_syncing_document()
		{
			return sync_doc;
		}
		public void synchronize_icloud_changes()
		{
			bool local_list_modified = false;
			bool need_update_set = false;
			Dictionary<string, Document> our_tokens = new Dictionary<string, Document> ();
			for (int i = 0; i != documents.Count; i += 1) {
				var dd = documents [i];
				if( String.IsNullOrEmpty(dd.a_token) )
					continue;
				NSDictionary dic = NSUbiquitousKeyValueStore.DefaultStore.GetDictionary (Document.a_token_icloud_prefix + dd.a_token);
				if (dic == null) {
					Console.WriteLine ("iCloud publish {0}", dd.a_token);
					dd.write_to_icloud (1);
					our_tokens.Add (dd.a_token, dd);
					continue;
				}
				NSNumber in_cloud = dic.ValueForKey ((NSString)"in_cloud") as NSNumber;
				if (in_cloud == null) { // bad, put back into iCloud :)
					Console.WriteLine ("iCloud publish bad {0}", dd.a_token);
					dd.write_to_icloud (1);
					our_tokens.Add (dd.a_token, dd);
					continue;
				}
				if (in_cloud.NIntValue != 0) {
					long last_change_id = NSUbiquitousKeyValueStore.DefaultStore.GetLong(Document.last_change_id_icloud_prefix + dd.a_token);
					if( dd.last_change_id != last_change_id ) {
						Console.WriteLine ("iCloud update last_change_id {0} local={1} cloud={2}", dd.a_token, dd.last_change_id, last_change_id);
						dd.need_to_sync(true);
						need_update_set = true;
					}
					our_tokens.Add (dd.a_token, dd);
					continue;
				}
				// remove from cloud
				Console.WriteLine ("iCloud remove local {0}", dd.a_token);
				local_list_modified = true;
				remove_doc (dd, null);
				i -= 1;
			}
			NSDictionary icd = NSUbiquitousKeyValueStore.DefaultStore.ToDictionary();
			foreach (var icdk in icd) {
				string key = icdk.Key as NSString;
				NSDictionary value = icdk.Value as NSDictionary;
				if (key == null || value == null || !key.StartsWith (Document.a_token_icloud_prefix))
					continue;
				string a_token = key.Substring (Document.a_token_icloud_prefix.Length);
				NSNumber in_cloud = value.ValueForKey ((NSString)"in_cloud") as NSNumber;
				string password_b64 = value.ValueForKey ((NSString)"e_key_b64") as NSString;
				string password_version = value.ValueForKey ((NSString)"cypher") as NSString;
				if (in_cloud == null || in_cloud.NIntValue == 0 || password_b64 == null || password_version == null)
					continue; // Removed from cloud or bad
				if (our_tokens.ContainsKey (a_token))
					continue;
				Console.WriteLine ("iCloud open {0}", a_token);
				string error_text;
				Document our_doc = create_standalone_doc (out error_text);
				our_doc.open_from_icloud (a_token, password_b64, password_version);
				local_list_modified = true;
				need_update_set = true;
				add_standalone_doc(our_doc);
			}
			if (local_list_modified) {
				send_anything_changed (null);
			}
			if (need_update_set) {
				sync_some_document (false, null);
			}
		}
		private void really_sync_some_document(bool force, Object originator)
		{
			if (force) {
				foreach (Document dd in documents) {
					dd.need_to_sync (true);
				}
			}
			if (sync_loader != null)
				return;
			foreach (Document dd in documents) {
				if (!dd.need_to_sync (false))
					continue;
				NameValueCollection nvc = dd.prepare_sync_request (full_version_receipt, full_version_r_token);
				if (nvc == null) { // Future cypher or other error
					send_anything_changed(originator);
					continue;
				}
				sync_doc = dd;
				sync_loader = new WebClientChinese();
				send_anything_changed(originator);

				sync_loader.client.Headers["Accept"] = "*/*";
				sync_loader.client.Headers["Pragma"] = "no-cache";
				sync_loader.UploadValuesCompleted += (sender, e) => {
					Document sdd = sync_doc;
					sync_doc = null;
					sync_loader = null;
					if( e.Result == null ){
						sdd.update_status = Document.UpdateStatus.ERROR_NETWORK;
					}else{
						string str = Encoding.UTF8.GetString(e.Result);
						if( str.Length > 300 )
							str = str.Substring(0, 300);
						Console.WriteLine("Server response: {0}", str);
						try {
							string was_r = full_version_receipt;
							string was_t = full_version_r_token;
							if( sdd.parse_sync(e.Result, ref full_version_receipt, ref full_version_r_token) ){
								//if( sdd == doc)
								//	send_anything_changed(null);
							}
							if( was_r != full_version_receipt || was_t != full_version_r_token ) {
								// Version becomes less or more full :)
								save_settings();
							}
						}catch(Exception ex){
							FlurryAnalytics.Flurry.LogEvent("Exception_parse_sync", NSDictionary.FromObjectsAndKeys(new object[]{ex.GetType().FullName, ex.Message, ex.Source}, new object[]{"FullName", "Message", "Source"}));
							Console.WriteLine ("Network error during sync {0}", ex.Message);
							sdd.update_status = Document.UpdateStatus.ERROR_NETWORK;
						}
					}
					send_anything_changed(null);
					really_sync_some_document(false, null); // Chain reaction on both success and failure
				};
				sync_loader.UploadValuesAsync ("https://smartbudgetapp2.appspot.com/sbs.php", "POST", nvc);
//				sync_loader.UploadValuesAsync ("http://localhost:10080/sbs.php", "POST", nvc);
				break;
			}
		}
	}
}

