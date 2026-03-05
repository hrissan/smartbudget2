using System;
using SQLite;
using System.Json;
using System.Collections.Generic;

namespace SmartBudgetCommon
{
	public class DBExpense
	{
		//public const decimal MAX_SMARTBUDGET_NUMBER = 999999999999M;
		public const int decimal_to_sqlite = 1000; // we store decimals as longs
		public static decimal to_decimal(long val_1000) {
			return Math.Round(((decimal)val_1000) / decimal_to_sqlite, 3);
		}
		public static long from_decimal(decimal val) {
			return (long)(val * decimal_to_sqlite);
		}
		[PrimaryKey]
		public string id { get; set; }
		public long date { get; set; } // .net-ready - ticks since 01.01.01
		public DBReccurence recurrence { get; set; }
		[MaxLength(128)]
		public string name { get; set; }

		public string category { get; set; }
		public bool is_any_loan() {
			return category == Document.LOAN_CATEGORY || category == Document.LOAN_BACK_CATEGORY;
		}
		public string sheet { get; set; }
		public bool planned { get; set; }

		public Sum sum;
		public Sum sum2;

		public string account {
			get { return sum.account; } 
			set { sum.account = value; }
		}
		//public long amount_1000 {
		//	get { return (long)(sum.amount * decimal_to_sqlite); } 
		//	set { sum.amount = (decimal)value / decimal_to_sqlite; }
		//}
		public long amount_1000 {
			get { return sum.amount_1000; } 
			set { sum.amount_1000 = value; }
		}
		[MaxLength(3)]
		public string currency{
			get { return sum.currency; } 
			set { sum.currency = value; }
		}
		public string account2 {
			get { return sum2.account; } 
			set { sum2.account = value; }
		}
		//public long amount2_1000 {
		//	get { return (long)(sum2.amount * decimal_to_sqlite); } 
		//	set { sum2.amount = (decimal)value / decimal_to_sqlite; }
		//}
		public long amount2_1000 {
			get { return sum2.amount_1000; } 
			set { sum2.amount_1000 = value; }
		}
		[MaxLength(3)]
		public string currency2 {
			get { return sum2.currency; } 
			set { sum2.currency = value; }
		}

		public DBExpense()
		{
			//name = "";
		}
		public DBExpense(JsonObject dic)
		{
//			id = dic ["id"];
			//name = "";
			update_from_diff (dic);
		}
		public void update_from_diff(JsonObject dic)
		{
			name = JSONHelper.read_string (dic, "n", name);
			date = JSONHelper.read_long (dic, "d", date);
			recurrence = (DBReccurence)JSONHelper.read_int (dic, "rec", (int)recurrence);
			sum.account = JSONHelper.read_string (dic, "ac", sum.account);
			sum.currency = JSONHelper.read_string (dic, "cu", sum.currency);
			sum.amount_1000 = JSONHelper.read_decimal_1000 (dic, "am", sum.amount_1000);
			//sum.amount = Math.Max (-DBExpense.MAX_SMARTBUDGET_NUMBER, Math.Min (DBExpense.MAX_SMARTBUDGET_NUMBER, sum.amount));
			sum2.account = JSONHelper.read_string (dic, "ac2", sum2.account);
			sum2.currency = JSONHelper.read_string (dic, "cu2", sum2.currency);
			sum2.amount_1000 = JSONHelper.read_decimal_1000 (dic, "am2", sum2.amount_1000);
			//sum2.amount = Math.Max (-DBExpense.MAX_SMARTBUDGET_NUMBER, Math.Min (DBExpense.MAX_SMARTBUDGET_NUMBER, sum2.amount));
			category = JSONHelper.read_string (dic, "ca", category);
			sheet = JSONHelper.read_string (dic, "sh", sheet);
			planned = JSONHelper.read_bool (dic, "p", planned);
		}
		public void save(JsonObject dic)
		{
//			dic.Add ("id",id); 
			dic.Add ("ac",sum.account); 
			dic.Add ("am",to_decimal(sum.amount_1000));
			dic.Add ("cu",sum.currency); 
			if (!String.IsNullOrEmpty(name))
				dic.Add ("n", name);
			if (recurrence != DBReccurence.NEVER)
				dic.Add ("rec", (int)recurrence);
			if (date != 0)
				dic.Add ("d", date);
			if (!String.IsNullOrEmpty(category))
				dic.Add ("ca", category);
			if (!String.IsNullOrEmpty(sheet))
				dic.Add ("sh", sheet);
			if (planned)
				dic.Add ("p", 1);
			if (sum2.IsValid()) {
				dic.Add ("ac2", sum2.account);
				dic.Add ("am2", to_decimal(sum2.amount_1000));
				dic.Add ("cu2", sum2.currency);
			}
		}
		public void save_diff(JsonObject dic, DBExpense was)
		{
			if( sum.account != was.sum.account )
				dic.Add ("ac", String.IsNullOrEmpty(sum.account) ? "" : sum.account); 
			if( sum.amount_1000 != was.sum.amount_1000 )
				dic.Add ("am", to_decimal(sum.amount_1000)); 
			if( sum.currency != was.sum.currency )
				dic.Add ("cu", String.IsNullOrEmpty(sum.currency) ? "" : sum.currency); 
			if( name != was.name )
				dic.Add ("n", String.IsNullOrEmpty(name) ? "" : name);
			if( recurrence != was.recurrence )
				dic.Add ("rec", (int)recurrence);
			if ( date != was.date)
				dic.Add ("d", date);
			if (category != was.category)
				dic.Add ("ca", String.IsNullOrEmpty(category) ? "" : category);
			if (sheet != was.sheet)
				dic.Add ("sh", String.IsNullOrEmpty(sheet) ? "" : sheet);
			if (planned != was.planned)
				dic.Add ("p", planned ? 1 : 0);
			if( sum2.account != was.sum2.account )
				dic.Add ("ac2", String.IsNullOrEmpty(sum2.account) ? "" : sum2.account); 
			if( sum2.amount_1000 != was.sum2.amount_1000 )
				dic.Add ("am2", to_decimal(sum2.amount_1000)); 
			if( sum2.currency != was.sum2.currency )
				dic.Add ("cu2", String.IsNullOrEmpty(sum2.currency) ? "" : sum2.currency); 
		}
/*		public static void fix_ids(JsonObject dic, int before_sent_size, int delta)
		{
			try{
				int sh = dic["sh"];
				if( sh != 0 && sh > before_sent_size )
					sh += delta;
				dic["sh"] = sh;
			}catch(Exception){
			}		
			try{
				int ca = dic["ca"];
				if( ca != 0 && ca > before_sent_size )
					ca += delta;
				dic["ca"] = ca;
			}catch(Exception){
			}		
			try{
				int ac = dic["ac"];
				if( ac != 0 && ac > before_sent_size )
					ac += delta;
				dic["ac"] = ac;
			}catch(Exception){
			}		
			try{
				int ac2 = dic["ac2"];
				if( ac2 != 0 && ac2 > before_sent_size )
					ac2 += delta;
				dic["ac2"] = ac2;
			}catch(Exception){
			}		
		}
		public void fix_ids(int before_sent_size, int delta)
		{
			if( id > before_sent_size )
				id += delta;
			if( sheet != 0 && sheet > before_sent_size )
				sheet += delta;
			if (category != 0 && category > before_sent_size)
				category += delta;
			if (account != 0 && account > before_sent_size)
				account += delta;
			if (account2 != 0 && account2 > before_sent_size)
				account2 += delta;
		}*/
		public DBExpense Clone()
		{
			return new DBExpense () { id=id,date=date,recurrence=recurrence,
				name=name,category=category,sheet=sheet,
				planned=planned,sum=sum,sum2=sum2,
			};
		}
		public bool IsEqualToExpense(DBExpense ex)
		{
			return id == ex.id && date == ex.date && recurrence == ex.recurrence &&
				name == ex.name && category == ex.category && sheet == ex.sheet && 
					sum.account == ex.sum.account && sum.amount_1000 == ex.sum.amount_1000 && sum.currency == ex.sum.currency &&
					sum2.account == ex.sum2.account && sum2.amount_1000 == ex.sum2.amount_1000 && sum2.currency == ex.sum2.currency;
		}
		public static long find_next_date(long date, DBReccurence rec)
		{
			if (date == 0)
				return 0;
			DateTime d = new DateTime (date, DateTimeKind.Local);
			switch(rec)
			{
				case DBReccurence.NEVER:
				break;
				case DBReccurence.EVERY_DAY: d = d.AddDays(1); break;
				case DBReccurence.EVERY_WEEK: d = d.AddDays(7); break;
				case DBReccurence.EVERY_2WEEKS: d = d.AddDays(14); break;
				case DBReccurence.EVERY_MONTH: d = d.AddMonths(1); break;
				case DBReccurence.EVERY_2MONTHS: d = d.AddMonths(2); break;
				case DBReccurence.EVERY_QUARTER: d = d.AddMonths(3); break;
				case DBReccurence.TWICE_A_YEAR: d = d.AddMonths(6); break;
				case DBReccurence.EVERY_YEAR: d = d.AddYears(1); break;
				case DBReccurence.RECURRENCE_COUNT: break; // Suppress warning
			}
			return d.Ticks;
		}
		public static DateTime find_next_date(DateTime d, DBReccurence rec)
		{
			switch(rec)
			{
				case DBReccurence.NEVER:
				break;
				case DBReccurence.EVERY_DAY: d = d.AddDays(1); break;
				case DBReccurence.EVERY_WEEK: d = d.AddDays(7); break;
				case DBReccurence.EVERY_2WEEKS: d = d.AddDays(14); break;
				case DBReccurence.EVERY_MONTH: d = d.AddMonths(1); break;
				case DBReccurence.EVERY_2MONTHS: d = d.AddMonths(2); break;
				case DBReccurence.EVERY_QUARTER: d = d.AddMonths(3); break;
				case DBReccurence.TWICE_A_YEAR: d = d.AddMonths(6); break;
				case DBReccurence.EVERY_YEAR: d = d.AddYears(1); break;
				case DBReccurence.RECURRENCE_COUNT: break; // Suppress warning
			}
			return d;
		}
		private static decimal[] year_multiplier = new decimal[]{0m, 365.25m, 52m, 26m, 12m, 6m, 4m, 2m, 1m };
		public static decimal get_month_multiplier(DBReccurence rec)
		{
			int val = (int)rec;
			if (val >= (int)DBReccurence.RECURRENCE_COUNT || val < 0)
				return 1;
			return Math.Round(year_multiplier[val] / 12, 3);
		}
/*		public static System.DateTime epoch = new DateTime(1970,1,1,0,0,0,0,DateTimeKind.Utc);
		public static long from_unix_to_ticks(double date)
		{
			if (date == 0)
				return 0;
			DateTime d = epoch; 
			d.AddSeconds (date);
			return d.Ticks;
		}
		public static double from_ticks_to_unix(long ticks)
		{
			if (ticks == 0)
				return 0;
			DateTime d = new DateTime (ticks, DateTimeKind.Utc);
			return d.Subtract (epoch).TotalSeconds;
		}*/
		public static void move_without_date_to_start(List<List<DBExpense>> result)
		{
			if (result.Count == 0)
				return;
			List<DBExpense> lali = result [result.Count - 1];
			if( lali[0].date == 0 ) {
				result.Insert (0, lali);
				result.RemoveAt (result.Count - 1);
			}
		}
	}
}

