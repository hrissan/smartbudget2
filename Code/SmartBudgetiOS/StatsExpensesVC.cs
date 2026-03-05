using System;
using System.Drawing;
using Foundation;
using UIKit;
using System.Collections.Generic;
using SmartBudgetCommon;

namespace SmartBudgetiOS
{
	public class StatsExpensesVC : UIViewController
	{
		TableSource table_source;
		UITableView expenseTable;
		UIView panelButtons;
		UIButton btnGroup;
		Document doc;
		CurrencyFormat report_currency_format;
		bool allow_groups;
		bool show_groups;
		private List<List<StatsVC.DBExpenseInstance>> tableItems = new List<List<StatsVC.DBExpenseInstance>> ();
		private StatsSettings settings; // for coloring purposes of transfers and exchanges
		private List<StatsVC.ExpenseSum> named_groups;
		private long max_named_balance;

		private string expenses_vc_name;

		private StatsExpensesVC parent_expenses_vc;
		private string filter_name;

		private StatsVC parent_stats_vc;
		private int cat_section;
		private string cat_category;
		private long day_date;

		public StatsExpensesVC (StatsVC parent_stats_vc, StatsSettings settings, int cat_section, string cat_category) //: base ("StatsExpensesVC", null)
		{
			this.allow_groups = true;
			this.show_groups = false;
			this.settings = settings;
			this.parent_stats_vc = parent_stats_vc;
			this.cat_section = cat_section;
			this.cat_category = cat_category;
		}
		public StatsExpensesVC (StatsVC parent_stats_vc, StatsSettings settings, long day_date) //: base ("StatsExpensesVC", null)
		{
			this.allow_groups = false;
			this.show_groups = false;
			this.settings = settings;
			this.parent_stats_vc = parent_stats_vc;
			this.day_date = day_date;
		}
		public StatsExpensesVC (StatsExpensesVC parent_expenses_vc, StatsSettings settings, string filter_name) //: base ("StatsExpensesVC", null)
		{
			this.allow_groups = false;
			this.show_groups = false;
			this.settings = settings;
			this.parent_expenses_vc = parent_expenses_vc;
			this.filter_name = filter_name;
		}
		~StatsExpensesVC()
		{
			Console.WriteLine ("~StatsExpensesVC");
		}
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			if (!IsViewLoaded) {
				expenseTable = Utility.free_view (expenseTable);
			}
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = AppDelegate.app.dark_background_color;

			expenseTable = AppDelegate.create_table_and_background (View, allow_groups ? 1 : 0);

			if (allow_groups) {
				panelButtons = BottomPanelView.create_bottom_panel (View);

				//btnHelp = BottomPanelView.create_help_button( panelButtons, "help");
				btnGroup = BottomPanelView.create_bottom_button( panelButtons, "chat");
				btnGroup.TouchUpInside += (sender, e) => {
					show_groups = !show_groups;
					expenseTable.ReloadData();
					update ();
				};
			}

			// Perform any additional setup after loading the view, typically from a nib.
			table_source = new TableSource (this);
			expenseTable.Source = table_source;
		}
		public override void ViewWillAppear (bool animated)
		{
			Console.WriteLine ("StatExpensesVC.ViewWillAppear");
			base.ViewWillAppear (animated);
			AppDelegate.app.docs.anything_changed += anything_changed;
			anything_changed ();
			update ();
		}
		public override void ViewWillDisappear (bool animated)
		{
			Console.WriteLine ("StatExpensesVC.ViewWillDisappear");
			AppDelegate.app.docs.anything_changed -= anything_changed;
			base.ViewWillDisappear (animated);
		}
		/*public override void ViewDidDisappear (bool animated)
		{
			Console.WriteLine ("StatExpensesVC.ViewDidDisappear");
			base.ViewDidDisappear (animated);
		}*/
		public override void ViewWillLayoutSubviews ()
		{
			base.ViewWillLayoutSubviews ();
			if( panelButtons != null )
				BottomPanelView.layout (panelButtons, null);
		}
		private void anything_changed(StatsExpensesVC expenses_vc, string other_filter_name)
		{
			anything_changed ();
			StatsVC.ExpenseSum gr = null;
			foreach (var sgr in named_groups) {
				if( sgr.expenses[0].ex.name == other_filter_name ) {
					gr = sgr;
					break;
				}
			}
			if (gr == null)
				expenses_vc.set_expenses (this.doc, new List<StatsVC.DBExpenseInstance>(), null);
			else {
				DBExpense ex = gr.expenses [0].ex;
				DBCategory cat = this.doc.get_category (ex.category);
				string tit = !String.IsNullOrEmpty (ex.name) ? ex.name : cat.get_loc_name ();
				expenses_vc.set_expenses (this.doc, gr.expenses, tit);
			}
		}
		private void anything_changed (Documents doc, Documents.DocumentChangeEventArgs e)
		{
			anything_changed ();
		}
		void anything_changed ()
		{
			if( parent_stats_vc != null ) {
				if (cat_category != null)
					parent_stats_vc.anything_changed (this, cat_section, cat_category);
				else
					parent_stats_vc.anything_changed (this, day_date);
				return;
			}
			if (parent_expenses_vc != null) {
				parent_expenses_vc.anything_changed(this, filter_name);
				return;
			}
		}
		void update()
		{
			if( btnGroup != null )
				AppDelegate.decorate_bottom_button(btnGroup, "chat", show_groups);
		}
		static public int sort_by_sum(StatsVC.ExpenseSum p1, StatsVC.ExpenseSum p2){
			return -Math.Abs(p1.sum_sum_1000).CompareTo(Math.Abs(p2.sum_sum_1000));
		}
		public void set_expenses(Document doc, List<StatsVC.DBExpenseInstance> expenses, string title)
		{
//			this.show_groups = show_groups;
			this.doc = doc;
			if( title != null ) // When deleting category we may not know the previous name, so we just keep it
				NavigationItem.Title = title;
			report_currency_format = CurrencyFormat.get_currency (settings.report_currency);
			tableItems.Clear ();
			//if( doc != null ) {
			//StatsVC.DBExpenseInstance last_expense = null;
			Dictionary<string, StatsVC.ExpenseSum> by_names = new Dictionary<string, StatsVC.ExpenseSum> ();

			DateTime previous_date = new DateTime();
			foreach (var it in expenses) {
				DateTime da = new DateTime (it.date, DateTimeKind.Local).Date;
				if (tableItems.Count == 0 || da != previous_date)
					tableItems.Add (new List<StatsVC.DBExpenseInstance>());
				previous_date = da;
				tableItems [tableItems.Count-1].Add (it);

				StatsVC.ExpenseSum by_name;
				string name = String.IsNullOrEmpty (it.ex.name) ? "" : it.ex.name;
				if (!by_names.TryGetValue (name, out by_name)) {
					by_name = new StatsVC.ExpenseSum ();
					by_names.Add (name, by_name);
				}
				by_name.expenses.Add (it);
				bool first = (settings.selected_accounts == null || settings.selected_accounts.IndexOf (it.ex.sum.account) != -1);
				bool second = it.ex.sum2.IsValid() && (settings.selected_accounts == null || settings.selected_accounts.IndexOf (it.ex.sum2.account) != -1);
				if(first)
					DBAccountBalance.update_balance (by_name.sum, it.ex.sum.currency, it.ex.sum.amount_1000);
				if(second)
					DBAccountBalance.update_balance (by_name.sum, it.ex.sum2.currency, it.ex.sum2.amount_1000);
			}
			if (tableItems.Count != 0) {
				List<StatsVC.DBExpenseInstance> lali = tableItems [tableItems.Count - 1];
				if( lali[0].date == 0 ) {
					tableItems.Insert (0, lali);
					tableItems.RemoveAt (tableItems.Count - 1);
				}
			}
			named_groups = new List<StatsVC.ExpenseSum>( by_names.Values );
			max_named_balance = 0;
			foreach (var gr in named_groups) {
				gr.sum_sum_1000 = StatsVC.convert_balance_1000(gr.sum, settings.report_currency);
				max_named_balance = Math.Max (max_named_balance, Math.Abs (gr.sum_sum_1000));
			}
			named_groups.Sort (sort_by_sum);
			//if (named_groups.Count < 2)
			//	this.show_groups = false;
			//DBExpense.split_by_categories (expenses, tableItems, null);
			//DBExpense.move_without_date_to_start (tableItems);
			//}
			if( expenseTable != null )
				expenseTable.ReloadData ();
			update ();
		}
		public class TableSource : UITableViewSource {
			StatsExpensesVC parent_vc;
			public TableSource (StatsExpensesVC parent_vc)
			{
				this.parent_vc = parent_vc;
			}
			public override nint NumberOfSections (UITableView tableView)
			{
				if (parent_vc.show_groups)
					return 1;
				return parent_vc.tableItems.Count;
			}
			public override nfloat GetHeightForHeader (UITableView tableView, nint section)
			{
				if (parent_vc.show_groups)
					return 0;
				return SectionHeader2.default_height;
			}
			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				if( editingStyle != UITableViewCellEditingStyle.Delete )
					return;
				List<StatsVC.DBExpenseInstance> li = parent_vc.tableItems[indexPath.Section];
				//DBExpense ex = parent_vc.doc.get_expense (li[indexPath.Row].id);
				//tableView.BeginUpdates ();
				FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"DeleteSwipeFromStats"}, new object[]{"action"}));
				AppDelegate.app.docs.execute_change (new ChangeExpenseRemove(parent_vc.doc, li[indexPath.Row].ex.id), null);
				//tableView.DeleteRows (new NSIndexPath[]{indexPath}, UITableViewRowAnimation.Left);
				//tableView.EndUpdates ();
				//updateVisibleCellCheckmarks (tableView);
			}
			public override UIView GetViewForHeader (UITableView tableView, nint section)
			{
				if (parent_vc.show_groups)
					return null;
				UILabel label;
				UITableViewHeaderFooterView hfv = SectionHeader2.deque_header(tableView, out label);

				//SectionHeader sh = SectionHeader.create_or_get_header ();

				StatsVC.DBExpenseInstance ex = parent_vc.tableItems[(int)section][0];
				if( ex.date == 0)
					label.Text = i18n.get ("SelectNoDate");
				else {
					string str = AppDelegate.app.date_week_formatter.ToString ((NSDate)new DateTime (ex.date, DateTimeKind.Local));
					if( ex.ex.planned )
						label.Text = i18n.get ("SectionPlannedFor").Replace("{date}", str);
					else
						label.Text = str;
				}
				return hfv;
			}
			public override nint RowsInSection (UITableView tableView, nint section)
			{
				if (parent_vc.show_groups)
					return parent_vc.named_groups.Count;
				return parent_vc.tableItems[(int)section].Count;
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				ExpenseCell cell = (ExpenseCell)tableView.DequeueReusableCell (ExpenseCell.Key);
				// if there are no cells to reuse, create a new one
				if (cell == null)
					cell = ExpenseCell.Create(tableView);
				if (parent_vc.show_groups) {
					StatsVC.ExpenseSum es = parent_vc.named_groups [indexPath.Row];
					DBExpense ex = es.expenses [0].ex;
					DBCategory cat = parent_vc.doc.get_category (ex.category);
					string tit = !String.IsNullOrEmpty (ex.name) ? ex.name : cat.get_loc_name ();
					long am_1000 = es.sum_sum_1000;
					string val = parent_vc.report_currency_format.format_approximate_amount (am_1000);
					cell.setImageNameValue(AppDelegate.app.get_category_image(cat), tit, val, am_1000 <= 0);
					cell.setIndicator (false, false);
					cell.setProgress (parent_vc.max_named_balance == 0 ? 0 : (float)Math.Abs((double)am_1000 / parent_vc.max_named_balance), am_1000 <= 0);
					cell.separator.Hidden = false;
				}else{
					List<List<StatsVC.DBExpenseInstance>> lli = parent_vc.tableItems;
					List<StatsVC.DBExpenseInstance> li = lli[indexPath.Section];
					DBExpense ex = parent_vc.doc.get_expense (li[indexPath.Row].ex.id);

					cell.setExpense (ex, parent_vc.doc, parent_vc.settings.selected_accounts);
					cell.separator.Hidden = (indexPath.Row == li.Count - 1) && (indexPath.Section != lli.Count - 1);
				}
				return cell;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				if (parent_vc.show_groups) {
					StatsVC.ExpenseSum es = parent_vc.named_groups [indexPath.Row];
					DBExpense ex = es.expenses [0].ex;

					StatsExpensesVC evc = new StatsExpensesVC (parent_vc, parent_vc.settings, ex.name);

					DBCategory cat = parent_vc.doc.get_category (ex.category);
					string tit = !String.IsNullOrEmpty (ex.name) ? ex.name : cat.get_loc_name ();

					evc.set_expenses (parent_vc.doc, es.expenses, tit);
					parent_vc.NavigationController.PushViewController(evc, true);
				} else {
					DBExpense ex = parent_vc.doc.get_expense (parent_vc.tableItems [indexPath.Section] [indexPath.Row].ex.id);
					parent_vc.NavigationController.PushViewController (ExpenseVC.create_or_reuse (parent_vc.doc, ex, null), true);
				}
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
		}	
	}
}

