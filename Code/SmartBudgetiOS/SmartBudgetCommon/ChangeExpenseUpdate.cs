using System;
using System.Json;

namespace SmartBudgetCommon
{
	public class ChangeExpenseUpdate : Change
	{
		private ExpenseResurrector r;
		private JsonObject was_diff;
		private JsonObject now_diff;
		private string id;
		public const string tip = "CEU";
		public ChangeExpenseUpdate (Document doc, JsonObject dic):base(doc, dic["u"])
		{
			r = new ExpenseResurrector (dic);
			id = dic ["id"];
			now_diff = JSONHelper.read_object (dic, "no");
			was_diff = JSONHelper.read_object (dic, "wa");
		}
		public ChangeExpenseUpdate (Document doc, DBExpense ex):base(doc)
		{
			r = new ExpenseResurrector ();
			this.id = ex.id;
			DBExpense was = doc.get_expense (ex.id);
			if( was != null ){
				now_diff = new JsonObject ();
				ex.save_diff (now_diff, was);
			}
		}

		public override void redo(Document doc)
		{
			was_diff = null;
			if (now_diff == null)
				return;
			DBExpense was = doc.get_expense (id);
			if (was == null)
				return;
			DBExpense ex = was.Clone();
			ex.update_from_diff (now_diff); // Now ex differes from was by now_diff

			was_diff = new JsonObject ();
			was.save_diff (was_diff, ex); // So we save was's difference

			r.redo (doc, ex);

			doc.remove_suggestion (was);
			if (!was.planned) {
				doc.update_balance (was.sum2.account, was.sum2.currency, -was.sum2.amount_1000);
				doc.update_balance (was.sum.account, was.sum.currency, -was.sum.amount_1000);
			}
			doc.conn.Update (ex);
			if (!ex.planned) {
				doc.update_balance (ex.sum.account, ex.sum.currency, ex.sum.amount_1000);
				doc.update_balance (ex.sum2.account, ex.sum2.currency, ex.sum2.amount_1000);
			}
			doc.insert_suggestion (ex);
		}
		public override void undo(Document doc)
		{
			if (was_diff == null)
				return;
			DBExpense ex = doc.get_expense (id);
			DBExpense was = ex.Clone();
			was.update_from_diff (was_diff);

			doc.remove_suggestion (ex);
			if (!ex.planned) {
				doc.update_balance (ex.sum2.account, ex.sum2.currency, -ex.sum2.amount_1000);
				doc.update_balance (ex.sum.account, ex.sum.currency, -ex.sum.amount_1000);
			}
			doc.conn.Update (was);
			if (!was.planned) {
				doc.update_balance (was.sum2.account, was.sum2.currency, was.sum2.amount_1000);
				doc.update_balance (was.sum.account, was.sum.currency, was.sum.amount_1000);
			}
			doc.insert_suggestion (was);
			r.undo (doc, ex);
		}
		public override JsonObject save()
		{
			JsonObject result = new JsonObject { {"t",tip}, {"u",uid}, {"id",id} };
			if (was_diff != null)
				result.Add ("wa", was_diff);
			if (now_diff != null)
				result.Add ("no", now_diff);
			r.save (result);
			return result;
		}
	}
}

