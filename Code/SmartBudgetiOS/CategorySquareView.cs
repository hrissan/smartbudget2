using System;
using System.Drawing;
using Foundation;
using UIKit;
using SmartBudgetCommon;

namespace SmartBudgetiOS
{
	public partial class CategorySquareView : UIView
	{
		public static readonly UINib Nib = UINib.FromName ("CategorySquareView", NSBundle.MainBundle);
		public string cat_id;
//		private Action<int> cat_click;
//		private Action<int> cat_hold;
		public CategorySquareView (IntPtr handle) : base (handle)
		{
		}
		~CategorySquareView()
		{
			Console.WriteLine ("~CategorySquareView");
		}
		public void set_category(DBCategory category, bool selected)
		{
			if (category == null) {
				cat_id = "";
				Hidden = true;
				return;
			}
			Hidden = false;
			cat_id = category.id;
			labelCategory.Text = category.get_loc_name ();
			imgCategory.Image = AppDelegate.app.get_category_image(category);
			imgSelected.Hidden = !selected;
		}
		public void set_selection(bool selected)
		{
			imgSelected.Hidden = !selected;
		}
		private TimerForButton hold_timer;
		public static CategorySquareView Create (UITableView tv, Action<string> cat_click, Action<string, UIView> cat_hold)
		{
			CategorySquareView v = (CategorySquareView)Nib.Instantiate (null, null) [0];
			v.hold_timer = new TimerForButton (v.btnSelect, delegate{
				cat_click.Invoke(v.cat_id);
			}, delegate{
				cat_hold.Invoke(v.cat_id, v);
			});
			v.BackgroundColor = tv.BackgroundColor;
			return v;
		}
	}
}

