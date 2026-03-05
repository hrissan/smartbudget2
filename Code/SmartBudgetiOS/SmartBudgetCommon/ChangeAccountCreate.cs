using System;
using System.Json;

namespace SmartBudgetCommon
{
	public class ChangeAccountCreate : Change
	{
		private DBAccount a = new DBAccount();
		public const string tip = "ChangeAccountCreate";
		public ChangeAccountCreate (Document doc, JsonObject dic):base(doc, dic["u"])
		{
			a.id = uid;
			a.name = dic ["name"];
		}
		public ChangeAccountCreate (Document doc, string name):base(doc)
		{
			a.id = uid;
			a.name = name;
		}

		public override void redo(Document doc)
		{
			a.order_pos = doc.hidden_account_line;
			doc.conn.Execute ("UPDATE DBAccount SET order_pos=order_pos+1 WHERE order_pos >= ? and removed <> 1", a.order_pos);
			doc.conn.Insert (a);
			doc.hidden_account_line += 1;
			doc.reset_sorted_accounts ();
		}
		public override void undo(Document doc)
		{
			doc.hidden_account_line -= 1;
			doc.conn.Delete (a);
			doc.conn.Execute ("UPDATE DBAccount SET order_pos=order_pos-1 WHERE order_pos > ? and removed <> 1", doc.hidden_account_line);
			doc.reset_sorted_accounts ();
		}
		public override JsonObject save()
		{
			return new JsonObject{ {"t",tip}, {"u",uid}, {"name",a.name} };
		}
	}
}

