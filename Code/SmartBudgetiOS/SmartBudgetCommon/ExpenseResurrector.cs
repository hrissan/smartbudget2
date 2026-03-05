using System;
using System.Json;

namespace SmartBudgetCommon
{
	public class ExpenseResurrector
	{
		private bool resurrect_category;
		private bool resurrect_sheet;
		private bool resurrect_account;
		private bool resurrect_account2;
		public ExpenseResurrector ()
		{
		}
		public ExpenseResurrector (JsonObject dic)
		{
			resurrect_category = JSONHelper.read_bool(dic, "r_category", false);
			resurrect_sheet = JSONHelper.read_bool(dic, "r_sheet", false);
			resurrect_account = JSONHelper.read_bool(dic, "r_account", false);
			resurrect_account2 = JSONHelper.read_bool(dic, "r_account2", false);
		}
		public void redo(Document doc, DBExpense ex)
		{
			resurrect_category = false;
			resurrect_sheet = false;
			resurrect_account = false;
			resurrect_account2 = false;
			if (!String.IsNullOrEmpty(ex.sheet) && doc.get_sheet (ex.sheet) == null) {
				ChangeSheetRemove.resurrect_sheet (doc, ex.sheet);
				resurrect_sheet = true;
			}
			if (!String.IsNullOrEmpty(ex.account) && doc.get_account (ex.account) == null) {
				ChangeAccountRemove.resurrect_account(doc, ex.account);
				resurrect_account = true;
			}
			if (!String.IsNullOrEmpty(ex.account2) && doc.get_account (ex.account2) == null) {
				ChangeAccountRemove.resurrect_account(doc, ex.account2);
				resurrect_account2 = true;
			}
			if (!String.IsNullOrEmpty(ex.category) && doc.get_category (ex.category) == null) {
				ChangeCategoryRemove.resurrect_category (doc, ex.category);
				resurrect_category = true;
			}
		}
		public void undo(Document doc, DBExpense ex)
		{
			if (resurrect_sheet) {
				ChangeSheetRemove.remove_sheet (doc, ex.sheet);
			}
			if (resurrect_account) {
				ChangeAccountRemove.remove_account (doc, ex.account);
			}
			if (resurrect_account2) {
				ChangeAccountRemove.remove_account (doc, ex.account2);
			}
			if (resurrect_category) {
				ChangeCategoryRemove.remove_category (doc, ex.category);
			}
		}
		public void save(JsonObject dic)
		{
			if (resurrect_category)
				dic.Add ("r_category", 1);
			if (resurrect_sheet)
				dic.Add ("r_sheet", 1);
			if (resurrect_account)
				dic.Add ("r_account", 1);
			if (resurrect_account2)
				dic.Add ("r_account2", 1);
		}
	}
}

