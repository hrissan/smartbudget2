using System;
using System.Json;
using System.Collections.Generic;

namespace SmartBudgetCommon
{
	public class ChangeSheetRemove : Change
	{
		private string id;
		private bool skip;
		public const string tip = "ChangeSheetRemove";
		public ChangeSheetRemove (Document doc, JsonObject dic):base(doc, dic["u"])
		{
			id = dic ["id"];
			skip = dic ["skip"];
		}
		public ChangeSheetRemove (Document doc, string id):base(doc)
		{
			this.id = id;
		}
		public override void redo(Document doc)
		{
			skip = doc.sorted_sheets.Count < 2 ? true : !remove_sheet(doc, id);
		}
		public static bool remove_sheet(Document doc, string id)
		{
			DBSheet a = doc.get_sheet (id);
			if (a == null)
				return false;
			int expense_count;
			long date;
			doc.get_sheet_count_date(id, out expense_count, out date);
			if (expense_count != 0)
				return false;
			doc.conn.Execute ("UPDATE DBSheet SET removed = 1 WHERE id = ?", id);
			doc.conn.Execute ("UPDATE DBSheet SET order_pos=order_pos-1 WHERE order_pos > ? and removed <> 1", a.order_pos);
			doc.reset_sorted_sheets ();
			return true;
		}
		public static void resurrect_sheet(Document doc, string id)
		{
			DBSheet a = doc.conn.Query<DBSheet> ("SELECT * FROM DBSheet WHERE id = ?", id)[0];
			doc.conn.Execute ("UPDATE DBSheet SET order_pos=order_pos+1 WHERE order_pos >= ? and removed <> 1", a.order_pos);
			doc.conn.Execute("UPDATE DBSheet SET removed = 0 WHERE id = ?", id);
			doc.reset_sorted_sheets ();
		}
		public override void undo(Document doc)
		{
			if (skip)
				return;
			resurrect_sheet (doc,id);
		}
		public override JsonObject save()
		{
			return new JsonObject{ {"t",tip}, {"u",uid}, {"skip",skip}, {"id",id} };
		}
	}
}