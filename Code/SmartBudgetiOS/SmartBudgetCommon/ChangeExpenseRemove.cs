using System;
using System.Json;
using System.Collections.Generic;

namespace SmartBudgetCommon
{
	public class ChangeExpenseRemove : Change
	{
		private string id;
		private DBExpense was;
		public const string tip = "ChangeExpenseRemove";
		public ChangeExpenseRemove (Document doc, JsonObject dic):base(doc, dic["u"])
		{
			this.id = dic["id"];
			JsonObject jwas = JSONHelper.read_object (dic, "wa");
			if (jwas != null) {
				was = new DBExpense (jwas);
				was.id = id;
			}
		}
		public ChangeExpenseRemove (Document doc, string id):base(doc)
		{
			this.id = id;
		}
		public override void redo(Document doc)
		{
			was = doc.get_expense (id);
			if (was == null)
				return;
			doc.remove_suggestion (was);
			if (!was.planned) {
				doc.update_balance (was.sum2.account, was.sum2.currency, -was.sum2.amount_1000);
				doc.update_balance (was.sum.account, was.sum.currency, -was.sum.amount_1000);
			}
			doc.conn.Delete (was);
		}
		public override void undo(Document doc)
		{
			if (was == null)
				return;
			doc.conn.Insert (was);
			if (!was.planned) {
				doc.update_balance (was.sum2.account, was.sum2.currency, was.sum2.amount_1000);
				doc.update_balance (was.sum.account, was.sum.currency, was.sum.amount_1000);
			}
			doc.insert_suggestion (was);
		}
		public override JsonObject save()
		{
			JsonObject result = new JsonObject{ {"t",tip}, {"u",uid}, {"id",id} };
			if (was != null) {
				JsonObject jwas = new JsonObject ();
				was.save (jwas);
				result.Add ("wa", jwas);
			}
			return result;
		}
	}
}