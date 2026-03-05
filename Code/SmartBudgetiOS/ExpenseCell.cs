using System;
using System.Drawing;
using Foundation;
using UIKit;
using SmartBudgetCommon;
using CoreGraphics;
using System.Collections.Generic;

namespace SmartBudgetiOS
{
	public partial class ExpenseCell : UITableViewCell
	{
		public static readonly UINib Nib = UINib.FromName ("ExpenseCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("ExpenseCell");
		public UIView separator;
		private CGRect initial_frame;
		private CGRect progress_frame;
		private float progress;
		public ExpenseCell (IntPtr handle) : base (handle)
		{
		}
		~ExpenseCell()
		{
			Console.WriteLine ("~ExpenseCell");
		}
		public void setFakeExpense(DBExpense ex, Document doc)
		{
			labelName.Text = ex.id;
			labelValue.Text = "-$500";
			labelName.TextColor = AppDelegate.app.neutral_color;
			labelValue.TextColor = AppDelegate.app.negative_color;
			NameValueCell.align_name_label_to_value_label (labelName, labelValue);
		}
		public void setExpense(DBExpense ex, Document doc, List<string> selected_accounts = null)
		{
			DBCategory cat = doc.get_category (ex.category);
			CurrencyFormat cf = CurrencyFormat.get_currency (ex.sum.currency);
			imgCategory.Image = AppDelegate.app.get_category_image(cat);
			setIndicator (ex.planned, Documents.planned_soon (ex.date));
			if (ex.sum2.IsValid ()) {
				bool first = (selected_accounts == null || selected_accounts.IndexOf (ex.sum.account) != -1);
				bool second = (selected_accounts == null || selected_accounts.IndexOf (ex.sum2.account) != -1);

				if (ex.category == Document.TRANSFER_CATEGORY) {
					DBAccount acc = doc.get_account (ex.sum.account);
					DBAccount acc2 = doc.get_account (ex.sum2.account);
					if(ex.sum.amount_1000 > 0 ){
						DBAccount tmp = acc; acc = acc2; acc2 = tmp;
						bool tmpb = first; first = second; second = tmpb;
					}
					if (String.IsNullOrEmpty (ex.name))
						labelName.Text = String.Format ("{0}->{1}", acc.name, acc2.name);
					else
						labelName.Text = ex.name;
					labelValue.Text = cf.format_amount_precise (Math.Abs(ex.sum.amount_1000));
					labelName.TextColor = AppDelegate.app.neutral_color;
					labelValue.TextColor = first == second ? AppDelegate.app.neutral_color : first ? AppDelegate.app.negative_color : AppDelegate.app.positive_color;
				} else { // convert
					CurrencyFormat cf2 = CurrencyFormat.get_currency (ex.sum2.currency);
					labelName.Text = cf.format_amount_precise (ex.sum.amount_1000);
					labelValue.Text = cf2.format_amount_precise (ex.sum2.amount_1000);
					labelName.TextColor = first ? AppDelegate.app.color_for_amount (ex.sum.amount_1000) : AppDelegate.app.neutral_color;
					labelValue.TextColor = second ? AppDelegate.app.color_for_amount (ex.sum2.amount_1000) : AppDelegate.app.neutral_color;
				}
			} else {
				if (String.IsNullOrEmpty (ex.name))
					labelName.Text = cat.get_loc_name();
				else
					labelName.Text = ex.name;
				labelValue.Text = cf.format_amount_precise (ex.sum.amount_1000);
				labelName.TextColor = AppDelegate.app.neutral_color;
				labelValue.TextColor = AppDelegate.app.color_for_amount (ex.sum.amount_1000);
			}
			//labelName.Alpha = ex.planned ? 0.5f : 1f;
			//labelValue.Alpha = ex.planned ? 0.5f : 1f;
			NameValueCell.align_name_label_to_value_label (labelName, labelValue);
		}
		public void setImageNameValue(UIImage img, string name, string value, bool negative)
		{
			imgCategory.Image = img;
			labelName.Text = name;
			labelName.TextColor = AppDelegate.app.neutral_color;
			labelValue.Text = value;
			labelValue.TextColor = negative ? AppDelegate.app.negative_color : AppDelegate.app.positive_color;
			NameValueCell.align_name_label_to_value_label (labelName, labelValue);
		}
		public void setIndicator(bool planned, bool soon)
		{
			imgIndicator.Hidden = !(planned);
			if (planned)
				imgIndicator.Image = soon ? AppDelegate.get_attention_icon () : AppDelegate.get_attention_icon_n ();
		}
		public void setProgress(float progress, bool negative)
		{
			this.progress = progress;
			if (progress != 0) {
				layout_progress ();
				viewProgress.Image = negative ? AppDelegate.progress_red() : AppDelegate.progress_green();
				viewProgress.Hidden = false;
			}else
				viewProgress.Hidden = true;
		}
		public void layout_progress()
		{
			CGRect r = progress_frame;
			nfloat delta = ContentView.Bounds.Width - initial_frame.Width;
			r.Width = (delta + progress_frame.Width) * progress;
			if (Utility.RTL)
				r.X += (delta + progress_frame.Width) - r.Width;
			viewProgress.Frame = r;
		}
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			if (progress != 0)
				layout_progress ();
		}

		public static ExpenseCell Create (UITableView tv)
		{
			ExpenseCell cell = (ExpenseCell)Nib.Instantiate (null, null) [0];
			cell.BackgroundColor = tv.BackgroundColor;
			cell.SelectedBackgroundView = new UIImageView (AppDelegate.selected_cell);
			//cell.imgIndicator.Image = AppDelegate.get_attention_icon();
			cell.separator = NameValueCell.add_separator (cell);
			cell.initial_frame = cell.ContentView.Bounds;
			cell.labelName.BackgroundColor = tv.BackgroundColor;
			cell.labelValue.BackgroundColor = tv.BackgroundColor;
			Utility.fix_rtl_label (cell.labelName);
			Utility.fix_rtl_label (cell.labelValue);
			Utility.fix_rtl_view (cell.imgCategory);
			Utility.fix_rtl_view (cell.imgIndicator);
			Utility.fix_rtl_view (cell.viewProgress);
			cell.progress_frame = cell.viewProgress.Frame;
			//cell.imgIndicator.Opaque = true;
			//cell.imgIndicator.BackgroundColor = tv.BackgroundColor;
			return cell;
		}
	}
}

