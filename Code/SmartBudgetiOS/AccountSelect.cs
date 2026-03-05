using System;
using System.Drawing;
using Foundation;
using UIKit;
using System.Collections.Generic;
using SmartBudgetCommon;
using System.Globalization;

namespace SmartBudgetiOS
{
	public class AccountSelect : UIViewController
	{
		private Action<AccountSelect, string> save_action;
		private Action<AccountSelect, List<string>> save_action_many;
		private List<string> accounts;
		private string disabled_account;
		private bool in_chain;
		private UITableView table;
		private UIView panelCheck;
		private UIView panelPlus;
		private UIButton btnPlus;
		private UIButton btnCheck;
		private UIBarButtonItem button_save;
		private UIBarButtonItem cancel_button;
		private Document doc;
		private bool show_all_accounts;
		private bool multiselect;
		public AccountSelect ()// : base ("AccountSelect", null)
		{
			cancel_button = new UIBarButtonItem (UIBarButtonSystemItem.Cancel, (sender, e) => {
				Utility.dismiss_or_pop (NavigationController, true);
			});
			button_save = new UIBarButtonItem (UIBarButtonSystemItem.Save, (sender, e) => {
				if( accounts != null && accounts.Count == doc.sorted_accounts.Count ) // All -> simple all
					accounts = null;
				this.save_action_many.Invoke(this, this.accounts);
				Utility.dismiss_or_pop (NavigationController, true);
			});
		}
		void construct(Document doc, string title, string initial_account, string disabled_account, bool in_chain, Action<AccountSelect, string> save_action)
		{
			this.doc = doc;
			this.accounts = new List<string> ();
			this.accounts.Add(initial_account);
			this.disabled_account = disabled_account;
			this.in_chain = in_chain;
			this.save_action = save_action;
			this.save_action_many = null;
			this.show_all_accounts = false;
			NavigationItem.Title = title;
			NavigationItem.LeftBarButtonItem = in_chain ? null : cancel_button;
			NavigationItem.RightBarButtonItem = null;
		}
		void construct(Document doc, string title, List<string> initial_accounts, Action<AccountSelect, List<string>> save_action_many)
		{
			this.doc = doc;
			this.accounts = initial_accounts;
			this.disabled_account = "-1";
			this.in_chain = false;
			this.save_action = null;
			this.save_action_many = save_action_many;
			this.show_all_accounts = true;
			this.multiselect = initial_accounts != null && initial_accounts.Count > 1;
			NavigationItem.Title = title;
			NavigationItem.LeftBarButtonItem = in_chain ? null : cancel_button;
			NavigationItem.RightBarButtonItem = null;
		}
		~AccountSelect()
		{
			Console.WriteLine ("~AccountSelect");
		}
		private static Utility.ReuseVC<AccountSelect> reuse = new Utility.ReuseVC<AccountSelect> ();
		public static AccountSelect create_or_reuse(Document doc, string title, string initial_account, string disabled_account, bool in_chain, Action<AccountSelect, string> save_action)
		{
			AccountSelect result = reuse.create_or_reuse();
			result.construct(doc, title, initial_account, disabled_account, in_chain, save_action);
			return result;
		}
		public static AccountSelect create_or_reuse(Document doc, string title, List<string> initial_accounts, Action<AccountSelect, List<string>> save_action_many)
		{
			AccountSelect result = reuse.create_or_reuse();
			result.construct(doc, title, initial_accounts, save_action_many);
			return result;
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
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

			panelCheck = BottomPanelView.create_bottom_panel (View);
			panelPlus = BottomPanelView.create_bottom_panel (View);

			btnCheck = BottomPanelView.create_bottom_button(panelCheck, "check");
			btnPlus = BottomPanelView.create_bottom_button(panelPlus, "plus");

			btnCheck.TouchUpInside += (sender, e) => {
				multiselect = !multiselect;
				AppDelegate.decorate_bottom_button(btnCheck, "check", multiselect);
				NavigationItem.RightBarButtonItem = multiselect ? button_save : null;
				table.ReloadData();
			};

			btnPlus.TouchUpInside += (sender, e) => {
				NavigationController.PushViewController(NameVC.create_or_reuse(doc, i18n.get ("AccountTitleNew"),i18n.get ("AccountNamePlaceholder"),false,false,false,"", "", (nvc,str)=>{
					FlurryAnalytics.Flurry.LogEvent("Account", NSDictionary.FromObjectsAndKeys(new object[]{"NewFromSelect"}, new object[]{"action"}));
					AppDelegate.app.docs.execute_change( new ChangeAccountCreate(doc, str));
				}), true);
			};

			table.Source = new TableSource (this);

			NavigationItem.RightBarButtonItem = multiselect ? button_save : null;
		}
		private void anything_changed()
		{
			panelCheck.Hidden = save_action_many == null;
			panelPlus.Hidden = !panelCheck.Hidden;
			table.ReloadData();
			if( !panelCheck.Hidden )
				AppDelegate.decorate_bottom_button(btnCheck, "check", multiselect);
		}
		private void anything_changed (Documents doc, Documents.DocumentChangeEventArgs e)
		{
			anything_changed ();
		}
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear (animated);
			AppDelegate.app.docs.anything_changed += anything_changed;
			anything_changed ();
			if( accounts != null && accounts.Count > 0 ) {
				int ind = doc.sorted_accounts.FindIndex ((a)=>a.id == accounts[0] );
				if (ind != -1)
					table.ScrollToRow (NSIndexPath.FromRowSection(ind, 1), UITableViewScrollPosition.Middle, false);
			}
		}
		public override void ViewWillDisappear (bool animated)
		{
			AppDelegate.app.docs.anything_changed -= anything_changed;
			base.ViewWillDisappear (animated);
		}
		public override void ViewWillLayoutSubviews ()
		{
			base.ViewWillLayoutSubviews ();
			BottomPanelView.layout (panelCheck, null);
			BottomPanelView.layout (panelPlus, null);
		}
		public class TableSource : UITableViewSource {
			AccountSelect parent_vc;
			public TableSource (AccountSelect parent_vc)
			{
				this.parent_vc = parent_vc;
			}
			public override nint NumberOfSections (UITableView tableView)
			{
				return 2;
			}
			public override nint RowsInSection (UITableView tableview, nint section)
			{
				if (section == 0)
					return parent_vc.show_all_accounts ? 1 : 0;
				return parent_vc.doc.sorted_accounts.Count;
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				SimpleCell cell = (SimpleCell)tableView.DequeueReusableCell (SimpleCell.Key);
				if (cell == null)
					cell = SimpleCell.Create(tableView);
				if (indexPath.Section == 0) {
					cell.configAccessory (true, !parent_vc.multiselect && parent_vc.accounts == null);
					cell.setImageName(null, i18n.get ("TotalAccount"), "");
				}else{
					DBAccount acc = parent_vc.doc.sorted_accounts [indexPath.Row];
					int expense_count;
					long date;
					parent_vc.doc.get_account_count_date(acc.id, out expense_count, out date);
					cell.configAccessory (true, (parent_vc.multiselect && parent_vc.accounts == null) || (parent_vc.accounts != null && parent_vc.accounts.IndexOf(acc.id) != -1));
					string cou = i18n.get ("OperationsNumber").Replace("{count}", expense_count.ToString ()); // Culture ok, 
					cell.setImageName(null, acc.name, cou, acc.id != parent_vc.disabled_account);
				}
				return cell;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == 0) {
					if (parent_vc.multiselect) {
						if (parent_vc.accounts == null || parent_vc.accounts.Count == parent_vc.doc.sorted_accounts.Count)
							parent_vc.accounts = new List<string> ();
						else
							parent_vc.accounts = null;
						updateVisibleCellCheckmarks (tableView);
					} else {
						parent_vc.accounts = null;
						updateVisibleCellCheckmarks (tableView);
						parent_vc.save_action_many.Invoke (parent_vc, parent_vc.accounts);
						if( !parent_vc.in_chain )
							Utility.dismiss_or_pop (parent_vc.NavigationController, true);
					}
				}
				if (indexPath.Section == 1) {
					DBAccount sh = parent_vc.doc.sorted_accounts [indexPath.Row];
					if(sh.id != parent_vc.disabled_account) {
						if (parent_vc.save_action != null) {
							parent_vc.accounts [0] = sh.id;
							updateVisibleCellCheckmarks (tableView);
							parent_vc.save_action.Invoke (parent_vc, parent_vc.accounts[0]);
							if( !parent_vc.in_chain )
								Utility.dismiss_or_pop (parent_vc.NavigationController, true);
						}else{
							if (parent_vc.multiselect) {
								if (parent_vc.accounts == null) {
									parent_vc.accounts = new List<string> ();
									foreach(DBAccount ash in parent_vc.doc.sorted_accounts){
										parent_vc.accounts.Add (ash.id);
									}
									parent_vc.accounts.Remove (sh.id);
								}else{
									if (parent_vc.accounts.IndexOf (sh.id) == -1)
										parent_vc.accounts.Add (sh.id);
									else
										parent_vc.accounts.Remove (sh.id);
								}
								updateVisibleCellCheckmarks (tableView);
							} else {
								parent_vc.accounts = new List<string> ();
								parent_vc.accounts.Add(sh.id);
								updateVisibleCellCheckmarks (tableView);
								parent_vc.save_action_many.Invoke (parent_vc, parent_vc.accounts);
								if( !parent_vc.in_chain )
									Utility.dismiss_or_pop (parent_vc.NavigationController, true);
							}
						}
					}
				}
/*				if (indexPath.Section == 0) {
					DBAccount acc = parent_vc.doc.sorted_accounts [indexPath.Row];
					if(acc.id != parent_vc.disabled_account) {
						parent_vc.account = acc.id;
						parent_vc.save_action.Invoke (parent_vc, parent_vc.account);
						updateVisibleCellCheckmarks (tableView);
						if( !parent_vc.in_chain )
							Utility.dismiss_or_pop (parent_vc.NavigationController, true);
					}
				}*/
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
			public void updateVisibleCellCheckmarks(UITableView tableView)
			{
				foreach (NSIndexPath c in tableView.IndexPathsForVisibleRows) {
					SimpleCell cell = tableView.CellAt (c) as SimpleCell;
					if (cell == null)
						continue;
					if (c.Section == 0){
						DBAccount sh = parent_vc.doc.sorted_accounts [c.Row];
						cell.configAccessory (true, !parent_vc.multiselect && parent_vc.accounts == null);
					}
					if (c.Section == 1){
						DBAccount sh = parent_vc.doc.sorted_accounts [c.Row];
						cell.configAccessory (true, (parent_vc.multiselect && parent_vc.accounts == null) || (parent_vc.accounts != null && parent_vc.accounts.IndexOf(sh.id) != -1));
					}
				}
			}
		}	
	}
}

