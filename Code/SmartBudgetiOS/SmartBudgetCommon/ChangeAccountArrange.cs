using System;
using System.Json;

namespace SmartBudgetCommon
{
	public class ChangeAccountArrange : Change
	{
		private int from;
		private int to;
		private int mhal;
		private bool skip;
		public const string tip = "ChangeAccountArrange";
		public ChangeAccountArrange (Document doc, JsonObject dic):base(doc, dic["u"])
		{
			from = dic ["from"];
			to = dic ["to"];
			skip = dic ["skip"];
			mhal = dic ["mhal"];
		}
		public ChangeAccountArrange (Document doc, int from, int to, int mhal):base(doc)
		{
			this.from = from;
			this.to = to;
			this.mhal = mhal;
		}
		public static bool arrange(Document doc, int from, int to, int mhal)
		{
			if (from < 0 || from >= doc.sorted_accounts.Count)
				return false;
			if (to < 0 || to >= doc.sorted_accounts.Count)
				return false;
			if (doc.hidden_account_line + mhal < 0 || doc.hidden_account_line + mhal > doc.sorted_accounts.Count)
				return false;
			DBAccount a = doc.sorted_accounts[from];
			doc.conn.Delete (a);
			doc.conn.Execute ("UPDATE DBAccount SET order_pos=order_pos-1 WHERE order_pos > ? and removed <> 1", from);
			a.order_pos = to;
			doc.conn.Execute ("UPDATE DBAccount SET order_pos=order_pos+1 WHERE order_pos >= ? and removed <> 1", to);
			doc.conn.Insert (a);
			doc.hidden_account_line += mhal;
			doc.reset_sorted_accounts ();
			return true;
		}
		public override void redo(Document doc)
		{
			skip = !arrange(doc, from, to, mhal);
		}
		public override void undo(Document doc)
		{
			if (skip)
				return;
			DBAccount a = doc.sorted_accounts[to];
			doc.hidden_account_line -= mhal;
			doc.conn.Delete (a);
			doc.conn.Execute ("UPDATE DBAccount SET order_pos=order_pos-1 WHERE order_pos > ? and removed <> 1", to);
			a.order_pos = from;
			doc.conn.Execute ("UPDATE DBAccount SET order_pos=order_pos+1 WHERE order_pos >= ? and removed <> 1", from);
			doc.conn.Insert (a);
			doc.reset_sorted_accounts ();
		}
		public override JsonObject save()
		{
			return new JsonObject{ {"t",tip}, {"u",uid}, {"to",to}, {"from",from} , {"mhal",mhal}, {"skip",skip} };
		}
	}
}