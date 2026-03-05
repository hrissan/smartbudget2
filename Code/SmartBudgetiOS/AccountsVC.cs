using System;
using System.Drawing;
using Foundation;
using UIKit;
using SmartBudgetCommon;
using System.Collections.Generic;

namespace SmartBudgetiOS
{
	public class AccountsVC : UIViewController
	{
		private List<DBAccountBalance> total_balance;
		private List<List<DBAccountBalance>> account_balances;
		private bool short_balance;
		private bool show_total;
		private int hide_hidden_accounts;
		//private TimerForButton currency_hold_timer;
		private UITableView table;
		private UIView panelButtons;
		private UIButton btnArrange;
		private UIButton btnCurrency;
		private UIButton btnHelp;
		private UIButton btnPlus;
		private UIButton btnShortBalance;
		private TimerForButton currency_hold_timer;
		private Document doc;
		public AccountsVC ()// : base ("AccountsVC", null)
		{
			NavigationItem.Title = i18n.get("AccountsTitle");
			//NavigationItem.RightBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Add, (sender, e) => {
			//	add_account();
			//});
			if (UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Pad) {
				NavigationItem.LeftBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Done, (sender, e) => {
					Utility.dismiss_or_pop (this.NavigationController, true);
					//this.NavigationItem.LeftBarButtonItem = null;
					//this.table.RemoveFromSuperview();
					//this.table.Source = null;
					//this.table = null;
				});
			}
		}
		void construct()
		{
			this.doc = AppDelegate.app.docs.selected_doc;
			hide_hidden_accounts = 1;
		}
		~AccountsVC()
		{
			Console.WriteLine ("~AccountsVC");
		}		
		private static Utility.ReuseVC<AccountsVC> reuse = new Utility.ReuseVC<AccountsVC> ();
		public static AccountsVC create_or_reuse()
		{
			AccountsVC result = reuse.create_or_reuse();
			result.construct();
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
		private void add_account()
		{
			Utility.push_or_present(NavigationController, NameVC.create_or_reuse(doc, i18n.get ("AccountTitleNew"),i18n.get ("AccountNamePlaceholder"),false,false,false,"", "", (nvc,str)=>{
				FlurryAnalytics.Flurry.LogEvent("Account", NSDictionary.FromObjectsAndKeys(new object[]{"New"}, new object[]{"action"}));
				AppDelegate.app.docs.execute_change( new ChangeAccountCreate(doc, str));
			}), true);
		}
		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			
			if (!IsViewLoaded) {
				table = Utility.free_view (table);
			}
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = AppDelegate.app.dark_background_color;

			table = AppDelegate.create_table_and_background (View, 1);
			panelButtons = BottomPanelView.create_bottom_panel (View);

			btnHelp = BottomPanelView.create_help_button( panelButtons, "help");
			btnArrange = BottomPanelView.create_bottom_button( panelButtons, "arrange");
			btnPlus = BottomPanelView.create_bottom_button( panelButtons, "plus");
			btnShortBalance = BottomPanelView.create_bottom_button( panelButtons, "more");
			btnCurrency = BottomPanelView.create_bottom_button( panelButtons, "currency");

			btnHelp.TouchUpInside += (sender, e) => {
				LayoutForHelp lh = new LayoutForHelp(NavigationController, table.Frame.Height);
				// From top
				AccountCell help_cell = null;
				foreach (UITableViewCell c in table.VisibleCells) {
					AccountCell ac = c as AccountCell;
					if (ac != null) {
						if( help_cell == null || ac.Frame.Y < help_cell.Frame.Y )
							help_cell = ac;
					}
				}
				if( help_cell != null)
					lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, help_cell, 0, i18n.get ("HelpTapHold"), LayoutForHelp.BubleType.HOLD, true);
				// From bottom
				if( hide_hidden_accounts == 1 )
					lh.create_help_label(LayoutForHelp.LARGE_WIDTH, table, 138, i18n.get ("HelpAccounts"), LayoutForHelp.BubleType.NO_TAILS, false);
				else
					lh.create_help_label(LayoutForHelp.LARGE_WIDTH, table, 138, i18n.get ("HelpHiddenAccounts"), LayoutForHelp.BubleType.NO_TAILS, false);
				lh.show ();
			};
			btnArrange.TouchUpInside += (sender, e) => {
				SetEditing(!table.Editing,true);
			};
			btnPlus.TouchUpInside += (sender, e) => {
				add_account();
			};
			currency_hold_timer = new TimerForButton (btnCurrency, delegate {
				AppDelegate.app.docs.report_currency = AppDelegate.app.docs.next_selected_currency(AppDelegate.app.docs.report_currency, "");
				AppDelegate.app.docs.save_settings ();
				AppDelegate.app.docs.send_anything_changed(null);
			}, delegate {
				//btnCurrency.CancelTracking (null);
				Utility.push_or_present(NavigationController, CurrencySelect.create_or_reuse(AppDelegate.app.docs.report_currency, (cs,cur)=>{
					AppDelegate.app.docs.report_currency = cur;
					AppDelegate.app.docs.save_settings ();
					AppDelegate.app.docs.send_anything_changed(null);
				}), true);
			});
			/*btnCurrency.TouchUpInside += (sender, e) => {
				if( short_balance )
					short_balance = false;
				else
					AppDelegate.app.docs.report_currency = AppDelegate.app.docs.next_selected_currency(AppDelegate.app.docs.report_currency, "");
				AppDelegate.app.save_settings();
				AppDelegate.app.docs.send_anything_changed(null);
			};*/
			btnShortBalance.TouchUpInside += (sender, e) => {
				AppDelegate.app.docs.short_balance = !short_balance;
				AppDelegate.app.docs.save_settings ();
				AppDelegate.app.docs.send_anything_changed(null);
			};
/*			currency_hold_timer = new TimerForButton (btnCurrency, delegate {
				if( short_balance )
					short_balance = false;
				else
					AppDelegate.app.docs.report_currency = AppDelegate.app.docs.next_selected_currency(AppDelegate.app.docs.report_currency, "");
				AppDelegate.app.save_settings();
				AppDelegate.app.docs.send_anything_changed(null);
			}, delegate {
				short_balance = !short_balance;
				AppDelegate.app.save_settings();
				AppDelegate.app.docs.send_anything_changed(null);
			});*/
			anything_change (null);

			short_balance = AppDelegate.app.docs.short_balance;
			table.Source = new TableSource (this);
		}
		public void select_doc(Document doc)
		{
			if (this.doc == doc)
				return;
			this.doc = doc;
			anything_change (null);
		}
		private void anything_change(Object originator)
		{
			short_balance = AppDelegate.app.docs.short_balance;
			if( originator != this)
			{
				//this.doc = AppDelegate.app.docs.selected_doc;
				show_total = calc_show_total();
				update_balances();
				table.ReloadData();
			}
		}
		private void anything_change (Documents doc, Documents.DocumentChangeEventArgs e)
		{
			anything_change (e.originator);
		}
		private bool calc_show_total()
		{
			if (doc == null)
				return false;
			if (hide_hidden_accounts == 0)
				return doc.sorted_accounts.Count > 1;
			return doc.hidden_account_line > 1;
		}
		public void update_balances()
		{
			btnHelp.Enabled = (doc != null);
			btnArrange.Enabled = (doc != null) && (table.Editing || doc.sorted_accounts.Count > 1);
			btnPlus.Enabled = (doc != null);
			btnShortBalance.Enabled = (doc != null);
			btnCurrency.Enabled = (doc != null) && !short_balance;
			total_balance = new List<DBAccountBalance> ();
			account_balances = new List<List<DBAccountBalance>> ();

			if (doc == null)
				return;
			for (int i = 0; i < doc.sorted_accounts.Count; ++i) {
				var a = doc.sorted_accounts [i];
				List<DBAccountBalance> ab = doc.get_balance (a.id);
				if( hide_hidden_accounts == 0 || i < doc.hidden_account_line)
					DBAccountBalance.merge_balance(total_balance, ab, true);
				account_balances.Add (ab);
			}
		}
		public void edit_account_options(DBAccount acc, UIView origin)
		{
			if (acc == null) // TODO - remove
				return;
			int expenses_count = 0;
			long recent_expense_date = 0;
			if( acc != null )
				doc.get_account_count_date (acc.id, out expenses_count, out recent_expense_date);
			List<string> other = new List<string>();
			//other.Add (i18n.get ("CopyBalance"));
			if (acc != null) {
				other.Add (i18n.get ("EditName"));
				if (expenses_count == 0 && doc.sorted_accounts.Count > 1)
					other.Add (i18n.get ("Delete"));
				if (expenses_count != 0 && doc.sorted_accounts.Count > 1) {
					other.Add (i18n.get ("MoveAllOperations"));
				}
			}
			UIActionSheet ash = new UIActionSheet(acc == null ? "" : acc.name, null, i18n.get("Cancel"), null, other.ToArray());
			ash.Clicked += (sender, e) => {
				if( ash.ButtonTitle(e.ButtonIndex) == i18n.get ("CopyBalance") ){
					List<DBAccountBalance> ab = acc == null ? total_balance : doc.get_balance (acc.id);
					UIPasteboard.General.String = AccountCell.copy_account_balance(short_balance, ab, AppDelegate.app.docs.report_currency);
				}
				if( ash.ButtonTitle(e.ButtonIndex) == i18n.get ("Delete") ){
					FlurryAnalytics.Flurry.LogEvent("Account", NSDictionary.FromObjectsAndKeys(new object[]{"Delete"}, new object[]{"action"}));
					AppDelegate.app.docs.execute_change (new ChangeAccountRemove(doc, acc.id), null);
				}
				if( ash.ButtonTitle(e.ButtonIndex) == i18n.get ("MoveAllOperations") ){
					Utility.push_or_present(NavigationController, AccountSelect.create_or_reuse(doc, i18n.get ("MoveToAccountTitle"), "", acc.id, false, (svc, new_acc_id)=>{
						DBAccount new_acc = doc.get_account(new_acc_id);
						string move_tit = i18n.get ("AccountMoveMenuTitle").Replace("{from}", acc.name).Replace("{to}", new_acc.name);
						UIActionSheet ash2 = new UIActionSheet(move_tit, null, i18n.get("Cancel"), i18n.get ("MoveAllOperations"));
						ash2.Clicked += (sender2, e2) => {
							if( e2.ButtonIndex == ash2.DestructiveButtonIndex ){
								FlurryAnalytics.Flurry.LogEvent("Account", NSDictionary.FromObjectsAndKeys(new object[]{"MoveAllOperations"}, new object[]{"action"}));
								AppDelegate.app.docs.execute_move_from_account(doc, acc.id, new_acc_id);
//								AppDelegate.app.docs.execute_change (new ChangeAccountRemove(doc, acc.id), null);
							}
						};
						Utility.show_action_sheet(ash2, NavigationController.View);
					}), true);
				}
				if( ash.ButtonTitle(e.ButtonIndex) == i18n.get ("EditName") ){
					Utility.push_or_present(NavigationController, NameVC.create_or_reuse(doc, i18n.get ("AccountTitle"),i18n.get ("AccountNamePlaceholder"),false,false,false,acc.name, "", (nvc,str)=>{
						FlurryAnalytics.Flurry.LogEvent("Account", NSDictionary.FromObjectsAndKeys(new object[]{"EditName"}, new object[]{"action"}));
						acc.name = str;
						AppDelegate.app.docs.execute_change( new ChangeAccountUpdate(doc, acc));
					}), true);
				}
			};
			Utility.show_action_sheet (ash, View, origin);
			//ash.ShowInView(View);
		}
		public override void SetEditing (bool editing, bool animated)
		{
			if (editing == table.Editing)
				return;
			bool was_show_total = show_total;
			int was_hide_hidden_account = hide_hidden_accounts;
			//Console.WriteLine ("Before BeginUpdates was_show_total = {0} show_total={1}", was_show_total, show_total);
			table.BeginUpdates ();
			//Console.WriteLine ("After BeginUpdates");
			hide_hidden_accounts = editing ? 0 : 1;
			update_balances ();
			show_total = calc_show_total();
			if( was_show_total && show_total )
				table.ReloadRows(new NSIndexPath[] { NSIndexPath.FromRowSection(0, TableSource.TOTAL - was_hide_hidden_account) }, UITableViewRowAnimation.Fade);
			if( was_show_total && !show_total )
				table.DeleteRows(new NSIndexPath[] { NSIndexPath.FromRowSection(0, TableSource.TOTAL - was_hide_hidden_account) }, UITableViewRowAnimation.Fade);
			if( hide_hidden_accounts == 0 ) {
				table.InsertSections (NSIndexSet.FromIndex(1), UITableViewRowAnimation.Fade);
				if( !was_show_total && show_total )
					table.InsertRows(new NSIndexPath[] { NSIndexPath.FromRowSection(0, TableSource.TOTAL - hide_hidden_accounts) }, UITableViewRowAnimation.Fade);
			} else {
				table.DeleteSections(NSIndexSet.FromIndex(1), UITableViewRowAnimation.Fade);
			}
			//Console.WriteLine ("Before EndUpdates");
			table.EndUpdates ();
			//Console.WriteLine ("After EndUpdates");
			table.SetEditing (editing, animated);
			AppDelegate.decorate_bottom_button (btnArrange, "arrange", editing);
//			btnArrange.SetBackgroundImage (editing ? UIImage.FromBundle("b4_backgound_high.png") : null, UIControlState.Normal);
		}
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			AppDelegate.app.docs.anything_changed += anything_change;
			anything_change (null);
		}
		public override void ViewWillDisappear(bool animated)
		{
			AppDelegate.app.docs.anything_changed -= anything_change;
			SetEditing (false, true);
			base.ViewWillDisappear (animated);
		}
		public override void ViewWillLayoutSubviews ()
		{
			base.ViewWillLayoutSubviews ();
			BottomPanelView.layout (panelButtons, btnHelp);
		}
		public class TableSource : UITableViewSource {
			AccountsVC parent_vc;
			AccountCell measure_cell;
			public const int NORMAL = 0;
			public const int HIDDEN = 1;
			public const int TOTAL = 2;

			public TableSource (AccountsVC parent_vc)
			{
				this.parent_vc = parent_vc;
				measure_cell = AccountCell.Create(parent_vc.table);
			}
			public override nint NumberOfSections (UITableView tableView)
			{
				return 3 - parent_vc.hide_hidden_accounts;
			}
			public override UIView GetViewForHeader (UITableView tableView, nint section)
			{
				if (section == TOTAL - parent_vc.hide_hidden_accounts)
					return null;
				if (section == HIDDEN) {
					UILabel label;
					UITableViewHeaderFooterView hfv = SectionHeader2.deque_header(tableView, out label);
					//SectionHeader sh = SectionHeader.create_or_get_header ();
					label.Text = i18n.get ("HiddenAccountsLine"); // TODO string
					return hfv;
				}
				return null;
			}
			public override nfloat GetHeightForHeader (UITableView tableView, nint section)
			{
				if (section == TOTAL - parent_vc.hide_hidden_accounts)
					return 0f;
				if (section == HIDDEN)
					return SectionHeader2.default_height;
				return 0f;
			}
			private int MyRowsInSection (UITableView tableview, nint section)
			{
				if (parent_vc.doc == null)
					return 0;
				if (section == TOTAL - parent_vc.hide_hidden_accounts)
					return parent_vc.show_total ? 1 : 0;
				if (section == HIDDEN)
					return parent_vc.doc.sorted_accounts.Count - parent_vc.doc.hidden_account_line;
				return parent_vc.doc.hidden_account_line;
			}
			public override nint RowsInSection (UITableView tableview, nint section)
			{
				int mr = MyRowsInSection (tableview, section);
				//Console.WriteLine ("RowsInSection {0} Rows={1}", section, mr);
				return mr;
			}
			public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == TOTAL - parent_vc.hide_hidden_accounts)
					return measure_cell.height_for_balance(parent_vc.short_balance, parent_vc.total_balance.Count);
				int row = indexPath.Row;
				if (indexPath.Section == HIDDEN)
					row += parent_vc.doc.hidden_account_line;
				return measure_cell.height_for_balance(parent_vc.short_balance, parent_vc.account_balances[row].Count);
			}
			void edit_account_options(UITableViewCell cc)
			{
				NSIndexPath ii = parent_vc.table.IndexPathForCell(cc);
				if (ii == null)
					return;
				if (ii.Section == TOTAL - parent_vc.hide_hidden_accounts) {
					parent_vc.edit_account_options(null, cc);
					return;
				}
				int rrow = ii.Row;
				if (ii.Section == HIDDEN)
					rrow += parent_vc.doc.hidden_account_line;
				DBAccount acc = parent_vc.doc.sorted_accounts [rrow];
				parent_vc.edit_account_options(acc, cc);
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				//Console.WriteLine ("Asked {0}:{1}", indexPath.Section, indexPath.Row);
				/*if (indexPath.Section == TOTAL - parent_vc.hide_hidden_accounts) {
					AccountCell acell = (AccountCell)tableView.DequeueReusableCell (AccountCell.Key);
					if (acell == null)
						acell = AccountCell.Create ();
					acell.set_account_balance (i18n.get ("AccountsSum"), parent_vc.short_balance, parent_vc.total_balance, AppDelegate.app.docs.report_currency);
					return acell;
				}*/
				AccountCell cell = (AccountCell)tableView.DequeueReusableCell (AccountCell.Key);
				if (cell == null) {
					WeakReference weak_this = new WeakReference (this);
					cell = AccountCell.Create( tableView, (cc)=>{
						TableSource strong_this = weak_this.Target as TableSource;
						if( strong_this == null )
							return;
						strong_this.edit_account_options(cc);
					});
				}
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
				if (indexPath.Section == TOTAL - parent_vc.hide_hidden_accounts) {
					cell.set_account_balance (i18n.get ("AccountsSum"), parent_vc.short_balance, parent_vc.total_balance, AppDelegate.app.docs.report_currency);
					return cell;
				}
				int row = indexPath.Row;
				if (indexPath.Section == HIDDEN)
					row += parent_vc.doc.hidden_account_line;
				//Console.WriteLine ("Asked {0}:{1}, given {2}", indexPath.Section, indexPath.Row, row);
				cell.set_account_balance (parent_vc.doc.sorted_accounts[row].name, parent_vc.short_balance, parent_vc.account_balances [row], AppDelegate.app.docs.report_currency);
				//cell.SelectionStyle = UITableViewCellSelectionStyle.Gray;
				return cell;
			}
/*			public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == 1 || indexPath.Section == 2)
					return false;
				return tableView.Editing;
			}*/
			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				if( editingStyle != UITableViewCellEditingStyle.Delete )
					return;
				int row = indexPath.Row;
				if (indexPath.Section == HIDDEN)
					row += parent_vc.doc.hidden_account_line;
				Console.WriteLine ("CommitEditingStyle {0}:{1}, given {2}", indexPath.Section, indexPath.Row, row);
				DBAccount acc = parent_vc.doc.sorted_accounts [row];
				bool was_show_total = parent_vc.show_total;
				tableView.BeginUpdates ();
				FlurryAnalytics.Flurry.LogEvent("Account", NSDictionary.FromObjectsAndKeys(new object[]{"Delete"}, new object[]{"action"}));
				AppDelegate.app.docs.execute_change (new ChangeAccountRemove(parent_vc.doc, acc.id), parent_vc);
				parent_vc.show_total = parent_vc.calc_show_total ();
				tableView.DeleteRows (new NSIndexPath[]{indexPath}, UITableViewRowAnimation.Left);
				if( was_show_total && !parent_vc.show_total )
					tableView.DeleteRows(new NSIndexPath[]{NSIndexPath.FromRowSection(0, TOTAL - parent_vc.hide_hidden_accounts)}, UITableViewRowAnimation.Fade);
				else if( parent_vc.show_total )
					tableView.ReloadRows(new NSIndexPath[]{NSIndexPath.FromRowSection(0, TOTAL - parent_vc.hide_hidden_accounts)}, UITableViewRowAnimation.Fade);
				//if( doc.sorted_accounts.Count == 1 )
				//	tableView.ReloadRows(new NSIndexPath[]{NSIndexPath.FromRowSection(0, NORMAL)}, UITableViewRowAnimation.Fade);
				tableView.EndUpdates ();
				updateVisibleCellCheckmarks (tableView);
			}
			public override bool CanMoveRow(UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == TOTAL - parent_vc.hide_hidden_accounts)
					return false;
				if (indexPath.Section == HIDDEN)
					return true;
				return true;
			}
			public override void MoveRow (UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
			{
				if (parent_vc.hide_hidden_accounts == 1) // should never be
					return;
				if (sourceIndexPath.Section == destinationIndexPath.Section &&
					sourceIndexPath.Row == destinationIndexPath.Row)
					return; // Obvious optimization saves lots of commands
				int from_row = sourceIndexPath.Row;
				if (sourceIndexPath.Section == HIDDEN)
					from_row += parent_vc.doc.hidden_account_line;
				int to_row = destinationIndexPath.Row;
				if (destinationIndexPath.Section == HIDDEN)
					to_row += parent_vc.doc.hidden_account_line;
				int mhal = sourceIndexPath.Section - destinationIndexPath.Section;
				if (mhal == -1) // Move is delete then insert. After delete, hidden_account_line seems to move
					to_row -= 1;
				Console.WriteLine ("MoveRow From {0}:{1}, given {2} To {3}:{4}, given {5} mhal={6}", sourceIndexPath.Section, sourceIndexPath.Row, from_row, destinationIndexPath.Section, destinationIndexPath.Row, to_row, mhal);
				AppDelegate.app.docs.arrange_account (parent_vc.doc, from_row, to_row, mhal, parent_vc);
//				AppDelegate.app.docs.execute_change (new ChangeAccountArrange(parent_vc.doc, from_row, to_row, mhal), parent_vc);
				parent_vc.update_balances ();
//				tableView.BeginUpdates ();
//				if( parent_vc.show_total )
//					tableView.ReloadRows(new NSIndexPath[]{NSIndexPath.FromRowSection(0, TOTAL - parent_vc.hide_hidden_accounts)}, UITableViewRowAnimation.Fade);
//				tableView.EndUpdates ();
			}
			public override NSIndexPath CustomizeMoveTarget (UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath proposedIndexPath)
			{
				//if (parent_vc.hide_hidden_accounts == 1) // should never be
				//	return proposedIndexPath;
				if (sourceIndexPath.Section == NORMAL) {
					if (parent_vc.doc.hidden_account_line == 1)
						return sourceIndexPath;// Do not hide last one
					if (proposedIndexPath.Section == NORMAL)
						return proposedIndexPath;
					if (proposedIndexPath.Section == HIDDEN)
						return proposedIndexPath;
					return NSIndexPath.FromRowSection (parent_vc.doc.sorted_accounts.Count - parent_vc.doc.hidden_account_line, HIDDEN);
				} // HIDDEN
				if (proposedIndexPath.Section == NORMAL)
					return proposedIndexPath;
				if (proposedIndexPath.Section == HIDDEN)
					return proposedIndexPath;
				return NSIndexPath.FromRowSection (parent_vc.doc.sorted_accounts.Count - parent_vc.doc.hidden_account_line - 1, HIDDEN);
			}
			public override bool ShouldIndentWhileEditing (UITableView tableView, NSIndexPath indexPath)
			{
				return false;
			}
			public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == TOTAL - parent_vc.hide_hidden_accounts)
					return UITableViewCellEditingStyle.None;
				int row = indexPath.Row;
				if (indexPath.Section == HIDDEN)
					row += parent_vc.doc.hidden_account_line;
				if(row == 0 && parent_vc.doc.hidden_account_line == 1)
					return UITableViewCellEditingStyle.None;
				DBAccount acc = parent_vc.doc.sorted_accounts [row];
				int expenses_count;
				long recent_expense_date;
				parent_vc.doc.get_account_count_date (acc.id, out expenses_count, out recent_expense_date);
				return expenses_count == 0 ? UITableViewCellEditingStyle.Delete : UITableViewCellEditingStyle.None;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				/*if (indexPath.Section == NORMAL) {
					if (tableView.Editing) {
						DBAccount acc = parent_vc.doc.sorted_accounts [indexPath.Row];
						parent_vc.edit_account_options(acc);
					} else {
						// TODO - show stats with current acc
					}
				}*/
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
			public void updateVisibleCellCheckmarks(UITableView tableView)
			{
			}
		}	
	}
}

