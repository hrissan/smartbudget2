using System;
using System.Json;
using System.Collections.Generic;

namespace SmartBudgetCommon
{
	public class ChangeCategoryRemove : Change
	{
		private string id;
		private bool skip;
		public const string tip = "ChangeCategoryRemove";
		public ChangeCategoryRemove (Document doc, JsonObject dic):base(doc, dic["u"])
		{
			id = dic ["id"];
			skip = dic ["skip"];
		}
		public ChangeCategoryRemove (Document doc, string id):base(doc)
		{
			this.id = id;
		}
		public override void redo(Document doc)
		{
			skip = !remove_category(doc, id);
		}
		public static bool remove_category(Document doc, string id)
		{
			DBCategory a = doc.get_category (id);
			if (a == null)
				return false;
			int expense_count;
			long date;
			doc.get_category_count_date(id, out expense_count, out date);
			if (expense_count != 0)
				return false;
			doc.conn.Execute ("UPDATE DBCategory SET removed = 1 WHERE id = ?", id);
			doc.reset_sorted_categories ();
			return true;
		}
		public static void resurrect_category(Document doc, string id)
		{
			doc.conn.Execute("UPDATE DBCategory SET removed = 0 WHERE id = ?", id);
			doc.reset_sorted_categories ();
		}
		public override void undo(Document doc)
		{
			if (skip)
				return;
			resurrect_category (doc,id);
		}
		public override JsonObject save()
		{
			return new JsonObject{ {"t",tip}, {"u",uid}, {"skip",skip}, {"id",id} };
		}
	}
}