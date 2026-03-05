using System;
using SQLite;
using System.Collections.Generic;

namespace SmartBudgetCommon
{
	public class DBAccountBalance
	{
		public string account { get; set; }
		[MaxLength(3)]
		public string currency { get; set; }
/*		[Ignore]
		public decimal sum { get; set; }
		public long sum_1000 {
			get { return (long)(sum * DBExpense.decimal_to_sqlite); } 
			set { sum = (decimal)value / DBExpense.decimal_to_sqlite; }
		}*/
		public long sum_1000 { get; set; }

		public void update_balance(List<DBAccountBalance> dest, bool positive)
		{
			int ind = dest.FindIndex ((d)=>d.currency == currency );
			if (ind == -1)
				dest.Add (new DBAccountBalance(){currency=currency, sum_1000=positive ? sum_1000 : -sum_1000});
			else {
				if (positive)
					dest[ind].sum_1000 += sum_1000;
				else
					dest[ind].sum_1000 -= sum_1000;
				if (dest [ind].sum_1000 == 0)
					dest.RemoveAt (ind);
			}
		}
		static public long find_balance_1000(List<DBAccountBalance> dest, string currency)
		{
			int ind = dest.FindIndex ((d)=>d.currency == currency );
			if (ind == -1)
				return 0;
			return dest [ind].sum_1000;
		}
		static public void merge_balance(List<DBAccountBalance> dest, List<DBAccountBalance> src, bool positive)
		{
			foreach (var a in src)
				a.update_balance (dest, positive);
		}
		static public void update_balance(List<DBAccountBalance> dest, string cur, long am_1000)
		{
			int ind = dest.FindIndex ((d)=>d.currency == cur );
			if (ind == -1)
				dest.Add (new DBAccountBalance(){currency=cur, sum_1000=am_1000});
			else {
				dest[ind].sum_1000 += am_1000;
				if (dest [ind].sum_1000 == 0)
					dest.RemoveAt (ind);
			}
		}
	}
}

