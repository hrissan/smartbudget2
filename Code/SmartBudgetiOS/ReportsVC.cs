using System;
using System.Drawing;
using Foundation;
using UIKit;
using SmartBudgetCommon;

namespace SmartBudgetiOS
{
	public class ReportsVC : UIViewController
	{
		private Action<ReportsVC, string> save_action;
		private string report;
		private string new_settings;
		private UITableView table;
		private UIView panelButtons;
		private UIButton btnPlus;
		private UIButton btnArrange;
		private Document doc;
		public ReportsVC ()// : base ("ReportsVC", null)
		{
			NavigationItem.Title = i18n.get("Reports");
			//NavigationItem.LeftBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Done, (sender, e) => {
			//	AppDelegate.dismiss_or_pop (NavigationController, true);
			//});
		}
		void construct(Document doc, string initial_report, string new_settings, Action<ReportsVC, string> save_action)
		{
			this.doc = doc;
			this.report = initial_report;
			this.new_settings = new_settings;
			this.save_action = save_action;
		}
		~ReportsVC()
		{
			Console.WriteLine ("~ReportsVC");
		}
		private static Utility.ReuseVC<ReportsVC> reuse = new Utility.ReuseVC<ReportsVC> ();
		public static ReportsVC create_or_reuse(Document doc, string initial_report, string new_settings, Action<ReportsVC, string> save_action)
		{
			ReportsVC result = reuse.create_or_reuse();
			result.construct(doc, initial_report, new_settings, save_action);
			return result;
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

			table = AppDelegate.create_table_and_background (View, 1);
			panelButtons = BottomPanelView.create_bottom_panel (View);

			btnArrange = BottomPanelView.create_bottom_button( panelButtons, "arrange");
			btnPlus = BottomPanelView.create_bottom_button( panelButtons, "plus");

			btnPlus.TouchUpInside += (sender, e) => {
				FlurryAnalytics.Flurry.LogEvent("Report", NSDictionary.FromObjectsAndKeys(new object[]{"New"}, new object[]{"action"}));
				AppDelegate.app.docs.execute_change(new ChangeReportCreate(doc, new_settings), null);
			};
			btnArrange.TouchUpInside += (sender, e) => {
				SetEditing(!table.Editing,true);
			};
			table.Source = new TableSource (this);
		}
		private void anything_changed()
		{
			table.ReloadData();
		}
		private void anything_changed (Documents doc, Documents.DocumentChangeEventArgs e)
		{
			if (e.originator != this) {
				anything_changed ();
			}
		}
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear (animated);
			AppDelegate.app.docs.anything_changed += anything_changed;
			anything_changed ();
			int ind = doc.sorted_reports.FindIndex ((a)=>a.id == report );
			if (ind != -1)
				table.ScrollToRow (NSIndexPath.FromRowSection(ind, 1), UITableViewScrollPosition.Middle, false);
			//			else
			//				table.SetContentOffset( PointF.Empty, false);
		}
		public override void SetEditing (bool editing, bool animated)
		{
			if (editing == table.Editing)
				return;
			table.SetEditing (editing, animated);
			AppDelegate.decorate_bottom_button (btnArrange, "arrange", editing);
			//btnArrange.SetBackgroundImage (editing ? UIImage.FromBundle("b4_backgound_high.png") : null, UIControlState.Normal);
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
			BottomPanelView.layout (panelButtons, null);
		}
		public class TableSource : UITableViewSource {
			ReportsVC parent_vc;
			public TableSource (ReportsVC parent_vc)
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
					return 1;
				return parent_vc.doc.sorted_reports.Count;
			}
			public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == 0)
					return tableView.RowHeight;
				return 80;
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				if (indexPath.Section == 0) {
					SimpleCell scell = (SimpleCell)tableView.DequeueReusableCell (SimpleCell.Key);
					if (scell == null)
						scell = SimpleCell.Create(tableView);
					scell.configAccessory (true, String.IsNullOrEmpty(parent_vc.report));
					scell.setImageName(null, i18n.get ("SimpleReport"), "");
					return scell;
				}

				ReportCell cell = (ReportCell)tableView.DequeueReusableCell (ReportCell.Key);
				if (cell == null)
					cell = ReportCell.Create(tableView);
				DBReport rep = parent_vc.doc.sorted_reports [indexPath.Row];
				StatsSettings stat = new StatsSettings (rep.id, rep.data);
				stat.validate (parent_vc.doc);
				string sh_title = null;
				if (stat.selected_sheets == null)
					sh_title = i18n.get ("TotalSheet");
				else{
					foreach (var ss in stat.selected_sheets) {
						DBSheet sh = parent_vc.doc.get_sheet (ss);
						if (sh_title == null)
							sh_title = sh.get_loc_name ();
						else
							sh_title = i18n.get ("List12").Replace ("{item1}", sh_title).Replace ("{item2}", sh.get_loc_name ());
					}
				}
				string acc_title = null;
				if (stat.selected_accounts == null)
					acc_title = i18n.get ("TotalAccount");
				else{
					foreach (var aa in stat.selected_accounts) {
						DBAccount acc = parent_vc.doc.get_account (aa);
						if (acc_title == null)
							acc_title = acc.name;
						else
							acc_title = i18n.get("List12").Replace("{item1}", acc_title).Replace("{item2}", acc.name);
					}
				}
				string dat = i18n.get ("List12").Replace ("{item1}", StatsVC.format_settings_date(stat)).Replace ("{item2}", stat.report_currency);
//				int expense_count;
//				long date;
//				parent_vc.doc.get_sheet_count_date(sh.id, out expense_count, out date);
				cell.configAccessory (true, parent_vc.report == rep.id);
				cell.setImageName(null, dat, sh_title, acc_title);
				return cell;
			}
			public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == 0)
					return false;
				return tableView.Editing;
			}
			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				if( editingStyle != UITableViewCellEditingStyle.Delete )
					return;
				DBReport rep = parent_vc.doc.sorted_reports [indexPath.Row];
				tableView.BeginUpdates ();
				FlurryAnalytics.Flurry.LogEvent("Report", NSDictionary.FromObjectsAndKeys(new object[]{"Delete"}, new object[]{"action"}));
				AppDelegate.app.docs.execute_change (new ChangeReportRemove(parent_vc.doc, rep.id), parent_vc);
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
				FlurryAnalytics.Flurry.LogEvent("Report", NSDictionary.FromObjectsAndKeys(new object[]{"Arrange"}, new object[]{"action"}));
				AppDelegate.app.docs.execute_change (new ChangeReportArrange(parent_vc.doc, sourceIndexPath.Row, destinationIndexPath.Row), parent_vc);
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
				return UITableViewCellEditingStyle.Delete;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == 0) {
					parent_vc.report = "";
				} else {
					DBReport rep = parent_vc.doc.sorted_reports [indexPath.Row];
					parent_vc.report = rep.id;
				}
				parent_vc.save_action.Invoke (parent_vc, parent_vc.report);
				updateVisibleCellCheckmarks (tableView);
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
				Utility.dismiss_or_pop (parent_vc.NavigationController, true);
			}
			public void updateVisibleCellCheckmarks(UITableView tableView)
			{
				foreach (NSIndexPath c in tableView.IndexPathsForVisibleRows) {
					SimpleCell cell = tableView.CellAt (c) as SimpleCell;
					if (cell == null)
						continue;
					if (c.Section == 0)
						cell.configAccessory (true, String.IsNullOrEmpty(parent_vc.report));
					else {
						DBReport rep = parent_vc.doc.sorted_reports [c.Row];
						cell.configAccessory (true, parent_vc.report == rep.id);
					}
					//c.updateCheckMark ();
				}
			}
		}	
	}
}

