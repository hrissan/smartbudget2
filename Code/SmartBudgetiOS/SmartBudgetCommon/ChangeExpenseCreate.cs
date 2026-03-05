using System;
using System.Json;

namespace SmartBudgetCommon
{
	public class ChangeExpenseCreate : Change
	{
		private ExpenseResurrector r;
		private DBExpense ex;
		public const string tip = "CEC";
		public ChangeExpenseCreate (Document doc, JsonObject dic):base(doc, dic["u"])
		{
			r = new ExpenseResurrector (dic);
			this.ex = new DBExpense (dic);
			this.ex.id = uid;
		}
		public ChangeExpenseCreate (Document doc, DBExpense ex):base(doc)
		{
			r = new ExpenseResurrector ();
			this.ex = ex.Clone();
			this.ex.id = uid;
		}

		public override void redo(Document doc)
		{
			r.redo (doc, ex);
			doc.conn.Insert (ex);
			if (!ex.planned) {
				doc.update_balance (ex.sum.account, ex.sum.currency, ex.sum.amount_1000);
				doc.update_balance (ex.sum2.account, ex.sum2.currency, ex.sum2.amount_1000);
			}
			doc.insert_suggestion (ex);
		}
		public override void undo(Document doc)
		{
			doc.remove_suggestion (ex);
			if (!ex.planned) {
				doc.update_balance (ex.sum2.account, ex.sum2.currency, -ex.sum2.amount_1000);
				doc.update_balance (ex.sum.account, ex.sum.currency, -ex.sum.amount_1000);
			}
			doc.conn.Delete (new DBExpense(){ id=ex.id });
			r.undo (doc, ex);
		}
		public override JsonObject save()
		{
			JsonObject result = new JsonObject { {"t",tip}, {"u",uid} };
			ex.save(result);
			r.save (result);
			return result;
		}
	}
}

