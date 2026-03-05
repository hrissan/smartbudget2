using System;
using SQLite;

namespace SmartBudgetCommon
{
	public class DBSheet
	{
		[PrimaryKey]
		public string id { get; set; }
		[MaxLength(128)]
		public string name { get; set; }
		public int order_pos { get; set; }
		public bool removed { get; set; } // for expense resurrector

		public string get_loc_name()
		{
			if (!String.IsNullOrEmpty (name))
				return name;
			return i18n.get ("EverydaySheet");
		}
	}
}

