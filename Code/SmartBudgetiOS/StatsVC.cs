using System;
using System.Drawing;
using Foundation;
using UIKit;
using CoreGraphics;
using SmartBudgetCommon;
using System.Collections.Generic;
using System.IO;
using QuickLook;

namespace SmartBudgetiOS
{
	public class StatsVC : UIViewController
	{
		StatsSettings settings;
		bool settings_modified;

		UIBarButtonItem cancel_button;
		UIBarButtonItem save_button;
		UIBarButtonItem reports_button;
		UIBarButtonItem done_button;

		UIDocumentInteractionController preview_vc;
		QLPreviewController preview2_vc;

		TitleButton title_button;
		UIView panelButtons;
		UIButton btnReportStyle;
		UIButton btnAccount;
		//UIButton btnExport;
		UIButton btnHelp;
		TimerForButton currency_hold_timer;
		TimerForButton btnMonthNext_timer;
		TimerForButton btnMonthPrev_timer;

		UITableView table;
		UITableView table2;
		UIButton btnCurrency;
		UIButton btnMonthPrev;
		UIButton btnMonth;
		UIButton btnMonthNext;

		CurrencyFormat report_currency_format;
		private Document doc;

		//private StatsExpensesVC expenses_vc_cat;
		//private string expenses_vc_cat_category;
		//private int expenses_vc_cat_section;
		//private StatsExpensesVC expenses_vc_day;
		//private long expenses_vc_day_date;

		private void construct(Document doc)
		{
			this.doc = doc;
			load_simple_settings ();
//			from_date = DateTime.Now.Date;
//			Console.WriteLine ("Ticks={0} Ticks/1E7={1} Ticks/1E7/86400={2}", from_date.Ticks, from_date.Ticks / 10000000, from_date.Ticks / 10000000 / 86400);
//			to_date = from_date.AddMonths(1);
//			style = ReportStyle.ByCat;
//			show_planned = false;
//			report_currency = "USD";


			//float wid = View.Frame.Width; // Load view to construct title button
			//Console.Write ("wid={0}", wid);
		}
		void load_simple_settings()
		{
			settings = new StatsSettings ("", AppDelegate.app.docs.last_stat_settings);
			report_currency_format = CurrencyFormat.get_currency (settings.report_currency);
			settings_modified = false;
			if (!String.IsNullOrEmpty (doc.selected_sheet)) {
				settings.selected_sheets = new List<string> ();
				settings.selected_sheets.Add (doc.selected_sheet);
			} else
				settings.selected_sheets = null;
			settings.selected_accounts = null;
			jump_to_last_interval ();
		}
		public StatsVC ()// : base ("StatsVC", null)
		{
			reports_button = new UIBarButtonItem (i18n.get("Reports"), UIBarButtonItemStyle.Plain, (sender, e) => { // TODO - change string
				NavigationController.PushViewController(ReportsVC.create_or_reuse(doc, settings.id, settings.save(), (rvc,rep)=>{
					if( String.IsNullOrEmpty(rep) ){
						load_simple_settings ();
					}else{
						DBReport rep_rep = doc.get_report(rep);
						settings = new StatsSettings(rep_rep.id, rep_rep.data);
						report_currency_format = CurrencyFormat.get_currency (settings.report_currency);
						settings_modified = false;
					}
					new_settings = true;
					anything_changed();
				}), true);
			});
			done_button = new UIBarButtonItem (UIBarButtonSystemItem.Done, (sender, e) => {
				// Save common settings here
				Utility.dismiss_or_pop (NavigationController, true);
			});
			cancel_button = new UIBarButtonItem (UIBarButtonSystemItem.Cancel, (sender, e) => {
				// revert to original settings
				DBReport rep_rep = doc.get_report(settings.id);
				settings = new StatsSettings(rep_rep.id, rep_rep.data);
				report_currency_format = CurrencyFormat.get_currency (settings.report_currency);
				settings_modified = false;
				new_settings = true;
				anything_changed();
			});
			save_button = new UIBarButtonItem (UIBarButtonSystemItem.Save, (sender, e) => {
				// update settings
				AppDelegate.app.docs.execute_change( new ChangeReportUpdate(doc, new DBReport{id=settings.id, data=settings.save ()}), null);
				settings_modified = false;
				style_changed();
			});
			NavigationItem.Title = i18n.get ("ReportTitle"); // Makes more sense on back buttons
			NavigationItem.RightBarButtonItem = reports_button;
			NavigationItem.LeftBarButtonItem = done_button;

			title_button = TitleButton.create_title_button ();
			NavigationItem.TitleView = title_button;

			title_button.button.TouchUpInside += (sender, e) => {
				NavigationController.PushViewController( SheetsSelect.create_or_reuse(doc, i18n.get ("SheetsTitle"), settings.selected_sheets, (svc, shs, sav)=>{
					if( sav ) {
						settings_modified = true;
						settings.selected_sheets = shs;
						new_settings = true;
						anything_changed();
					}
					NavigationController.PopViewController(false);
					Utility.play_transition(NavigationController.View, UIViewAnimationTransition.CurlUp);
				}), false );
				Utility.play_transition(NavigationController.View, UIViewAnimationTransition.CurlDown);
			};
		}
		~StatsVC()
		{
			Console.WriteLine ("~StatsVC");
		}
		private static Utility.ReuseVC<StatsVC> reuse = new Utility.ReuseVC<StatsVC> ();
		public static StatsVC create_or_reuse(Document doc)
		{
			StatsVC result = reuse.create_or_reuse();
			result.construct(doc);
			return result;
		}
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			if (!IsViewLoaded) {
				table = Utility.free_view (table);
			}
			// Release any cached data, images, etc that aren't in use.
		}
		public override void ViewWillAppear (bool animated)
		{
			Console.WriteLine ("Stats.ViewWillAppear");
			base.ViewWillAppear (animated);
			AppDelegate.app.docs.anything_changed += anything_changed;
			anything_changed ();
		}
		public override void ViewWillDisappear (bool animated)
		{
			Console.WriteLine ("Stats.ViewWillDisappear");
			AppDelegate.app.docs.anything_changed -= anything_changed;
			base.ViewWillDisappear (animated);
		}
		/*public override void ViewDidDisappear (bool animated)
		{
			Console.WriteLine ("Stats.ViewDidDisappear");
			base.ViewDidDisappear (animated);
		}*/
		public override void ViewWillLayoutSubviews ()
		{
			base.ViewWillLayoutSubviews ();
			BottomPanelView.layout (panelButtons, btnHelp);
		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = AppDelegate.app.dark_background_color;

			table = AppDelegate.create_table_and_background (View, 2);
			table2 = AppDelegate.create_table_and_background (View, 2);

			panelButtons = BottomPanelView.create_bottom_panel (View);

			btnHelp = BottomPanelView.create_help_button( panelButtons, "help");
			btnReportStyle = BottomPanelView.create_bottom_button( panelButtons, "stats");
			btnAccount = BottomPanelView.create_bottom_button( panelButtons, "account");
			btnCurrency = BottomPanelView.create_bottom_button( panelButtons, "currency");
			//btnExport = BottomPanelView.create_bottom_button( panelButtons, "export");

			btnMonth = AppDelegate.create_flat_bottom_button (View, "", 1);
			btnMonthPrev = BottomPanelView.create_bottom_button_rc (View, "arrow_left", 40); // TODO - custom element
			btnMonthNext = BottomPanelView.create_bottom_button_rc (View, "arrow_right", 40); // TODO - custom element
			//float pos_y = View.Bounds.Height - 2 * AppDelegate.BUTTON_ROW_HEIGHT;
			float ARROW_WIDTH = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? 80 : 44;
			CGRect rr = btnMonth.Frame;
			rr.X = ARROW_WIDTH;
			rr.Width = View.Bounds.Width - 2 * ARROW_WIDTH;
			btnMonth.Frame = rr;
			rr.X = 0;
			rr.Width = ARROW_WIDTH;
			btnMonthPrev.Frame = rr;
			rr.X = View.Bounds.Width - ARROW_WIDTH;
			rr.Width = ARROW_WIDTH;
			btnMonthNext.Frame = rr;

			//RectangleF rr = btnMonth.Frame;
			//rr.X = btnMonthPrev.Frame.Right;
			//rr.Width = btnMonthNext.Frame.Left - rr.X;
			//btnMonth.Frame = rr;
			btnMonthPrev.AutoresizingMask = UIViewAutoresizing.FlexibleRightMargin | UIViewAutoresizing.FlexibleTopMargin;
			btnMonth.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin;
			btnMonthNext.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleTopMargin;

			btnReportStyle.TouchUpInside += (sender, e) => {
				UIActionSheet ash3 = new UIActionSheet(i18n.get ("ReportStyleTitle"), null, i18n.get ("Cancel"), null, i18n.get ("ByCat"), i18n.get ("ByDay"), i18n.get ("BySum"));
				ash3.Clicked += (sender3, e3) => {
					if( ash3.ButtonTitle(e3.ButtonIndex) == i18n.get ("ByCat") ){
						settings_modified = true;
						settings.style = StatsSettings.ReportStyle.ByCat;
						if( String.IsNullOrEmpty(settings.id) )
							AppDelegate.app.docs.last_stat_settings = settings.save();
						style_changed();
					}
					if( ash3.ButtonTitle(e3.ButtonIndex) == i18n.get ("ByDay") ){
						settings_modified = true;
						settings.style = StatsSettings.ReportStyle.ByDay;
						if( String.IsNullOrEmpty(settings.id) )
							AppDelegate.app.docs.last_stat_settings = settings.save();
						style_changed();
					}
					if( ash3.ButtonTitle(e3.ButtonIndex) == i18n.get ("BySum") ){
						settings_modified = true;
						settings.style = StatsSettings.ReportStyle.BySum;
						if( String.IsNullOrEmpty(settings.id) )
							AppDelegate.app.docs.last_stat_settings = settings.save();
						style_changed();
					}
				};
				Utility.show_action_sheet(ash3, View, btnReportStyle);
			};
			btnHelp.TouchUpInside += (sender, e) => {
				LayoutForHelp lh = new LayoutForHelp(NavigationController, table.Frame.Height);

				lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, NavigationController.NavigationBar, 0, i18n.get ("HelpSwitchSheet"), LayoutForHelp.BubleType.BUTTON);
				lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, btnMonthPrev, 0, i18n.get ("HelpTapHold"), LayoutForHelp.BubleType.BUTTON, false);
				UIView vv = lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, btnMonthNext, 0, i18n.get ("HelpTapHold"), LayoutForHelp.BubleType.BUTTON, false);
				lh.create_tail(btnCurrency, 0, vv);
				lh.show ();
			};
			btnAccount.TouchUpInside += (sender, e) => {
				NavigationController.PushViewController( AccountSelect.create_or_reuse(doc, i18n.get ("AccountsTitle"), settings.selected_accounts, (svc, shs)=>{
					settings_modified = true;
					settings.selected_accounts = shs;
					new_settings = true;
					anything_changed();
				}), true );
			};
			/*btnExport.TouchUpInside += (sender, e) => {
				UIActionSheet ash3 = new UIActionSheet(i18n.get("ExportReportTitle"), null, i18n.get ("Cancel"), null, i18n.get ("ExportXSLXButtonTitle"), i18n.get ("PreviewButtonTitle"));
				ash3.Clicked += (sender3, e3) => {
					if(ash3.ButtonTitle(e3.ButtonIndex) == i18n.get ("ExportXSLXButtonTitle")){
						//FlurryAnalytics.Flurry.LogEvent("Document", NSDictionary.FromObjectsAndKeys(new object[]{"ExportXSLX"}, new object[]{"action"}));
						string fname = Path.Combine(Documents.get_caches_path(), "report.xlsx"); // TODO - better name
						byte[] backup_data = export_xlsx();
						// TODO - exception
						File.WriteAllBytes(fname, backup_data);
						preview_vc = UIDocumentInteractionController.FromUrl( NSUrl.FromFilename(fname) );
						RectangleF rr3 = View.ConvertRectFromView(btnExport.Bounds, btnExport);
						preview_vc.PresentOptionsMenu(rr3, View, true);
					}
					if(ash3.ButtonTitle(e3.ButtonIndex) == i18n.get ("PreviewButtonTitle")){
						//FlurryAnalytics.Flurry.LogEvent("Document", NSDictionary.FromObjectsAndKeys(new object[]{"PreviewXSLX"}, new object[]{"action"}));
						string fname = Path.Combine(Documents.get_caches_path(), "report.xlsx"); // TODO - better name
						byte[] backup_data = export_xlsx();
						// TODO - exception
						File.WriteAllBytes(fname, backup_data);
						preview2_vc = new QLPreviewController();
						preview2_vc.DataSource = new Utility.PreviewDataSource( NSUrl.FromFilename(fname), i18n.get ("PreviewButtonTitle") );
						NavigationController.PresentViewController(preview2_vc, true, null);
					}
				};
				Utility.show_action_sheet(ash3, View, btnReportStyle);
			};*/
			currency_hold_timer = new TimerForButton (btnCurrency, delegate {
				settings_modified = true;
				settings.report_currency = AppDelegate.app.docs.next_selected_currency(settings.report_currency, "");
				if( String.IsNullOrEmpty(settings.id) )
					AppDelegate.app.docs.last_stat_settings = settings.save();
				report_currency_format = CurrencyFormat.get_currency (settings.report_currency);
				style_changed();
			}, delegate {
				NavigationController.PushViewController(CurrencySelect.create_or_reuse(AppDelegate.app.docs.report_currency, (cs,cur)=>{
					settings_modified = true;
					settings.report_currency = cur;
					if( String.IsNullOrEmpty(settings.id) )
						AppDelegate.app.docs.last_stat_settings = settings.save();
					report_currency_format = CurrencyFormat.get_currency (settings.report_currency);
					style_changed();
				}), true);
			});

			btnMonthPrev_timer = new TimerForButton (btnMonthPrev, delegate {
				settings_modified = true;
				if( settings.get_all_time() ) {
					settings.months = 1;
					jump_to_first_interval();
				}
				else
					settings.jump(-1);
				if( String.IsNullOrEmpty(settings.id) )
					AppDelegate.app.docs.last_stat_settings = settings.save();
//				settings.months = 1;
				new_settings = true;
				anything_changed();
			}, delegate {
				settings_modified = true;
				if( settings.get_all_time() )
					settings.months = 1;
				jump_to_first_interval();
				if( String.IsNullOrEmpty(settings.id) )
					AppDelegate.app.docs.last_stat_settings = settings.save();
				new_settings = true;
				anything_changed();
			});
			btnMonthNext_timer = new TimerForButton (btnMonthNext, delegate {
				settings_modified = true;
				if( settings.get_all_time() ) {
					settings.months = 1;
					jump_to_last_interval();
				}
				else
					settings.jump(1);
				if( String.IsNullOrEmpty(settings.id) )
					AppDelegate.app.docs.last_stat_settings = settings.save();
				//				settings.months = 1;
				//				settings.days = 0;
				new_settings = true;
				anything_changed();
			}, delegate {
				settings_modified = true;
				if( settings.get_all_time() )
					settings.months = 1;
				jump_to_last_interval();
				if( String.IsNullOrEmpty(settings.id) )
					AppDelegate.app.docs.last_stat_settings = settings.save();
				new_settings = true;
				anything_changed();
			});
			btnMonth.TouchUpInside += (sender, e) => {
				NavigationController.PushViewController(StatPeriodVC.create_or_reuse(settings, (dvc, da)=>{
					settings_modified = true;
					if( String.IsNullOrEmpty(settings.id) )
						AppDelegate.app.docs.last_stat_settings = settings.save();
					new_settings = true;
					anything_changed();
				}), true);
			};


			//AppDelegate.app.docs.anything_changed += (docs, e) => {
			//	anything_changed();
			//	style_changed();
			//};
			//anything_changed();
			table.Source = new TableSource (this);
			table2.Source = new TableSource2 (this);
		}
		static DBExpense get_first_expense(Document doc, List<string> selected_sheets)
		{
			DBExpense first_expense = null;
			for(int planned = 0; planned != 2; planned +=1)
			{
				if (selected_sheets == null) {
					List<DBExpense> ex = doc.get_first_expenses_for_sheet ("", planned == 1);
					if (ex.Count != 0) {
						if (first_expense == null || ex [0].date < first_expense.date)
							first_expense = ex [0];
					}
				}
				else {
					foreach (var s in selected_sheets) {
						List<DBExpense> ex = doc.get_first_expenses_for_sheet (s, planned == 1);
						if (ex.Count != 0) {
							if (first_expense == null || ex [0].date < first_expense.date)
								first_expense = ex [0];
						}
					}
				}
				if (first_expense != null) // check planned only if no actual
					return first_expense;
			}
			return first_expense;
		}
		static DBExpense get_last_expense(Document doc, List<string> selected_sheets)
		{
			DBExpense last_expense = null;
			for(int planned = 0; planned != 2; planned +=1)
			{
				if (selected_sheets == null) {
					List<DBExpense> ex = doc.get_expenses_for_sheet ("", planned == 1, null);
					if (ex.Count != 0) {
						if (last_expense == null || ex [0].date > last_expense.date)
							last_expense = ex [0];
					}
				}
				else {
					foreach (var s in selected_sheets) {
						List<DBExpense> ex = doc.get_expenses_for_sheet (s, planned == 1, null);
						if (ex.Count != 0) {
							if (last_expense == null || ex [0].date > last_expense.date)
								last_expense = ex [0];
						}
					}
				}
				if (last_expense != null) // check planned only if no actual
					return last_expense;
			}
			return last_expense;
		}
		void jump_to_first_interval()
		{
			settings.validate(doc);

			if (settings.get_all_time ())
				return;
			DBExpense first_expense = get_first_expense (doc, settings.selected_sheets);
			if (first_expense == null)
				return;
			DateTime to_date = new DateTime (first_expense.date, DateTimeKind.Local).Date;
			while (settings.from_date < to_date)
				settings.jump (1);
			while (settings.from_date > to_date)
				settings.jump (-1);
		}
		void jump_to_last_interval()
		{
			settings.validate(doc);

			if (settings.get_all_time ())
				return;

			DBExpense last_expense = get_last_expense (doc, settings.selected_sheets);
			if (last_expense == null)
				return;
			DateTime to_date = new DateTime (last_expense.date, DateTimeKind.Local).Date;
			while (settings.from_date < to_date)
				settings.jump (1);
			while (settings.from_date > to_date)
				settings.jump (-1);
		}
		public struct DBExpenseInstance
		{
			public DBExpense ex;
			public long date;
		};
		public class ExpenseSum {
			public List<DBExpenseInstance> expenses = new List<DBExpenseInstance>();
			public List<DBAccountBalance> sum = new List<DBAccountBalance>();
			public long sum_sum_1000;
		};
		const int NORMAL = 0;
		const int PLANNED = 1;
		const int PLANNED_NO_DATE = 2;
		const int PER_MONTH = 3;
		public class ExpenseGroup {
			public string cat; // for categories
			public long date; // for days
			public ExpenseSum[] expenses = new ExpenseSum[]{ new ExpenseSum(), new ExpenseSum(), new ExpenseSum(), new ExpenseSum() };

			public long sort_sum_1000;
		};
		ExpenseGroup[] balances;
		ExpenseGroup total_balance;
		List<ExpenseGroup>[] cat_groups = new List<ExpenseGroup>[2];
		List<ExpenseGroup>[] split_cat_groups  = new List<ExpenseGroup>[8];
		long[] cat_max_balances_1000 = new long[8];

		List<ExpenseGroup> day_groups;
		long day_max_balance_1000;
		List<ExpenseGroup> split_day_groups = new List<ExpenseGroup>();

		static public long convert_balance_1000(List<DBAccountBalance> bal, string currency)
		{
			long sum_1000 = 0;
			foreach (var a in bal) {
				long result_1000 = 0;
				if (AppDelegate.app.docs.convert_currency (ref result_1000, currency, a.sum_1000, a.currency))
					sum_1000 += result_1000;
			}
			return sum_1000;
		}
		public void convert_balance(ExpenseGroup gr)
		{
			gr.sort_sum_1000 = 0;
			for(int i = 0; i != gr.expenses.Length; ++i) {
				gr.expenses[i].sum_sum_1000 = convert_balance_1000 (gr.expenses[i].sum, settings.report_currency);
				gr.sort_sum_1000 += gr.expenses [i].sum_sum_1000;
			}
		}
		bool in_accounts(DBExpense ex)
		{
			if (settings.selected_accounts == null)
				return true;
			if( settings.selected_accounts.Contains(ex.sum.account) )
				return true;
			if( ex.sum2.IsValid() && settings.selected_accounts.Contains(ex.sum2.account) )
				return true;
			return false;
		}
		static public int sort_by_sum(ExpenseGroup p1, ExpenseGroup p2){
			return -Math.Abs(p1.sort_sum_1000).CompareTo(Math.Abs(p2.sort_sum_1000));
		}
		static public int sort_by_date(ExpenseGroup p1, ExpenseGroup p2){
			return -p1.date.CompareTo(p2.date);
		}
		static public string format_settings_date(StatsSettings settings)
		{
			string all_string = "";
			string days_string = i18n.localize_plural ("NDays", settings.days);
			string months_string = i18n.localize_plural ("NMonths", settings.months);
			string date_string = AppDelegate.app.date_formatter.ToString ((NSDate)settings.from_date);
			if (settings.get_all_time ())
				all_string = i18n.get ("AllTime");
			else if (settings.days == 0 && settings.months == 1 && settings.from_date.Day == 1)
				all_string = AppDelegate.app.month_year_formatter.ToString ((NSDate)settings.from_date);
			else if (settings.days == 0 && settings.months != 0)
				all_string = date_string + ", " + months_string;
			else if (settings.days != 0 && settings.months == 0)
				all_string = date_string + ", " + days_string;
			else if (settings.days != 0 && settings.months != 0)
				all_string = date_string + ", " + months_string + ", " + days_string;
			return all_string;
		}
		void style_changed() // Currency, style or planned
		{
			NavigationItem.LeftBarButtonItem = (settings_modified && !String.IsNullOrEmpty(settings.id)) ? cancel_button : done_button;
			NavigationItem.RightBarButtonItem = (settings_modified && !String.IsNullOrEmpty(settings.id)) ? save_button : reports_button;

			AppDelegate.decorate_bottom_button(btnAccount, "account", settings.selected_accounts != null);
			btnAccount.Enabled = doc.sorted_accounts.Count > 1;

			btnMonth.SetTitle (format_settings_date(settings), UIControlState.Normal);

			table2.Hidden = settings.style != StatsSettings.ReportStyle.BySum;
			table.Hidden = !table2.Hidden;
			//convert_balance (total_balance);
			//for (int sec = 0; sec != 2; ++sec) {
			//	convert_balance (balances [sec]);
			//}
			balances = new ExpenseGroup[2]{new ExpenseGroup (), new ExpenseGroup ()};
			//total_balance = new ExpenseGroup ();
			for (int sec = 0; sec != 8; ++sec) {
				split_cat_groups [sec] = new List<ExpenseGroup> ();
			}
			for (int sec = 0; sec != 2; ++sec) {
				foreach(var gr in cat_groups[sec]){
					bool special = gr.cat == Document.TRANSFER_CATEGORY || gr.cat == Document.CONVERT_CATEGORY;
					if (settings.get_all_time ()) {
						if( gr.expenses [PER_MONTH].expenses.Count != 0 ) {
							ExpenseGroup new_cat_gr = new ExpenseGroup ();
							new_cat_gr.cat = gr.cat;
							new_cat_gr.expenses [PER_MONTH] = gr.expenses [PER_MONTH];
							//new_cat_gr.sort_sum = gr.expenses [PER_MONTH].sum_sum;
							convert_balance (new_cat_gr);
							int sec2 = special ? (new_cat_gr.sort_sum_1000 > 0 ? 0 : 1) : sec;
							DBAccountBalance.merge_balance (balances [sec2].expenses [PER_MONTH].sum, new_cat_gr.expenses[PER_MONTH].sum, true);
							split_cat_groups [PER_MONTH*2 + sec2].Add (new_cat_gr);
						}
					}
					if (gr.expenses [NORMAL].expenses.Count != 0) {
						ExpenseGroup new_cat_gr = new ExpenseGroup ();
						new_cat_gr.cat = gr.cat;
						new_cat_gr.expenses [NORMAL] = gr.expenses [NORMAL];
						//new_cat_gr.sort_sum = gr.expenses [NORMAL].sum_sum;
						convert_balance (new_cat_gr);
						int sec2 = special ? (new_cat_gr.sort_sum_1000 > 0 ? 0 : 1) : sec;
						DBAccountBalance.merge_balance (balances [sec2].expenses [NORMAL].sum, new_cat_gr.expenses[NORMAL].sum, true);
						split_cat_groups [NORMAL*2 + sec2].Add (new_cat_gr);
					}
					//if (!settings.get_all_time ()) {
					if (gr.expenses [PLANNED].expenses.Count != 0) {	
						ExpenseGroup new_cat_gr = new ExpenseGroup ();
						new_cat_gr.cat = gr.cat;
						new_cat_gr.expenses [PLANNED] = gr.expenses [PLANNED];
						//new_cat_gr.sort_sum = gr.expenses [PLANNED].sum_sum;
						convert_balance (new_cat_gr);
						int sec2 = special ? (new_cat_gr.sort_sum_1000 > 0 ? 0 : 1) : sec;
						DBAccountBalance.merge_balance (balances [sec2].expenses [PLANNED].sum, new_cat_gr.expenses[PLANNED].sum, true);
						split_cat_groups [PLANNED*2 + sec2].Add (new_cat_gr);
					}
					//}
					if (gr.expenses [PLANNED_NO_DATE].expenses.Count != 0) {
						ExpenseGroup new_cat_gr = new ExpenseGroup ();
						new_cat_gr.cat = gr.cat;
						new_cat_gr.expenses [PLANNED_NO_DATE] = gr.expenses [PLANNED_NO_DATE];
						//new_cat_gr.sort_sum = gr.expenses [PLANNED_NO_DATE].sum_sum;
						convert_balance (new_cat_gr);
						int sec2 = special ? (new_cat_gr.sort_sum_1000 > 0 ? 0 : 1) : sec;
						DBAccountBalance.merge_balance (balances [sec2].expenses [PLANNED_NO_DATE].sum, new_cat_gr.expenses[PLANNED_NO_DATE].sum, true);
						split_cat_groups [PLANNED_NO_DATE*2 + sec2].Add (new_cat_gr);
					}
				}
			}
			total_balance = new ExpenseGroup ();
			for (int sec = 0; sec != 2; ++sec) {
				convert_balance (balances [sec]);
				for(int j = 0; j != 1; ++j) // In 2.0.2 3->1 Only current balances
					DBAccountBalance.merge_balance(total_balance.expenses[0].sum, balances[sec].expenses[j].sum, true);
			}
			convert_balance (total_balance);
			long cat_max_balance_1000 = 0;
			for (int sec = 0; sec != 6; ++sec) {
				foreach(var gr in split_cat_groups[sec]){
					cat_max_balance_1000 = Math.Max(cat_max_balance_1000, Math.Abs(gr.sort_sum_1000));
				}
				split_cat_groups [sec].Sort (sort_by_sum);
			}
//			for(int i = 0; i != 2; ++i)
//				for(int j = 0; j != 3; ++j)
//					cat_max_balance = Math.Max(cat_max_balance, Math.Abs(balances[i].expenses[j].sum_sum));
			for (int sec = 0; sec != 6; ++sec)
				cat_max_balances_1000 [sec] = cat_max_balance_1000;
			cat_max_balance_1000 = 0;
			for (int sec = 6; sec != 8; ++sec) {
				foreach(var gr in split_cat_groups[sec]){
					cat_max_balance_1000 = Math.Max(cat_max_balance_1000, Math.Abs(gr.sort_sum_1000));
				}
				split_cat_groups [sec].Sort (sort_by_sum);
			}
			for (int sec = 6; sec != 8; ++sec)
				cat_max_balances_1000 [sec] = cat_max_balance_1000;
			split_day_groups = new List<ExpenseGroup> ();
			day_max_balance_1000 = 0;
			foreach(var gr in day_groups){
				if (gr.expenses [NORMAL].expenses.Count != 0 && gr.expenses [PLANNED].expenses.Count != 0) {
					ExpenseGroup new_day_gr = new ExpenseGroup ();
					new_day_gr.expenses [NORMAL] = gr.expenses [NORMAL];
					new_day_gr.date = gr.date;
					//new_day_gr.sort_sum = gr.expenses [NORMAL].sum_sum;
					convert_balance (new_day_gr);
					day_max_balance_1000 = Math.Max(day_max_balance_1000, Math.Abs(new_day_gr.sort_sum_1000));
					split_day_groups.Add (new_day_gr);
					new_day_gr = new ExpenseGroup ();
					new_day_gr.expenses [PLANNED] = gr.expenses [PLANNED];
					new_day_gr.date = gr.date;
					//new_day_gr.sort_sum = gr.expenses [PLANNED].sum_sum;
					convert_balance (new_day_gr);
					day_max_balance_1000 = Math.Max(day_max_balance_1000, Math.Abs(new_day_gr.sort_sum_1000));
					split_day_groups.Add (new_day_gr);
				} else {
					convert_balance (gr);
					day_max_balance_1000 = Math.Max(day_max_balance_1000, Math.Abs(gr.sort_sum_1000));
					split_day_groups.Add (gr);
				}
			}
			//foreach(var gr in split_day_groups){
			//	convert_balance (gr);
				//gr.sort_sum = gr.expenses.sum_sum;
			//}
			//day_groups.Sort (sort_by_sum);
			table.ReloadData ();
			table2.ReloadData ();
		}
		void update_balance(DBExpenseInstance ex, decimal multiplier, int index, ExpenseGroup[] balances, Dictionary<string, ExpenseGroup>[] cats_collectors, Dictionary<long, ExpenseGroup> day_collector)
		{
			long sum_amount_1000 = (long)(ex.ex.sum.amount_1000 * multiplier);
			long sum_amount2_1000 = (long)(ex.ex.sum2.amount_1000 * multiplier);
			bool first = (settings.selected_accounts == null || settings.selected_accounts.IndexOf (ex.ex.sum.account) != -1);
			bool second = ex.ex.sum2.IsValid() && (settings.selected_accounts == null || settings.selected_accounts.IndexOf (ex.ex.sum2.account) != -1);
			if (!first && !second)
				return;
			//if(first)
			//	DBAccountBalance.update_balance (total_balance.expenses[index].sum, ex.ex.sum.currency, sum_amount_1000);
			//if(second)
			//	DBAccountBalance.update_balance (total_balance.expenses[index].sum, ex.ex.sum2.currency, sum_amount2_1000);

			int sec = ex.ex.sum2.IsValid() ? 0 : sum_amount_1000 > 0 ? 0 : 1;

			//if(first)
			//	DBAccountBalance.update_balance (balances[sec].expenses[index].sum, ex.ex.sum.currency, sum_amount_1000);
			//if(second)
			//	DBAccountBalance.update_balance (balances[sec].expenses[index].sum, ex.ex.sum2.currency, sum_amount2_1000);

			ExpenseGroup cat_gr;
			if (!cats_collectors[sec].TryGetValue (ex.ex.category, out cat_gr)) {
				cat_gr = new ExpenseGroup ();
				cat_gr.cat = ex.ex.category;
				cats_collectors[sec].Add (ex.ex.category, cat_gr);
			}
			cat_gr.expenses[index].expenses.Add (ex);
			if(first)
				DBAccountBalance.update_balance (cat_gr.expenses[index].sum, ex.ex.sum.currency, sum_amount_1000);
			if(second)
				DBAccountBalance.update_balance (cat_gr.expenses[index].sum, ex.ex.sum2.currency, sum_amount2_1000);

			if( ex.date != 0 && index != PER_MONTH && index != PLANNED_NO_DATE ) {
				DateTime datedate = new DateTime(ex.date, DateTimeKind.Local).Date;
				ExpenseGroup day_gr;
				if (!day_collector.TryGetValue (datedate.Ticks, out day_gr)) {
					day_gr = new ExpenseGroup ();
					day_gr.date = datedate.Ticks;
					day_collector.Add (datedate.Ticks, day_gr);
				}
				day_gr.expenses[index].expenses.Add (ex);
				if(first)
					DBAccountBalance.update_balance (day_gr.expenses[index].sum, ex.ex.sum.currency, sum_amount_1000);
				if(second)
					DBAccountBalance.update_balance (day_gr.expenses[index].sum, ex.ex.sum2.currency, sum_amount2_1000);
			}
		}
		byte[] export_xlsx()
		{
			XLSXStringsExporter exporter = new XLSXStringsExporter();
			Document.append_expense_titles (exporter);
			return exporter.finish();
		}
		public void anything_changed(StatsExpensesVC expenses_vc, int cat_section, string cat_category)
		{
			anything_changed ();
			DBCategory cat = this.doc.get_category(cat_category);
			ExpenseGroup gr = null;
			foreach (var sgr in split_cat_groups[cat_section]) {
				if( sgr.cat == cat_category ) {
					gr = sgr;
					break;
				}
			}
			if (gr == null || cat == null)
				expenses_vc.set_expenses (this.doc, new List<StatsVC.DBExpenseInstance>(), null);
			else
				expenses_vc.set_expenses (this.doc, find_expenses(gr), cat.get_loc_name());
		}
		public void anything_changed(StatsExpensesVC expenses_vc, long day_date)
		{
			anything_changed ();
			ExpenseGroup gr = null;
			foreach (var sgr in split_day_groups) {
				if( sgr.date == day_date ) {
					gr = sgr;
					break;
				}
			}
			if (gr == null)
				expenses_vc.set_expenses (this.doc, new List<StatsVC.DBExpenseInstance>(), null);
			else {
				string date_str = AppDelegate.app.date_formatter.ToString ((NSDate)new DateTime (gr.date, DateTimeKind.Local));
				expenses_vc.set_expenses (this.doc, find_expenses(gr), date_str);
			}
		}
		private void anything_changed (Documents doc, Documents.DocumentChangeEventArgs e)
		{
			anything_changed ();
		}
		private Document calc_doc;
		private int calc_doc_version;
		private bool new_settings;

		void anything_changed() // Sheet, Account, Date
		{
			if (!String.IsNullOrEmpty (settings.id) && doc.get_report (settings.id) == null) {
				// Report deletede
				load_simple_settings ();
			}
			settings.validate (doc);
			string tit = i18n.get ("TotalSheet");
			if (settings.selected_sheets != null) {
				if (settings.selected_sheets.Count == 1) {
					DBSheet sh = doc.get_sheet (settings.selected_sheets [0]);
					tit = sh.get_loc_name();
				} else
					tit = i18n.get ("SeveralSheets");
			}
			title_button.set_text_image(tit, null);

			if (calc_doc == doc && calc_doc_version == doc.local_version && !new_settings ) {
				//Console.WriteLine ("Light stats calculations");
				style_changed ();
				return;
			}
			calc_doc = doc;
			calc_doc_version = doc.local_version;
			new_settings = false;
			Console.WriteLine ("Heavy stats calculations");

			List<DBExpense> expenses = null;
			bool all_time = settings.get_all_time ();
			DateTime to_date = settings.from_date.AddMonths (settings.months).AddDays (settings.days);
			if (settings.selected_sheets == null) {
				if( all_time )
					expenses = doc.get_full_expenses_for_sheet ("", false);
				else
					expenses = doc.get_full_expenses_for_sheet ("", false, settings.from_date.Ticks, to_date.Ticks);
			}
			else {
				foreach (var s in settings.selected_sheets) {
					List<DBExpense> ex;
					if( all_time )
						ex = doc.get_full_expenses_for_sheet (s, false);
					else
						ex = doc.get_full_expenses_for_sheet (s, false, settings.from_date.Ticks, to_date.Ticks);
					if (expenses == null)
						expenses = ex;
					else
						expenses.AddRange (ex);
				}
			}
			List<DBExpense> planned_expenses = null;
			if (settings.selected_sheets == null)
				planned_expenses = doc.get_full_expenses_for_sheet ("", true);
			else {
				foreach (var s in settings.selected_sheets) {
					List<DBExpense> ex = doc.get_full_expenses_for_sheet (s, true);
					if (planned_expenses == null)
						planned_expenses = ex;
					else
						planned_expenses.AddRange (ex);
				}
			}
			List<DBExpense> planned_no_date = new List<DBExpense>();
			List<DBExpenseInstance> planned_instances = new List<DBExpenseInstance>();
			List<DBExpense> planned_per_months = new List<DBExpense>();
			foreach (var plex in planned_expenses) {
				//if (!in_accounts (plex))
				//	continue;
				if (plex.date == 0) {
					//if( all_time )
					//	planned_instances.Add (new DBExpenseInstance{ex=plex, date=plex.date});
					//else
					planned_no_date.Add (plex);
					continue;
				}
				if (plex.recurrence == DBReccurence.NEVER) {
					if (all_time || plex.date >= settings.from_date.Ticks && plex.date < to_date.Ticks)
						planned_instances.Add (new DBExpenseInstance{ex=plex, date=plex.date});
					continue;
				}
				//if (plex.date >= to_date.Ticks)
				//	continue;
				DateTime sd = new DateTime (plex.date, DateTimeKind.Local);
				if(!all_time)
					while (sd.Ticks < to_date.Ticks) { // Can take a long time :)
						if(sd.Ticks >= settings.from_date.Ticks)
							planned_instances.Add (new DBExpenseInstance{ex=plex, date=sd.Ticks});
						sd = DBExpense.find_next_date (sd, plex.recurrence);
					}
				if( all_time )
					planned_per_months.Add (plex);
			}
			balances = new ExpenseGroup[2]{new ExpenseGroup (), new ExpenseGroup ()};
			Dictionary<string, ExpenseGroup>[] cats_collectors = new Dictionary<string, ExpenseGroup>[2]{new Dictionary<string, ExpenseGroup>(), new Dictionary<string, ExpenseGroup>()};
			Dictionary<long, ExpenseGroup> day_collector = new Dictionary<long, ExpenseGroup>();
			foreach (var ex in expenses) {
				update_balance (new DBExpenseInstance{ex=ex, date=ex.date}, 1, NORMAL, balances, cats_collectors, day_collector);
			}
			foreach (var ex in planned_instances) {
				update_balance (ex, 1, PLANNED, balances, cats_collectors, day_collector);
			}
			foreach (var ex in planned_no_date) {
				update_balance (new DBExpenseInstance{ex=ex, date=ex.date}, 1, PLANNED_NO_DATE, balances, cats_collectors, day_collector);
			}
			foreach (var ex in planned_per_months) {
				decimal multiplier = DBExpense.get_month_multiplier (ex.recurrence);
				update_balance (new DBExpenseInstance{ex=ex, date=ex.date}, multiplier, PER_MONTH, balances, cats_collectors, day_collector);
			}
			for(int i = 0; i != 2; ++i){
				cat_groups [i] = new List<ExpenseGroup>( cats_collectors [i].Values );
			}
			day_groups = new List<ExpenseGroup> (day_collector.Values);
			day_groups.Sort (sort_by_date);
			style_changed ();
		}
		public List<DBExpenseInstance> find_expenses(ExpenseGroup gr)
		{
			for(int i = 0; i != gr.expenses.Length; ++i ) {
				if( gr.expenses[i].expenses.Count == 0 )
					continue;
				return gr.expenses [i].expenses;
			}
			return null;
		}
		static string[] section_titles = new string[]{"StatIncome20", "StatOutcome20", "StatIncomePlanned", "StatOutcomePlanned", "StatIncomePlannedNoDate", "StatOutcomePlannedNoDate", "StatIncomePlannedPerMonth", "StatOutcomePlannedPerMonth"};
		public class TableSource : UITableViewSource {
			StatsVC parent_vc;
			public TableSource (StatsVC parent_vc)
			{
				this.parent_vc = parent_vc;
			}
			public override nint NumberOfSections (UITableView tableView)
			{
				if( parent_vc.settings.style == StatsSettings.ReportStyle.ByCat )
					return 8;
				if( parent_vc.settings.style == StatsSettings.ReportStyle.ByDay )
					return 1;
				return 1;
			}
			public override nint RowsInSection (UITableView tableview, nint section)
			{
				if( parent_vc.settings.style == StatsSettings.ReportStyle.ByCat ) {
					var gr = parent_vc.split_cat_groups [section];
					return gr.Count;
				}
				if( parent_vc.settings.style == StatsSettings.ReportStyle.ByDay )
					return parent_vc.split_day_groups.Count;;
				//if( parent_vc.settings.style == StatsSettings.ReportStyle.ByDay )
				//	return parent_vc.day_groups.Count;
				return 3;
			}
			public override UIView GetViewForHeader (UITableView tableView, nint section)
			{
				if( parent_vc.settings.style == StatsSettings.ReportStyle.ByCat ) {
					UILabel label;
					UITableViewHeaderFooterView hfv = SectionHeader2.deque_header(tableView, out label);
					//SectionHeader sh = SectionHeader.create_or_get_header ();

					long sum_1000 = parent_vc.balances [section % 2].expenses [section/2].sum_sum_1000;
					string tit = StatsVC.section_titles[section];
					string val = parent_vc.report_currency_format.format_approximate_amount (sum_1000);
					//ecell.setImageNameValue(null, i18n.get (tit), val, section % 2 == 1);
					//ecell.setProgress (parent_vc.cat_max_balances[section] == 0 ? 0 : (float)Math.Abs(sum / parent_vc.cat_max_balances[section]), section % 2 == 1);
					//return ecell.ContentView;
					label.Text = i18n.get (tit).Replace("{amount}", val);
					return hfv;
				}
				if (parent_vc.settings.style == StatsSettings.ReportStyle.ByDay)
					return null;
				return null;
			}
			public override nfloat GetHeightForHeader (UITableView tableView, nint section)
			{
				if( parent_vc.settings.style == StatsSettings.ReportStyle.ByCat ) {
					var gr = parent_vc.split_cat_groups [section];
					return gr.Count == 0 ? 0 : SectionHeader2.default_height;
				}
				if (parent_vc.settings.style == StatsSettings.ReportStyle.ByDay)
					return 0;
				return 0;
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				ExpenseGroup gr;
				if( parent_vc.settings.style == StatsSettings.ReportStyle.ByCat ){
					ExpenseCell ecell = (ExpenseCell)tableView.DequeueReusableCell (ExpenseCell.Key);
					if (ecell == null)
						ecell = ExpenseCell.Create(tableView);
					gr = parent_vc.split_cat_groups[indexPath.Section][indexPath.Row];
					DBCategory cat = parent_vc.doc.get_category (gr.cat);
					long am_1000 = gr.sort_sum_1000;
					string val = parent_vc.report_currency_format.format_approximate_amount (am_1000);
					ecell.setImageNameValue(AppDelegate.app.get_category_image(cat), cat.get_loc_name(), val, am_1000 <= 0);
					ecell.setIndicator (indexPath.Section / 2 != NORMAL, false);
					ecell.setProgress (parent_vc.cat_max_balances_1000[indexPath.Section] == 0 ? 0 : (float)Math.Abs((double)am_1000 / parent_vc.cat_max_balances_1000[indexPath.Section]), am_1000 <= 0);
					return ecell;
				}
				if( parent_vc.settings.style == StatsSettings.ReportStyle.ByDay ){
					ExpenseCell ecell = (ExpenseCell)tableView.DequeueReusableCell (ExpenseCell.Key);
					if (ecell == null)
						ecell = ExpenseCell.Create(tableView);
					gr = parent_vc.split_day_groups[indexPath.Row];
					long am_1000= gr.sort_sum_1000;
					string val = parent_vc.report_currency_format.format_approximate_amount (am_1000);
					string date_str = AppDelegate.app.date_formatter.ToString ((NSDate)new DateTime (gr.date, DateTimeKind.Local));
					ecell.setImageNameValue(null, date_str, val, am_1000 <= 0);
					ecell.setIndicator (gr.expenses[PLANNED].expenses.Count != 0, false);
					ecell.setProgress (parent_vc.day_max_balance_1000 == 0 ? 0 : (float)Math.Abs((double)am_1000 / parent_vc.day_max_balance_1000), am_1000 <= 0);
					return ecell;
				}
				SimpleCell cell = (SimpleCell)tableView.DequeueReusableCell (SimpleCell.Key);
				if (cell == null)
					cell = SimpleCell.Create(tableView);
				cell.configAccessory (false, false);
				/*if (parent_vc.settings.style == StatsSettings.ReportStyle.ByDay) {
					gr = parent_vc.day_groups [indexPath.Row];
					string day_name = AppDelegate.app.date_week_formatter.ToString (new DateTime(gr.date, DateTimeKind.Local));
					decimal am = gr.sort_sum;
					string val = parent_vc.report_currency_format.format_approximate_amount (am);
					cell.setImageName (null, day_name, val);
				}
				if (parent_vc.settings.style == StatsSettings.ReportStyle.BySum) {
					gr = indexPath.Row == 2 ? parent_vc.total_balance : parent_vc.balances [indexPath.Row];
					decimal am = gr.sort_sum;
					string val = parent_vc.report_currency_format.format_approximate_amount (am);
					cell.setImageName (null, "Balance", val);
				}*/
				return cell;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				ExpenseGroup gr;
				if (parent_vc.settings.style == StatsSettings.ReportStyle.ByCat) {
					gr = parent_vc.split_cat_groups[indexPath.Section][indexPath.Row];
					StatsExpensesVC evc = new StatsExpensesVC (parent_vc, parent_vc.settings, indexPath.Section, gr.cat);

					DBCategory cat = parent_vc.doc.get_category (gr.cat);
					evc.set_expenses (parent_vc.doc, parent_vc.find_expenses(gr), cat.get_loc_name ());
					parent_vc.NavigationController.PushViewController(evc, true);
				}else if (parent_vc.settings.style == StatsSettings.ReportStyle.ByDay) {
					gr = parent_vc.split_day_groups[indexPath.Row];
					StatsExpensesVC evc = new StatsExpensesVC (parent_vc, parent_vc.settings, gr.date);

					string date_str = AppDelegate.app.date_formatter.ToString ((NSDate)new DateTime (gr.date, DateTimeKind.Local));
					evc.set_expenses (parent_vc.doc, parent_vc.find_expenses(gr), date_str);

					parent_vc.NavigationController.PushViewController(evc, true);
				}
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
		}	
		public class TableSource2 : UITableViewSource {
			StatsVC parent_vc;
			AccountCell measure_cell;
			public TableSource2 (StatsVC parent_vc)
			{
				this.parent_vc = parent_vc;
				measure_cell = AccountCell.Create(parent_vc.table);
			}
			public override nint NumberOfSections (UITableView tableView)
			{
				return 2;
			}
			public override nint RowsInSection (UITableView tableview, nint section)
			{
				if (section == 0)
					return 6;
				return 1;
			}
			public override UIView GetViewForHeader (UITableView tableView, nint section)
			{
				return null;
			}
			public override nfloat GetHeightForHeader (UITableView tableView, nint section)
			{
				return 0;
			}
			public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == 0) {
					var bal = parent_vc.balances [indexPath.Row % 2].expenses [indexPath.Row / 2];
					//if (bal.sum.Count == 0)
					//	return 0;
					return measure_cell.height_for_balance(false, bal.sum.Count);
				}
				//int count = parent_vc.total_balance;
				//if (count == 0) // Do not show empty
				//	return 0;
				return measure_cell.height_for_balance(false, parent_vc.total_balance.expenses[0].sum.Count);
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				AccountCell cell = (AccountCell)tableView.DequeueReusableCell (AccountCell.Key);
				if (cell == null)
					cell = AccountCell.Create(tableView);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
				if (indexPath.Section == 0) {
					var bal = parent_vc.balances [indexPath.Row % 2].expenses [indexPath.Row / 2];
					string tit = StatsVC.section_titles[indexPath.Row];
					cell.set_account_balance (i18n.get (tit).Replace(": {amount}", ""), false, bal.sum, parent_vc.settings.report_currency);
					return cell;
				}
				cell.set_account_balance (i18n.get ("StatBalance"), false, parent_vc.total_balance.expenses[0].sum, parent_vc.settings.report_currency);
				return cell;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				tableView.DeselectRow (indexPath, true);
			}
		}	
	}
}

