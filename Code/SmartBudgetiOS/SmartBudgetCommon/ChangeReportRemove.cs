using System;
using System.Json;
using System.Collections.Generic;

namespace SmartBudgetCommon
{
	public class ChangeReportRemove : Change
	{
		private string id;
		private bool skip;
		public const string tip = "ChangeReportRemove";
		public ChangeReportRemove (Document doc, JsonObject dic):base(doc, dic["u"])
		{
			id = dic ["id"];
			skip = dic ["skip"];
		}
		public ChangeReportRemove (Document doc, string id):base(doc)
		{
			this.id = id;
		}
		public override void redo(Document doc)
		{
			skip = !remove_report(doc, id);
		}
		public static bool remove_report(Document doc, string id)
		{
			List<DBReport> aa = doc.conn.Query<DBReport> ("SELECT * FROM DBReport WHERE id = ? and removed <> 1", id);
			if (aa.Count == 0)
				return false;
			doc.conn.Execute ("UPDATE DBReport SET removed = 1 WHERE id = ?", id);
			doc.conn.Execute ("UPDATE DBReport SET order_pos=order_pos-1 WHERE order_pos > ? and removed <> 1", aa[0].order_pos);
			doc.reset_sorted_reports ();
			return true;
		}
		public static void resurrect_report(Document doc, string id)
		{
			DBReport a = doc.conn.Query<DBReport> ("SELECT * FROM DBReport WHERE id = ?", id)[0];
			doc.conn.Execute ("UPDATE DBReport SET order_pos=order_pos+1 WHERE order_pos >= ? and removed <> 1", a.order_pos);
			doc.conn.Execute("UPDATE DBReport SET removed = 0 WHERE id = ?", id);
			doc.reset_sorted_sheets ();
		}
		public override void undo(Document doc)
		{
			if (skip)
				return;
			resurrect_report (doc,id);
		}
		public override JsonObject save()
		{
			return new JsonObject{ {"t",tip}, {"u",uid}, {"skip",skip}, {"id",id} };
		}
	}
}

