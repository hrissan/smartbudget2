using System;
using System.Json;

namespace SmartBudgetCommon
{
	public class ChangeSheetArrange : Change
	{
		private int from;
		private int to;
		private bool skip;
		public const string tip = "ChangeSheetArrange";
		public ChangeSheetArrange (Document doc, JsonObject dic):base(doc, dic["u"])
		{
			from = dic ["from"];
			to = dic ["to"];
			skip = dic ["skip"];
		}
		public ChangeSheetArrange (Document doc, int from, int to):base(doc)
		{
			this.from = from;
			this.to = to;
		}
		public override void redo(Document doc)
		{
			skip = true;
			if (from < 0 || from >= doc.sorted_sheets.Count)
				return;
			if (to < 0 || to >= doc.sorted_sheets.Count)
				return;
			skip = false;
			DBSheet a = doc.sorted_sheets[from];
			doc.conn.Delete (a);
			doc.conn.Execute ("UPDATE DBSheet SET order_pos=order_pos-1 WHERE order_pos > ? and removed <> 1", from);
			a.order_pos = to;
			doc.conn.Execute ("UPDATE DBSheet SET order_pos=order_pos+1 WHERE order_pos >= ? and removed <> 1", to);
			doc.conn.Insert (a);
			doc.reset_sorted_sheets ();
		}
		public override void undo(Document doc)
		{
			if (skip)
				return;
			DBSheet a = doc.sorted_sheets[to];
			doc.conn.Delete (a);
			doc.conn.Execute ("UPDATE DBSheet SET order_pos=order_pos-1 WHERE order_pos > ? and removed <> 1", to);
			a.order_pos = from;
			doc.conn.Execute ("UPDATE DBSheet SET order_pos=order_pos+1 WHERE order_pos >= ? and removed <> 1", from);
			doc.conn.Insert (a);
			doc.reset_sorted_sheets ();
		}
		public override JsonObject save()
		{
			return new JsonObject{ {"t",tip}, {"u",uid}, {"to",to}, {"from",from} , {"skip",skip} };
		}
	}
}