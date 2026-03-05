using System;
using System.Json;
using System.Collections.Generic;

namespace SmartBudgetCommon
{
	public class ChangeAccountRemove : Change
	{
		private string id;
		private bool skip;
		public const string tip = "ChangeAccountRemove";
		public ChangeAccountRemove (Document doc, JsonObject dic):base(doc,dic["u"])
		{
			id = dic ["id"];
			skip = dic ["skip"];
		}
		public ChangeAccountRemove (Document doc, string id):base(doc)
		{
			this.id = id;
		}
		public override void redo(Document doc)
		{
			skip = doc.sorted_accounts.Count < 2 ? true : !remove_account(doc, id);
		}
		public static bool remove_account(Document doc, string id)
		{
			DBAccount a = doc.get_account (id);
			if (a == null)
				return false;
			if(doc.hidden_account_line > a.order_pos )
				doc.hidden_account_line -= 1;
			doc.conn.Execute ("UPDATE DBAccount SET removed = 1 WHERE id = ?", id);
			doc.conn.Execute ("UPDATE DBAccount SET order_pos=order_pos-1 WHERE order_pos > ? and removed <> 1", a.order_pos);
			doc.reset_sorted_accounts ();
			return true;
		}
		public static void resurrect_account(Document doc, string id)
		{
			DBAccount a = doc.conn.Query<DBAccount> ("SELECT * FROM DBAccount WHERE id = ?", id)[0];
			doc.conn.Execute ("UPDATE DBAccount SET order_pos=order_pos+1 WHERE order_pos >= ? and removed <> 1", a.order_pos);
			doc.conn.Execute("UPDATE DBAccount SET removed = 0 WHERE id = ?", id);
			if(doc.hidden_account_line >= a.order_pos )
				doc.hidden_account_line += 1;
			doc.reset_sorted_accounts ();
		}
		public override void undo(Document doc)
		{
			if (skip)
				return;
			resurrect_account (doc, id);
		}
		public override JsonObject save()
		{
			return new JsonObject{ {"t",tip}, {"u",uid}, {"skip",skip}, {"id",id} };
		}
	}
}