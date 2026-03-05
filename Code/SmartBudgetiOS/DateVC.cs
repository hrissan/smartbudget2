using System;
using System.Drawing;
using Foundation;
using UIKit;
using SmartBudgetCommon;

namespace SmartBudgetiOS
{
	public partial class DateVC : UIViewController
	{
		bool in_chain;
		string nodate_title;
		NSDate date;
		Action<DateVC, long> on_save;
		UIButton btnTodayDate;
		UIButton btnClearDate;

//		NSDateFormatter daf = new NSDateFormatter ();

		UIBarButtonItem next_button;
		UIBarButtonItem cancel_button;
		UIBarButtonItem save_button;

		public DateVC () : base ("DateVC", null)
		{
//			daf.TimeStyle = NSDateFormatterStyle.Long;
//			daf.DateStyle = NSDateFormatterStyle.Long;

			next_button = new UIBarButtonItem (i18n.get ("Next"), UIBarButtonItemStyle.Plain, (sender, e) => {
				on_save.Invoke (this, ((DateTime)this.date).ToLocalTime().Ticks);
				kill_picker();
			});
			save_button = new UIBarButtonItem (UIBarButtonSystemItem.Save, (sender, e) => {
				on_save.Invoke (this, ((DateTime)this.date).ToLocalTime().Ticks);
				Utility.dismiss_or_pop(NavigationController, true);
				kill_picker();
			});
			cancel_button = new UIBarButtonItem (UIBarButtonSystemItem.Cancel, (sender, e) => {
				Utility.dismiss_or_pop(NavigationController, true);
				kill_picker();
			});
		}
		void construct(string title, string nodate_title, bool in_chain, long initial_date, Action<DateVC, long> on_save)
		{
			this.nodate_title = nodate_title;
			this.in_chain = in_chain;
			this.date = initial_date != 0 ? (NSDate)new DateTime(initial_date, DateTimeKind.Local) : (NSDate)DateTime.Now;
			//			Console.WriteLine ("date={0}", daf.ToString (this.date));
			this.on_save = on_save;
			NavigationItem.Title = title;
			if (in_chain) {
				NavigationItem.LeftBarButtonItem = null;
				NavigationItem.RightBarButtonItem = next_button;
			} else {
				NavigationItem.LeftBarButtonItem = cancel_button;
				NavigationItem.RightBarButtonItem = save_button;
			}
		}
		~DateVC()
		{
			Console.WriteLine ("~DateVC");
		}	
		private static Utility.ReuseVC<DateVC> reuse = new Utility.ReuseVC<DateVC> ();
		public static DateVC create_or_reuse(string title, string nodate_title, bool in_chain, long initial_date, Action<DateVC, long> on_save)
		{
			//GC.Collect(); // DatePicker is casuing huge memory spikes. We try to contain them
			DateVC result = reuse.create_or_reuse();
			result.construct(title, nodate_title, in_chain, initial_date, on_save);
			return result;
		}
		private void kill_picker()
		{
			picker.RemoveFromSuperview ();
			picker.Dispose ();
			picker = null;
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
			AppDelegate.create_table_background(View, 0, picker.Frame.Y - AppDelegate.BUTTON_ROW_HEIGHT, UIViewAutoresizing.FlexibleHeight);
			AppDelegate.create_table_cover(View, picker, picker.Frame.Y - AppDelegate.BUTTON_ROW_HEIGHT);

			btnClearDate = AppDelegate.create_flat_bottom_button (View, nodate_title, picker.Frame.Y); // TODO - string
			btnClearDate.TouchUpInside += (sender, e) => {
				on_save.Invoke (this, 0);
				if( !in_chain )
					Utility.dismiss_or_pop(NavigationController, true);
				kill_picker();
			};

			btnTodayDate = AppDelegate.create_flat_bottom_button (View, i18n.get ("SelectTodayDate"), picker.Frame.Y); // TODO - string
			btnTodayDate.TouchUpInside += (sender, e) => {
				date = (NSDate)DateTime.Now;
				picker.SetDate (date, false);
				update_label();
			};

			picker.ValueChanged += (sender, e) => {
				date = picker.Date;
				update_label();
			};
			picker.TintColor = AppDelegate.app.white_text_color;
			picker.SetDate (date, false);
			update_label();
			// Perform any additional setup after loading the view, typically from a nib.
		}
		public void update_label()
		{
			btnClearDate.Hidden = nodate_title == null;
			btnTodayDate.Hidden = !btnClearDate.Hidden;

			labelValue.Text = AppDelegate.app.date_week_formatter.ToString (date);
		}
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			update_label ();
		}
	}
}

