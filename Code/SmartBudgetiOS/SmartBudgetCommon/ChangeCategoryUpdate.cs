using System;
using System.Json;

namespace SmartBudgetCommon
{
	public class ChangeCategoryUpdate : Change
	{
		private string id;
		private JsonObject was_diff;
		private JsonObject now_diff;
		public const string tip = "ChangeCategoryUpdate";
		public ChangeCategoryUpdate (Document doc, JsonObject dic):base(doc, dic["u"])
		{
			id = dic ["id"];
			now_diff = JSONHelper.read_object (dic, "no");
			was_diff = JSONHelper.read_object (dic, "wa");
		}
		public ChangeCategoryUpdate (Document doc, DBCategory cat):base(doc)
		{
			this.id = cat.id;
			DBCategory was = doc.get_category (cat.id);
			if( was != null ){
				now_diff = new JsonObject ();
				cat.save_diff (now_diff, was);
			}
		}

		public override void redo(Document doc)
		{
			was_diff = null;
			if (now_diff == null)
				return;
			DBCategory was = doc.get_category(id);
			if (was == null)
				return;
			DBCategory ex = was.Clone();
			ex.update_from_diff (now_diff); // Now ex differes from was by now_diff

			was_diff = new JsonObject ();
			was.save_diff (was_diff, ex); // So we save was's difference

			doc.conn.Update (ex);
			doc.reset_sorted_categories ();
		}
		public override void undo(Document doc)
		{
			if (was_diff == null)
				return;
			DBCategory ex = doc.get_category(id);
			DBCategory was = ex.Clone();
			was.update_from_diff (was_diff);

			doc.conn.Update (was);
			doc.reset_sorted_categories ();
		}
		public override JsonObject save()
		{
			JsonObject result = new JsonObject{ {"t",tip}, {"u",uid}, {"id",id} };
			if (was_diff != null)
				result.Add ("wa", was_diff);
			if (now_diff != null)
				result.Add ("no", now_diff);
			return result;
		}
	}
}