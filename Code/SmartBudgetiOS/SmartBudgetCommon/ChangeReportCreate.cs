using System;
using System.Json;

namespace SmartBudgetCommon
{
	public class ChangeReportCreate : Change
	{
		private DBReport report = new DBReport(); // For undo
		public const string tip = "ChangeReportCreate";
		public ChangeReportCreate (Document doc, JsonObject dic):base(doc, dic["u"])
		{
			report.data = dic ["data"];
			report.id = uid;
		}
		public ChangeReportCreate (Document doc, string data):base(doc)
		{
			report.data = data;
			report.id = uid;
		}

		public override void redo(Document doc)
		{
			report.order_pos = doc.sorted_reports.Count;
			doc.conn.Insert (report);
			doc.reset_sorted_reports ();
		}
		public override void undo(Document doc)
		{
			doc.conn.Delete (report);
			doc.reset_sorted_reports ();
		}
		public override JsonObject save()
		{
			JsonObject result = new JsonObject { {"t",tip}, {"u",uid},
				{"data", report.data}};
			return result;
		}
	}
}
