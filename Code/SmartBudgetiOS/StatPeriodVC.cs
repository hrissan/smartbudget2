using System;
using System.Drawing;
using Foundation;
using UIKit;
using SmartBudgetCommon;

namespace SmartBudgetiOS
{
	public partial class StatPeriodVC : UIViewController
	{
//		string nodate_title;
		StatsSettings settings;
		NSDate date;
		int months;
		int days;
		Action<StatPeriodVC, bool> on_save;
		UIButton btnClearDate;
		UITableView table;
		//NSDateFormatter daf = new NSDateFormatter ();

		public StatPeriodVC () : base ("StatPeriodVC", null)
		{
			//this.date = TicksToNSDate(initial_date != 0 ? initial_date : DateTime.Now.Ticks);
			//daf.TimeStyle = NSDateFormatterStyle.Long;
			//daf.DateStyle = NSDateFormatterStyle.Long;
			//Console.WriteLine ("date={0}", daf.ToString (this.date));
			//Console.WriteLine ("date={0}", daf.ToString (this.date));
			NavigationItem.Title = i18n.get ("ReportPeriod");
			NavigationItem.RightBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Save, (sender, e) => {
				this.settings.days = this.days;
				this.settings.months = this.months;
				this.settings.from_date = ((DateTime)this.date).ToLocalTime();
				on_save.Invoke (this, true);
				Utility.dismiss_or_pop(NavigationController, true);
				kill_picker();
			});
			NavigationItem.LeftBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Cancel, (sender, e) => {
				Utility.dismiss_or_pop(NavigationController, true);
				kill_picker();
			});
		}
		void construct(StatsSettings settings, Action<StatPeriodVC, bool> on_save)
		{
			this.settings = settings;
			this.date = (NSDate)settings.from_date.ToUniversalTime();
			this.months = settings.months;
			this.days = settings.days;
			if (months == 0 && days == 0)
				months = 1;
			this.on_save = on_save;
		}
		~StatPeriodVC()
		{
			Console.WriteLine ("~StatPeriodVC");
		}	
		private static Utility.ReuseVC<StatPeriodVC> reuse = new Utility.ReuseVC<StatPeriodVC> ();
		public static StatPeriodVC create_or_reuse(StatsSettings settings, Action<StatPeriodVC, bool> on_save)
		{
			StatPeriodVC result = reuse.create_or_reuse();
			result.construct(settings, on_save);
			return result;
		}		
		private void kill_picker()
		{
			picker.RemoveFromSuperview ();
			picker.Dispose ();
			picker = null;
			periodPicker.RemoveFromSuperview ();
			periodPicker.Dispose ();
			periodPicker = null;
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
			//AppDelegate.create_table_background(View, 0, picker.Frame.Y - AppDelegate.BUTTON_ROW_HEIGHT, UIViewAutoresizing.FlexibleHeight);
			//AppDelegate.create_table_cover(View, picker, picker.Frame.Y - AppDelegate.BUTTON_ROW_HEIGHT);
			table = AppDelegate.create_table_and_background (tablePlace, 1);

			btnClearDate = AppDelegate.create_flat_bottom_button (tablePlace, i18n.get ("AllTime"), tablePlace.Bounds.Height);
			btnClearDate.TouchUpInside += (sender, e) => {
				this.settings.days = 0;
				this.settings.months = 0;
				this.settings.from_date = ((DateTime)this.date).ToLocalTime();
				on_save.Invoke (this, true);
				Utility.dismiss_or_pop(NavigationController, true);
				kill_picker();
			};

			table.Source = new TableSource (this);

			picker.ValueChanged += (sender, e) => {
				date = picker.Date;
				update_label();
			};
			picker.SetDate (date, false);

			periodPicker.Model = new PeriodModel (this);
			periodPicker.Select (months, 0, false);
			periodPicker.Select (days, 1, false);

			periodPicker.Hidden = true;

			/*btnDate.TouchUpInside += (sender, e) => {
				picker.Hidden = false;
				periodPicker.Hidden = true;
			};
			btnPeriod.TouchUpInside += (sender, e) => {
				picker.Hidden = true;
				periodPicker.Hidden = false;
			};*/
			// Perform any additional setup after loading the view, typically from a nib.
		}
		public void update_label()
		{
			table.ReloadData ();
/*			labelValue.Text = AppDelegate.app.date_week_formatter.ToString (date);
			string all_string = "";
			string days_string = i18n.localize_plural ("NDays", days);
			string months_string = i18n.localize_plural ("NMonths", months);
			if (days == 0 && months != 0)
				all_string = months_string;
			else if (days != 0 && months == 0)
				all_string = days_string;
			else if (days != 0 && months != 0)
				all_string = months_string + ", " + days_string;
			labelPeriod.Text = all_string;*/
		}
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			update_label();
		}
		public class PeriodModel : UIPickerViewModel {
			StatPeriodVC parent_vc;
			public PeriodModel(StatPeriodVC parent_vc)
			{
				this.parent_vc = parent_vc;
			}
			public override nint GetComponentCount (UIPickerView picker)
			{
				return 2;
			}
			public override nint GetRowsInComponent (UIPickerView picker, nint component)
			{
				if (component == 0)
					return 25;
				return 32;
			}
			public override string GetTitle (UIPickerView picker, nint row, nint component)
			{
				if (component == 0)
					return i18n.localize_plural ("NMonths", (int)row);
				return i18n.localize_plural ("NDays", (int)row);
			}
			public override void Selected (UIPickerView picker, nint row, nint component)
			{
				if (component == 0) {
					parent_vc.months = (int)row;
				} else {
					parent_vc.days = (int)row;
				}
				parent_vc.update_label ();
			}
		}
		public class TableSource : UITableViewSource {
			StatPeriodVC parent_vc;
			public TableSource (StatPeriodVC parent_vc)
			{
				this.parent_vc = parent_vc;
			}
			public override nint NumberOfSections(UITableView tableview)
			{
				return 1;
			}
			public override nint RowsInSection (UITableView tableview, nint section)
			{
				return 2;
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				SimpleCell cell = (SimpleCell)tableView.DequeueReusableCell (SimpleCell.Key);
				if (cell == null)
					cell = SimpleCell.Create(tableView);
				if (indexPath.Row == 0) {
					cell.setImageName (null, AppDelegate.app.date_week_formatter.ToString (parent_vc.date), "");
					cell.configAccessory (true, !parent_vc.picker.Hidden);
				} else {
					string all_string = "";
					string days_string = i18n.localize_plural ("NDays", parent_vc.days);
					string months_string = i18n.localize_plural ("NMonths", parent_vc.months);
					if (parent_vc.days == 0 && parent_vc.months != 0)
						all_string = months_string;
					else if (parent_vc.days != 0 && parent_vc.months == 0)
						all_string = days_string;
					else if (parent_vc.days != 0 && parent_vc.months != 0)
						all_string = months_string + ", " + days_string;
					cell.setImageName (null, all_string, "");
					cell.configAccessory (true, parent_vc.picker.Hidden);
				}
				return cell;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Row == 0) {
					parent_vc.picker.Hidden = false;
					parent_vc.periodPicker.Hidden = true;
				}else{
					parent_vc.picker.Hidden = true;
					parent_vc.periodPicker.Hidden = false;
				}
				updateVisibleCellCheckmarks (tableView);
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
			private void updateVisibleCellCheckmarks(UITableView tableView)
			{
				foreach (var ii in tableView.IndexPathsForVisibleRows) {
					SimpleCell cell = tableView.CellAt (ii) as SimpleCell;
					if (cell == null)
						continue;
					if (ii.Row == 0)
						cell.configAccessory (true, !parent_vc.picker.Hidden);
					else
						cell.configAccessory (true, parent_vc.picker.Hidden);
				}
			}
		}
	}
}

