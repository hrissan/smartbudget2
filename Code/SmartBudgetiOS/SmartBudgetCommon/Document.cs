using System;
using System.Collections.Generic;
using System.Json;
using System.Text;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Specialized;
using SmartBudgetiOS;
using Foundation;
using Mono.Security.Cryptography;
using System.Threading;
using SQLite;
using System.IO.Compression;

namespace SmartBudgetCommon
{
	public class Document
	{
		public SQLite.SQLiteConnection conn;
		public string file_name { get; private set; }
		public string file_folder { get; private set; }
		public int commands_size { get; private set; }

		// Server part begin
		private int last_change_commands_size_v;
		public int last_change_commands_size { get {
				return last_change_commands_size_v;
			} 
			private set{
				last_change_commands_size_v = value;
				write_to_key_value ("last_change_commands_size", last_change_commands_size_v.ToString (CultureInfo.InvariantCulture));
			}
		}
/*		private int commands_sent_size_v;
		public int commands_sent_size { get {
				return commands_sent_size_v;
			} 
			private set{
				commands_sent_size_v = value;
				write_to_key_value ("commands_sent_size", commands_sent_size_v.ToString (CultureInfo.InvariantCulture));
			}
		}*/
		private int last_change_id_v;
		public int last_change_id { get {
				return last_change_id_v;
			} 
			private set{
				last_change_id_v = value;
				write_to_key_value ("last_change_id", last_change_id_v.ToString (CultureInfo.InvariantCulture));
			}
		}
/*		private string commands_sent_data_v;
		public string commands_sent_data { get {
				return commands_sent_data_v;
			} 
			private set{
				commands_sent_data_v = value;
				write_to_key_value ("commands_sent_data", commands_sent_data_v);
			}
		}*/
		private string e_key_b64_v;
		public string e_key_b64 { get {
				return e_key_b64_v;
			} 
			private set{
				e_key_b64_v = value;
				write_to_key_value ("e_key_b64", e_key_b64_v);
			}
		}
		private string cypher_v;
		public string cypher { get {
				return cypher_v;
			} 
			private set{
				cypher_v = value;
				write_to_key_value ("cypher", cypher_v);
			}
		}
		private string a_token_v;
		public string a_token { get {
				return a_token_v;
			} 
			private set{
				a_token_v = value;
				write_to_key_value ("a_token", a_token_v);
			}
		}
		private string server_status_v;
		public string server_status { get {
				return server_status_v;
			} 
			private set{
				server_status_v = value;
				write_to_key_value ("server_status", server_status_v);
			}
		}
		public enum UpdateStatus
		{
			UPDATED, NEED_UPDATE, ERROR_NETWORK
		};
		public UpdateStatus update_status = UpdateStatus.UPDATED;
		public const string SERVER_STATUS_AUTH_FAILED = "AUTH_FAILED";
		public const string SERVER_STATUS_CORRUPTED = "CORRUPTED";
		public const string SERVER_STATUS_FUTURE_COMMANDS = "FUTURE_COMMANDS";
		//public const string SERVER_STATUS_BAD_RECEIPT = "BAD_RECEIPT";
		public const string SERVER_STATUS_SYNC = "SYNC";
		public const string SERVER_STATUS_BAD_RECEIPT = "BAD_RECEIPT";
		const int MAX_SERVER_COMMIT_SIZE = 100000;
		const string CYPHER_V1 = "AES-128";
		const string CYPHER_V2 = "zip-AES-128";
		const int CYPHER_V1_BLOCKSIZE = 16;
		private byte[] zip(byte[] data)
		{
			using (var ms = new MemoryStream())
			{
				using (var ds = new DeflateStream(ms, CompressionMode.Compress))
				{
					ds.Write(data, 0, data.Length);
				}
				return ms.ToArray();
			}
		}
		private byte[] unzip(byte[] data)
		{
			using (var ms = new MemoryStream())
			{
				using (var cp = new MemoryStream(data))
				using (var ds = new DeflateStream(cp, CompressionMode.Decompress))
				{
					ds.CopyTo (ms);
				}
				return ms.ToArray();
			}
		}
		private byte[] encrypt(byte[] data)
		{
			if (cypher == CYPHER_V2)
				return encrypt_aes128 (zip (data));
			if (cypher == CYPHER_V1)
				return encrypt_aes128 (data);
			return null;
		}
		private byte[] decrypt(byte[] data)
		{
			if (cypher == CYPHER_V2)
				return unzip(decrypt_aes128 (data));
			if (cypher == CYPHER_V1)
				return decrypt_aes128 (data);
			return null;
		}
		private byte[] encrypt_aes128(byte[] data)
		{
			byte[] IV = KeyBuilder.IV (CYPHER_V1_BLOCKSIZE);
			byte[] key = Convert.FromBase64String (e_key_b64);

			AesManaged am = new AesManaged();
			am.Padding = PaddingMode.PKCS7;
			am.KeySize = CYPHER_V1_BLOCKSIZE*8;
			am.Key = key;
			am.IV = IV;
			ICryptoTransform encryptor = am.CreateEncryptor ();
			MemoryStream result = new MemoryStream ();
			CryptoStream csEncrypt = new CryptoStream (result, encryptor, CryptoStreamMode.Write);
			result.Write (IV, 0, IV.Length);
			csEncrypt.Write (data, 0, data.Length);
			csEncrypt.FlushFinalBlock ();
			byte[] res = result.ToArray();
			//byte[] res2 = CCCrypto.aes_128_encrypt (data, key, IV);
			//if (!DBCategory.ByteArrayEqualityComparer.SEquals (res, res2))
			//	throw new Exception ("encrypt: AesManaged != CCCrypt");
			return res;
		}
		private byte[] decrypt_aes128(byte[] data)
		{
			byte[] key = Convert.FromBase64String (e_key_b64);
			AesManaged am = new AesManaged();
			am.Padding = PaddingMode.PKCS7;
			am.KeySize = CYPHER_V1_BLOCKSIZE*8;
			int IV_len = CYPHER_V1_BLOCKSIZE;
			am.Key = key;
			byte[] IV = new byte[IV_len];
			Array.Copy (data, IV, IV_len);
			am.IV = IV;
			ICryptoTransform decryptor = am.CreateDecryptor ();
			MemoryStream result = new MemoryStream ();
			CryptoStream csEncrypt = new CryptoStream (result, decryptor, CryptoStreamMode.Write);
			csEncrypt.Write(data, IV_len, data.Length - IV_len);
			csEncrypt.FlushFinalBlock ();
			byte[] res = result.ToArray();
			//byte[] res2 = CCCrypto.aes_128_decrypt (data, key);
			//if (!DBCategory.ByteArrayEqualityComparer.SEquals (res, res2))
			//	throw new Exception ("decrypt: AesManaged != CCCrypt");
			return res;
		}
		public bool prepare_commit(NameValueCollection result, string full_version_receipt, string full_version_r_token) // true if changed something
		{
			if (a_token == "") {
				result ["cmd"] = "create_doc";
				Console.WriteLine ("create_doc: file_name={0}", file_name);
			} else {
				result ["cmd"] = "sync";
				result ["a_token"] = a_token;
				string last_change_str = (last_change_id == -1) ? "0" : last_change_id.ToString (CultureInfo.InvariantCulture); // update from zero if replacing data
				result ["last_change"] = last_change_str;
				Console.WriteLine ("sync: a_token={0} file_name={1} last_change={2}", a_token, file_name, last_change_str);
			}
			if (last_change_commands_size >= commands_size) // BAD or no commit
				return true;
//			if( !String.IsNullOrEmpty(commands_sent_data) )
//				return true;
			if (last_change_id != -1 ) {
				int to_send_count = commands_size - last_change_commands_size; // Always > 0
				const int MAX_COMMANDS_IN_FIRST_COMMIT = 50;
				const int MAX_COMMANDS_IN_ANY_COMMIT = 400;
				if( last_change_id == 0 && to_send_count > MAX_COMMANDS_IN_FIRST_COMMIT )
					to_send_count = MAX_COMMANDS_IN_FIRST_COMMIT;
				if (to_send_count > MAX_COMMANDS_IN_ANY_COMMIT)
					to_send_count = MAX_COMMANDS_IN_ANY_COMMIT;
				List<DBCommand> prep_commands = conn.Query<DBCommand> ("SELECT * FROM DBCommand WHERE id >= ? AND id < ? ORDER BY id", last_change_commands_size, last_change_commands_size + to_send_count);
				int total_char_size = 0;
				StringBuilder jcommands = new StringBuilder ();
				jcommands.Append ("[");
				for(int i = 0; i < prep_commands.Count; ++i)
				{
					if(i != 0)
						jcommands.Append (",");
					jcommands.Append( prep_commands[i].data );
					total_char_size += prep_commands [i].data.Length;
					if( total_char_size > MAX_SERVER_COMMIT_SIZE ){
						to_send_count = i;
						break;
					}
				}
				jcommands.Append ("]");

				//throw new IndexOutOfRangeException ("Good good good");

				byte[] original_bytes = Encoding.UTF8.GetBytes (jcommands.ToString ());
				byte[] encBytes = encrypt(original_bytes);
				if (encBytes == null) { // Future cypher in current database
					server_status = SERVER_STATUS_FUTURE_COMMANDS;
					update_status = UpdateStatus.UPDATED;
					return false;
				}
				Console.WriteLine ("Encryption: {0}->{1}", original_bytes.Length, encBytes.Length);

				string body = Utility.urlsafe_b64encode(encBytes);
				result ["body"] = body;
				result ["delta"] = to_send_count.ToString(CultureInfo.InvariantCulture);
				Console.WriteLine ("Body: length={0} to_send_count={1} r_token={2}", body.Length, to_send_count, full_version_r_token);
			}
			if (!String.IsNullOrEmpty (full_version_r_token))
				result ["r_token"] = full_version_r_token;
			else if (!String.IsNullOrEmpty (full_version_receipt)) {
				result ["receipt_data"] = full_version_receipt;
				result ["receipt_platform"] = "iOS"; // TODO - platform dependent
			}
			return true;
		}
		public bool need_commit()
		{
			return last_change_commands_size < commands_size;
		}
		public bool need_to_sync(bool force)
		{
			if( !is_published() )
				return false;
			if (force)
				update_status = UpdateStatus.NEED_UPDATE;
			if( server_status == SERVER_STATUS_SYNC && update_status == UpdateStatus.UPDATED && need_commit())
				update_status = UpdateStatus.NEED_UPDATE;
			return update_status == UpdateStatus.NEED_UPDATE;
		}
		public string get_cloud_problem_text(bool only_bad_problems)
		{
			if( update_status == Document.UpdateStatus.ERROR_NETWORK )
				return only_bad_problems ? "" : "CheckInternetCloud";
			if( server_status == Document.SERVER_STATUS_CORRUPTED )
				return "DocsHintCloudCorrupted20";
			if( server_status == Document.SERVER_STATUS_AUTH_FAILED )
				return "DocsHintCloudUnpublished20";
			if( server_status == Document.SERVER_STATUS_FUTURE_COMMANDS )
				return "DocsHintCloudFuture";
			if (server_status == Document.SERVER_STATUS_BAD_RECEIPT )
				return only_bad_problems ? "" : "DocsHintCloudBadReceipt";
			return "";
		}
		public bool is_published()
		{
			return e_key_b64 != "";
		}
		public void prepare_publish()
		{
			if (is_published ())
				return;
			AesManaged am = new AesManaged();
			am.KeySize = 16*8;
			am.GenerateKey ();
			/*
			byte[] what = new byte[8]{0x6b,0x0e,0x36,0x04,0x5a,0x3b,0x03,0xa3};
			for (int i = 0; i != 8; ++i)
				am.Key [8+i] = (byte)(am.Key[i] ^ what[i]); // Weaken cypher according to 
			*/
			e_key_b64 = Convert.ToBase64String (am.Key);
			cypher = CYPHER_V2;
			last_change_id = 0;
			//commands_sent_data = "";
			last_change_commands_size = 0;
			//commands_sent_size = 0;
			a_token = "";
			server_status = SERVER_STATUS_SYNC;
			update_status = UpdateStatus.UPDATED;
			//prepare_commit (50); // Keep size of first commit small
		}
		public void unpublish()
		{
			write_to_icloud (0);
			e_key_b64 = "";
			cypher = "";
			last_change_id = 0;
			//commands_sent_data = "";
			last_change_commands_size = 0;
			//commands_sent_size = 0;
			a_token = "";
			server_status = "";
			update_status = UpdateStatus.UPDATED;
		}
		public NameValueCollection prepare_sync_request(string full_version_receipt, string full_version_r_token)
		{
			try {
				NameValueCollection result = new NameValueCollection ();
				if (!prepare_commit (result, full_version_receipt, full_version_r_token))
					return null;
				return result;
			}catch(Exception ex){
				FlurryAnalytics.Flurry.LogEvent("Exception_prepare_sync_request", NSDictionary.FromObjectsAndKeys(new object[]{ex.GetType().FullName, ex.Message, ex.Source}, new object[]{"FullName", "Message", "Source"}));
				server_status = SERVER_STATUS_CORRUPTED;
				update_status = UpdateStatus.UPDATED;
				return null;
			}
		}
		public const string a_token_icloud_prefix = "a_token:";
		public const string last_change_id_icloud_prefix = "cs:";
		public void write_to_icloud(int in_cloud)
		{
			if( !String.IsNullOrEmpty(a_token) ) {
				NSUbiquitousKeyValueStore.DefaultStore.SetDictionary(Document.a_token_icloud_prefix + a_token, prepare_cloud_dic(in_cloud));
				//NSUbiquitousKeyValueStore.DefaultStore.Synchronize ();
			}
		}
		public NSDictionary prepare_cloud_dic(int in_cloud)
		{
			var objects = new object [] { in_cloud, e_key_b64, cypher };
			var keys = new object [] { "in_cloud", "e_key_b64", "cypher" };
			var dic = NSDictionary.FromObjectsAndKeys (objects, keys);
			/*NSMutableDictionary dic = new NSMutableDictionary ();
			dic.Add ("in_cloud", NSNumber.FromInt32(in_cloud));
			dic.Add ("password_b64", e_key_b64);
			dic.Add ("password_version", cypher);
			dic.Add ("counter", last_change_id);*/
			return dic;
		}
		public void open_from_icloud (string a_token, string e_key_b64, string cypher)
		{
			this.e_key_b64 = e_key_b64;
			this.cypher = cypher;
			last_change_id = -1; // replace
			last_change_commands_size = 0;
			this.a_token = a_token;
			server_status = SERVER_STATUS_SYNC;
			update_status = UpdateStatus.NEED_UPDATE;
		}
		public bool parse_sync(byte[] resultBytes, ref string full_version_receipt, ref string full_version_r_token) // true if changed something
		{
			Stream ss = new MemoryStream (resultBytes);
			JsonObject jbody = null;
			using (TextReader tr = new StreamReader (ss, System.Text.Encoding.UTF8)) {
				jbody = (JsonObject)JsonObject.Load (tr);
			}
			string result = jbody["result"];
			if (result == "create_doc") {
				conn.BeginTransaction(); // TODO - search for BeginTransaction and wrap with try-finally
				server_status = SERVER_STATUS_SYNC;
				a_token = jbody ["a_token"];
				//last_change_id = jbody ["last_change"];
				//last_change_commands_size = commands_sent_size;
				//commands_sent_data = "";
				update_status = UpdateStatus.NEED_UPDATE; // Will sync last_change on next update
				conn.Commit ();
				Console.WriteLine ("Receive: create_doc a_token={0} file_name={1} last_change={2}", a_token, file_name, last_change_id);
				write_to_icloud (1);
				return true;
			}
			if (result == "bad_receipt") {
				Console.WriteLine ("Receive: bad_receipt a_token={0} file_name={1} last_change={2} r_data={3} r_toke={4}", a_token, file_name, last_change_id, full_version_receipt, full_version_r_token);
				if (!String.IsNullOrEmpty (full_version_r_token)) {
					Console.WriteLine ("Forgeting r_token {0}", full_version_r_token);
					server_status = SERVER_STATUS_SYNC;
					update_status = UpdateStatus.UPDATED; // We still did not commit
					full_version_r_token = "";
				} else {
					server_status = SERVER_STATUS_BAD_RECEIPT;
					update_status = UpdateStatus.UPDATED;
					full_version_receipt = "";
				}
				return true;
			}
			if( result == "auth_failed" ) {
				Console.WriteLine ("Receive: auth_failed a_token={0} file_name={1} last_change={2}", a_token, file_name, last_change_id);
				server_status = SERVER_STATUS_AUTH_FAILED;
				update_status = UpdateStatus.UPDATED;
				return true;
			}
			if( result == "bad" ) {
				Console.WriteLine ("Receive: bad a_token={0} file_name={1} last_change={2}", a_token, file_name, last_change_id);
				server_status = SERVER_STATUS_CORRUPTED;
				update_status = UpdateStatus.UPDATED;
				return true;
			}

			//bool found_commit = false;
			List<Change> server_commands = new List<Change>();

			int new_last_change_id = last_change_id;
			foreach(JsonObject obj in jbody["changes"])
			{
				int iid = obj["id"];
				new_last_change_id = iid;
				string change_body_str = obj["body"];
				byte[] changed_body = Convert.FromBase64String( change_body_str );
				/*if( !found_commit && def_commands.Count == 0 && commands_sent_data == change_body_str
				   && last_change_id != 0) // found_commit only if not replacing
				{
					found_commit = true;
					continue;
				}*/
				byte[] commands_data = decrypt(changed_body);
				if( commands_data == null ) // Unknown cypher
				{
					server_status = SERVER_STATUS_FUTURE_COMMANDS;
					update_status = UpdateStatus.UPDATED;
					return true;
				}
				//string commands_str = Encoding.UTF8.GetString (commands_data);
				JsonArray jcommands = (JsonArray)JsonArray.Load (new StreamReader(new MemoryStream(commands_data), System.Text.Encoding.UTF8));
				Console.WriteLine ("Decrypt {0}->{1} commands={2}", changed_body.Length, commands_data.Length, jcommands.Count);
				foreach(JsonObject jcom in jcommands)
				{
					Change cmd = Change.create_change(this, jcom);
					if( cmd == null )
					{
						server_status = SERVER_STATUS_FUTURE_COMMANDS;
						update_status = UpdateStatus.UPDATED;
						return true;
					}
					server_commands.Add(cmd);
				}
			}

			conn.BeginTransaction(); // TODO - search for BeginTransaction and wrap with try-finally
			//if( found_commit ) // No undo/redo of commited commands neccessary
			//{
			//	last_change_commands_size = commands_sent_size;
			//	commands_sent_data = "";
			//}

			if (server_commands.Count != 0) { // Will insert before some our commands, need undo/redo
				Console.WriteLine("Received: update|commit a_token={0} file_name={1} server_commands.Count={2}, last_change_id={3}, last_change_commands_size={4}, commands_size={5}", a_token, file_name, server_commands.Count, last_change_id, last_change_commands_size, commands_size);
				if (last_change_id == -1) {
					// Replace database, any new commands make no sense (they refer to replaced commands) and are discarded
					List<DBCommand> prev_commands = conn.Query<DBCommand> ("SELECT * FROM DBCommand ORDER BY id");
					for (int i = prev_commands.Count; --i >= 0;) {
						JsonObject jc = (JsonObject)JsonObject.Parse (prev_commands [i].data);
						Change ch = Change.create_change (this, jc);
						ch.undo (this);
					}
					conn.Execute ("DELETE From DBCommand"); // After that the database should be empty
					commands_size = 0;
					for (int i = 0; i < server_commands.Count; ++i) {
						execute_change_no_event_no_transaction (server_commands [i]);
					}
					last_change_commands_size = commands_size;
				} else {
					List<DBCommand> prev_commands = conn.Query<DBCommand> ("SELECT * FROM DBCommand WHERE id >= ? and id < ? ORDER BY id", last_change_commands_size, last_change_commands_size + server_commands.Count);
					bool eq = true;
					if (prev_commands.Count == server_commands.Count) {
						for (int i = 0; i != prev_commands.Count; ++i) {
							JsonObject jc = (JsonObject)JsonObject.Parse (prev_commands [i].data);
							Change ch = Change.create_change (this, jc);
							if( ch.get_uid() != server_commands[i].get_uid() ) {
								eq = false;
								break;
							}
						}
					} else
						eq = false;
					if( !eq ) {
						prev_commands = conn.Query<DBCommand> ("SELECT * FROM DBCommand WHERE id >= ? ORDER BY id", last_change_commands_size);
						List<Change> prev_changes = new List<Change> ();
						Dictionary<string, Change> prev_uids = new Dictionary<string, Change> ();
						for (int i = 0; i < prev_commands.Count; ++i) {
							JsonObject jc = (JsonObject)JsonObject.Parse (prev_commands [i].data);
							Change ch = Change.create_change (this, jc);
							prev_uids.Add (ch.get_uid (), ch);
							prev_changes.Add (ch);
						}
						for (int i = prev_commands.Count; --i >= 0;) {
							prev_changes [i].undo (this);
							//prev_changes[i].fix_ids (this, commands_before_sent_size, def_commands.Count);
						}
						conn.Execute ("DELETE From DBCommand WHERE id >= ?", last_change_commands_size);
						commands_size = last_change_commands_size;
						for (int i = 0; i < server_commands.Count; ++i) {
							string uid = server_commands [i].get_uid ();
							Change prev;
							if (prev_uids.TryGetValue (uid, out prev)) {
								prev_uids.Remove (uid);
								prev_changes.Remove (prev);
							}
							execute_change_no_event_no_transaction (server_commands [i]);
						}
						for (int i = 0; i < prev_changes.Count; ++i) {
							execute_change_no_event_no_transaction (prev_changes [i]);
						}
					}
					last_change_commands_size += server_commands.Count;
				}
				string more = JSONHelper.read_string (jbody, "more", "");
				update_status = String.IsNullOrEmpty(more) ? UpdateStatus.UPDATED : UpdateStatus.NEED_UPDATE;
				increment_local_version ();
			} else { // No changes
				update_status = UpdateStatus.UPDATED;
				Console.WriteLine("Received: no changes a_token={0} file_name={1} server_commands.Count={2}, last_change_id={3}, last_change_commands_size={4}, commands_size={5}", a_token, file_name, server_commands.Count, last_change_id, last_change_commands_size, commands_size);
			}
			string r_token = JSONHelper.read_string (jbody, "r_token", "");
			if (!String.IsNullOrEmpty (r_token)) {
				full_version_r_token = r_token;
				Console.WriteLine ("Receive: r_token a_token={0} file_name={1} last_change={2} r_data={3} r_toke={4}", a_token, file_name, last_change_id, full_version_receipt, full_version_r_token);
			}
			server_status = SERVER_STATUS_SYNC;
			last_change_id = new_last_change_id;
			update_selected_sheet ();
			conn.Commit ();
			//Console.WriteLine("last_change_id={0} last_change_commands_size={1}, commands_size={2}", last_change_id, last_change_commands_size, commands_size);
			if (result == "commited") {
				Console.WriteLine ("iCloud set last_change_id {0} local={1}", a_token, last_change_id);
				NSUbiquitousKeyValueStore.DefaultStore.SetLong(last_change_id_icloud_prefix + a_token, last_change_id);
				//NSUbiquitousKeyValueStore.DefaultStore.Synchronize ();
			}
			return true;
		}
		// Server part end

		private string selected_sheet_v;
		public string selected_sheet { get {
				return selected_sheet_v;
			} 
			set{
				selected_sheet_v = value;
				write_to_key_value ("selected_sheet", selected_sheet_v);
			}
		}
		private int hidden_account_line_v;
		public int hidden_account_line { get {
				return hidden_account_line_v;
			} 
			set{
				hidden_account_line_v = value;
				write_to_key_value ("hidden_account_line", hidden_account_line_v.ToString (CultureInfo.InvariantCulture));
			}
		}	
		private int local_version_v;
		public int local_version { get {
				return local_version_v;
			} 
			set{
				local_version_v = value;
				write_to_key_value ("local_version", local_version_v.ToString (CultureInfo.InvariantCulture));
			}
		}	
		private void increment_local_version()
		{
			local_version += 1;
		}
		private List<DBAccount> sorted_accounts_v;
		public List<DBAccount> sorted_accounts { get {
				if (sorted_accounts_v == null) {
					sorted_accounts_v = conn.Query<DBAccount> ("SELECT * FROM DBAccount WHERE removed <> 1 ORDER BY order_pos");
				}
				return sorted_accounts_v;
			}
		}
		public void reset_sorted_accounts(){
			sorted_accounts_v = null;
		}
		public DBAccount get_account(string acc_id)
		{
			return sorted_accounts.Find (a => a.id == acc_id);
		}
		public void get_account_count_date(string account_id, out int expenses_count, out long recent_expense_date)
		{
			var equery = conn.Query<DBExpense> ("SELECT id, date FROM DBExpense WHERE account == ? ORDER BY date DESC LIMIT 1", account_id);
			var equery2 = conn.Query<DBExpense> ("SELECT id, date FROM DBExpense WHERE account2 == ? ORDER BY date DESC LIMIT 1", account_id);
			recent_expense_date = (equery.Count > 0 && equery2.Count > 0) ? Math.Max(equery[0].date, equery2[0].date) : equery.Count > 0 ? equery[0].date : equery2.Count > 0 ? equery2[0].date : 0;
			// TODO - limit count(*) to arbitrary LIMIT
			int ec1 = conn.ExecuteScalar<int> ("SELECT count(*) FROM DBExpense WHERE account = ? and (account2 IS NULL or account2 <> ?)", account_id, account_id); // We skip expenses with both accounts equal
			int ec2 = conn.ExecuteScalar<int> ("SELECT count(*) FROM DBExpense WHERE account2 = ?", account_id);
			expenses_count = ec1 + ec2;
		}
		public string next_selected_account(string account_id)
		{
			int ind = sorted_accounts.FindIndex( c => c.id == account_id );
			if( ind == -1 )
				return sorted_accounts[0].id;
			if( ind >= hidden_account_line )
				return sorted_accounts[(ind + 1) % sorted_accounts.Count].id;
			return sorted_accounts[(ind + 1) % hidden_account_line].id;
		}
		private List<DBSheet> sorted_sheets_v;
		public List<DBSheet> sorted_sheets { get {
				if (sorted_sheets_v == null) {
					sorted_sheets_v = conn.Query<DBSheet> ("SELECT * FROM DBSheet WHERE removed <> 1 ORDER BY order_pos");
				}
				return sorted_sheets_v;
			}
		}
		public void reset_sorted_sheets(){
			sorted_sheets_v = null;
		}
		public DBSheet get_sheet(string sheet_id)
		{
			return sorted_sheets.Find (s => s.id == sheet_id);
		}
		public void get_sheet_count_date(string sheet_id, out int expenses_count, out long recent_expense_date)
		{
			var equery = conn.Query<DBExpense> ("SELECT id, date FROM DBExpense WHERE sheet = ? ORDER BY date DESC LIMIT 1", sheet_id);
			recent_expense_date = equery.Count > 0 ? equery[0].date : 0;
			expenses_count = conn.ExecuteScalar<int> ("SELECT count(*) FROM DBExpense WHERE sheet = ?", sheet_id);
		}
		public void get_sheet_next_planned_date(string sheet_id, out long sheet_next_planned_date, bool in_sheet)
		{
			var equery = conn.Query<DBExpense> ("SELECT id, date FROM DBExpense WHERE sheet " + (in_sheet ? "=" : "<>") + " ? and planned = 1 and date > 0 ORDER BY date LIMIT 1", sheet_id);
			sheet_next_planned_date = equery.Count > 0 ? equery[0].date : 0;
		}
		private List<DBReport> sorted_reports_v;
		public List<DBReport> sorted_reports { get {
				if (sorted_reports_v == null) {
					sorted_reports_v = conn.Query<DBReport> ("SELECT * FROM DBReport WHERE removed <> 1 ORDER BY order_pos");
					/*sorted_reports_v = new List<StatsSettings>();
					foreach (var rep in reps) {
						try {
							sorted_reports_v.Add (new StatsSettings(rep.id, (JsonObject)JsonObject.Parse(rep.data)));
						}catch(Exception){
							sorted_reports_v.Add (new StatsSettings(rep.id));
						}
					}*/
				}
				return sorted_reports_v;
			}
		}
		public void reset_sorted_reports(){
			sorted_reports_v = null;
		}
		public DBReport get_report(string rep_id)
		{
			return sorted_reports.Find (a => a.id == rep_id);
		}
		private void update_schema_nothing_to_1()
		{
			conn.BeginTransaction();
			conn.CreateTable<DBKeyValue> ();
			// We need data to be very long
			conn.Execute ("CREATE TABLE IF NOT EXISTS DBCommand(id integer primary key not null, data text not null)");
			conn.CreateTable<DBCommand>();
			conn.CreateTable<DBAccount>();
			conn.CreateTable<DBSheet>();
			conn.CreateTable<DBCategory>();
			conn.CreateTable<DBAccountBalance>();
			// For balance summing
			conn.Execute ("CREATE INDEX IF NOT EXISTS DBAccountBalance_account_currency_idx ON DBAccountBalance(account, currency)");
			conn.CreateTable<DBExpenseString>();
			// For search
			conn.Execute ("CREATE INDEX IF NOT EXISTS DBExpenseString_word_planned_date_eid ON DBExpenseString(word COLLATE BINARY, planned, date, expense_id)");
			conn.Execute ("CREATE INDEX IF NOT EXISTS DBExpenseString_sheet_word_planned_date_eid ON DBExpenseString(sheet, word COLLATE BINARY, planned, date, expense_id)");
			// For suggestions
			conn.Execute ("CREATE INDEX IF NOT EXISTS DBExpenseString_word_date_eid_ename ON DBExpenseString(word COLLATE BINARY, date, expense_id, expense_name)");
			conn.Execute ("CREATE INDEX IF NOT EXISTS DBExpenseString_category_word_date_eid_ename ON DBExpenseString(category, word COLLATE BINARY, date, expense_id, expense_name)");

			conn.CreateTable<DBExpense>();
			// For gettings list for all sheets or specific sheet
			conn.Execute ("CREATE INDEX IF NOT EXISTS DBExpense_planned_date_id ON DBExpense(planned, date, id)");
			conn.Execute ("CREATE INDEX IF NOT EXISTS DBExpense_sheet_planned_date_id ON DBExpense(sheet, planned, date, id)");

			// For gettings count(expenses) for category, sheet, account
			conn.Execute ("CREATE INDEX IF NOT EXISTS DBExpense_category_date ON DBExpense(category, date)");
			conn.Execute ("CREATE INDEX IF NOT EXISTS DBExpense_sheet_date ON DBExpense(sheet, date)");
			conn.Execute ("CREATE INDEX IF NOT EXISTS DBExpense_account_date ON DBExpense(account, date)");
			conn.Execute ("CREATE INDEX IF NOT EXISTS DBExpense_account2_date ON DBExpense(account2, date)");

			// We need data to be very long
			conn.Execute ("CREATE TABLE IF NOT EXISTS DBReport(id text primary key not null, order_pos integer not null, data text not null, removed integer not null)");

			write_to_key_value ("schema", "1");
			conn.Commit ();
		}
		private void update_schema_1_to_2()
		{
//			conn.BeginTransaction ();
//			write_to_key_value ("schema", "2");
//			conn.Commit ();
		}
		public Document (string file_folder, string file_name)
		{
			//AppDelegate.ReportTime("Document.Document 1");
			this.file_folder = file_folder;
			this.file_name = file_name;
			conn = new SQLite.SQLiteConnection (Path.Combine(file_folder, file_name));
			//AppDelegate.ReportTime("Document.Document 2");

			conn.Execute ("CREATE TABLE IF NOT EXISTS DBKeyValue(key varchar(32) primary key not null, value text not null)");
			//AppDelegate.ReportTime("Document.Document 3");
			Dictionary<string,string> all_key_values = read_all_from_key_value ();

			string schema = read_from_dic(all_key_values, "schema", "");
			if( schema == "" )
				update_schema_nothing_to_1();
			//if( schema == "1" )
			//	update_schema_1_to_2();

			//AppDelegate.ReportTime("Document.Document 4");
			List<DBCommand> last_cmds = conn.Query<DBCommand> ("SELECT id FROM DBCommand ORDER BY id DESC LIMIT 1");//SELECT max(id) as id, data FROM DBCommand");
			commands_size = last_cmds.Count == 0 ? 0 : last_cmds[0].id + 1;
			//AppDelegate.ReportTime("Document.Document 4.3");
			//int prev_commands_size = conn.Table<DBCommand> ().Count ();

			//AppDelegate.ReportTime("Document.Document 4.5");
			local_version_v = int.Parse (read_from_dic(all_key_values, "local_version", "0"), CultureInfo.InvariantCulture);;
			selected_sheet_v = read_from_dic(all_key_values, "selected_sheet", "");
			hidden_account_line_v = int.Parse (read_from_dic(all_key_values, "hidden_account_line", "0"), CultureInfo.InvariantCulture);

			if ( commands_size == 0) { // No commands - new database
				conn.BeginTransaction();
				execute_change_no_event_no_transaction (new ChangeAccountCreate(this, i18n.get("AccountCash")));
				execute_change_no_event_no_transaction (new ChangeSheetCreate(this, "")); // Empty is translated to Everyday sheet
				create_v1_categories ();
				selected_sheet = sorted_sheets [0].id; // write to database
				conn.Commit ();
			}

			//AppDelegate.ReportTime("Document.Document 5");
			last_change_commands_size_v = int.Parse (read_from_dic(all_key_values, "last_change_commands_size", "0"), CultureInfo.InvariantCulture);
			last_change_id_v = int.Parse (read_from_dic(all_key_values, "last_change_id", "0"), CultureInfo.InvariantCulture);
			e_key_b64_v = read_from_dic(all_key_values, "e_key_b64", "");
			cypher_v = read_from_dic(all_key_values, "cypher", "");
			a_token_v = read_from_dic(all_key_values, "a_token", "");
			server_status_v = read_from_dic(all_key_values, "server_status", "");

			//AppDelegate.ReportTime("Document.Document 6");
			update_selected_sheet ();
		}
		private static string read_from_dic(Dictionary<string,string> dic, string key, string def)
		{
			string result;
			if (dic.TryGetValue (key, out result))
				return result;
			return def;
		}
		private Dictionary<string,string> read_all_from_key_value()
		{
			Dictionary<string,string> result = new Dictionary<string,string> ();
			var query = conn.Query<DBKeyValue> ("SELECT * FROM DBKeyValue");
			foreach (var q in query)
				result.Add(q.key, q.value);
			return result;
		}
		private string read_from_key_value(string key, string def)
		{
			var query = conn.Query<DBKeyValue> ("SELECT * FROM DBKeyValue WHERE key = ?", key);
			return query.Count == 0 ? def : query [0].value;
		}
		private void write_to_key_value(string key, string value)
		{
			DBKeyValue kv = new DBKeyValue(){key=key, value=value};
			var query = conn.Query<DBKeyValue> ("SELECT * FROM DBKeyValue WHERE key = ?", key);
			if (query.Count == 0)
				conn.Insert (kv);
			else
				conn.Update (kv);
		}
		public bool try_rename_file(string new_folder, string new_name)
		{
			bool result = false;
			conn.Dispose ();
			conn = null;
			try {
				FileInfo old_fi = new FileInfo (Path.Combine(file_folder, file_name));
				old_fi.MoveTo(Path.Combine(new_folder,new_name));
				file_folder = new_folder;
				file_name = new_name;
				result = true;
			}catch(IOException ex) {
				Console.WriteLine ("rename_file collision - {0}", ex.Message);
			}
			conn = new SQLite.SQLiteConnection (Path.Combine(file_folder, file_name));
			return result;
		}
		public void load_from_backup(JsonObject jbody)
		{
			int version = jbody ["db_version"];
			if( version != 20 )
				return;
			a_token = JSONHelper.read_string (jbody, "a_token", "");
			conn.BeginTransaction();
			if (a_token != "") {
				e_key_b64 = jbody["e_key_b64"];
				last_change_id = jbody["last_change_id"];
				cypher = jbody["cypher"];
				last_change_commands_size = jbody["last_change_commands_size"];
			}
			//Thread.Sleep (5000);
			//int background_style_id = JSONHelper.read_int (jbody, "background_style", 0);
			JsonArray jcommands = null;
			JsonValue jv;
			if (jbody.TryGetValue ("commands", out jv))
				jcommands = (JsonArray)jv;
			if (jcommands == null) { // Inivitation
				last_change_id = -1;
				last_change_commands_size = 0;
			}else {
				// TODO - optimize by stepping
				List<DBCommand> prev_commands = conn.Query<DBCommand> ("SELECT * FROM DBCommand ORDER BY id");
				for(int i = prev_commands.Count; --i >= 0;)
				{
					JsonObject jc = (JsonObject)JsonObject.Parse (prev_commands[i].data);
					Change ch = Change.create_change(this, jc);
					ch.undo (this);
				}
				conn.Execute ("DELETE From DBCommand"); // After that the database should be empty
				//conn.Commit ();
				//conn.BeginTransaction();
				commands_size = 0;
				foreach (JsonValue jc in jcommands) {
					Change ch = Change.create_change(this, (JsonObject)jc);
					execute_change_no_event_no_transaction (ch);
				}
			}
			selected_sheet = JSONHelper.read_string (jbody, "selected_sheet", ""); // TODO - write in backup from 1.*
			update_selected_sheet ();
			increment_local_version ();
			conn.Commit ();
		}
		private byte[] create_backup_or_invitation(string app_version_info, string os_version, bool invitation) {
			JsonObject jbody = new JsonObject ();
			jbody.Add ("db_version", 20);
			jbody.Add ("selected_sheet", selected_sheet);
			jbody.Add ("app_version_info",app_version_info);
			jbody.Add ("os_version",os_version);
			jbody.Add ("platform","iOS");
			if (is_published () && a_token != "") {
				jbody.Add ("a_token", a_token);
				jbody.Add ("e_key_b64", e_key_b64);
				jbody.Add ("last_change_id", last_change_id);
				jbody.Add ("cypher", cypher);
				jbody.Add ("last_change_commands_size", last_change_commands_size);
				//jbody.Add ("commands_sent_size", invitation ? 0 : commands_sent_size);
				//jbody.Add ("commands_sent_data", invitation ? "" : commands_sent_data);
			}
			//jbody.Add ("background_style", background_style_id);
			if( !invitation ) {
				JsonArray jcommands = new JsonArray();
				List<DBCommand> prev_commands = conn.Query<DBCommand> ("SELECT * FROM DBCommand ORDER BY id");
				foreach(var c in prev_commands)
				{
					JsonObject jc = (JsonObject)JsonObject.Parse (c.data);
					jcommands.Add (jc);
				}
				jbody.Add ("commands", jcommands);
			}
			return Encoding.UTF8.GetBytes( jbody.ToString() );
		}
		public byte[] create_backup(string app_version_info, string os_version) {
			return create_backup_or_invitation (app_version_info, os_version, false);
		}
		public byte[] create_invitation(string app_version_info, string os_version) {
			return create_backup_or_invitation (app_version_info, os_version, true);
		}
		public static void append_expense_titles(XLSXStringsExporter exporter)
		{
			exporter.start_row();
			exporter.add_value("A", i18n.get("csvID"));
			exporter.add_value("B", i18n.get("RecurrenceTitle"));
			exporter.add_value("C", i18n.get("DateTitle"));
			exporter.add_value("D", i18n.get("PlannedTitle")); // TODO - add string
			exporter.add_value("E", i18n.get("CategoryTitle"));
			exporter.add_value("F", i18n.get("SheetTitle"));
			exporter.add_value("G", i18n.get("ExpenseNameTitle"));
			exporter.add_value("H", i18n.get("AccountTitle"));
			exporter.add_value("I", i18n.get("CurrencyTitle"));
			exporter.add_value("J", i18n.get("AmountTitle"));
			exporter.add_value("K", i18n.get("AccountTitle"));
			exporter.add_value("L", i18n.get("CurrencyTitle"));
			exporter.add_value("M", i18n.get("AmountTitle"));
			exporter.finish_row();
		}
		public void append_expense(XLSXStringsExporter exporter, DBExpense expense)
		{
			exporter.start_row();
			exporter.add_value("A", expense.id);
			if( expense.recurrence != DBReccurence.NEVER )
				exporter.add_value("B", i18n.get (expense.recurrence.ToString()));
			if( expense.date != 0 )
				exporter.add_value("C", new DateTime(expense.date, DateTimeKind.Local).ToString(CultureInfo.InvariantCulture));
			if( expense.planned )
				exporter.add_value("D", 1);
			DBCategory cat = get_category (expense.category);
			exporter.add_value("E", cat.get_loc_name());
			DBSheet sh = get_sheet (expense.sheet);
			exporter.add_value("F", sh.get_loc_name());
			exporter.add_value("G", expense.name);
			DBAccount acc = get_account (expense.sum.account);
			exporter.add_value("H", acc.name);
			exporter.add_value("I", expense.sum.currency);
			exporter.add_value("J", DBExpense.to_decimal(expense.sum.amount_1000));
			if( expense.sum2.IsValid() )
			{
				DBAccount acc2 = get_account (expense.sum2.account);
				exporter.add_value("K", acc2.name);
				exporter.add_value("L", expense.sum2.currency);
				exporter.add_value("M", DBExpense.to_decimal(expense.sum2.amount_1000));
			}
			exporter.finish_row();
		}
		public byte[] export_xlsx() {
			XLSXStringsExporter exporter = new XLSXStringsExporter();
			append_expense_titles (exporter);
			List<DBExpense> expenses = get_full_expenses_for_sheet ("", true);
			//expenses.Reverse ();
			foreach (var ex in expenses)
				append_expense (exporter, ex);
			expenses = get_full_expenses_for_sheet ("", false);
			foreach (var ex in expenses)
				append_expense (exporter, ex);
			return exporter.finish();
		}
		public byte[] create_database_copy() {
			return File.ReadAllBytes (Path.Combine(file_folder, file_name));
		}
		private void create_initial_category (string name_key, int sign, string image_name, int old_id)
		{
			execute_change_no_event_no_transaction (new ChangeCategoryCreate(this, new DBCategory(){name="", name_key=name_key, sign=sign, image_name=image_name}));
		}
		private void create_v1_categories()
		{
			create_initial_category("CatApartment", -1, "CatApartment.png", -12);
			create_initial_category("CatFoodHome", -1, "CatFoodHome.png", -13);
			create_initial_category("CatFoodRestaurant", -1, "CatFoodRestaurant.png", -14);
			create_initial_category("CatCarPlanned", -1, "CatCarPlanned.png", -15);
			create_initial_category("CatCarEmergency", -1, "CatCarEmergency.png", -16);
			create_initial_category("CatSport", -1, "CatSport.png", -17);
			create_initial_category("CatCat", -1, "CatCat.png", -18);
			create_initial_category("CatDog", -1, "CatDog.png", -19);
			create_initial_category("CatPet", -1, "CatPet.png", -20);
			create_initial_category("CatGiftsLost", -1, "CatGifts.png", -21);
			create_initial_category("CatTransportPublic", -1, "CatTransportPublic.png", -22);
			create_initial_category("CatFishing", -1, "CatFishing.png", -23);
			create_initial_category("CatHunting", -1, "CatHunting.png", -24);
			create_initial_category("CatGamblingLost", -1, "CatGambling.png", -25);
			create_initial_category("CatGaveFamily", -1, "CatFamily.png", -26);
			create_initial_category("CatBaby", -1, "CatBaby.png", -27);
			create_initial_category("CatMedicine", -1, "CatMedicine.png", -28);
			create_initial_category("CatGadgets", -1, "CatGadgets.png", -29);
			create_initial_category("CatCinema", -1, "CatCinema.png", -30);
			create_initial_category("CatTheaters", -1, "CatTheaters.png", -31);
			create_initial_category("CatRoleplaying", -1, "CatRoleplaying.png", -32);

			create_initial_category("CatSalary", 1, "CatSalary.png", -33);
			create_initial_category("CatOverwork", 1, "CatOverwork.png", -34);
			create_initial_category("CatBonus", 1, "CatBonus.png", -35);
			create_initial_category("CatGamblingWon", 1, "CatGambling.png", -36);
			create_initial_category("CatApple", 1, "CatApple.png", -37);
			create_initial_category("CatFound", 1, "CatFound.png", -38);
			create_initial_category("CatGiftsWon", 1, "CatGifts.png", -39);

			create_initial_category("CatGovernment", -1, "CatGovernment.png", -40);
			create_initial_category("CatClothes", -1, "CatClothes.png", -41);
			create_initial_category("CatSpirits", -1, "CatSpirits.png", -42);
			create_initial_category("CatCommunications", -1, "CatCommunications.png", -43);
			create_initial_category("CatTransportTravel", -1, "CatTransportTravel.png", -44);
			create_initial_category("CatHotel", -1, "CatHotel.png", -45);
			create_initial_category("CatEducation", -1, "CatEducation.png", -46);
			create_initial_category("CatUnknown", -1, "CatUnknown.png", -47);
			create_initial_category("CatCelebration", -1, "CatCelebration.png", -48);
			create_initial_category("CatTookFromFamily", 1, "CatFamily.png", -49);
		}
		public void execute_change_no_event_no_transaction(Change ch)
		{
			ch.redo (this);
			conn.Insert(new DBCommand(){id=commands_size, data=ch.save().ToString()});
			commands_size += 1;
		}
		private void update_selected_sheet()
		{
			if (!String.IsNullOrEmpty(selected_sheet_v)) {
				DBSheet sh = get_sheet (selected_sheet_v);
				if( sh == null )
					selected_sheet = sorted_sheets.Count == 0 ? "" : sorted_sheets [0].id;
			}
		}
		public void execute_change_no_event(Change ch)
		{
			// block spoils debugging, TODO fix before release
			//			conn.RunInTransaction (() => {
			conn.BeginTransaction();
			execute_change_no_event_no_transaction (ch);
			update_selected_sheet ();
			increment_local_version ();
			conn.Commit();
			//			});
		}
		public bool arrange_account_no_event(int from, int to, int mhal)
		{
			conn.BeginTransaction();
			bool result = ChangeAccountArrange.arrange (this, from, to, mhal);
			update_selected_sheet ();
			increment_local_version ();
			conn.Commit();
			return result;
		}
		public void execute_move_from_sheet(string sheet_id, string to_sheet_id)
		{
			var equery = conn.Query<DBExpense> ("SELECT * FROM DBExpense WHERE sheet == ?", sheet_id);
			conn.BeginTransaction();
			foreach (var ex in equery) {
				DBExpense nex = ex.Clone ();
				nex.sheet = to_sheet_id;
				execute_change_no_event_no_transaction(new ChangeExpenseUpdate(this, nex));
			}
			increment_local_version ();
			conn.Commit();
		}
		public void execute_delete_from_sheet(string sheet_id)
		{
			var equery = conn.Query<DBExpense> ("SELECT * FROM DBExpense WHERE sheet == ?", sheet_id);
			conn.BeginTransaction();
			foreach (var ex in equery) {
				execute_change_no_event_no_transaction(new ChangeExpenseRemove(this, ex.id));
			}
			increment_local_version ();
			conn.Commit();
		}
		private void replace_account(DBExpense ex, string account_id, string to_account_id)
		{
			DBExpense nex = ex.Clone ();
			if (nex.account == account_id)
				nex.account = to_account_id;
			if (nex.account2 == account_id)
				nex.account2 = to_account_id;
			execute_change_no_event_no_transaction(new ChangeExpenseUpdate(this, nex));
		}
		public void execute_move_from_account(string account_id, string to_account_id)
		{
			var equery = conn.Query<DBExpense> ("SELECT * FROM DBExpense WHERE account = ?", account_id);
			conn.BeginTransaction();
			foreach (var ex in equery) {
				replace_account (ex, account_id, to_account_id);
			}
			equery = conn.Query<DBExpense> ("SELECT * FROM DBExpense WHERE account2 = ?", account_id);
			foreach (var ex in equery) {
				replace_account (ex, account_id, to_account_id);
			}
			increment_local_version ();
			conn.Commit();
		}
		public void update_balance(string account, string currency, long amount_1000)
		{
			if( String.IsNullOrEmpty(account) || String.IsNullOrEmpty(currency) )
				return;
			DBAccountBalance dba = new DBAccountBalance () { account=account, currency=currency, sum_1000=amount_1000 };
			var q = conn.Query<DBAccountBalance> ("select * from DBAccountBalance where account=? and currency=?", account, currency);
			if (q.Count == 0){
				conn.Insert (dba);
			}else{
				dba.sum_1000 += q [0].sum_1000;
				conn.Execute("update DBAccountBalance SET sum_1000=? where account=? and currency=?", dba.sum_1000, account, currency);
			}
		}
		public List<DBAccountBalance> get_balance(string account)
		{
			var q = conn.Query<DBAccountBalance> ("select * from DBAccountBalance where account=? and sum_1000 <> 0 order by currency", account);
			return q;
		}
		public void insert_suggestion (DBExpense ex)
		{
			if (String.IsNullOrEmpty (ex.name))
				return;
			string folded = StringFolding.fold_maximum (ex.name);
			string[] words = StringFolding.Split(folded);
			foreach (string w in words) {
				conn.Insert (new DBExpenseString(){sheet=ex.sheet, category=ex.category, word=w, planned=ex.planned, date=ex.date, expense_id=ex.id, expense_name=ex.name});
			}
		}
		public void remove_suggestion (DBExpense ex)
		{
			if (String.IsNullOrEmpty (ex.name))
				return;
			string folded = StringFolding.fold_maximum (ex.name);
			string[] words = StringFolding.Split(folded);
			foreach (string w in words) {
				// We set all fields to make use of index
				conn.Execute("DELETE FROM DBExpenseString WHERE sheet = ? and category = ? and word = ? and planned = ? and date = ? and expense_id = ?", ex.sheet, ex.category, w, ex.planned, ex.date, ex.id);
				// If we have several equal words for this ref_id, first DELETE deletes all, other DELETEs are nops
			}
		}
		private void fill_single_suggestion( Dictionary<string, HashSet<string>> ref_ids, string category, string folded, int limit)
		{
			var query = String.IsNullOrEmpty(category) ? conn.Query<DBExpenseString> ("SELECT word, date, expense_id, expense_name FROM DBExpenseString WHERE word GLOB ? ORDER BY word DESC, date DESC, expense_id DESC LIMIT ?", folded + "*", limit) 
				: conn.Query<DBExpenseString> ("SELECT word, date, expense_id, expense_name FROM DBExpenseString WHERE category = ? and word GLOB ? ORDER BY word DESC, date DESC, expense_id DESC LIMIT ?", category, folded + "*", limit);
			//DBExpenseString previous = null; // We manually implement DISTINCT
			foreach(DBExpenseString s in query)
			{
				//if( previous == null || previous.expense_id != s.expense_id ) {
				HashSet<string> val;
				if (!ref_ids.TryGetValue (s.expense_name, out val)) {
					val = new HashSet<string> ();
					ref_ids.Add (s.expense_name, val);
				}
				val.Add(s.expense_id);
				//}
				//previous = s;
			}
		}
		public Dictionary<string, HashSet<string>> fill_suggestion( string category, string name)
		{
			int limit = 200;
			Dictionary<string, HashSet<string>> result = new Dictionary<string, HashSet<string>>();
			string folded = StringFolding.fold_maximum (name);
			string[] words = StringFolding.Split(folded);
			if (words.Length == 0) {
				fill_single_suggestion(result, category, "", limit*5);
				return result;
			}
			if (words.Length == 1) {
				fill_single_suggestion(result, category, words[0], limit*5);
				return result;
			}
			int word_counter = 0;
			foreach (string w in words) {
				if( word_counter == 0)
					fill_single_suggestion(result, category, w, limit*10);
				else{
					Dictionary<string, HashSet<string>> ref_ids = new Dictionary<string, HashSet<string>>();
					fill_single_suggestion(ref_ids, category, w, limit*10);
					foreach(string k in new List<string>(result.Keys)){
						HashSet<string> val;
						if (!ref_ids.TryGetValue (k, out val))
							result.Remove (k);
					}
					//result.IntersectWith (ref_ids);
				}
				word_counter += 1;
			}
			return result;
		}
		public List<DBExpense> get_first_expenses_for_sheet(string sheet, bool planned) {
			int limit = 25;
			List<DBExpense> query = String.IsNullOrEmpty(sheet) ? conn.Query<DBExpense> ("SELECT id, planned, date FROM DBExpense WHERE planned = ? and date > 0 ORDER BY planned ASC, date ASC, id ASC LIMIT ?", planned, limit)
				: conn.Query<DBExpense> ("SELECT id, planned, date FROM DBExpense WHERE sheet = ? AND planned = ? and date > 0 ORDER BY planned ASC, date ASC, id ASC LIMIT ?", sheet, planned, limit);
			return query;
		}
		public List<DBExpense> get_all_planned_expenses_for_sheet(string sheet) {
			bool planned = true;
			List<DBExpense> query = String.IsNullOrEmpty(sheet) ? conn.Query<DBExpense> ("SELECT id, planned, date FROM DBExpense WHERE planned = ? ORDER BY planned DESC, date DESC, id DESC", planned)
				: conn.Query<DBExpense> ("SELECT id, planned, date FROM DBExpense WHERE sheet = ? AND planned = ? ORDER BY planned DESC, date DESC, id DESC", sheet, planned);
			return query;
		}
		public List<DBExpense> get_expenses_for_sheet(string sheet, bool planned, DBExpense last_expense) {
			int limit = 25;
			if( last_expense == null ) {
				List<DBExpense> query = String.IsNullOrEmpty(sheet) ? conn.Query<DBExpense> ("SELECT id, planned, date FROM DBExpense WHERE planned = ? ORDER BY planned DESC, date DESC, id DESC LIMIT ?", planned, limit)
					: conn.Query<DBExpense> ("SELECT id, planned, date FROM DBExpense WHERE sheet = ? AND planned = ? ORDER BY planned DESC, date DESC, id DESC LIMIT ?", sheet, planned, limit);
				return query;
			}
			// Search more expenses with the same date
			List<DBExpense> query_date_eq = String.IsNullOrEmpty(sheet) ? conn.Query<DBExpense> ("SELECT id, planned, date FROM DBExpense WHERE planned = ? and date = ? and id < ? ORDER BY planned DESC, date DESC, id DESC LIMIT ?", planned, last_expense.date, last_expense.id, limit)
				: conn.Query<DBExpense> ("SELECT id, planned, date FROM DBExpense WHERE sheet = ? AND planned = ? and date = ? and id < ? ORDER BY planned DESC, date DESC, id DESC LIMIT ?", sheet, planned, last_expense.date, last_expense.id, limit);
			if( query_date_eq.Count > 50 )
				return query_date_eq;
			// Search more expenses with the next date
			List<DBExpense> query_date_less = String.IsNullOrEmpty(sheet) ? conn.Query<DBExpense> ("SELECT id, planned, date FROM DBExpense WHERE planned = ? and date < ? ORDER BY planned DESC, date DESC, id DESC LIMIT ?", planned, last_expense.date, limit)
				: conn.Query<DBExpense> ("SELECT id, planned, date FROM DBExpense WHERE sheet = ? AND planned = ? and date < ? ORDER BY planned DESC, date DESC, id DESC LIMIT ?", sheet, planned, last_expense.date, limit);
			query_date_eq.AddRange(query_date_less);
			return query_date_eq;
		}
		// Next two are for stats
		public List<DBExpense> get_full_expenses_for_sheet(string sheet, bool planned) {
			List<DBExpense> query = String.IsNullOrEmpty(sheet) ? conn.Query<DBExpense> ("SELECT * FROM DBExpense WHERE planned = ? ORDER BY date DESC, id DESC", planned)
				: conn.Query<DBExpense> ("SELECT * FROM DBExpense WHERE sheet = ? AND planned = ? ORDER BY date DESC, id DESC", sheet, planned);
			return query;
		}
		public List<DBExpense> get_full_expenses_for_sheet(string sheet, bool planned, long from_date, long to_date) {
			List<DBExpense> query = String.IsNullOrEmpty(sheet) ? conn.Query<DBExpense> ("SELECT * FROM DBExpense WHERE planned = ? AND date >= ? AND date < ? ORDER BY date DESC, id DESC", planned, from_date, to_date)
				: conn.Query<DBExpense> ("SELECT * FROM DBExpense WHERE sheet = ? AND planned = ? AND date >= ? AND date < ? ORDER BY date DESC, id DESC", sheet, planned, from_date, to_date);
			return query;
		}
		private List<DBExpense> intersect_lists(List<DBExpense> l1, List<DBExpense> l2)
		{
			List<DBExpense> result = new List<DBExpense> ();
			int pos1 = 0;
			int pos2 = 0;
			while(pos1 != l1.Count && pos2 != l2.Count) {
				int dif = sort_by_date_id(l1[pos1], l2[pos2]);
				if (dif < 0)
					pos1 += 1;
				else if (dif > 0)
					pos2 += 1;
				else {
					result.Add (l1[pos1]);
					pos1 += 1;
					pos2 += 1;
				}
			}
			return result;
		}
		private List<DBExpense> make_distinct(List<DBExpenseString> strs)
		{
			strs.Sort (sort_by_date_id);
			List<DBExpense> result = new List<DBExpense> ();
			DBExpenseString previous = null;
			foreach(DBExpenseString s in strs)
			{
				if( previous == null || sort_by_date_id(s, previous) != 0 )
				   result.Add ( new DBExpense(){id=s.expense_id, planned=s.planned, date=s.date});
				previous = s;
		   	}
			return result;
		}
		public static int sort_by_date_id(DBExpense p1, DBExpense p2)
		{
			int dif = -p1.date.CompareTo(p2.date); // more first
			if (dif != 0)
				return dif;
			return -p1.id.CompareTo(p2.id); // by alphabet
		}
		public static int sort_by_date_id(DBExpenseString p1, DBExpenseString p2)
		{
			int dif = -p1.date.CompareTo(p2.date); // more first
			if (dif != 0)
				return dif;
			return -p1.expense_id.CompareTo(p2.expense_id); // by alphabet
		}
		public List<DBExpense> search_expenses_for_sheet(string name, string sheet, bool planned)
		{
			int limit = 100;
			string folded = String.IsNullOrEmpty(name) ? "" : StringFolding.fold_maximum (name);
			string[] words = StringFolding.Split(folded);
			if (words.Length == 0)// no search
				return get_expenses_for_sheet (sheet, planned, null);
			// TODO - multiword search
			List<DBExpenseString> query = String.IsNullOrEmpty(sheet) ? conn.Query<DBExpenseString> ("SELECT expense_id, planned, date FROM DBExpenseString WHERE word GLOB ? AND planned = ? ORDER BY word DESC, planned DESC, date DESC, expense_id DESC LIMIT ?", words [0] + "*", planned, words.Length == 1 ? limit : limit*10)
				: conn.Query<DBExpenseString> ("SELECT expense_id, planned, date FROM DBExpenseString WHERE sheet = ? AND word GLOB ? AND planned = ?  ORDER BY word DESC, planned DESC, date DESC, expense_id DESC LIMIT ?", sheet, words [0] + "*", planned, words.Length == 1 ? limit : limit*10);
			List<DBExpense> result = make_distinct (query);
			// We implement DISTINCT, because SQLite cannot understand how to optimize it
			int word_counter = 0;
			foreach (string w in words) {
				if (word_counter != 0){
					List<DBExpenseString> query2 = String.IsNullOrEmpty(sheet) ? conn.Query<DBExpenseString> ("SELECT expense_id, planned, date FROM DBExpenseString WHERE word GLOB ? AND planned = ? ORDER BY word DESC, planned DESC, date DESC, expense_id DESC LIMIT ?", w + "*", planned, limit*10)
						: conn.Query<DBExpenseString> ("SELECT expense_id, planned, date FROM DBExpenseString WHERE sheet = ? AND word GLOB ? AND planned = ?  ORDER BY word DESC, planned DESC, date DESC, expense_id DESC LIMIT ?", sheet, w + "*", planned, limit*10);
					List<DBExpense> result2 = make_distinct (query2);
					result = intersect_lists (result, result2);
				}
				word_counter += 1;
			}
			return result;
		}
		public DBExpense get_expense(string id)
		{
			//List<DBExpense> query = new List<DBExpense> ();

			IntPtr NegativePointer = new IntPtr (-1);
			string CommandText = "SELECT * FROM DBExpense WHERE id = ?";
			var stmt = SQLite3.Prepare2 (conn.Handle, CommandText);
			try {
				SQLite3.BindText (stmt, 1, id, -1, NegativePointer);
				//int cc = SQLite3.ColumnCount (stmt);

				while (SQLite3.Step (stmt) == SQLite3.Result.Row) {
					var obj = new DBExpense();
					int ind = 0;
					obj.id = SQLite3.ColumnString(stmt, ind++);
					obj.date = SQLite3.ColumnInt64(stmt, ind++);
					obj.recurrence = (DBReccurence)SQLite3.ColumnInt(stmt, ind++);
					obj.name = SQLite3.ColumnString(stmt, ind++);
					obj.category = SQLite3.ColumnString(stmt, ind++);
					obj.sheet = SQLite3.ColumnString(stmt, ind++);
					obj.planned = SQLite3.ColumnInt(stmt, ind++) != 0;
					obj.account = SQLite3.ColumnString(stmt, ind++);
					obj.amount_1000 = SQLite3.ColumnInt64(stmt, ind++);
					obj.currency = SQLite3.ColumnString(stmt, ind++);
					obj.account2 = SQLite3.ColumnString(stmt, ind++);
					obj.amount2_1000 = SQLite3.ColumnInt64(stmt, ind++);
					obj.currency2 = SQLite3.ColumnString(stmt, ind++);
					return obj;
					//query.Add(obj);
				}
			}
			finally
			{
				SQLite3.Finalize(stmt);
			}
			return null;
			//List<DBExpense> query = conn.Query<DBExpense> ("SELECT * FROM DBExpense WHERE id = ?", id);
			//return query.Count == 0 ? null : query [0];
		}
		public List<string> get_used_currencies()
		{
			List<DBAccountBalance> query = conn.Query<DBAccountBalance> ("SELECT DISTINCT 1 as 'account', currency, 1 as 'sum' FROM DBAccountBalance"); // TODO where sum <> 0
			List<string> used = new List<string> ();
			foreach (DBAccountBalance a in query) {
				used.Add (a.currency);
			}
			return used;
		}
		private Dictionary<string, DBCategory> cat_cash;
		public const string TRANSFER_CATEGORY = "-2";
		public const string CONVERT_CATEGORY = "-3";
		public const string LOAN_CATEGORY = "-4";
		public const string LOAN_BACK_CATEGORY = "-5";
		public void get_category_count_date(string cat_id, out int expenses_count, out long recent_expense_date)
		{
			var equery = conn.Query<DBExpense> ("SELECT id, date FROM DBExpense WHERE category = ? ORDER BY date DESC LIMIT 1", cat_id);
			recent_expense_date = equery.Count > 0 ? equery[0].date : 0;
			expenses_count = conn.ExecuteScalar<int> ("SELECT count(*) FROM DBExpense WHERE category = ?", cat_id);
		}
		public DBCategory get_category(string cat_id)
		{
			if (cat_cash == null)
			{
				cat_cash = new Dictionary<string, DBCategory> ();
				cat_cash.Add (TRANSFER_CATEGORY, new DBCategory () {id=TRANSFER_CATEGORY,sign=0,name="",name_key="CatTransfer",image_name="CatTransfer.png" });
				cat_cash.Add (CONVERT_CATEGORY, new DBCategory () {id=CONVERT_CATEGORY,sign=0,name="",name_key="CatConvert",image_name="CatConvert.png" });
				cat_cash.Add (LOAN_CATEGORY, new DBCategory () {id=LOAN_CATEGORY,sign=0,name="",name_key="CatLoan",image_name="CatLoan.png" });
				cat_cash.Add (LOAN_BACK_CATEGORY, new DBCategory () {id=LOAN_BACK_CATEGORY,sign=0,name="",name_key="CatLoanBack",image_name="CatLoanBack.png" }); // TODO - string, image

				var query = conn.Query<DBCategory> ("SELECT * FROM DBCategory WHERE removed <> 1");
				foreach (var ca in query) {
					cat_cash.Add (ca.id, ca);
				}
			}
			DBCategory cat;
			cat_cash.TryGetValue (cat_id, out cat);
			return cat;
		}
		/*public DBCategory read_category(string cat_id)
		{
			var query = conn.Query<DBCategory> ("SELECT * FROM DBCategory WHERE id = ? and removed <> 1", cat_id);
			if (query.Count == 0)
				return null;
			return query [0];
		}*/
		public void reset_sorted_categories(){
			cat_cash = null;
		}
		public class IdCountDate
		{
			public int id { get; set; }
			public int count { get; set; }
			public long date { get; set; }
		};
		public List<DBCategory> get_signed_categories(int sign)
		{
			List<DBCategory> query = conn.Query<DBCategory> ("SELECT * FROM DBCategory WHERE sign = ? and removed <> 1 ORDER BY id", sign);
			return query;
		}
	}
}
 