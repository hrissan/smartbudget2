using System;
using SQLite;

namespace SmartBudgetCommon
{
	public class DBExpenseString
	{
		public string sheet { get; set; }
		public string category { get; set; }
		[MaxLength(32)]
		public string word { get; set; }
		public bool planned { get; set; }
		public long date { get; set; }
		public string expense_id { get; set; }
		[MaxLength(128)]
		public string expense_name { get; set; }
	}
}

