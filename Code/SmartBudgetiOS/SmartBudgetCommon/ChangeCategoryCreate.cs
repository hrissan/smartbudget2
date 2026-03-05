using System;
using System.Json;

namespace SmartBudgetCommon
{
	public class ChangeCategoryCreate : Change
	{
		private DBCategory a;
		public const string tip = "ChangeCategoryCreate";
		public ChangeCategoryCreate (Document doc, JsonObject dic):base(doc, dic["u"])
		{
			a = new DBCategory (dic);
			a.id = uid;
		}
		public ChangeCategoryCreate (Document doc, DBCategory cat):base(doc)
		{
			a = cat;
			a.id = uid;
		}

		public override void redo(Document doc)
		{
			doc.conn.Insert (a);
			doc.reset_sorted_categories ();
		}
		public override void undo(Document doc)
		{
			doc.conn.Delete (a);
			doc.reset_sorted_categories ();
		}
		public override JsonObject save()
		{
			JsonObject result = new JsonObject { {"t",tip}, {"u",uid} };
			a.save (result);
			return result;
		}
	}
}

