using System;
using System.Drawing;
using Foundation;
using UIKit;
using CoreGraphics;
using SmartBudgetCommon;
using AddressBook;
using AddressBookUI;
using System.Collections.Generic;

namespace SmartBudgetiOS
{
	public partial class ExpenseVC : UIViewController
	{
		private bool expense_changed;
		private DBExpense ex;
		private long return_date; // For new LOAN expense
		private DBExpense connected_planned_expense;
		private UIBarButtonItem cancel_item;
		private UIBarButtonItem done_item;
		private UIBarButtonItem save_item;
		private UIView panelButtons;
		private UITableView table;
		private UIButton btnHelp;
		private UIButton btnSheet;
		private UIButton btnRemove;
		private UIButton btnPerform;
		private UIButton btnPlan;
		private UIButton btnContact;

		private UIImage contact_image;
		private bool show_sheet;

		private bool previous_date_selected_automatically;
		private bool save_automatic_date;
		private DateTime initial_date_today;

		private int cells_count;
		private int date_index;
		private int amount1_index;
		private int amount2_index;
		private int category_index;
		private int name_index;
		private int return_date_index;
		private int recurrence_index;
		private int sheet_index;
		private Document doc;

		public ExpenseVC () : base ("ExpenseVC", null)
		{
			cancel_item = new UIBarButtonItem(UIBarButtonSystemItem.Cancel, (sender, e) => {
				Utility.dismiss_or_pop (NavigationController, true);
			});
			done_item = new UIBarButtonItem(UIBarButtonSystemItem.Done, (sender, e) => {
				Utility.dismiss_or_pop (NavigationController, true);
			});
			save_item = new UIBarButtonItem(UIBarButtonSystemItem.Save, (sender, e) => {
				if( String.IsNullOrEmpty(ex.id) ){
					bool also_plan = !ex.planned && ex.recurrence != DBReccurence.NEVER;
					if( also_plan )
					{
						DBExpense planned_expense = ex.Clone();
						planned_expense.date = DBExpense.find_next_date(ex.date, ex.recurrence);
						planned_expense.planned = true;
						FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"NewAlsoPlan"}, new object[]{"action"}));
						AppDelegate.app.docs.execute_change( new ChangeExpenseCreate(doc, planned_expense) );
					}
					if( ex.category == Document.LOAN_CATEGORY )
					{
						DBExpense loan_back_expense = ex.Clone();
						loan_back_expense.date = return_date;
						loan_back_expense.category = Document.LOAN_BACK_CATEGORY;
						loan_back_expense.planned = true;
						loan_back_expense.sum.amount_1000 = -loan_back_expense.sum.amount_1000;
						FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"NewAlsoLoanBack"}, new object[]{"action"}));
						AppDelegate.app.docs.execute_change( new ChangeExpenseCreate(doc, loan_back_expense) );
					}
					if( save_automatic_date ) {
						AppDelegate.app.docs.new_expense_date = ex.date;
						if( new DateTime(ex.date, DateTimeKind.Local).Date != initial_date_today.Date ) {
							AppDelegate.app.docs.new_expense_date_set_utc = DateTime.UtcNow;
						}else{
							AppDelegate.app.docs.new_expense_date_set_utc = new DateTime(0, DateTimeKind.Utc);
						}
						AppDelegate.app.docs.save_settings ();
					}
					FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"New"}, new object[]{"action"}));
					AppDelegate.app.docs.execute_change( new ChangeExpenseCreate(doc, ex) );
				}
				else
					AppDelegate.app.docs.execute_change( new ChangeExpenseUpdate(doc, ex) );
				bool pop_two = false;
				if( connected_planned_expense != null )
				{
					if( connected_planned_expense.recurrence == DBReccurence.NEVER )
					{
						//NavigationController.PopViewControllerAnimated(true); // TODO - Check on iPAD
						AppDelegate.app.docs.execute_change( new ChangeExpenseRemove(doc, connected_planned_expense.id) );
						pop_two = true;
					}
					else
					{
						connected_planned_expense.date = DBExpense.find_next_date(connected_planned_expense.date, connected_planned_expense.recurrence);
						AppDelegate.app.docs.execute_change( new ChangeExpenseUpdate(doc, connected_planned_expense) );
					}
				}
				if( pop_two ) {
					UINavigationController nc = NavigationController.PresentingViewController as UINavigationController;
					if( nc.ViewControllers.Length == 1 )
						nc.PresentingViewController.DismissViewController(true, null);
					else {
						nc.PopViewController(false);
						nc.DismissViewController(true, null);
					}
				}
				else
					Utility.dismiss_or_pop (NavigationController, true);
			});
		}
		void construct(Document doc, DBExpense ex, DBExpense connected_planned_expense, long return_date)
		{
			this.doc = doc;
			expense_changed = String.IsNullOrEmpty(ex.id);
			previous_date_selected_automatically = false;
			this.return_date = return_date;
			this.ex = ex;
			this.connected_planned_expense = connected_planned_expense;
			this.show_sheet = false; // We decided to never show it
			if (String.IsNullOrEmpty(ex.id) && !ex.planned) {
				initial_date_today = DateTime.Now;
				if (ex.date == 0) {
					ex.date = AppDelegate.app.docs.recently_selected_new_expense_date () ? AppDelegate.app.docs.new_expense_date : initial_date_today.Ticks;
					save_automatic_date = true;
				} else {
				}
				previous_date_selected_automatically = new DateTime (ex.date, DateTimeKind.Local).Date != initial_date_today.Date;
			}
			NavigationItem.Title = get_expense_kind ();
		}
		~ExpenseVC()
		{
			Console.WriteLine ("~ExpenseVC");
		}
		private static Utility.ReuseVC<ExpenseVC> reuse = new Utility.ReuseVC<ExpenseVC> ();
		public static ExpenseVC create_or_reuse(Document doc, DBExpense ex, DBExpense connected_planned_expense, long return_date = 0)
		{
			ExpenseVC result = reuse.create_or_reuse();
			result.construct(doc, ex, connected_planned_expense, return_date);
			return result;
		}
		public override bool ShouldAutorotate ()
		{
			return true;
		}
		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations ()
		{
			return UIInterfaceOrientationMask.All;
		}
		//[Obsolete ("Deprecated in iOS6. Replace it with both GetSupportedInterfaceOrientations and PreferredInterfaceOrientationForPresentation")]
		//public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		//{
		//	return true;
		//}
		string get_expense_kind()
		{
			if( ex.category == Document.LOAN_CATEGORY )
				return i18n.get("CatLoan");
			if( ex.category == Document.LOAN_BACK_CATEGORY )
				return i18n.get("CatLoanBack"); // TODO - add string
			if( ex.category == Document.TRANSFER_CATEGORY )
				return i18n.get(@"CatTransfer");
			if( ex.category == Document.CONVERT_CATEGORY )
				return i18n.get(@"CatConvert");
			if( ex.sum.amount_1000 <= 0 )
				return i18n.get("EXPENSE_KIND");
			return i18n.get("INCOME_KIND");
		}
		public void anything_changed()
		{
//			if (!String.IsNullOrEmpty (ex.id) && doc.get_expense (ex.id) == null)
//				Utility.dismiss_or_pop (NavigationController, true);
			// TODO - see if account sheet or category deleted, react accordingly
			DBSheet sh = doc.get_sheet (ex.sheet);
			if (sh == null && doc.sorted_sheets.Count != 0)
				ex.sheet =  doc.sorted_sheets [0].id;
			DBCategory cat = doc.get_category (ex.category);
			if (cat == null) {
				var sc = doc.get_signed_categories(Math.Sign(ex.sum.amount_1000));
				if (sc.Count != 0)
					ex.category = sc [0].id;
			}
			DBAccount acc = doc.get_account (ex.sum.account);
			if (acc == null && doc.sorted_accounts.Count != 0)
				ex.sum.account = doc.sorted_accounts [0].id;
			if (ex.sum2.IsValid ()) {
				acc = doc.get_account (ex.sum2.account);
				if (acc == null && doc.sorted_accounts.Count != 0)
					ex.sum2.account = doc.sorted_accounts [0].id;
			}
			update_contact ();
			update ();
		}
		private static void add_split(List<string> els, string el)
		{
			if (el == null)
				return;
			string[] name_parts = StringFolding.Split (el);
			els.AddRange (name_parts);
		}
		private static List<string> fill_elements(ABPerson peop)
		{
			List<string> elements = new List<string> ();
			add_split(elements, peop.FirstName);
			add_split(elements, peop.MiddleName);
			add_split(elements, peop.LastName);

			add_split(elements, peop.Organization);
			add_split(elements, peop.Department);
			add_split(elements, peop.JobTitle);

			add_split(elements, peop.Nickname);

			add_split(elements, peop.FirstNamePhonetic);
			add_split(elements, peop.MiddleNamePhonetic);
			add_split(elements, peop.LastNamePhonetic);

			add_split(elements, peop.Prefix);
			add_split(elements, peop.Suffix);
			return elements;
		}
		public static string format_person(ABPerson peop)
		{
			string na = peop.ToString();
			if (na == null) // TODO - why? Copied from iPhone code
				na = "";
			List<string> name_parts = new List<string>( StringFolding.Split (na) );

			List<string> elements = fill_elements (peop);
			string combined = "";
			foreach (string el in elements) {
				int ind = name_parts.IndexOf (el);
				if (ind != -1)
					continue;
				combined += (combined == "") ? " (" : ", ";
				combined += el;
			}
			if (combined != "")
				combined += ")";
			na += combined;
			if( na == "" )
				na = i18n.get("ContactIndicator");
			return na;
		}
		private static ABPerson find_best_contact(ABAddressBook ab, string name)
		{
			if (ab == null)
				return null;
			Console.WriteLine ("find_best_contact");
			ABPerson best_contact = null;
			string[] name_parts = StringFolding.Split (name);
			var people = ab.GetPeopleWithName (name);
			if (people.Length == 1)
				best_contact = people [0];
			int best_weight = -1000;
			foreach (var peop in people) {
				List<string> elements = fill_elements (peop);
				int weight_found = 0;
				foreach (string part in name_parts) {
					int ind = elements.IndexOf (part);
					if (ind == -1) { // element of the name not found exactly - very bad
						weight_found -= 12;
						continue;
					}
					elements.RemoveAt (ind);
					weight_found += 5; // ok - part of the name found exactly
				}
				Console.WriteLine ("ABPerson=({0}) name=({1}) weight_found={2}-{3}={4}", peop.ToString (), name, weight_found, elements.Count, weight_found - elements.Count);
				weight_found -= elements.Count; // excess elements - bad
				if (weight_found > best_weight) {
					best_contact = peop;
					best_weight = weight_found;
				}
			}
			return best_contact;
		}
		public void update_contact()
		{
			ABPerson best_contact = null;
			if (ex.is_any_loan() && !String.IsNullOrEmpty(ex.name)) {
				NSError error;
				using (ABAddressBook ab = ABAddressBook.Create(out error)) {
					best_contact = find_best_contact (ab, ex.name);
				}
			}
			contact_image = best_contact != null ? UIImage.FromBundle ("b_contact.png") : null; // TODO image?
			btnContact.Enabled = best_contact != null;
		}
		public void update()
		{
			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
				NavigationItem.LeftBarButtonItem = expense_changed ? cancel_item : done_item;
			else
				NavigationItem.LeftBarButtonItem = expense_changed ? cancel_item : null;
			NavigationItem.RightBarButtonItem = expense_changed ? save_item : null;
			btnRemove.Enabled = !String.IsNullOrEmpty(ex.id);
			btnPlan.Hidden = !(!ex.planned && !expense_changed); // TODO check logic
			btnPerform.Hidden = !(ex.planned && !expense_changed);
			{
				bool fu = AppDelegate.app.docs.full_version;
				viewWarning.Hidden = !(String.IsNullOrEmpty(ex.id) && !fu);
				if (!fu) {
					float pr = Math.Min(1f, AppDelegate.app.docs.get_shareware_progress (doc) * 1f / Documents.SHAREWARE_LIMIT);
					CGRect fr = warningRed.Frame;
					float si = (float)Math.Floor (fr.Width * (1 - pr));
					fr.X += fr.Width - si;
					fr.Width = si;
					warningGreen.Frame = fr;
				}
			}
			bool show_rec = ex.planned || (String.IsNullOrEmpty(ex.id) && connected_planned_expense == null);
			cells_count = 0;
			if (ex.is_any_loan()) {
				date_index = cells_count++;
				amount1_index = cells_count++;
				amount2_index = -1;
				category_index = -1;
				name_index = cells_count++;
				return_date_index = String.IsNullOrEmpty(ex.id) ? (cells_count++) : -1;
				recurrence_index = -1;
			} else if (ex.category == Document.TRANSFER_CATEGORY || ex.category == Document.CONVERT_CATEGORY) {
				date_index = cells_count++;
				amount1_index = cells_count++;
				amount2_index = cells_count++;
				category_index = -1;
				name_index = cells_count++;
				return_date_index = -1;
				recurrence_index = show_rec ? (cells_count++) : -1;
			} else {
				date_index = cells_count++;
				amount1_index = cells_count++;
				amount2_index = -1;
				category_index = cells_count++;
				name_index = cells_count++;
				return_date_index = -1;
				recurrence_index = show_rec ? (cells_count++) : -1;
			}
			sheet_index = show_sheet ? (cells_count++) : -1;
			btnSheet.Enabled = !show_sheet && doc.sorted_sheets.Count > 1;
			table.ReloadData ();
		}
		private void anything_changed (Documents doc, Documents.DocumentChangeEventArgs e)
		{
			anything_changed ();
		}
		public override void ViewWillLayoutSubviews ()
		{
			base.ViewWillLayoutSubviews ();
			BottomPanelView.layout (panelButtons, btnHelp);
			update (); // To change warning progress size
		}
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = AppDelegate.app.dark_background_color;

			UIView middle_view = View;
/*			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) {
				RectangleF rr = View.Bounds;
				rr.Width -= 160;
				rr.X += 80;
				rr.Height -= 0;
				middle_view = new UIView (rr);
				middle_view.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
				View.BackgroundColor = AppDelegate.app.table_background_color;
				View.Add (middle_view);
			}*/
			//float margins = 0;
			//if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
			//	margins = 80;

			//table = AppDelegate.create_table_and_background (middle_view, 2, margins, true);
			table = AppDelegate.create_table_and_background (middle_view, 2, true);
			panelButtons = BottomPanelView.create_bottom_panel (View);

			btnHelp = BottomPanelView.create_help_button( panelButtons, "help");
			btnSheet = BottomPanelView.create_bottom_button( panelButtons, "sheet");
			btnContact = BottomPanelView.create_bottom_button( panelButtons, "contact");
			btnRemove = BottomPanelView.create_bottom_button( panelButtons, "trash");

			btnPlan = AppDelegate.create_flat_bottom_button (middle_view, i18n.get ("ExpensePlanButton"), 1);
			btnPerform = AppDelegate.create_flat_bottom_button (middle_view, i18n.get ("ExpensePerformButton"), 1);

			btnHelp.TouchUpInside += (sender, e) => {
				LayoutForHelp lh = new LayoutForHelp(NavigationController, table.Frame.Height);
				// From top
				if( previous_date_selected_automatically ) {
					NameValueCell cell = table.CellAt( NSIndexPath.FromRowSection(0, 0) ) as NameValueCell;
					if( cell != null) {
						lh.create_help_label(LayoutForHelp.LARGE_WIDTH, cell.get_image_view(), 0, i18n.get ("HelpDateRemembered"), LayoutForHelp.BubleType.BUTTON, true);
					}
				}else if (ex.planned) {
					NameValueCell cell = table.CellAt( NSIndexPath.FromRowSection(0, 0) ) as NameValueCell;
					if( cell != null) {
						lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, cell.get_image_view(), 0, i18n.get (Documents.planned_soon (ex.date) ? "HelpDateApproaching" : "PlannedTitle"), LayoutForHelp.BubleType.BUTTON, true);
					}
				}
				if( btnSheet.Enabled )
					lh.create_help_label(LayoutForHelp.LARGE_WIDTH, btnSheet, 0, i18n.get ("HelpSelectSheet"), LayoutForHelp.BubleType.BUTTON, false);
				if( !viewWarning.Hidden )
					lh.create_help_label(LayoutForHelp.LARGE_WIDTH, viewWarning, 0, i18n.get ("HelpFreeProgress"), LayoutForHelp.BubleType.BUTTON, false);
				lh.show();
			};

			btnPlan.TouchUpInside += (sender, e) => {
				if( AppDelegate.app.docs.get_shareware_progress(doc) >= Documents.SHAREWARE_LIMIT ) {
					FlurryAnalytics.Flurry.LogEvent("Limit", NSDictionary.FromObjectsAndKeys(new object[]{"Plan"}, new object[]{"from"}));
					Utility.slide_up(NavigationController, SettingsVC.create_or_reuse(true));
					return;
				}
				DBExpense new_ex = ex.Clone();
				new_ex.date = 0; // introduced in 2.0
				new_ex.planned = true;
				new_ex.id = "";
				FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"PlanStart"}, new object[]{"action"}));
				Utility.slide_up(NavigationController, ExpenseVC.create_or_reuse(doc, new_ex, null));
			};
			btnPerform.TouchUpInside += (sender, e) => {
				if( AppDelegate.app.docs.get_shareware_progress(doc) >= Documents.SHAREWARE_LIMIT ) {
					FlurryAnalytics.Flurry.LogEvent("Limit", NSDictionary.FromObjectsAndKeys(new object[]{"Perform"}, new object[]{"from"}));
					Utility.slide_up(NavigationController, SettingsVC.create_or_reuse(true));
					return;
				}
				DBExpense new_ex = ex.Clone();
				//if( new_ex.recurrence == DBReccurence.NEVER )
				//	new_ex.date = DateTime.Now.Ticks;
				new_ex.planned = false;
				new_ex.id = "";
				new_ex.recurrence = DBReccurence.NEVER;
				FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"PerformStart"}, new object[]{"action"}));
				Utility.slide_up(NavigationController, ExpenseVC.create_or_reuse(doc, new_ex, ex));
			};
			btnContact.TouchUpInside += (sender, e) => {
				if (ex.is_any_loan() && !String.IsNullOrEmpty(ex.name)) {
					NSError error;
					using (ABAddressBook ab = ABAddressBook.Create(out error)) {
						ABPerson best_contact = find_best_contact (ab, ex.name);
						if( best_contact != null ){
							ABPersonViewController picker = new ABPersonViewController ();
							picker.DisplayedPerson = best_contact;
							picker.AddressBook = ab;
							picker.AllowsActions = true;
							picker.AllowsEditing = false;
//							picker.DisplayedProperties.Add(ABPersonProperty.Email);
//							picker.DisplayedProperties.Add(ABPersonProperty.Phone);
							NavigationController.PushViewController (picker, true);
							FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"ViewContact"}, new object[]{"action"}));
						}
					}
				}
				//people_found = true;
				//ex.name = "";
				//expense_changed = true;
				//update_contact();
				//update ();
			};
			btnSheet.TouchUpInside += (sender, e) => {
				start_edit_sheet();
			};
			btnRemove.TouchUpInside += (sender, e) => {
				UIActionSheet ash2 = new UIActionSheet(i18n.get ("DeleteExpenseTitle"), null, i18n.get("Cancel"), i18n.get ("Delete"));
				ash2.Clicked += (sender2, e2) => {
					if( e2.ButtonIndex == ash2.DestructiveButtonIndex )
					{
						FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"Delete"}, new object[]{"action"}));
						AppDelegate.app.docs.execute_change( new ChangeExpenseRemove(doc, ex.id) );
						Utility.dismiss_or_pop(NavigationController, true);
					}
				};
				Utility.show_action_sheet(ash2, View, btnRemove);
				//ash2.ShowInView(View);
			};
			labelWarning.Text = i18n.get ("LimitTitle");
			viewWarning.Frame = btnPlan.Frame;
			table.Source = new TableSource(this);
			update_contact ();
			update ();
		}
		void start_edit_sheet ()
		{
			NavigationController.PushViewController( SheetsSelect.create_or_reuse(doc, i18n.get ("SheetTitle"), ex.sheet, "-1", (svc, sh)=>{
				FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"EditSheet"}, new object[]{"action"}));
				this.expense_changed = true;
				this.ex.sheet = sh;
				//this.show_sheet = true;
				this.update_contact();
				this.update ();
			}), true );
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			AppDelegate.app.docs.anything_changed += anything_changed;
			anything_changed ();
		}
		public override void ViewWillDisappear (bool animated)
		{
			AppDelegate.app.docs.anything_changed -= anything_changed;
			base.ViewWillDisappear (animated);
		}

		public class TableSource : UITableViewSource {
			ExpenseVC parent_vc;
			public TableSource (ExpenseVC parent_vc)
			{
				this.parent_vc = parent_vc;
			}
			public override nint RowsInSection (UITableView tableview, nint section)
			{
				return parent_vc.cells_count;
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				// TODO - different name value cell for ipad/iphone and date
//				string nvc_name = (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) ? @"NameValueCelliPad" : @"NameValueCell";
//				string nvdc_name = (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) ? @"NameValueDateCelliPad" : @"NameValueDateCell";

				if (indexPath.Row == parent_vc.date_index) {
					NameValueCell dcell = (NameValueCell)tableView.DequeueReusableCell (NameValueCell.Key);
					if (dcell == null)
						dcell = NameValueCell.Create(tableView);
					if (parent_vc.previous_date_selected_automatically)
						dcell.setImage (AppDelegate.get_attention_calendar());
//					else if (!String.IsNullOrEmpty(parent_vc.ex.id) && parent_vc.ex.planned && Documents.planned_soon (parent_vc.ex.date))
//						dcell.setImage (AppDelegate.get_attention_icon());
					else if(parent_vc.ex.planned)
						dcell.setImage ( Documents.planned_soon (parent_vc.ex.date) ? AppDelegate.get_attention_icon () : AppDelegate.get_attention_icon_n () );
					else
						dcell.setImage (null);

					if( parent_vc.ex.date == 0)
						dcell.setNameValueComment("", "", i18n.get ("SelectNoDate"));
					else
						dcell.setNameValueComment(AppDelegate.app.date_week_formatter.ToString((NSDate)new DateTime(parent_vc.ex.date, DateTimeKind.Local)), "", "");
					//dcell.setValueColor (AppDelegate.app.neutral_color);
					return dcell;
				}
				NameValueCell cell = (NameValueCell)tableView.DequeueReusableCell (NameValueCell.Key);
				if (cell == null)
					cell = NameValueCell.Create(tableView);
				if (indexPath.Row == parent_vc.amount1_index) {
					cell.setImage (null);
					DBAccount acc1 = parent_vc.doc.get_account (parent_vc.ex.sum.account);
					CurrencyFormat cf1 = CurrencyFormat.get_currency (parent_vc.ex.sum.currency);
					cell.setNameValueComment (acc1.name, cf1.format_amount_precise (parent_vc.ex.sum.amount_1000), "");
					cell.setValueColor (parent_vc.ex.sum.amount_1000 <= 0 ? AppDelegate.app.negative_color : AppDelegate.app.positive_color);
					return cell;
				}
				if (indexPath.Row == parent_vc.amount2_index) {
					cell.setImage (null);
					DBAccount acc2 = parent_vc.doc.get_account (parent_vc.ex.sum2.account);
					CurrencyFormat cf2 = CurrencyFormat.get_currency (parent_vc.ex.sum2.currency);
					cell.setNameValueComment (acc2.name, cf2.format_amount_precise (parent_vc.ex.sum2.amount_1000), "");
					cell.setValueColor (parent_vc.ex.sum2.amount_1000 <= 0 ? AppDelegate.app.negative_color : AppDelegate.app.positive_color);
					return cell;
				}
				if (indexPath.Row == parent_vc.category_index) {
					DBCategory cat = parent_vc.doc.get_category (parent_vc.ex.category);
					cell.setImage (AppDelegate.app.get_category_image(cat));
					cell.setNameValueComment (cat.get_loc_name(), "", "");
					return cell;
				}
				if (indexPath.Row == parent_vc.name_index) {
					cell.setImage (parent_vc.contact_image);
					cell.setNameValueComment (parent_vc.ex.name, "", String.IsNullOrEmpty(parent_vc.ex.name) ? i18n.get ("ExpenseNameComment") : "");
					return cell;
				}
				if (indexPath.Row == parent_vc.recurrence_index) {
					cell.setImage (null);
					cell.setNameValueComment (i18n.get ("RecurrenceTitle"), i18n.get (parent_vc.ex.recurrence.ToString()), "");
					cell.setValueColor (AppDelegate.app.neutral_color);
					return cell;
				}
				if (indexPath.Row == parent_vc.return_date_index) {
					cell.setImage (null);
					if( parent_vc.return_date == 0)
						cell.setNameValueComment ("", "", i18n.get ("ExpenseReturnDate"));
					else
						cell.setNameValueComment (i18n.get ("ExpenseReturnDate"), AppDelegate.app.date_formatter.ToString((NSDate)new DateTime(parent_vc.return_date, DateTimeKind.Local)), "");
					cell.setValueColor (AppDelegate.app.neutral_color);
					return cell;
				}
				if (indexPath.Row == parent_vc.sheet_index) {
					cell.setImage (null);
					DBSheet sh = parent_vc.doc.get_sheet (parent_vc.ex.sheet);
					cell.setNameValueComment (i18n.get ("SheetTitle"), sh.get_loc_name(), "");
					cell.setValueColor (AppDelegate.app.neutral_color);
					return cell;
				}
				return null;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Row == parent_vc.date_index) {
					parent_vc.NavigationController.PushViewController(DateVC.create_or_reuse(i18n.get ("DateTitle"),parent_vc.ex.planned ? i18n.get ("SelectNoDate") : null, false, parent_vc.ex.date, (dvc, da)=>{
						FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"EditDate"}, new object[]{"action"}));
						parent_vc.previous_date_selected_automatically = false;
						parent_vc.expense_changed = true;
						parent_vc.ex.date = da;
						if( da == 0 && parent_vc.ex.recurrence != DBReccurence.NEVER )
							parent_vc.ex.recurrence = DBReccurence.NEVER;
						parent_vc.update_contact();
						parent_vc.update ();
					}),true);
				}
				if (indexPath.Row == parent_vc.return_date_index) {
					parent_vc.NavigationController.PushViewController(DateVC.create_or_reuse(i18n.get ("ExpenseReturnDate"),i18n.get ("SelectNoDate"),false,parent_vc.return_date, (dvc, da)=>{
						FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"EditReturnDate"}, new object[]{"action"}));
						parent_vc.expense_changed = true;
						parent_vc.return_date = da;
						parent_vc.update_contact();
						parent_vc.update ();
					}),true);
				}
				if (indexPath.Row == parent_vc.amount1_index) {
					parent_vc.NavigationController.PushViewController (AmountVC.create_or_reuse(parent_vc.doc, parent_vc.ex.sum, new Sum(),Math.Sign (parent_vc.ex.sum.amount_1000),false, (avc, sum)=>{
						FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"EditAmount1"}, new object[]{"action"}));
						parent_vc.expense_changed = true;
						parent_vc.ex.sum = sum;
						if (parent_vc.ex.category == Document.TRANSFER_CATEGORY) {
							parent_vc.ex.sum2.currency = sum.currency;
							parent_vc.ex.sum2.amount_1000 = -sum.amount_1000;
						}
						parent_vc.update_contact();
						parent_vc.update ();
					}), true);
				}
				if (indexPath.Row == parent_vc.amount2_index) {
					if (parent_vc.ex.category == Document.TRANSFER_CATEGORY) {
						parent_vc.NavigationController.PushViewController (AccountSelect.create_or_reuse( parent_vc.doc, i18n.get ("AccountToTitle"), parent_vc.ex.sum2.account, "-1", false, (avc, acc)=>{
							FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"EditAccountTo"}, new object[]{"action"}));
							parent_vc.expense_changed = true;
							parent_vc.ex.sum2.account = acc;
							parent_vc.update_contact();
							parent_vc.update ();
						}), true);
					}else
						parent_vc.NavigationController.PushViewController (AmountVC.create_or_reuse(parent_vc.doc, parent_vc.ex.sum2,parent_vc.ex.sum,-Math.Sign (parent_vc.ex.sum.amount_1000),false, (avc, sum)=>{
							FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"EditAmount2"}, new object[]{"action"}));
							parent_vc.expense_changed = true;
							parent_vc.ex.sum2 = sum;
							parent_vc.update_contact();
							parent_vc.update ();
						}), true);
				}
				if (indexPath.Row == parent_vc.name_index) {
					parent_vc.NavigationController.PushViewController(NameVC.create_or_reuse(parent_vc.doc, i18n.get ("ExpenseNameTitle"), i18n.get ("ExpenseNamePlaceholder"),parent_vc.ex.is_any_loan(),true,false,parent_vc.ex.name, parent_vc.ex.category, (nvc, str) => {
						FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"EditName"}, new object[]{"action"}));
						parent_vc.expense_changed = true;
						parent_vc.ex.name = str;
						parent_vc.update_contact();
						parent_vc.update ();
					}), true);
				}
				if (indexPath.Row == parent_vc.category_index) {
					parent_vc.NavigationController.PushViewController (CategoryVC.create_or_reuse(parent_vc.doc, Math.Sign(parent_vc.ex.sum.amount_1000), parent_vc.ex.category, false, (cvc, cat)=>{
						FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"EditCategory"}, new object[]{"action"}));
						parent_vc.expense_changed = true;
						parent_vc.ex.category = cat;
						parent_vc.update_contact();
						parent_vc.update ();
					}), true);
				}
				if (indexPath.Row == parent_vc.recurrence_index) {
					parent_vc.NavigationController.PushViewController (RecurrenceSelect.create_or_reuse(parent_vc.ex.recurrence, (rvc, rec)=>{
						FlurryAnalytics.Flurry.LogEvent("Expense", NSDictionary.FromObjectsAndKeys(new object[]{"EditRecurrence"}, new object[]{"action"}));
						parent_vc.expense_changed = true;
						parent_vc.ex.recurrence = rec;
						if( rec != DBReccurence.NEVER && parent_vc.ex.date == 0 )
							parent_vc.ex.date = DateTime.Now.Ticks;
						parent_vc.update_contact();
						parent_vc.update ();
					}), true);
				}
				if (indexPath.Row == parent_vc.sheet_index) {
					parent_vc.start_edit_sheet ();
				}
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
		}
	}
}

