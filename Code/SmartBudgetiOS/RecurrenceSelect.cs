using System;
using System.Drawing;
using Foundation;
using UIKit;
using SmartBudgetCommon;
using System.Collections.Generic;

namespace SmartBudgetiOS
{
	public class RecurrenceSelect : UIViewController
	{
		private Action<RecurrenceSelect, DBReccurence> save_action;
		private DBReccurence rec;
		private List<String> popular;
		private UITableView table;
		public RecurrenceSelect ()// : base ("RecurrenceSelect", null)
		{
			NavigationItem.Title = i18n.get("RecurrenceTitle");
		}
		void construct(DBReccurence initial_rec, Action<RecurrenceSelect, DBReccurence> save_action)
		{
			this.rec = initial_rec;
			this.save_action = save_action;
		}
		~RecurrenceSelect()
		{
			Console.WriteLine ("~RecurrenceSelect");
		}
		private static Utility.ReuseVC<RecurrenceSelect> reuse = new Utility.ReuseVC<RecurrenceSelect> ();
		public static RecurrenceSelect create_or_reuse(DBReccurence initial_rec, Action<RecurrenceSelect, DBReccurence> save_action)
		{
			RecurrenceSelect result = reuse.create_or_reuse();
			result.construct(initial_rec, save_action);
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

			table = AppDelegate.create_table_and_background (View, 0);

			table.Source = new TableSource (this);
		}
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear (animated);
			table.ScrollToRow (NSIndexPath.FromRowSection((int)rec, 0), UITableViewScrollPosition.Middle, false);
		}
		public class TableSource : UITableViewSource {
			RecurrenceSelect parent_vc;
			public TableSource (RecurrenceSelect parent_vc)
			{
				this.parent_vc = parent_vc;
			}
			public override nint NumberOfSections(UITableView tableview)
			{
				return 1;
			}
			public override nint RowsInSection (UITableView tableview, nint section)
			{
				return (int)DBReccurence.RECURRENCE_COUNT;
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				SimpleCell cell = (SimpleCell)tableView.DequeueReusableCell (SimpleCell.Key);
				if (cell == null)
					cell = SimpleCell.Create(tableView);
				DBReccurence r = (DBReccurence)indexPath.Row;
				cell.setImageName (null, i18n.get (r.ToString()), "");
				cell.configAccessory (true, parent_vc.rec == r);
				return cell;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				DBReccurence r = (DBReccurence)indexPath.Row;
				parent_vc.rec = r;
				updateVisibleCellCheckmarks (tableView);
				parent_vc.save_action.Invoke (parent_vc, parent_vc.rec);
				Utility.dismiss_or_pop (parent_vc.NavigationController, true);
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
			private void updateVisibleCellCheckmarks(UITableView tableView)
			{
				foreach (var ii in tableView.IndexPathsForVisibleRows) {
					SimpleCell cell = tableView.CellAt (ii) as SimpleCell;
					if (cell == null)
						continue;
					DBReccurence r = (DBReccurence)ii.Row;
					cell.configAccessory (true, parent_vc.rec == r);
				}
			}
		}
	}
}