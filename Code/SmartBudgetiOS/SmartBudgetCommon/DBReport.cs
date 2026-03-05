using System;
using SQLite;

namespace SmartBudgetCommon
{
	public class DBReport
	{
		[PrimaryKey]
		public string id { get; set; }
		public int order_pos { get; set; }
		public string data { get; set; }
		public bool removed { get; set; } // for expense resurrector
	}
}

