using System;
using System.Drawing;
using Foundation;
using UIKit;
using System.Collections.Generic;
using SmartBudgetCommon;

namespace SmartBudgetiOS
{
	public class CurrencyVC : UIViewController
	{
		private UITableView table;
		public CurrencyVC ()// : base ("CurrencyVC", null)
		{
			NavigationItem.Title = i18n.get("CurrenciesTitle");
			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
				ContentSizeForViewInPopover = new SizeF(320, 460);
		}
		~CurrencyVC()
		{
			Console.WriteLine ("~CurrencyVC");
		}
		private static Utility.ReuseVC<CurrencyVC> reuse = new Utility.ReuseVC<CurrencyVC> ();
		public static CurrencyVC create_or_reuse()
		{
			CurrencyVC result = reuse.create_or_reuse();
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

			table = AppDelegate.create_table_and_background (View, 0);
			table.AllowsSelectionDuringEditing = true;

			table.Source = new TableSource (this);
			table.ReloadData ();
			table.Editing = true;
			//AppDelegate.app.docs.anything_changed += (docs, e) => {
			//	if(e.originator != this)
			//		table.ReloadData ();
			//};
		}
		public class TableSource : UITableViewSource {
			List<String> popular = AppDelegate.app.docs.get_popular_currencies();
			List<String> all = CurrencyFormat.get_all_currencies();
			CurrencyVC currency_vc;
			public TableSource (CurrencyVC currency_vc)
			{
				this.currency_vc = currency_vc;
			}
			public override nint NumberOfSections(UITableView tableview)
			{
				return 3;
			}
			public override UIView GetViewForHeader (UITableView tableView, nint section)
			{
				UILabel label;
				UITableViewHeaderFooterView hfv = SectionHeader2.deque_header(tableView, out label);
				//SectionHeader sh = SectionHeader.create_or_get_header ();
				label.Text = section == 0 ? i18n.get("CurrencySelectedHeader") : section == 1 ? i18n.get ("CurrencyPopularHeader") : i18n.get ("CurrencyOtherHeader");
				return hfv;
			}
			public override nfloat GetHeightForHeader (UITableView tableView, nint section)
			{
				return SectionHeader2.default_height;
			}
			public override nint RowsInSection (UITableView tableview, nint section)
			{
				switch (section) {
				case 0:
					return AppDelegate.app.docs.selected_currencies.Count;
				case 1:
					return popular.Count;
				}
				return all.Count;
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				CurrencyCell cell = (CurrencyCell)tableView.DequeueReusableCell (CurrencyCell.Key);
				// if there are no cells to reuse, create a new one
				if (cell == null)
					cell = CurrencyCell.Create(tableView);
				string cur = indexPath.Section == 0 ? AppDelegate.app.docs.selected_currencies [indexPath.Row] :
					indexPath.Section == 1 ? popular [indexPath.Row] : all [indexPath.Row];
				cell.setCurrency(cur);
				cell.updateCheckMark (AppDelegate.app.docs.selected_currencies.IndexOf (cell.iso_symbol) != -1);
				nint ris = RowsInSection (tableView, indexPath.Section);
				cell.separator.Hidden = indexPath.Row == ris - 1;
				return cell;
			}
			public override bool CanMoveRow(UITableView tableView, NSIndexPath indexPath)
			{
				return indexPath.Section == 0;
			}
			public override void MoveRow (UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
			{
				string cur = AppDelegate.app.docs.selected_currencies [sourceIndexPath.Row];
				AppDelegate.app.docs.selected_currencies.RemoveAt (sourceIndexPath.Row);
				AppDelegate.app.docs.selected_currencies.Insert (destinationIndexPath.Row, cur);
				AppDelegate.app.docs.send_anything_changed (currency_vc);
				AppDelegate.app.docs.save_settings ();
			}
			public override NSIndexPath CustomizeMoveTarget (UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath proposedIndexPath)
			{
				if (proposedIndexPath.Section != 0)
					return NSIndexPath.FromRowSection (AppDelegate.app.docs.selected_currencies.Count-1, 0);
				return proposedIndexPath;
			}
			public override bool ShouldIndentWhileEditing (UITableView tableView, NSIndexPath indexPath)
			{
				return false;
			}
			public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
			{
				return UITableViewCellEditingStyle.None;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				string cur = indexPath.Section == 0 ? AppDelegate.app.docs.selected_currencies [indexPath.Row] :
					indexPath.Section == 1 ? popular [indexPath.Row] : all [indexPath.Row];
				int ind = AppDelegate.app.docs.selected_currencies.IndexOf (cur);
				if (ind == -1) {
					AppDelegate.app.docs.selected_currencies.Add (cur);
					AppDelegate.app.docs.send_anything_changed (currency_vc);
					AppDelegate.app.docs.save_settings ();
					updateVisibleCellCheckmarks (tableView);
					tableView.InsertRows (new NSIndexPath[]{NSIndexPath.FromRowSection(AppDelegate.app.docs.selected_currencies.Count-1, 0)}, UITableViewRowAnimation.Fade);
				} else {
					if( AppDelegate.app.docs.selected_currencies.Count > 1 )
					{
						AppDelegate.app.docs.selected_currencies.RemoveAt (ind);
						AppDelegate.app.docs.send_anything_changed (currency_vc);
						AppDelegate.app.docs.save_settings ();
						updateVisibleCellCheckmarks (tableView);
						tableView.DeleteRows (new NSIndexPath[]{NSIndexPath.FromRowSection(ind, 0)}, UITableViewRowAnimation.Fade);
					}
				}
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
			private void updateVisibleCellCheckmarks(UITableView tableView)
			{
				foreach (CurrencyCell c in tableView.VisibleCells) {
					c.updateCheckMark (AppDelegate.app.docs.selected_currencies.IndexOf (c.iso_symbol) != -1);
				}
			}
		}
	}
}

