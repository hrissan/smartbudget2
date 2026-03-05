using System;
using System.Json;

namespace SmartBudgetCommon
{
	public class ChangeAccountUpdate : Change
	{
		private DBAccount a = new DBAccount();
		private string prev_name;
		public const string tip = "ChangeAccountUpdate";
		public ChangeAccountUpdate (Document doc, JsonObject dic):base(doc, dic["u"])
		{
			a.id = dic ["id"];
			a.name = dic ["name"];
			prev_name = JSONHelper.read_string(dic, "prev_name", null);
		}
		public ChangeAccountUpdate (Document doc, DBAccount acc):base(doc)
		{
			a.id = acc.id;
			a.name = acc.name;
		}

		public override void redo(Document doc)
		{
			prev_name = null;
			DBAccount acc = doc.get_account (a.id);
			if (acc == null)
				return;
			prev_name = acc.name;
			doc.conn.Execute("update DBAccount SET name=? where id=?", a.name, a.id);
			doc.reset_sorted_accounts ();
		}
		public override void undo(Document doc)
		{
			if (prev_name == null)
				return;
			doc.conn.Execute("update DBAccount SET name=? where id=?", prev_name, a.id);
			doc.reset_sorted_accounts ();
		}
		public override JsonObject save()
		{
			JsonObject result = new JsonObject{ {"t",tip}, {"u",uid}, {"name",a.name}, {"id",a.id} };
			if (prev_name != null)
				result.Add ("prev_name", prev_name);
			return result;
		}
	}
}