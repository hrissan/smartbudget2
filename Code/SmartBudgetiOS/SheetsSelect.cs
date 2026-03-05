using System;
using System.Drawing;
using Foundation;
using UIKit;
using System.Collections.Generic;
using SmartBudgetCommon;
using System.Globalization;

namespace SmartBudgetiOS
{
	public class SheetsSelect : UIViewController
	{
		private Action<SheetsSelect, string> save_action;
		private Action<SheetsSelect, List<string>, bool> save_action_many;
		private List<string> sheets;
		private string disabled_sheet;
		private UITableView table;
		private UIView panelCheck;
		private UIView panelPlus;
		private UIButton btnPlus;
		private UIButton btnCheck;
		private UIBarButtonItem button_save;
		private UIBarButtonItem cancel_button;
		private Document doc;
		private bool show_all_sheets;
		private bool multiselect;

		public SheetsSelect ()// : base ("SheetsSelect", null)
		{
			cancel_button = new UIBarButtonItem (UIBarButtonSystemItem.Cancel, (sender, e) => {
				this.save_action_many.Invoke(this, this.sheets, false);
			});
			button_save = new UIBarButtonItem (UIBarButtonSystemItem.Save, (sender, e) => {
				if( sheets != null && sheets.Count == doc.sorted_sheets.Count ) // All -> simple all
					sheets = null;
				this.save_action_many.Invoke(this, this.sheets, true);
				//UINavigationController nc = NavigationController;
				//nc.PopViewControllerAnimated(false);
				//AppDelegate.play_transition(nc.View, UIViewAnimationTransition.CurlUp);
			});
		}
		void construct(Document doc, string title, string initial_sheet, string disabled_sheet, Action<SheetsSelect, string> save_action)
		{
			this.doc = doc;
			this.sheets = new List<string> ();
			this.sheets.Add(initial_sheet);
			this.disabled_sheet = disabled_sheet;
			this.save_action_many = null;
			this.save_action = save_action;
			this.show_all_sheets = false;
			NavigationItem.Title = title;
			NavigationItem.LeftBarButtonItem = null;
		}
		void construct(Document doc, string title, List<string> initial_sheets, Action<SheetsSelect, List<string>, bool> save_action_many)
		{
			this.doc = doc;
			this.sheets = initial_sheets;
			this.disabled_sheet = "-1";
			this.save_action_many = save_action_many;
			this.save_action = null;
			this.show_all_sheets = true;
			this.multiselect = initial_sheets != null && initial_sheets.Count > 1;
			NavigationItem.Title = title;
			NavigationItem.LeftBarButtonItem = cancel_button;
		}
		~SheetsSelect()
		{
			Console.WriteLine ("~SheetsSelect");
		}
		private static Utility.ReuseVC<SheetsSelect> reuse = new Utility.ReuseVC<SheetsSelect> ();
		public static SheetsSelect create_or_reuse(Document doc, string title, string initial_sheet, string disabled_sheet, Action<SheetsSelect, string> save_action)
		{
			SheetsSelect result = reuse.create_or_reuse();
			result.construct(doc, title, initial_sheet, disabled_sheet, save_action);
			return result;
		}		
		public static SheetsSelect create_or_reuse(Document doc, string title, List<string> initial_sheets, Action<SheetsSelect, List<string>, bool> save_action_many)
		{
			SheetsSelect result = reuse.create_or_reuse();
			result.construct(doc, title, initial_sheets, save_action_many);
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
				NavigationController.PushViewController(NameVC.create_or_reuse(doc, i18n.get ("SheetTitleNew"),i18n.get ("SheetNamePlaceholder"),false,false,false,"", "", (nvc,str)=>{
					FlurryAnalytics.Flurry.LogEvent("Sheet", NSDictionary.FromObjectsAndKeys(new object[]{"NewFromSelect"}, new object[]{"action"}));
					AppDelegate.app.docs.execute_change( new ChangeSheetCreate(doc, str));
				}), true);
			};

			// Perform any additional setup after loading the view, typically from a nib.
			table.Source = new TableSource (this);

			NavigationItem.RightBarButtonItem = multiselect ? button_save : null;
		}
		private void anything_changed()
		{
			table.ReloadData();
			panelCheck.Hidden = (save_action_many == null);
			panelPlus.Hidden = !panelCheck.Hidden;
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

			if( sheets != null && sheets.Count > 0 ) {
				int ind = doc.sorted_sheets.FindIndex ((a)=>a.id == sheets[0] );
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
			SheetsSelect parent_vc;
			public TableSource (SheetsSelect parent_vc)
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
					return parent_vc.show_all_sheets ? 1 : 0;
				return parent_vc.doc.sorted_sheets.Count;
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				SimpleCell cell = (SimpleCell)tableView.DequeueReusableCell (SimpleCell.Key);
				if (cell == null)
					cell = SimpleCell.Create(tableView);
				if (indexPath.Section == 0) {
					cell.configAccessory (true, !parent_vc.multiselect && parent_vc.sheets == null);
					cell.setImageName(null, i18n.get ("TotalSheet"), "");
				}else{
					DBSheet sh = parent_vc.doc.sorted_sheets [indexPath.Row];
					int expense_count;
					long date;
					parent_vc.doc.get_sheet_count_date(sh.id, out expense_count, out date);
					cell.configAccessory (true, (parent_vc.multiselect && parent_vc.sheets == null) || (parent_vc.sheets != null && parent_vc.sheets.IndexOf(sh.id) != -1));
					string cou = i18n.get ("OperationsNumber").Replace("{count}", expense_count.ToString ()); // Culture ok, 
					cell.setImageName(null, sh.get_loc_name(), cou, sh.id != parent_vc.disabled_sheet);
				}
				return cell;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == 0) {
					if (parent_vc.multiselect) {
						if (parent_vc.sheets == null || parent_vc.sheets.Count == parent_vc.doc.sorted_sheets.Count)
							parent_vc.sheets = new List<string> ();
						else
							parent_vc.sheets = null;
						updateVisibleCellCheckmarks (tableView);
					} else {
						parent_vc.sheets = null;
						updateVisibleCellCheckmarks (tableView);
						parent_vc.save_action_many.Invoke (parent_vc, parent_vc.sheets, true);
					}
//					AppDelegate.dismiss_or_pop (parent_vc.NavigationController, true);
				}
				if (indexPath.Section == 1) {
					DBSheet sh = parent_vc.doc.sorted_sheets [indexPath.Row];
					if(sh.id != parent_vc.disabled_sheet) {
						if (parent_vc.save_action != null) {
							parent_vc.sheets [0] = sh.id;
							updateVisibleCellCheckmarks (tableView);
							parent_vc.save_action.Invoke (parent_vc, parent_vc.sheets[0]);
							Utility.dismiss_or_pop (parent_vc.NavigationController, true);
						}else{
							if (parent_vc.multiselect) {
								if (parent_vc.sheets == null) {
									parent_vc.sheets = new List<string> ();
									foreach(DBSheet ash in parent_vc.doc.sorted_sheets){
										parent_vc.sheets.Add (ash.id);
									}
									parent_vc.sheets.Remove (sh.id);
								}else{
									if (parent_vc.sheets.IndexOf (sh.id) == -1)
										parent_vc.sheets.Add (sh.id);
									else
										parent_vc.sheets.Remove (sh.id);
								}
								updateVisibleCellCheckmarks (tableView);
							} else {
								parent_vc.sheets = new List<string> ();
								parent_vc.sheets.Add(sh.id);
								updateVisibleCellCheckmarks (tableView);
								parent_vc.save_action_many.Invoke (parent_vc, parent_vc.sheets, true);
							}
						}
					}
				}
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
			public void updateVisibleCellCheckmarks(UITableView tableView)
			{
				foreach (NSIndexPath c in tableView.IndexPathsForVisibleRows) {
					SimpleCell cell = tableView.CellAt (c) as SimpleCell;
					if (cell == null)
						continue;
					if (c.Section == 0){
						DBSheet sh = parent_vc.doc.sorted_sheets [c.Row];
						cell.configAccessory (true, !parent_vc.multiselect && parent_vc.sheets == null);
					}
					if (c.Section == 1){
						DBSheet sh = parent_vc.doc.sorted_sheets [c.Row];
						cell.configAccessory (true, (parent_vc.multiselect && parent_vc.sheets == null) || (parent_vc.sheets != null && parent_vc.sheets.IndexOf(sh.id) != -1));
					}
				}
			}
		}	
	}
}

