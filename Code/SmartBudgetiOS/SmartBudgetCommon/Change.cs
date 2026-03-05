using System;
using System.Json;
using System.Security.Cryptography;
using System.Globalization;

namespace SmartBudgetCommon
{
	abstract public class Change
	{
		public abstract void redo(Document doc);
		public abstract void undo(Document doc);
		public abstract JsonObject save();
		//public abstract void fix_ids(Document doc, int before_sent_size, int delta);
		protected string uid;
		protected Document doc;
		public Document get_doc()
		{
			return doc;
		}
		public string get_uid()
		{
			return uid;
		}
		public Change(Document doc){
			this.doc = doc;
			RNGCryptoServiceProvider p = new RNGCryptoServiceProvider ();
			byte[] bytes = new byte[6];
			p.GetBytes (bytes);
			string postfix = Convert.ToBase64String (bytes);
			string prefix;
			int cmd = doc.commands_size;
			if (cmd < (1 << 12))
				prefix = cmd.ToString ("X3", CultureInfo.InvariantCulture);
			else if (cmd < (1 << 16))
				prefix = "H" + cmd.ToString ("X4", CultureInfo.InvariantCulture); 
			else
				prefix = "Q" + cmd.ToString ("X8", CultureInfo.InvariantCulture); 
			// H is greater than any hex digit, Q is arbitrary in the middle
			this.uid = prefix + postfix;
		}
		public Change(Document doc, string uid){
			this.doc = doc;
			this.uid = uid;
		}

		public class FutureCommandException : Exception {
			public FutureCommandException(string Message):base(Message)
			{
			}
		};
		public static Change create_change(Document doc, JsonObject dic)
		{
			string tip = dic ["t"];
			if (tip == ChangeExpenseCreate.tip)
				return new ChangeExpenseCreate (doc, dic);
			if (tip == ChangeExpenseUpdate.tip)
				return new ChangeExpenseUpdate (doc, dic);
			if (tip == ChangeExpenseRemove.tip)
				return new ChangeExpenseRemove (doc, dic);

			if (tip == ChangeAccountCreate.tip)
				return new ChangeAccountCreate (doc, dic);
			if (tip == ChangeAccountRemove.tip)
				return new ChangeAccountRemove (doc, dic);
			if (tip == ChangeAccountArrange.tip)
				return new ChangeAccountArrange (doc, dic);
			if (tip == ChangeAccountUpdate.tip)
				return new ChangeAccountUpdate (doc, dic);

			if (tip == ChangeCategoryCreate.tip)
				return new ChangeCategoryCreate (doc, dic);
			if (tip == ChangeCategoryRemove.tip)
				return new ChangeCategoryRemove (doc, dic);
			if (tip == ChangeCategoryUpdate.tip)
				return new ChangeCategoryUpdate (doc, dic);

			if (tip == ChangeSheetCreate.tip)
				return new ChangeSheetCreate (doc, dic);
			if (tip == ChangeSheetRemove.tip)
				return new ChangeSheetRemove (doc, dic);
			if (tip == ChangeSheetArrange.tip)
				return new ChangeSheetArrange (doc, dic);
			if (tip == ChangeSheetUpdate.tip)
				return new ChangeSheetUpdate (doc, dic);

			if (tip == ChangeReportCreate.tip)
				return new ChangeReportCreate (doc, dic);
			if (tip == ChangeReportRemove.tip)
				return new ChangeReportRemove (doc, dic);
			if (tip == ChangeReportArrange.tip)
				return new ChangeReportArrange (doc, dic);
			if (tip == ChangeReportUpdate.tip)
				return new ChangeReportUpdate (doc, dic);
			throw new FutureCommandException ("create_change class not found" + tip);
		}
	}
}

