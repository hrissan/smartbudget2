using System;
using SQLite;

namespace SmartBudgetCommon
{
	public class DBCommand
	{
		[PrimaryKey]
		public int id { get; set; }
		public string data { get; set; }
	}
}

