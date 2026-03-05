using System;
using System.Drawing;
using Foundation;
using UIKit;
using System.Collections.Generic;
using SmartBudgetCommon;
using System.Globalization;

namespace SmartBudgetiOS
{
	public class SheetsVC : UIViewController
	{
		private Action<SheetsVC, string> save_action;
		private bool show_all_sheets;
		private string sheet;
		//private TableSource table_source;
		private UITableView table;

		private UIView panelButtons;
		private UIButton btnArrange;
		private UIButton btnHelp;
		private UIButton btnPlus;
		private Document doc;
		private UIBarButtonItem done_button;
		public SheetsVC ()// : base ("SheetsVC", null)
		{
			NavigationItem.Title = i18n.get ("SheetsTitle");
			done_button = new UIBarButtonItem (UIBarButtonSystemItem.Done, (sender, e) => {
				UINavigationController nc = NavigationController;
				Utility.dismiss_or_pop (NavigationController, false);
				Utility.play_transition(nc.View, UIViewAnimationTransition.CurlUp);
			});
		}
		void construct(Document doc, string initial_sheet, bool show_all_sheets, Action<SheetsVC, string> save_action)
		{
			this.doc = doc;
			this.sheet = initial_sheet;
			this.show_all_sheets = show_all_sheets;
			this.save_action = save_action;
			NavigationItem.LeftBarButtonItem = show_all_sheets ? done_button : null;
		}
		~SheetsVC()
		{
			Console.WriteLine ("~SheetsVC");
		}
		private static Utility.ReuseVC<SheetsVC> reuse = new Utility.ReuseVC<SheetsVC> ();
		public static SheetsVC create_or_reuse(Document doc, string initial_sheet, bool show_all_sheets, Action<SheetsVC, string> save_action)
		{
			SheetsVC result = reuse.create_or_reuse();
			result.construct(doc, initial_sheet, show_all_sheets, save_action);
			return result;
		}
		private void add_sheet()
		{
			Utility.push_or_present(NavigationController, NameVC.create_or_reuse(doc, i18n.get ("SheetTitleNew"),i18n.get ("SheetNamePlaceholder"),false,false,false,"", "", (nvc,str)=>{
				FlurryAnalytics.Flurry.LogEvent("Sheet", NSDictionary.FromObjectsAndKeys(new object[]{"New"}, new object[]{"action"}));
				AppDelegate.app.docs.execute_change( new ChangeSheetCreate(doc, str));
			}), true);
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
			panelButtons = BottomPanelView.create_bottom_panel (View);

			btnHelp = BottomPanelView.create_help_button( panelButtons, "help");
			btnArrange = BottomPanelView.create_help_button( panelButtons, "arrange");
			btnPlus = BottomPanelView.create_help_button( panelButtons, "plus");

			btnHelp.TouchUpInside += (sender, e) => {
				LayoutForHelp lh = new LayoutForHelp(NavigationController, table.Frame.Height);
				// From top
				SimpleCell help_cell = null;
				foreach (var ip in table.IndexPathsForVisibleRows) {
					if( ip.Section != 1 )
						continue;
					SimpleCell ac = table.CellAt(ip) as SimpleCell;
					if (ac != null) {
						if( help_cell == null || ac.Frame.Y < help_cell.Frame.Y )
							help_cell = ac;
					}
				}
				if( help_cell != null)
					lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, help_cell, 0, i18n.get ("HelpTapHold"), LayoutForHelp.BubleType.HOLD, true);
				// From bottom
				lh.create_help_label(LayoutForHelp.LARGE_WIDTH, table, 0, i18n.get ("HelpSheets"), LayoutForHelp.BubleType.NO_TAILS, false);
				lh.show ();
			};

			btnArrange.TouchUpInside += (sender, e) => {
				SetEditing(!table.Editing,true);
			};
			btnPlus.TouchUpInside += (sender, e) => {
				add_sheet();
			};
			table.Source = new TableSource (this);
		}
		private void anything_changed()
		{
			table.ReloadData();
		}
		private void anything_changed (Documents doc, Documents.DocumentChangeEventArgs e)
		{
			if( e.originator != this)
				anything_changed ();
		}
		public override void SetEditing (bool editing, bool animated)
		{
			if (editing == table.Editing)
				return;
			table.SetEditing (editing, animated);
			AppDelegate.decorate_bottom_button (btnArrange, "arrange", editing);
			//btnArrange.SetBackgroundImage (editing ? UIImage.FromBundle("b4_backgound_high.png") : null, UIControlState.Normal);
		}
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			AppDelegate.app.docs.anything_changed += anything_changed;
			anything_changed ();
		}
		public override void ViewWillDisappear(bool animated)
		{
			AppDelegate.app.docs.anything_changed -= anything_changed;
			SetEditing (false, true);
			base.ViewWillDisappear (animated);
		}
		public override void ViewWillLayoutSubviews ()
		{
			base.ViewWillLayoutSubviews ();
			BottomPanelView.layout (panelButtons, btnHelp);
		}
		private void show_context_menu(DBSheet sh, UIView origin)
		{
			int expenses_count;
			long recent_expense_date;
			doc.get_sheet_count_date (sh.id, out expenses_count, out recent_expense_date);
			string del_operations_title = null;
			List<string> other = new List<string>();
			other.Add (i18n.get ("EditName"));
			if (expenses_count == 0 && doc.sorted_sheets.Count > 1)
				other.Add (i18n.get ("Delete"));
			if(expenses_count != 0 ) {
				del_operations_title = i18n.get ("DeleteAllOperations");
				if( doc.sorted_sheets.Count > 1 )
					other.Add( i18n.get ("MoveAllOperations") );
			}
			UIActionSheet ash = new UIActionSheet(sh.get_loc_name(), null, i18n.get("Cancel"), del_operations_title, other.ToArray());
			ash.Clicked += (sender, e) => {
				if( e.ButtonIndex == ash.DestructiveButtonIndex ){
					string move_tit = i18n.get ("DeleteAllOperationsMenuTitle").Replace("{from}", sh.get_loc_name());
					UIActionSheet ash2 = new UIActionSheet(move_tit, null, i18n.get("Cancel"), i18n.get ("Delete"));
					ash2.Clicked += (sender2, e2) => {
						if( e2.ButtonIndex == ash2.DestructiveButtonIndex ){
							UIActionSheet ash3 = new UIActionSheet(move_tit, null, i18n.get("Cancel"), i18n.get ("Delete"));
							ash3.Clicked += (sender3, e3) => {
								if( e3.ButtonIndex == ash3.DestructiveButtonIndex ){
									FlurryAnalytics.Flurry.LogEvent("Sheet", NSDictionary.FromObjectsAndKeys(new object[]{"DeleteAllOperations"}, new object[]{"action"}));
									AppDelegate.app.docs.execute_delete_from_sheet(doc, sh.id);
								}
							};
							Utility.show_action_sheet(ash3, View);
						}
					};
					Utility.show_action_sheet(ash2, View, origin);
				}
				if( ash.ButtonTitle(e.ButtonIndex) == i18n.get ("Delete") ){
					FlurryAnalytics.Flurry.LogEvent("Sheet", NSDictionary.FromObjectsAndKeys(new object[]{"Delete"}, new object[]{"action"}));
					AppDelegate.app.docs.execute_change (new ChangeSheetRemove(doc, sh.id), null);
				}
				if( ash.ButtonTitle(e.ButtonIndex) == i18n.get ("MoveAllOperations") ){
					NavigationController.PushViewController(SheetsSelect.create_or_reuse(doc, i18n.get ("MoveToSheetTitle"), "", sh.id, (svc, new_sh_id)=>{
						DBSheet new_sh = doc.get_sheet(new_sh_id);
						string move_tit = i18n.get ("SheetMoveMenuTitle").Replace("{from}", sh.get_loc_name()).Replace("{to}", new_sh.get_loc_name());
						UIActionSheet ash2 = new UIActionSheet(move_tit, null, i18n.get("Cancel"), i18n.get ("MoveAllOperations"));
						ash2.Clicked += (sender2, e2) => {
							if( e2.ButtonIndex == ash2.DestructiveButtonIndex ){
								FlurryAnalytics.Flurry.LogEvent("Sheet", NSDictionary.FromObjectsAndKeys(new object[]{"MoveAllOperations"}, new object[]{"action"}));
								AppDelegate.app.docs.execute_move_from_sheet(doc, sh.id, new_sh_id);
//								AppDelegate.app.docs.execute_change (new ChangeSheetRemove(doc, sh.id), null);
							}
						};
						Utility.show_action_sheet(ash2, NavigationController.View);
					}), true);
				}
				if( ash.ButtonTitle(e.ButtonIndex) == i18n.get ("EditName") ){
					NavigationController.PushViewController(NameVC.create_or_reuse(doc, i18n.get ("SheetTitle"),i18n.get ("SheetNamePlaceholder"),false,false,false,sh.get_loc_name(), "", (nvc,str)=>{
						sh.name = str;
						FlurryAnalytics.Flurry.LogEvent("Sheet", NSDictionary.FromObjectsAndKeys(new object[]{"EditName"}, new object[]{"action"}));
						AppDelegate.app.docs.execute_change( new ChangeSheetUpdate(doc, sh));
					}), true);
				}
			};
			Utility.show_action_sheet(ash, View, origin);
		}
		public class TableSource : UITableViewSource {
			SheetsVC parent_vc;
			public TableSource (SheetsVC parent_vc)
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
			void show_context_menu(UITableViewCell cc)
			{
				NSIndexPath ii = parent_vc.table.IndexPathForCell(cc);
				if( ii == null || ii.Section == 0 )
					return;
				DBSheet sh = parent_vc.doc.sorted_sheets [ii.Row];
				parent_vc.show_context_menu(sh, cc);
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				SimpleCell cell = (SimpleCell)tableView.DequeueReusableCell (SimpleCell.Key);
				if (cell == null) {
					//cell = SimpleCell.Create ();
					WeakReference weak_this = new WeakReference (this);
					cell = SimpleCell.Create( tableView, (cc)=>{
						TableSource strong_this = weak_this.Target as TableSource;
						if( strong_this == null )
							return;
						strong_this.show_context_menu(cc);
					});
					//cell.center_image ();
				}
				if (indexPath.Section == 0) {
					cell.configAccessory (!parent_vc.show_all_sheets, String.IsNullOrEmpty(parent_vc.sheet));
					cell.setImageName(null, i18n.get ("TotalSheet"), "");
				} else {
					DBSheet sh = parent_vc.doc.sorted_sheets [indexPath.Row];
					int expense_count;
					long date;
					parent_vc.doc.get_sheet_count_date(sh.id, out expense_count, out date);
					cell.configAccessory (!parent_vc.show_all_sheets, parent_vc.sheet == sh.id);
					string cou = i18n.get ("OperationsNumber").Replace("{count}", expense_count.ToString ()); // Culture ok, 

					long planned_date;
					parent_vc.doc.get_sheet_next_planned_date (sh.id, out planned_date, true);

					cell.setImageName(Documents.planned_soon(planned_date) ? AppDelegate.get_attention_icon_small() : null, sh.get_loc_name(), cou);
				}
				return cell;
			}
			public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == 0)
					return false;
				return tableView.Editing;
			}
			/*public override int IndentationLevel (UITableView tableView, NSIndexPath indexPath)
			{
				return 0;
			}*/
			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				if( editingStyle != UITableViewCellEditingStyle.Delete )
					return;
				DBSheet sh = parent_vc.doc.sorted_sheets [indexPath.Row];
				tableView.BeginUpdates ();
				FlurryAnalytics.Flurry.LogEvent("Sheet", NSDictionary.FromObjectsAndKeys(new object[]{"Delete"}, new object[]{"action"}));
				AppDelegate.app.docs.execute_change (new ChangeSheetRemove(parent_vc.doc, sh.id), parent_vc);
				tableView.DeleteRows (new NSIndexPath[]{indexPath}, UITableViewRowAnimation.Left);
				tableView.EndUpdates ();
				updateVisibleCellCheckmarks (tableView);
			}
			public override bool CanMoveRow(UITableView tableView, NSIndexPath indexPath)
			{
				return indexPath.Section == 1;
			}
			public override void MoveRow (UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
			{
				if (sourceIndexPath.Section == destinationIndexPath.Section &&
				    sourceIndexPath.Row == destinationIndexPath.Row)
					return; // Obvious optimization saves lots of commands
				FlurryAnalytics.Flurry.LogEvent("Sheet", NSDictionary.FromObjectsAndKeys(new object[]{"Arrange"}, new object[]{"action"}));
				AppDelegate.app.docs.execute_change (new ChangeSheetArrange(parent_vc.doc, sourceIndexPath.Row, destinationIndexPath.Row), parent_vc);
			}
			public override NSIndexPath CustomizeMoveTarget (UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath proposedIndexPath)
			{
				if (proposedIndexPath.Section == 0)
					return NSIndexPath.FromRowSection (0, 1);
				return proposedIndexPath;
			}
			public override bool ShouldIndentWhileEditing (UITableView tableView, NSIndexPath indexPath)
			{
				return false;
			}
			public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == 0)
					return UITableViewCellEditingStyle.None;
				if(parent_vc.doc.sorted_sheets.Count < 2)
					return UITableViewCellEditingStyle.None;
				//return UITableViewCellEditingStyle.Delete;
				DBSheet sh = parent_vc.doc.sorted_sheets [indexPath.Row];
				int expenses_count;
				long recent_expense_date;
				parent_vc.doc.get_sheet_count_date (sh.id, out expenses_count, out recent_expense_date);
				return expenses_count == 0 ? UITableViewCellEditingStyle.Delete : UITableViewCellEditingStyle.None;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == 0) {
					parent_vc.sheet = "";
				} else {
					DBSheet sh = parent_vc.doc.sorted_sheets [indexPath.Row];
					parent_vc.sheet = sh.id;
				}
				parent_vc.save_action.Invoke (parent_vc, parent_vc.sheet);
				updateVisibleCellCheckmarks (tableView);
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
			public void updateVisibleCellCheckmarks(UITableView tableView)
			{
				foreach (NSIndexPath c in tableView.IndexPathsForVisibleRows) {
					SimpleCell cell = tableView.CellAt (c) as SimpleCell;
					if (cell == null)
						continue;
					if (c.Section == 0)
						cell.configAccessory (!parent_vc.show_all_sheets, String.IsNullOrEmpty(parent_vc.sheet));
					else {
						DBSheet sh = parent_vc.doc.sorted_sheets [c.Row];
						cell.configAccessory (!parent_vc.show_all_sheets, parent_vc.sheet == sh.id);
					}
					//c.updateCheckMark ();
				}
			}
		}	
	}
}

