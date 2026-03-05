using System;
using SQLite;

namespace SmartBudgetCommon
{
	public class DBKeyValue
	{
		[PrimaryKey, MaxLength(32)]
		public string key { get; set; }
		public string value { get; set; }
	}
}

