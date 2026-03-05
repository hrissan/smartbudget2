using System;
using System.Drawing;
using Foundation;
using UIKit;
using SmartBudgetCommon;
using System.Collections.Generic;

namespace SmartBudgetiOS
{
	public class CurrencySelect : UIViewController
	{
		private Action<CurrencySelect, string> save_action;
		private string currency;
		private List<String> popular = new List<String>();
		private List<String> all;
		private UITableView table;
		public CurrencySelect ()// : base ("CurrencySelect", null)
		{
			NavigationItem.Title = i18n.get("CurrencyTitle");
			//NavigationItem.RightBarButtonItem = new UIBarButtonItem(i18n.get("CurrenciesTitle"), UIBarButtonItemStyle.Plain, (sender, e) => {
			//	NavigationController.PushViewController(CurrencyVC.create_or_reuse(), true);
			//});
			all = CurrencyFormat.get_all_currencies();
		}
		void construct(string initial_currency, Action<CurrencySelect, string> save_action)
		{
			this.currency = initial_currency;
			this.save_action = save_action;
		}
		~CurrencySelect()
		{
			Console.WriteLine ("~CurrencySelect");
		}
		private static Utility.ReuseVC<CurrencySelect> reuse = new Utility.ReuseVC<CurrencySelect> ();
		public static CurrencySelect create_or_reuse(string initial_currency, Action<CurrencySelect, string> save_action)
		{
			CurrencySelect result = reuse.create_or_reuse();
			result.construct (initial_currency, save_action);
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

			table.Source = new TableSource (this);
			//anything_changed ();
			//AppDelegate.app.docs.anything_changed += (docs, e) => {
			//	anything_changed();
			//};
		}
		public void anything_changed()
		{
			popular = AppDelegate.app.docs.get_popular_currencies();
			// Prevent 2 checkmarks in first 2 sections of our list
			foreach (var cur in AppDelegate.app.docs.selected_currencies) {
				popular.Remove (cur);
			}
			/*if (AppDelegate.app.docs.selected_currencies.IndexOf (currency) == -1 &&
				popular.IndexOf (currency) == -1) {
				popular.Add (currency);
			}*/
			table.ReloadData ();
		}
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear (animated);
			anything_changed ();
			int ind = AppDelegate.app.docs.selected_currencies.IndexOf(currency);
			if (ind != -1)
				table.ScrollToRow (NSIndexPath.FromRowSection(ind, 0), UITableViewScrollPosition.Middle, false);
			else {
				ind = popular.IndexOf(currency);
				if (ind != -1)
					table.ScrollToRow (NSIndexPath.FromRowSection(ind, 1), UITableViewScrollPosition.Middle, false);
				else {
					ind = all.IndexOf(currency);
					if (ind != -1)
						table.ScrollToRow (NSIndexPath.FromRowSection(ind, 2), UITableViewScrollPosition.Middle, false);
				}
			}
		}
		public class TableSource : UITableViewSource {
			CurrencySelect currency_vc;
			public TableSource (CurrencySelect currency_vc)
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
				if (section == 0)
					return AppDelegate.app.docs.selected_currencies.Count;
				if (section == 1)
					return currency_vc.popular.Count;
				return currency_vc.all.Count;
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				CurrencyCell cell = (CurrencyCell)tableView.DequeueReusableCell (CurrencyCell.Key);
				// if there are no cells to reuse, create a new one
				if (cell == null)
					cell = CurrencyCell.Create(tableView);
				string cur = indexPath.Section == 0 ? AppDelegate.app.docs.selected_currencies [indexPath.Row] :
					indexPath.Section == 1 ? currency_vc.popular [indexPath.Row] : currency_vc.all [indexPath.Row];
				cell.setCurrency(cur);
				cell.updateCheckMark (cell.iso_symbol == currency_vc.currency);
				return cell;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				string cur = indexPath.Section == 0 ? AppDelegate.app.docs.selected_currencies [indexPath.Row] :
					indexPath.Section == 1 ? currency_vc.popular [indexPath.Row] : currency_vc.all [indexPath.Row];
				currency_vc.currency = cur;
				updateVisibleCellCheckmarks (tableView);
				currency_vc.save_action.Invoke (currency_vc, currency_vc.currency);
				Utility.dismiss_or_pop (currency_vc.NavigationController, true);
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
			private void updateVisibleCellCheckmarks(UITableView tableView)
			{
				foreach (CurrencyCell c in tableView.VisibleCells) {
					c.updateCheckMark (c.iso_symbol == currency_vc.currency);
				}
			}
		}
	}
}