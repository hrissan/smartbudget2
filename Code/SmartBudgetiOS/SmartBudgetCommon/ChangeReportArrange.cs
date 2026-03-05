using System;
using System.Json;

namespace SmartBudgetCommon
{
	public class ChangeReportArrange : Change
	{
		private int from;
		private int to;
		private bool skip;
		public const string tip = "ChangeReportArrange";
		public ChangeReportArrange (Document doc, JsonObject dic):base(doc, dic["u"])
		{
			from = dic ["from"];
			to = dic ["to"];
			skip = dic ["skip"];
		}
		public ChangeReportArrange (Document doc, int from, int to):base(doc)
		{
			this.from = from;
			this.to = to;
		}
		public override void redo(Document doc)
		{
			skip = true;
			if (from < 0 || from >= doc.sorted_reports.Count)
				return;
			if (to < 0 || to >= doc.sorted_reports.Count)
				return;
			skip = false;
			DBReport a = doc.sorted_reports[from];
			doc.conn.Delete (new DBReport{id=a.id});
			doc.conn.Execute ("UPDATE DBReport SET order_pos=order_pos-1 WHERE order_pos > ? and removed <> 1", from);
//			a.order_pos = to;
			doc.conn.Execute ("UPDATE DBReport SET order_pos=order_pos+1 WHERE order_pos >= ? and removed <> 1", to);
			doc.conn.Insert (new DBReport{id=a.id, order_pos=to,removed=false,data=a.data});
			doc.reset_sorted_reports ();
		}
		public override void undo(Document doc)
		{
			if (skip)
				return;
			DBReport a = doc.sorted_reports[to];
			doc.conn.Delete (new DBReport{id=a.id});
			doc.conn.Execute ("UPDATE DBReport SET order_pos=order_pos-1 WHERE order_pos > ? and removed <> 1", to);
//			a.order_pos = from;
			doc.conn.Execute ("UPDATE DBReport SET order_pos=order_pos+1 WHERE order_pos >= ? and removed <> 1", from);
			doc.conn.Insert (new DBReport{id=a.id, order_pos=from,removed=false,data=a.data});
			doc.reset_sorted_reports ();
		}
		public override JsonObject save()
		{
			return new JsonObject{ {"t",tip}, {"u",uid}, {"to",to}, {"from",from} , {"skip",skip} };
		}
	}
}

