using System;
using System.Json;

namespace SmartBudgetCommon
{
	public class ChangeSheetCreate : Change
	{
		private DBSheet a = new DBSheet();
		public const string tip = "ChangeSheetCreate";
		public ChangeSheetCreate (Document doc, JsonObject dic):base(doc, dic["u"])
		{
			a.id = uid;
			a.name = dic ["name"];
		}
		public ChangeSheetCreate (Document doc, string name):base(doc)
		{
			a.id = uid;
			a.name = name;
		}

		public override void redo(Document doc)
		{
			a.order_pos = doc.sorted_sheets.Count;
			doc.conn.Insert (a);
			doc.reset_sorted_sheets ();
		}
		public override void undo(Document doc)
		{
			doc.conn.Delete (a);
			doc.reset_sorted_sheets ();
			//doc.selected_sheet = doc.selected_sheet; // make sure selected always exists
		}
		public override JsonObject save()
		{
			return new JsonObject{ {"t",tip}, {"u",uid}, {"name",a.name} };
		}
	}
}

