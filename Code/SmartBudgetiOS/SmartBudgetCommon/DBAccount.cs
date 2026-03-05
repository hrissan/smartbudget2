using System;
using SQLite;

namespace SmartBudgetCommon
{
	public class DBAccount
	{
		[PrimaryKey]
		public string id { get; set; }
		[MaxLength(128)]
		public string name { get; set; }
		public int order_pos { get; set; }
		public bool removed { get; set; } // for expense resurrector
	}
}

