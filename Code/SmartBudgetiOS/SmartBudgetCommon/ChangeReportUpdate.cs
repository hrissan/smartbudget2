using System;
using System.Json;

namespace SmartBudgetCommon
{
	public class ChangeReportUpdate : Change
	{
		private DBReport a = new DBReport();
		private string prev_data;
		public const string tip = "ChangeReportUpdate";
		public ChangeReportUpdate (Document doc, JsonObject dic):base(doc, dic["u"])
		{
			a.id = dic ["id"];
			a.data = dic ["data"];
			prev_data = JSONHelper.read_string(dic, "prev_data", null);
		}
		public ChangeReportUpdate (Document doc, DBReport report):base(doc)
		{
			a.id = report.id;
			a.data = report.data;
		}

		public override void redo(Document doc)
		{
			prev_data = null;
			DBReport rep = doc.get_report(a.id);
			if( rep == null )
				return;
			prev_data = rep.data;
			doc.conn.Execute("update DBReport SET data=? where id=?", a.data, a.id);
			doc.reset_sorted_reports ();
		}
		public override void undo(Document doc)
		{
			if( prev_data == null )
				return;
			doc.conn.Execute("update DBReport SET data=? where id=?", prev_data, a.id);
			doc.reset_sorted_reports ();
		}
		public override JsonObject save()
		{
			JsonObject result = new JsonObject{ {"t",tip}, {"u",uid}, {"id",a.id},
				{"data",a.data}};
			if( prev_data != null )
				result.Add("prev_data", prev_data);
			return result;
		}
	}
}

