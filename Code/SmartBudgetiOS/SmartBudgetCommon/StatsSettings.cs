using System;
using System.Json;
using System.Collections.Generic;

namespace SmartBudgetCommon
{
	public class StatsSettings
	{
		public string id;
		public enum ReportStyle { ByCat, ByDay, BySum };
		public ReportStyle style;

		public DateTime from_date;
		public int months;
		public int days;
		public List<string> selected_sheets; // null == all sheets
		public List<string> selected_accounts; // null == all accounts
		public bool separate_planned = true;
		public string report_currency = "USD";

		public void validate(Document doc)
		{
			if( selected_sheets != null ) {
				List<string> new_sh = new List<string> ();
				foreach (var shid in selected_sheets) {
					DBSheet sh = doc.get_sheet(shid);
					if (sh != null) {
						new_sh.Add (shid);
					}
				}
				selected_sheets = new_sh.Count != 0 ? new_sh : null;
			}
			if( selected_accounts != null ) {
				List<string> new_ac = new List<string> ();
				foreach (var acid in selected_accounts) {
					DBAccount acc = doc.get_account(acid);
					if (acc != null) {
						new_ac.Add (acid);
					}
				}
				selected_accounts = new_ac.Count != 0 ? new_ac : null;
			}
		}
		public StatsSettings (string id = "")
		{
			this.id = id;
			style = ReportStyle.ByCat;
			from_date = DateTime.Now;
			int have_year = from_date.Year;
			int have_month = from_date.Month;

			from_date = new DateTime (have_year, have_month, 1, 0, 0, 0, DateTimeKind.Local);

			months = 1;
			days = 0;
		}
		public StatsSettings(string id, string data):this(id)
		{
			try {
				JsonObject json = (JsonObject)JsonObject.Parse (data);
				Enum.TryParse (JSONHelper.read_string (json, "style", ""), out style);
				from_date = new DateTime (JSONHelper.read_long (json, "from_date", 0), DateTimeKind.Local);
				months = JSONHelper.read_int (json, "months", 0);
				days = JSONHelper.read_int (json, "days", 0);
				report_currency = JSONHelper.read_string (json, "report_currency", "USD");
				separate_planned = JSONHelper.read_bool (json, "separate_planned", false);
				JsonArray jsh = JSONHelper.read_array(json, "selected_sheets");
				if( jsh != null ) {
					selected_sheets = new List<string> ();
					foreach (var ja in jsh)
						selected_sheets.Add (ja);
				}
				JsonArray jac = JSONHelper.read_array(json,"selected_accounts");
				if( jac != null ) {
					selected_accounts = new List<string> ();
					foreach (var ja in jac)
						selected_accounts.Add (ja);
				}
			}catch(Exception){
			}
		}
		public string save()
		{
			JsonObject result = new JsonObject { {"style",style.ToString()}, {"from_date",from_date.Ticks}, {"months",months}, {"days",days}, // Culture ok
				{"report_currency",report_currency}, {"separate_planned",separate_planned} };
			if (selected_sheets != null) {
				JsonArray jsh = new JsonArray ();
				foreach (var sh in selected_sheets)
					jsh.Add (sh);
				result.Add ("selected_sheets", jsh);
			}
			if (selected_accounts != null) {
				JsonArray jac = new JsonArray ();
				foreach (var ac in selected_accounts)
					jac.Add (ac);
				result.Add ("selected_accounts", jac);
			}
			return result.ToString();
		}
		public bool get_all_time()
		{
			return months == 0 && days == 0;
		}
		public void set_all_time()
		{
			months = 0;
			days = 0;
		}
		public void jump(int multiplier)
		{
			from_date = from_date.AddMonths (months*multiplier).AddDays (days*multiplier);
		}
		public string format_for_table()
		{
			return String.Format ("Custom Report {0}", id);
		}
	}
}

