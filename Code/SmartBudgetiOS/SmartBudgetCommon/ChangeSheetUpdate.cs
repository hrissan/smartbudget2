using System;
using System.Json;

namespace SmartBudgetCommon
{
	public class ChangeSheetUpdate : Change
	{
		private DBSheet a = new DBSheet();
		private string prev_name;
		public const string tip = "ChangeSheetUpdate";
		public ChangeSheetUpdate (Document doc, JsonObject dic):base(doc, dic["u"])
		{
			a.id = dic ["id"];
			a.name = dic ["name"];
			prev_name = JSONHelper.read_string(dic, "prev_name", null);
		}
		public ChangeSheetUpdate (Document doc, DBSheet sh):base(doc)
		{
			a.id = sh.id;
			a.name = sh.name;
		}

		public override void redo(Document doc)
		{
			prev_name = null;
			DBSheet sh = doc.get_sheet(a.id);
			if (sh == null)
				return;
			prev_name = sh.name;
			doc.conn.Execute("update DBSheet SET name=? where id=?", a.name, a.id);
			doc.reset_sorted_sheets ();
		}
		public override void undo(Document doc)
		{
			if (prev_name == null)
				return;
			doc.conn.Execute("update DBSheet SET name=? where id=?", prev_name, a.id);
			doc.reset_sorted_sheets ();
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