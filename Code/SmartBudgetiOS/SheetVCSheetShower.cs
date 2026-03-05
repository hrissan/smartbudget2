using System;
using SmartBudgetCommon;
using System.Collections.Generic;
using UIKit;
using System.Drawing;
using CoreGraphics;
using Foundation;
using ObjCRuntime;

namespace SmartBudgetiOS
{
	public partial class SheetVC
	{
		public class SheetShower {
			TableSource table_source;
			private List<List<DBExpense>> tableItems;
			private int tableItemsPlannedSectionsCount;
			private string filter_string;
			bool filter_scope_all;
			bool jump_on_show;
			private List<List<DBExpense>> filteredItems = new List<List<DBExpense>> ();

			UISearchDisplayController sdc;
			SheetVC parent_vc;
			DBExpense last_expense;
			public TitleButton title_button;
			public UIImageView imgCloudFrames;
			public UIImageView imgCloudProblem;
			public UIActivityIndicatorView loadActivity;
			public UITableView expenseTable;
			public UIView panelSheet;
			UIButton btnHelpSheet;
			UIButton btnAccounts;
			UIButton btnDocuments;
			UIButton btnDocumentsCounter;
			UIImageView imgDocumentsCounter;
			UIButton btnStats;
			UIButton btnSettings;
			public Document doc;
			public SheetShower(SheetVC parent_vc)
			{
				this.parent_vc = parent_vc;
				title_button = TitleButton.create_title_button ();
				title_button.button.TouchUpInside += (sender, e) => {
					parent_vc.NavigationController.PushViewController( SheetsVC.create_or_reuse(doc, doc.selected_sheet, true, (svc, sh)=>{
						if( doc.selected_sheet != sh )
						{
							doc.selected_sheet = sh;
							AppDelegate.app.docs.send_anything_changed(null);
						}
						this.jump_on_show = true;
						parent_vc.NavigationController.PopViewController(false);
						Utility.play_transition(parent_vc.NavigationController.View, UIViewAnimationTransition.CurlUp);
					}), false );
					Utility.play_transition(parent_vc.NavigationController.View, UIViewAnimationTransition.CurlDown);
				};
			}
			public void ViewDidLoad ()
			{
				AppDelegate.ReportTime("SheetVC.SheetShower 1");

				CGRect rr_cdv = parent_vc.View.Bounds;
				rr_cdv.Y -= AppDelegate.NOTEBOOK_HEADER_HEIGHT;
				rr_cdv.Height += AppDelegate.NOTEBOOK_HEADER_HEIGHT - BottomPanelView.PANEL_HEIGHT;
				parent_vc.currentDocView = new UIView (rr_cdv);
//				parent_vc.currentDocView.Bounds = parent_vc.currentDocViewParagonBounds ();
//				parent_vc.currentDocView.Center = parent_vc.currentDocViewParagonCenter ();
				parent_vc.currentDocView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
				parent_vc.currentDocView.Opaque = false;
				parent_vc.currentDocView.BackgroundColor = UIColor.Clear;
				parent_vc.View.AddSubview (parent_vc.currentDocView);
//				Console.WriteLine ("CDV: c={0},{1}", parent_vc.currentDocView.Center.X, parent_vc.currentDocView.Center.Y);

				expenseTable = AppDelegate.create_table_and_background (parent_vc.currentDocView, AppDelegate.NOTEBOOK_HEADER_HEIGHT, rr_cdv.Height - AppDelegate.NOTEBOOK_HEADER_HEIGHT, true);
				panelSheet = BottomPanelView.create_bottom_panel (parent_vc.View);

				btnHelpSheet = BottomPanelView.create_help_button( panelSheet, "help");
				if (UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Pad) {
					btnAccounts = BottomPanelView.create_bottom_button( panelSheet, "account");
					btnAccounts.TouchUpInside += (sender, e) => {
						Utility.slide_up(parent_vc.NavigationController, AccountsVC.create_or_reuse());
					};
				}
				btnDocuments = BottomPanelView.create_bottom_button( panelSheet, "document");
				btnStats = BottomPanelView.create_bottom_button( panelSheet, "stats");
				btnSettings = BottomPanelView.create_bottom_button( panelSheet, "settings"); 

				btnHelpSheet.TouchUpInside += (sender, e) => {
					GC.Collect();
					int cc = parent_vc.NavigationController.ViewControllers.Length;
					Console.WriteLine("GC total memory={0} cc={1}", GC.GetTotalMemory(true), cc);

					//Utility.simulate_memory_warning();
					//return;
					LayoutForHelp lh = new LayoutForHelp(parent_vc.NavigationController, expenseTable.Frame.Height);
					// From top
					lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, parent_vc.NavigationController.NavigationBar, 0, i18n.get ("HelpSwitchSheet"), LayoutForHelp.BubleType.BUTTON);
					lh.create_help_label(LayoutForHelp.LARGE_WIDTH, parent_vc.NavigationController.NavigationBar, parent_vc.NavigationController.NavigationBar.Bounds.Width/2 - 31, i18n.get ("HelpAddOperation"), LayoutForHelp.BubleType.BUTTON);
					UIImage att = title_button.button.ImageForState(UIControlState.Normal);
					if( att != null ) {
						nfloat delta = title_button.get_attention_delta();
						lh.create_help_label(LayoutForHelp.LARGE_WIDTH, title_button, -delta, i18n.get ("HelpPlannedOpDifferentSheet"), LayoutForHelp.BubleType.BUTTON);
					}

					if( expenseTable.ContentOffset.Y < parent_vc.tableHeader.Frame.Height / 2 ) {
						// From top
						lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, parent_vc.searchBar, -parent_vc.NavigationController.NavigationBar.Bounds.Width/2 + 60, i18n.get ("HelpTextSearch"), LayoutForHelp.BubleType.BUTTON);
					}
					else {
						// From bottom
						lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, expenseTable, 0, i18n.get ("HelpSlideForSearchPlanning"), LayoutForHelp.BubleType.ARROW_DOWN, false);
					}
					lh.show ();
				};
				btnStats.TouchUpInside += (sender, e) => {
					Utility.slide_up(parent_vc.NavigationController, StatsVC.create_or_reuse(doc));
				};
				btnSettings.TouchUpInside += (sender, e) => {
					Utility.show_popover_or_dialog(parent_vc.NavigationController, SettingsVC.create_or_reuse(false), btnSettings);
				};
				btnDocuments.TouchUpInside += (sender, e) => {
					//GC.Collect();
					//long mem = GC.GetTotalMemory(true);
					parent_vc.animate_to_scroll(true);
				};
				//parent_vc.searchBar = new UISearchBar (new Rectangle (0, 0, 320, 44));
				//expenseTable.TableHeaderView = parent_vc.searchBar;
				parent_vc.tableHeader.BackgroundColor = AppDelegate.table_background_color;

				Utility.fix_rtl_label (parent_vc.labelPlan);
				Utility.fix_rtl_view (parent_vc.btnHeaderPlan);
				Utility.fix_rtl_view (parent_vc.imgPlan);
				parent_vc.labelPlan.Text = i18n.get ("PlanNewOperation");

				expenseTable.TableHeaderView = parent_vc.tableHeader;
				parent_vc.btnHeaderPlan.TouchUpInside += (sender, e) => {
					if( AppDelegate.app.docs.get_shareware_progress(doc) >= Documents.SHAREWARE_LIMIT ) {
						FlurryAnalytics.Flurry.LogEvent("Limit", NSDictionary.FromObjectsAndKeys(new object[]{"NewPlanStart"}, new object[]{"from"}));
						Utility.slide_up(parent_vc.NavigationController, SettingsVC.create_or_reuse(true));
						return;
					}
					AmountVC am = AppDelegate.start_expense_chain(true, doc);
					FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"NewPlanStart"}, new object[]{"from"}));
					if( am != null )
						Utility.push_or_present (parent_vc.NavigationController, am, true);
				};
				expenseTable.TableFooterView = new UIView (new RectangleF(0,0,320,1));

				//sdc = new UISearchDisplayController (parent_vc.searchBar, parent_vc);

				//parent_vc.searchBar.ShowsScopeBar = true;//UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Pad;
				// On iPad 6.1.3 scope bar sometimes obscures "Plan operation"

				AppDelegate.ReportTime("SheetVC.SheetShower 2");
				rebuild_data ();
				table_source = new TableSource (this);
				//sdc.SearchResultsSource = table_source;
				//sdc.Delegate = new MySearchDisplayDelegate(this);

				expenseTable.Source = table_source;
				expenseTable.ReloadData ();
				adjust_footer ();
				jump_on_show = true;

				AppDelegate.ReportTime("SheetVC.SheetShower 3");
				{
					CGRect rr = btnDocuments.Bounds;
					imgCloudFrames = new UIImageView (new CGRect(rr.Width/2 - 9, 4, 18, 12));
					imgCloudFrames.AnimationImages = new UIImage[] { UIImage.FromBundle("new_design/wifi_small_1.png"), UIImage.FromBundle("new_design/wifi_small_2.png"), UIImage.FromBundle("new_design/wifi_small_3.png") };
					imgCloudFrames.AnimationDuration = 0.9;
					imgCloudFrames.StartAnimating ();
					imgCloudFrames.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin | UIViewAutoresizing.FlexibleBottomMargin;
					imgCloudProblem = new UIImageView (new CGRect(rr.Width/2 - 9, 0, 18, 18));
					imgCloudProblem.Image = AppDelegate.error_triangle_small;
					imgCloudProblem.AutoresizingMask = imgCloudFrames.AutoresizingMask;

					btnDocumentsCounter = new UIButton(UIButtonType.Custom);
					const int COUNTER_SIZE = 20;
					btnDocumentsCounter.Frame = new CGRect (rr.Width - COUNTER_SIZE - 4, 4, COUNTER_SIZE, COUNTER_SIZE);
					btnDocumentsCounter.SetTitleColor (AppDelegate.app.dark_background_color, UIControlState.Disabled);
					//btnDocumentsCounter.SetBackgroundImage(UIImage.FromBundle("new_design/counter.png"), UIControlState.Disabled);
					btnDocumentsCounter.Enabled = false;
					btnDocumentsCounter.Font = UIFont.BoldSystemFontOfSize (17);
					btnDocumentsCounter.AutoresizingMask = imgCloudFrames.AutoresizingMask;

					imgDocumentsCounter = new UIImageView (btnDocumentsCounter.Frame);
					imgDocumentsCounter.AnimationImages = new UIImage[] { UIImage.FromBundle("new_design/counter.png"), UIImage.FromBundle("new_design/counter2.png") };
					imgDocumentsCounter.AnimationDuration = 0.2;
					imgDocumentsCounter.StartAnimating ();
					imgDocumentsCounter.AutoresizingMask = btnDocumentsCounter.AutoresizingMask;
/*					CABasicAnimation theAnimation = CABasicAnimation.FromKeyPath ("opacity");
					theAnimation.Duration=0.2;
					theAnimation.RepeatCount=float.PositiveInfinity;
					theAnimation.AutoReverses=true;
					theAnimation.From = NSNumber.FromFloat (1);
					theAnimation.To = NSNumber.FromFloat (0.5f);
					btnDocumentsCounter.Layer.AddAnimation (theAnimation, "animateOpacity");
*/
					loadActivity = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.White);
					loadActivity.Center = btnDocumentsCounter.Center;
					loadActivity.HidesWhenStopped = true;
					loadActivity.AutoresizingMask = imgCloudFrames.AutoresizingMask;

					btnDocuments.AddSubview (imgCloudFrames);
					btnDocuments.AddSubview (imgCloudProblem);
					btnDocuments.AddSubview (imgDocumentsCounter);
					btnDocuments.AddSubview (btnDocumentsCounter);
					btnDocuments.AddSubview (loadActivity);
				}

				AppDelegate.ReportTime("SheetVC.SheetShower 4");
				update_ui();
				AppDelegate.ReportTime("SheetVC.SheetShower 5");
				AppDelegate.app.docs.anything_changed += (docs, e) => {
					anything_changed();
					//if( sdc.Active ) {
						//update_filtered();
						//sdc.SearchResultsTableView.ReloadData();
					//}
				};
			}
			public void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
			{
			}
			public void ViewWillAppear ()
			{
				if (jump_on_show)
					jump_at_best_position ();
				jump_on_show = false;
			}
			public void AppDidEnterForeground()
			{
				jump_at_best_position ();
			}
			public void viewWillLayoutSubviews()
			{
				adjust_footer ();
				BottomPanelView.layout (panelSheet, btnHelpSheet);
			}
			public void start_new_expense()
			{
				if( AppDelegate.app.docs.get_shareware_progress(doc) >= Documents.SHAREWARE_LIMIT ) {
					FlurryAnalytics.Flurry.LogEvent("Limit", NSDictionary.FromObjectsAndKeys(new object[]{"NewExpenseStart"}, new object[]{"from"}));
					Utility.slide_up(parent_vc.NavigationController, SettingsVC.create_or_reuse(true));
					return;
				}
				FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"NewExpenseStart"}, new object[]{"from"}));
				AmountVC am = AppDelegate.start_expense_chain(false, doc);
				if( am != null )
					Utility.push_or_present (parent_vc.NavigationController, am, true);
			}
			void jump_at_best_position()
			{
				NSIndexPath ip = null;
				const int MAX_SHOW = 3;
				int total_counter = 0;
				if (tableItems.Count > tableItemsPlannedSectionsCount) // Have not planned
					ip = NSIndexPath.FromRowSection (0, tableItemsPlannedSectionsCount); // First non planned
				else if(tableItemsPlannedSectionsCount > 0 )
					ip = NSIndexPath.FromRowSection (tableItems[tableItemsPlannedSectionsCount - 1].Count - 1, tableItemsPlannedSectionsCount - 1); // Last planned
				// Get back one by one planned soon, but not more than 3
				for (int i = tableItemsPlannedSectionsCount; --i >= 0 && total_counter < MAX_SHOW;) {
					for(int j = tableItems [i].Count; --j >=0 && total_counter < MAX_SHOW;){
						DBExpense ex = tableItems [i] [0];
						if (Documents.planned_soon (ex.date)) {
							ip = NSIndexPath.FromRowSection (j, i);
							total_counter += 1;
						} else
							total_counter = MAX_SHOW;
					}
				}
				if (ip != null)
					expenseTable.ScrollToRow (ip, UITableViewScrollPosition.Top, false);
				else
					expenseTable.SetContentOffset (new CGPoint(0, expenseTable.TableHeaderView.Frame.Height), false);
			}
			void adjust_footer()
			{
				int sections = tableItems.Count;
				int rows = 0;
				foreach (var t in tableItems) {
					rows += t.Count;
				}
				nfloat height = sections * SectionHeader2.default_height + rows * expenseTable.RowHeight;
				nfloat th = expenseTable.Frame.Height;
				nfloat footer_height = th - height;
				if (footer_height < 1)
					footer_height = 1;
				UIView v = expenseTable.TableFooterView;
				if( v.Frame.Height != footer_height ) { // Valuable optimization
					v.Frame = new CGRect (0, 0, 320, footer_height);
					expenseTable.TableFooterView = v;
				}
			}
			public void select_doc(Document doc)
			{
				this.doc = doc;
				//jump_on_show = true;
				anything_changed (); // Will be posted anyway
				jump_at_best_position ();
			}
			public static void split_by_categories(List<DBExpense> items, List<List<DBExpense>> result, DBExpense last_expense, List<NSIndexPath> rows_to_add, NSMutableIndexSet sections_to_add)
			{
				const long DAY_IN_TICKS = 86400L * 10000000L;
				//DateTime previous_date;
				long previous_date = 0;
				if( last_expense != null ) {
					//DateTime debug_previous_date = new DateTime (last_expense.date, DateTimeKind.Local).Date;
					previous_date = last_expense.date / DAY_IN_TICKS;
				}
				foreach (var it in items) {
					//DateTime debug_da = new DateTime (it.date, DateTimeKind.Local).Date;
					long da = it.date / DAY_IN_TICKS;
					if (result.Count == 0 || da != previous_date) {
						if( sections_to_add != null )
							sections_to_add.Add ((uint)result.Count); // New section
						result.Add (new List<DBExpense>());
						rows_to_add = null; // Not add rows any more
					}
					previous_date = da;
					if (rows_to_add != null)
						rows_to_add.Add (NSIndexPath.FromRowSection (result [result.Count - 1].Count, result.Count - 1));
					result [result.Count-1].Add (it);
				}
			}
			public void rebuild_data()
			{
				tableItems = new List<List<DBExpense>> ();
				tableItemsPlannedSectionsCount = 0;
				last_expense = null;
				if( doc != null ) {
					List<DBExpense> items = doc.get_all_planned_expenses_for_sheet (doc.selected_sheet);
					split_by_categories (items, tableItems, null, null, null);
					DBExpense.move_without_date_to_start (tableItems);
					tableItemsPlannedSectionsCount = tableItems.Count;
					items = doc.get_expenses_for_sheet (doc.selected_sheet, false, null);
					if( items.Count != 0 )
						last_expense = items [items.Count-1];
					//Console.WriteLine ("Loaded {0}", items.Count);
					List<List<DBExpense>> result = new List<List<DBExpense>> ();
					split_by_categories (items, result, null, null, null);
					tableItems.AddRange( result );
				}
			}
			public void update_ui()
			{
				string selected_sheet = doc == null ? "" : doc.selected_sheet;

				if( String.IsNullOrEmpty(selected_sheet) )
				{
					parent_vc.searchBar.ScopeButtonTitles = new[] { i18n.get("TotalSheet") };
					title_button.set_text_image(i18n.get ("TotalSheet"), null);
				}
				else
				{
					DBSheet sh = doc.get_sheet (selected_sheet);
//					if (sh == null)
//						sh = sh;
					long planned_date;
					doc.get_sheet_next_planned_date (selected_sheet, out planned_date, false);
					string scope_name = sh.get_loc_name();
					const int MAX_LENGTH = 15; // TODO - measure string instead?
					if (scope_name.Length > MAX_LENGTH)
						scope_name = scope_name.Substring (0, MAX_LENGTH) + "…";
					parent_vc.searchBar.ScopeButtonTitles = new[] { scope_name, i18n.get("TotalSheet") };
					title_button.set_text_image (sh.get_loc_name(), Documents.planned_soon (planned_date) ? AppDelegate.attention_icon_top : null);
				}
				if (doc != null && doc.is_published ()) {
					if (doc.need_to_sync(false)) {
						imgCloudFrames.Hidden = false;
						imgCloudProblem.Hidden = true;
					} else {
						string problem = doc.get_cloud_problem_text(false);
						imgCloudFrames.Hidden = true;
						imgCloudProblem.Hidden = String.IsNullOrEmpty(problem);
					}
				} else {
					imgCloudFrames.Hidden = true;
					imgCloudProblem.Hidden = true;
				}

				int nd = AppDelegate.app.docs.new_documents.Count;
				btnDocumentsCounter.Hidden = nd == 0;
				imgDocumentsCounter.Hidden = nd == 0;
				if (nd != 0)
					btnDocumentsCounter.SetTitle (Math.Min (9, nd).ToString (), UIControlState.Normal);// Culture ok, show max 9
				bool loa = AppDelegate.app.docs.loading_operations_in_progress != 0;
				if (loa && !loadActivity.IsAnimating)
					loadActivity.StartAnimating ();
				if (!loa && loadActivity.IsAnimating)
					loadActivity.StopAnimating ();
			}
			public void anything_changed()
			{
				AppDelegate.ReportTime ("SheetVCSheetShower.anything_changed 1");

				rebuild_data ();
				expenseTable.ReloadData ();
				adjust_footer ();
				update_ui ();

				AppDelegate.ReportTime ("SheetVCSheetShower.anything_changed 2");
			}

			void load_more() {
				if (last_expense == null)
					return;
				string selected_sheet = doc.selected_sheet;

				DateTime was = DateTime.UtcNow;
				List<DBExpense> items = doc.get_expenses_for_sheet (selected_sheet, false, last_expense);
				//DateTime now = DateTime.UtcNow;
				//Console.WriteLine("load_more1 {0, 8} mksec", now.Subtract (was).Ticks / 10);
				//was = now;
				//Console.WriteLine ("Loaded more {0}", items.Count);
				if (items.Count == 0) {
					last_expense = null;
					return;
				}
				List<NSIndexPath> rows_to_add = new List<NSIndexPath>();
				NSMutableIndexSet sections_to_add = new NSMutableIndexSet ();
				split_by_categories (items, tableItems, last_expense, rows_to_add, sections_to_add);
				last_expense = items [items.Count-1];
				//now = DateTime.UtcNow;
				//Console.WriteLine("load_more2 {0, 8} mksec", now.Subtract (was).Ticks / 10);
				//was = now;
				expenseTable.BeginUpdates ();
				expenseTable.InsertRows(rows_to_add.ToArray(), UITableViewRowAnimation.None);
				expenseTable.InsertSections(sections_to_add, UITableViewRowAnimation.None);
				expenseTable.EndUpdates ();
				//expenseTable.ReloadData ();
				//adjust_footer (); - save time, if loading more, footer is already absent
				//now = DateTime.UtcNow;
				//Console.WriteLine("load_more3 {0, 8} mksec", now.Subtract (was).Ticks / 10);
				//was = now;
			}
			private void update_filtered()
			{
				filteredItems = new List<List<DBExpense>> ();
				if (doc == null)
					return;
				if (String.IsNullOrEmpty (filter_string))
					return;
				List<DBExpense> items;
				string sh = filter_scope_all ? "" : doc.selected_sheet;
				items = doc.search_expenses_for_sheet (filter_string, sh, true);
				split_by_categories (items, filteredItems, null, null, null);
				DBExpense.move_without_date_to_start (filteredItems);
				items = doc.search_expenses_for_sheet (filter_string, sh, false);
				List<List<DBExpense>> result = new List<List<DBExpense>> ();
				split_by_categories (items, result, null, null, null);
				filteredItems.AddRange( result);
			}
			public class MySearchDisplayDelegate : UISearchDisplayDelegate {
				SheetShower sheet_shower;
				CGRect searchbarrect;
				public MySearchDisplayDelegate(SheetShower sheet_shower)
				{
					this.sheet_shower = sheet_shower;
					searchbarrect = sheet_shower.parent_vc.searchBar.Frame;
				}
				public override void DidLoadSearchResults (UISearchDisplayController controller, UITableView tableView)
				{
					tableView.BackgroundColor = AppDelegate.table_background_color;
					tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
					tableView.RegisterClassForHeaderFooterViewReuse (typeof(UITableViewHeaderFooterView), SectionHeader2.Key);
				}
				public override bool ShouldReloadForSearchScope (UISearchDisplayController controller, nint forSearchOption)
				{
					sheet_shower.filter_string = controller.SearchBar.Text;
					sheet_shower.filter_scope_all = ((sheet_shower.doc == null || String.IsNullOrEmpty(sheet_shower.doc.selected_sheet)) && forSearchOption == 0) || forSearchOption == 1;

					sheet_shower.update_filtered ();
					return true;
				}
				public override bool ShouldReloadForSearchString (UISearchDisplayController controller, string forSearchString)
				{
					nint scope_id = controller.SearchBar.SelectedScopeButtonIndex;
					sheet_shower.filter_string = forSearchString;
					sheet_shower.filter_scope_all = ((sheet_shower.doc == null || String.IsNullOrEmpty(sheet_shower.doc.selected_sheet)) && scope_id == 0) || scope_id == 1;

					sheet_shower.update_filtered();
					return true;
				}
				public override void DidEndSearch (UISearchDisplayController controller)
				{
					searchbarrect.Width = this.sheet_shower.parent_vc.currentDocView.Bounds.Width;
					this.sheet_shower.parent_vc.searchBar.Frame = searchbarrect;
				}
				public override void DidBeginSearch (UISearchDisplayController controller)
				{
				}
				public override void WillHideSearchResults (UISearchDisplayController controller, UITableView tableView)
				{
				}
			};
			public class TableSource : UITableViewSource {
				SheetShower sheet_shower;
				public TableSource (SheetShower sheet_shower)
				{
					this.sheet_shower = sheet_shower;
				}
				public override nint NumberOfSections (UITableView tableView)
				{
					//if (tableView == sheet_shower.sdc.SearchResultsTableView)
						//return sheet_shower.filteredItems.Count;
					return sheet_shower.tableItems.Count;
				}
				/*				private void jump_at_header(UIScrollView scrollView)
				{
					float pos_y = scrollView.ContentOffset.Y;
					float head_height = sheet_shower.parent_vc.tableHeader.Frame.Height;
					if (pos_y < head_height) {
						if (pos_y < head_height / 2)
							scrollView.SetContentOffset (new PointF(0, 0), true);
						else
							scrollView.SetContentOffset (new PointF(0, head_height), true);
					}
				}
				public override void DecelerationEnded (UIScrollView scrollView)
				{
					if (scrollView == sheet_shower.sdc.SearchResultsTableView)
						return;
					jump_at_header (scrollView);
				}
				public override void DraggingEnded (UIScrollView scrollView, bool willDecelerate)
				{
					if (scrollView == sheet_shower.sdc.SearchResultsTableView)
						return;
					//Console.WriteLine ("DraggingEnded {0}", willDecelerate);
					if( !willDecelerate )
						jump_at_header (scrollView);
				}*/
				public override void Scrolled (UIScrollView scrollView)
				{
					//if (scrollView == sheet_shower.sdc.SearchResultsTableView)
						//return;
					nfloat height = scrollView.Bounds.Height;
					nfloat bottom_pos_y = scrollView.ContentOffset.Y + height;
					if (bottom_pos_y > scrollView.ContentSize.Height - height)
						sheet_shower.load_more ();
				}
				public override nfloat GetHeightForHeader (UITableView tableView, nint section)
				{
					return SectionHeader2.default_height;
				}
				public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
				{
					if( editingStyle != UITableViewCellEditingStyle.Delete )
						return;
					List<DBExpense> li;
					//if (tableView == sheet_shower.sdc.SearchResultsTableView)
						//li = sheet_shower.filteredItems[indexPath.Section];
					//else
						li = sheet_shower.tableItems[indexPath.Section];
					DBExpense ex = sheet_shower.doc.get_expense (li[indexPath.Row].id);
					//tableView.BeginUpdates ();
					FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"DeleteSwipe"}, new object[]{"action"}));
					AppDelegate.app.docs.execute_change (new ChangeExpenseRemove(sheet_shower.doc, ex.id), null);
					//tableView.DeleteRows (new NSIndexPath[]{indexPath}, UITableViewRowAnimation.Left);
					//tableView.EndUpdates ();
					//updateVisibleCellCheckmarks (tableView);
				}
				public override UIView GetViewForHeader (UITableView tableView, nint section)
				{
					UILabel label;
					UITableViewHeaderFooterView hfv = SectionHeader2.deque_header(tableView, out label);

					DBExpense ex;
					// no get_expense, have date already
					//Console.WriteLine("GetViewForHeader");
					//if (tableView == sheet_shower.sdc.SearchResultsTableView)
						//ex = sheet_shower.filteredItems[(int)section][0];
					//else
						ex = sheet_shower.tableItems[(int)section][0];
					if( ex.date == 0)
						label.Text = i18n.get ("SectionNoDate");
					else {
						string str = AppDelegate.app.date_week_formatter.ToString ((NSDate)new DateTime (ex.date, DateTimeKind.Local));
						if( ex.planned )
							label.Text = i18n.get ("SectionPlannedFor").Replace("{date}", str);
						else
							label.Text = str;
					}
					return hfv;
				}
				public override nint RowsInSection (UITableView tableView, nint section)
				{
					//if (tableView == sheet_shower.sdc.SearchResultsTableView)
						//return sheet_shower.filteredItems[(int)section].Count;
					return sheet_shower.tableItems[(int)section].Count;
				}
				public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
				{
					ExpenseCell cell = (ExpenseCell)tableView.DequeueReusableCell (ExpenseCell.Key);
					if (cell == null)
						cell = ExpenseCell.Create(tableView);
					List<List<DBExpense>> lli;
					//if (tableView == sheet_shower.sdc.SearchResultsTableView)
						//lli = sheet_shower.filteredItems;
					//else
						lli = sheet_shower.tableItems;
					List<DBExpense> li = lli[indexPath.Section];
					//DateTime was = DateTime.UtcNow;
					DBExpense ex = sheet_shower.doc.get_expense (li[indexPath.Row].id);
					cell.setExpense (ex, sheet_shower.doc);
					//cell.setFakeExpense(ex, sheet_shower.doc);
					//DateTime now = DateTime.UtcNow;
					//Console.WriteLine("get_expense {0, 8} mksec", now.Subtract (was).Ticks / 10);

					cell.separator.Hidden = (indexPath.Row == li.Count - 1) && (indexPath.Section != lli.Count - 1);
					return cell;
				}
				public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
				{
					DBExpense ex;
					//if (tableView == sheet_shower.sdc.SearchResultsTableView)
						//ex = sheet_shower.doc.get_expense (sheet_shower.filteredItems[indexPath.Section][indexPath.Row].id);
					//else
						ex = sheet_shower.doc.get_expense (sheet_shower.tableItems[indexPath.Section][indexPath.Row].id);
					Utility.push_or_present (sheet_shower.parent_vc.NavigationController, ExpenseVC.create_or_reuse (sheet_shower.doc, ex, null), true);
					tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
				}
			}	
		}
	}
}

