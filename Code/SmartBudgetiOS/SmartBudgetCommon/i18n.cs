using System;
using System.Collections.Generic;

namespace SmartBudgetCommon
{
	static public class i18n
	{
		static private Dictionary<string, string> all_keys = new Dictionary<string, string>();
		public static string ReplaceFirst(string text, string search, string replace)
		{
			int pos = text.IndexOf(search);
			if (pos < 0)
			{
				return text;
			}
			return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}
		/*public static string between(string str, string left, string right)
		{
			int ind1 = str.IndexOf (left);
			int ind2 = str.IndexOf (right);
			if (ind1 == -1 || ind2 == -1)
				return "";
			if( ind1 < ind2 )
				return str.Substring(ind1+left.Length, ind2-ind2-left.Length);
			return str.Substring (ind2 + right.Length, ind1 - ind2 - right.Length);
		}*/
		static public string get(string key)
		{
			return get (key, "");
		}
		static public string get(string key, string comment)
		{
			#if SILVERLIGHT || __ANDROID__
			string val;
			if (!all_keys.TryGetValue (key, out val))
				return key;
			return val;
			#else
				return Foundation.NSBundle.MainBundle.LocalizedString (key, comment);
			#endif
		}
		static private void add(string key, string value)
		{
			//Console.WriteLine ("i18n.add {0} {1}", key, value);
			all_keys.Add (key, value);
		}
		static public void add_strings(string i18n_txt)
		{
			var lines = i18n_txt.Split ("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			foreach (string line in lines) {
				int eqind = line.IndexOf ('=');
				if (eqind == -1)
					continue;
				var left = line.Substring (0, eqind).Replace("\"","").Trim();
				var right = line.Substring (eqind+1).Replace("\\n","\n");
				int bas = right.IndexOf ("\\");
				if (bas != -1)
					throw new Exception ("Unknown escape character");
				int leq = right.IndexOf ('"');
				int req = right.LastIndexOf('"');
				if (req == -1 || leq == -1 || req <= leq)
					continue;
				add(left, right.Substring(leq + 1, req-leq-1));
			}
		}
		static private bool is_0(int count) {
			return count == 0;
		}
		static private bool is_1(int count) {
			return count == 1;
		}
		static private bool is_2(int count) {
			return count == 2;
		}
		static private bool is_0_or_1(int count) {
			return count == 0 || count == 1;
		}
		static private bool ends_in_1_excluding_11(int count) {
			return count % 10 == 1 && !(count % 100 == 11);
		}
		static private bool ends_in_2_4_excluding_12_14(int count) {
			return (count % 10 >= 2 && count % 10 <= 4) && !(count % 100 >= 12 && count % 100 <= 14);
		}
		static private bool ends_in_03_10(int count) {
			return count % 100 >= 3 && count % 100 <= 10;
		}
		static private bool ends_in_00_02(int count) {
			return count % 100 >= 0 && count % 100 <= 2;
		}

		static private string plural_helper(string key, int count)
		{
			// Supported rules #0 #1 #2 #7 #9 #12
			// #0 {count} months
			// #1 {{is_1}}1 month{{else}}{count} months
			// #2 {{is_0_or_1}}{count} month{{else}}{count} months
			// #7 {{ends_in_1_excluding_11}}{count} месяц{{ends_in_2-4_excluding_12-14}}{count} месяца{{else}}{count} месяцев
			// #9 {{is_1}}1 month{{ends_in_2-4_excluding_12-14}}{count} months{{else}}{count} months
			// #12 {{is_0}}0 months{{is_1}}1 month{{is_2}}2 months{{ends_in_00-02}}{count} months{{ends_in_03-10}}{count} months{{else}}{count} months
			string str = get (key);
			int start = str.IndexOf ("{{");
			while (start != -1) {
				int finish = str.IndexOf ("}}", start);
				if (finish == -1) // not finished token
					break;
				finish += 2;
				int next_start = str.IndexOf ("{{", finish);
				string rule = str.Substring (start, finish - start);
				string result = next_start == -1 ? str.Substring(finish) : str.Substring (finish, next_start - finish);
				switch (rule) {
				case "{{is_0}}": // #12
					if (is_0 (count))
						return result;
					break;
				case "{{is_1}}": // #1 #9 #12
					if (is_1 (count))
						return result;
					break;
				case "{{is_2}}": // #12
					if (is_2 (count))
						return result;
					break;
				case "{{is_0_or_1}}": // #2
					if (is_0_or_1 (count))
						return result;
					break;
				case "{{ends_in_1_excluding_11}}": // #7
					if (ends_in_1_excluding_11 (count))
						return result;
					break;
				case "{{ends_in_2-4_excluding_12-14}}": // #7 #9
					if (ends_in_2_4_excluding_12_14 (count))
						return result;
					break;
				case "{{ends_in_00-02}}": // #12
					if (ends_in_00_02 (count))
						return result;
					break;
				case "{{ends_in_03-10}}": // #12
					if (ends_in_03_10 (count))
						return result;
					break;
				case "{{else}}": // #1 #2 #7 #12
					return result;
				}
				start = next_start;
			}
			//if (start == -1) 
			return str; // #0, everything
/*			string[] parts = str.Split ("\n".ToCharArray (), StringSplitOptions.RemoveEmptyEntries);
			foreach (var part in parts) {
				int prefix_pos = part.IndexOf (":}");
				if (prefix_pos == -1)
					return part;
				prefix_pos += 2;
				switch (part.Substring (0, prefix_pos)) {
				case "{is_0:}": // #12
					if (is_0 (count))
						return part.Substring (prefix_pos);
					break;
				case "{is_1:}": // #1 #9 #12
					if (is_1 (count))
						return part.Substring (prefix_pos);
					break;
				case "{is_2:}": // #12
					if (is_2 (count))
						return part.Substring (prefix_pos);
					break;
				case "{is_0_or_1:}": // #2
					if (is_0_or_1 (count))
						return part.Substring (prefix_pos);
					break;
				case "{ends_in_1_excluding_11:}": // #7
					if (ends_in_1_excluding_11 (count))
						return part.Substring (prefix_pos);
					break;
				case "{ends_in_2-4_excluding_12-14:}": // #7 #9
					if (ends_in_2_4_excluding_12_14 (count))
						return part.Substring (prefix_pos);
					break;
				case "{ends_in_00-02:}": // #12
					if (ends_in_00_02 (count))
						return part.Substring (prefix_pos);
					break;
				case "{ends_in_03-10:}": // #12
					if (ends_in_03_10 (count))
						return part.Substring (prefix_pos);
					break;
				case "{everything_else:}": // #1 #2 #7 #12
					return part.Substring (prefix_pos);
				}
			}
			return "";*/
		}
		static public string localize_plural(string key, int count)
		{
			string count_str = count.ToString (); // Culture ok
			return plural_helper (key, count).Replace ("{count}", count_str);
		}
	}
}
/*
 * Plural rule #0 (1 form)

Families: Asian (Chinese, Japanese, Korean, Vietnamese), Persian, Turkic/Altaic (Turkish), Thai, Lao
everything: 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, …

Plural rule #1 (2 forms)

Families: Germanic (Danish, Dutch, English, Faroese, Frisian, German, Norwegian, Swedish), Finno-Ugric (Estonian, Finnish, Hungarian), Language isolate (Basque), Latin/Greek (Greek), Semitic (Hebrew), Romanic (Italian, Portuguese, Spanish, Catalan)
is 1: 1
everything else: 0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, …

Plural rule #2 (2 forms)

Families: Romanic (French, Brazilian Portuguese)
is 0 or 1: 0, 1
everything else: 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, …

Plural rule #3 (3 forms)

Families: Baltic (Latvian)
is 0: 0
ends in 1, excluding 11: 1, 21, 31, 41, 51, 61, 71, 81, 91, 101, 121, 131, 141, 151, 161, 171, 181, 191, 201, 221, 231, 241, 251, 261, 271, 281, 291, …
everything else: 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 22, 23, 24, 25, 26, 27, 28, 29, 30, 32, 33, 34, 35, 36, 37, 38, 39, 40, 42, 43, 44, 45, 46, 47, 48, 49, 50, 52, 53, 54, 55, …

Plural rule #4 (4 forms)

Families: Celtic (Scottish Gaelic)
is 1 or 11: 1, 11
is 2 or 12: 2, 12
is 3-10 or 13-19: 3, 4, 5, 6, 7, 8, 9, 10, 13, 14, 15, 16, 17, 18, 19
everything else: 0, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, …

Plural rule #5 (3 forms)

Families: Romanic (Romanian)
is 1: 1
is 0 or ends in 01-19, excluding 1: 0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, …
everything else: 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, …

Plural rule #6 (3 forms)

Families: Baltic (Lithuanian)
ends in 1, excluding 11: 1, 21, 31, 41, 51, 61, 71, 81, 91, 101, 121, 131, 141, 151, 161, 171, 181, 191, 201, 221, 231, 241, 251, 261, 271, 281, 291, …
ends in 0 or ends in 11-19: 0, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 130, 140, 150, 160, 170, 180, 190, 200, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, …
everything else: 2, 3, 4, 5, 6, 7, 8, 9, 22, 23, 24, 25, 26, 27, 28, 29, 32, 33, 34, 35, 36, 37, 38, 39, 42, 43, 44, 45, 46, 47, 48, 49, 52, 53, 54, 55, 56, 57, 58, 59, 62, 63, 64, 65, 66, 67, 68, 69, 72, 73, …

Plural rule #7 (3 forms)

Families: Slavic (Belarusian, Bosnian, Croatian, Serbian, Russian, Ukrainian)
ends in 1, excluding 11: 1, 21, 31, 41, 51, 61, 71, 81, 91, 101, 121, 131, 141, 151, 161, 171, 181, 191, 201, 221, 231, 241, 251, 261, 271, 281, 291, …
ends in 2-4, excluding 12-14: 2, 3, 4, 22, 23, 24, 32, 33, 34, 42, 43, 44, 52, 53, 54, 62, 63, 64, 72, 73, 74, 82, 83, 84, 92, 93, 94, 102, 103, 104, 122, 123, 124, 132, 133, 134, 142, 143, 144, 152, 153, 154, 162, 163, 164, 172, 173, 174, 182, 183, …
everything else: 0, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 25, 26, 27, 28, 29, 30, 35, 36, 37, 38, 39, 40, 45, 46, 47, 48, 49, 50, 55, 56, 57, 58, 59, 60, 65, 66, 67, 68, 69, 70, 75, 76, 77, …

Plural rule #8 (3 forms)

Families: Slavic (Slovak, Czech)
is 1: 1
is 2-4: 2, 3, 4
everything else: 0, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, …

Plural rule #9 (3 forms)

Families: Slavic (Polish)
is 1: 1
ends in 2-4, excluding 12-14: 2, 3, 4, 22, 23, 24, 32, 33, 34, 42, 43, 44, 52, 53, 54, 62, 63, 64, 72, 73, 74, 82, 83, 84, 92, 93, 94, 102, 103, 104, 122, 123, 124, 132, 133, 134, 142, 143, 144, 152, 153, 154, 162, 163, 164, 172, 173, 174, 182, 183, …
everything else: 0, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 25, 26, 27, 28, 29, 30, 31, 35, 36, 37, 38, 39, 40, 41, 45, 46, 47, 48, 49, 50, 51, 55, 56, 57, 58, 59, 60, 61, 65, 66, 67, 68, …

Plural rule #10 (4 forms)

Families: Slavic (Slovenian, Sorbian)
ends in 01: 1, 101, 201, …
ends in 02: 2, 102, 202, …
ends in 03-04: 3, 4, 103, 104, 203, 204, …
everything else: 0, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, …

Plural rule #11 (5 forms)

Families: Celtic (Irish Gaelic)
is 1: 1
is 2: 2
is 3-6: 3, 4, 5, 6
is 7-10: 7, 8, 9, 10
everything else: 0, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, …

Plural rule #12 (6 forms)

Families: Semitic (Arabic)
is 1: 1
is 2: 2
ends in 03-10: 3, 4, 5, 6, 7, 8, 9, 10, 103, 104, 105, 106, 107, 108, 109, 110, 203, 204, 205, 206, 207, 208, 209, 210, …
everything else but is 0 and ends in 00-02, excluding 0-2: 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, … 
ends in 00-02, excluding 0-2: 100, 101, 102, 200, 201, 202, …
is 0: 0

Plural rule #13 (4 forms)

Families: Semitic (Maltese)
is 1: 1
is 0 or ends in 01-10, excluding 1: 0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, …
ends in 11-19: 11, 12, 13, 14, 15, 16, 17, 18, 19, 111, 112, 113, 114, 115, 116, 117, 118, 119, 211, 212, 213, 214, 215, 216, 217, 218, 219, …
everything else: 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, …

Plural rule #14 (3 forms)

Families: Slavic (Macedonian)
ends in 1: 1, 11, 21, 31, 41, 51, 61, 71, 81, 91, 101, 111, 121, 131, 141, 151, 161, 171, 181, 191, 201, 211, 221, 231, 241, 251, 261, 271, 281, 291, …
ends in 2: 2, 12, 22, 32, 42, 52, 62, 72, 82, 92, 102, 112, 122, 132, 142, 152, 162, 172, 182, 192, 202, 212, 222, 232, 242, 252, 262, 272, 282, 292, …
everything else: 0, 3, 4, 5, 6, 7, 8, 9, 10, 13, 14, 15, 16, 17, 18, 19, 20, 23, 24, 25, 26, 27, 28, 29, 30, 33, 34, 35, 36, 37, 38, 39, 40, 43, 44, 45, 46, 47, 48, 49, 50, 53, 54, 55, 56, 57, 58, 59, 60, 63, …

Plural rule #15 (2 forms)

Families: Icelandic
ends in 1, excluding 11: 1, 21, 31, 41, 51, 61, 71, 81, 91, 101, 121, 131, 141, 151, 161, 171, 181, 191, 201, 221, 231, 241, 251, 261, 271, 281, 291, …
everything else: 0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 22, 23, 24, 25, 26, 27, 28, 29, 30, 32, 33, 34, 35, 36, 37, 38, 39, 40, 42, 43, 44, 45, 46, 47, 48, 49, 50, 52, 53, 54, …

Plural rule #16 (6 forms)

Families: Celtic (Breton)
is 1: 1
ends in 1, excluding 1, 11, 71, 91: 21, 31, 41, 51, 61, 81, 101, 121, 131, 141, 151, 161, 181, 201, 221, 231, 241, 251, 261, 281, ...
ends in 2, excluding 12, 72, 92: 2, 22, 32, 42, 52, 62, 82, 102, 122, 132, 142, 152, 162, 182, 202, 222, 232, 242, 252, 262, 282, ...
ends in 3, 4 or 9 excluding 13, 14, 19, 73, 74, 79, 93, 94, 99: 3, 4, 9, 23, 24, 29, 33, 34, 39, 43, 44, 49, 53, 54, 59, ...
ends in 1000000: 1000000: 1000000, 2000000, 3000000, 4000000, 5000000, 6000000, 7000000, 8000000, 9000000, 10000000, ...
everything else: 0, 5, 6, 7, 8, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 25, 26, 27, 28, 30, 35, 36, 37, 38, 40, ...

*/
