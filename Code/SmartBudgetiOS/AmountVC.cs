using System;
using System.Drawing;
using Foundation;
using UIKit;
using CoreGraphics;
using SmartBudgetCommon;
using System.Collections.Generic;

namespace SmartBudgetiOS
{
	public partial class AmountVC : UIViewController
	{
		private AmountEnter amount_enter = new AmountEnter();
		private Sum sum;
		private Sum other_sum_for_convert;
		private int allowed_sign;
		private Action<AmountVC, Sum> save_action;
		private UIButton btnHelp;
		private Document doc;
		private UIView our_keyboard;
		private UIButton[] btnDigits;
		private UIView panelButtons;
		private UIButton btnPlusMinus;
		private UIButton btnCurrency;
		private UIButton btnDelete;
		private UIButton btnComma;
		private UIButton btnAccount;

		private TimerForButton currency_hold_timer;
		private TimerForButton account_hold_timer;
		private TimerForButton delete_hold_timer;

		UIBarButtonItem next_button;
		UIBarButtonItem cancel_button;
		UIBarButtonItem save_button;

		enum CalcOpEnum { NONE, PLUS, MINUS, MULTIPLY, DIVIDE };
		private List<long> calc_amounts_1000 = new List<long>();
		private List<CalcOpEnum> calc_ops = new List<CalcOpEnum>();
		private UIView panel_calc;
		private UIButton btnCalcPlus;
		private UIButton btnCalcMinus;
		private UIButton btnCalcMultiply;
		private UIButton btnCalcDivide;
		private UIButton btnCalcEq;

		public AmountVC () : base ("AmountVC", null)
		{
			NavigationItem.Title = i18n.get ("AmountTitle");

			next_button = new UIBarButtonItem(i18n.get ("Next"), UIBarButtonItemStyle.Plain, (sender, e) => {
				this.save_amount();
				this.save_action.Invoke(this, this.sum);
			});
			save_button = new UIBarButtonItem (UIBarButtonSystemItem.Save, (sender, e) => {
				this.save_amount();
				this.save_action.Invoke(this, this.sum);
				Utility.dismiss_or_pop(this.NavigationController, true);
			});
			cancel_button = new UIBarButtonItem (UIBarButtonSystemItem.Cancel, (sender, e) => {
				Utility.dismiss_or_pop(this.NavigationController, true);
			});
		}
		void construct(Document doc, Sum initial_sum, Sum other_sum_for_convert, int allowed_sign, bool in_chain, Action<AmountVC, Sum> save_action)
		{
			this.doc = doc;
			this.sum = initial_sum;
			this.other_sum_for_convert = other_sum_for_convert;
			this.allowed_sign = allowed_sign;
			this.save_action = save_action;
			CurrencyFormat our_currency = CurrencyFormat.get_currency (sum.currency);
			amount_enter.maximum_precision = our_currency.get_maximum_precision ();
			amount_enter.float_behavior = our_currency.float_behavior ();
			amount_enter.from_amount (DBExpense.to_decimal(sum.amount_1000), allowed_sign);
			NavigationItem.RightBarButtonItem = in_chain ? next_button : save_button;
			NavigationItem.LeftBarButtonItem = in_chain ? (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? cancel_button : null) : cancel_button;
		}
		~AmountVC()
		{
			Console.WriteLine ("~AmountVC");
		}
		private static Utility.ReuseVC<AmountVC> reuse = new Utility.ReuseVC<AmountVC> ();
		public static AmountVC create_or_reuse(Document doc, Sum initial_sum, Sum other_sum_for_convert, int allowed_sign, bool in_chain, Action<AmountVC, Sum> save_action)
		{
			AmountVC result = reuse.create_or_reuse();
			result.construct (doc, initial_sum, other_sum_for_convert, allowed_sign, in_chain, save_action);
			return result;
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		public static UIImage number_dark = UIImage.FromBundle ("new_design/number_dark.png").StretchableImage(10, 10);
		public static UIImage number_light = UIImage.FromBundle ("new_design/number_light.png").StretchableImage(10, 10);
		public static UIImage number_delete = UIImage.FromBundle("new_design/number_delete.png");
		public static UIFont number_font = UIFont.FromName ("HelveticaNeue-Light", 32);
		public static UIButton setup_digit_button(UIView vv, int column, int row, bool inverted)
		{
			UIButton result = new UIButton (UIButtonType.Custom);
			CGRect rr = vv.Bounds;
			int pos_x = ((int)vv.Bounds.Width + 2) * column / 3;
			int pos_x_2 = ((int)vv.Bounds.Width + 2) * (column + 1) / 3;
			int pos_y = ((int)vv.Bounds.Height) * row / 4;
			int pos_y_2 = ((int)vv.Bounds.Height) * (row + 1) / 4;
			result.Frame = new RectangleF (pos_x - 1, pos_y, pos_x_2 - pos_x + 1, pos_y_2 - pos_y);
			result.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin | UIViewAutoresizing.FlexibleRightMargin;
			result.TitleLabel.Font = number_font;
			UIControlState dark = inverted ? UIControlState.Highlighted : UIControlState.Normal;
			UIControlState light = inverted ? UIControlState.Normal : UIControlState.Highlighted;
			result.SetTitleColor (AppDelegate.app.gray_text_color, UIControlState.Normal);
			result.SetTitleColor (AppDelegate.app.gray_text_color, UIControlState.Highlighted);
			result.SetBackgroundImage(number_dark, dark);
			result.SetBackgroundImage(number_light, light);
			vv.Add (result);
			return result;
		}
		public static UIImage calc_normal = UIImage.FromBundle ("new_design/calc_button.png").StretchableImage(14, 14);
		public static UIImage calc_high = UIImage.FromBundle ("new_design/calc_button_select.png").StretchableImage(14, 14);
		const float CALC_BUTTON_HEIGHT = 44;
		public static UIButton create_calc_button(UIView parent, string tit)
		{
			UIButton result = new UIButton (UIButtonType.Custom);
			result.Frame = new RectangleF (0, 0, 64, CALC_BUTTON_HEIGHT);
			result.SetTitleColor (AppDelegate.app.neutral_color, UIControlState.Normal);
			result.SetTitle (tit, UIControlState.Normal);
			result.SetBackgroundImage(calc_normal, UIControlState.Normal);
			result.SetBackgroundImage(calc_high, UIControlState.Highlighted);
			parent.Add (result);
			return result;
		}
		public static UIButton[] setup_digits(UIView vv)
		{
			UIButton[] btnDigits = new UIButton[10];
			NSNumberFormatter nf = new NSNumberFormatter ();
			for (int i = 0; i != 10; ++i) {
				if( i == 0 )
					btnDigits [i] = setup_digit_button (vv, 1, 3, false);
				else
					btnDigits [i] = setup_digit_button (vv, (i - 1) % 3, (i - 1) / 3, false);
				btnDigits[i].SetTitle (nf.StringFromNumber(NSNumber.FromInt32(i)), UIControlState.Normal);
			}
			return btnDigits;
		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = AppDelegate.app.dark_background_color;
//			float keyboard_width = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? 420 : 320;
//			float keyboard_height = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? number_dark.Size.Height * 5 : number_dark.Size.Height * 4;
			nfloat keyboard_width = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? 320 : 320;
			nfloat keyboard_height = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? number_dark.Size.Height * 4 : number_dark.Size.Height * 4;
			our_keyboard = new UIView (new CGRect((View.Bounds.Width - keyboard_width)/2, View.Bounds.Height - keyboard_height, keyboard_width, keyboard_height));
			our_keyboard.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin | UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin;
			our_keyboard.BackgroundColor = UIColor.Clear;
			View.Add (our_keyboard);

			btnDigits = setup_digits (our_keyboard);
			for (int i = 0; i != 10; ++i) {
				int num = i; // bind delegate to this variable. not to shared i :)
				btnDigits[i].TouchUpInside += (sender, e) => {
					amount_enter.number_click(num);
					update_view();
				};
			}
			btnDelete = setup_digit_button (our_keyboard, 2, 3, true);
			btnDelete.SetImage(number_delete, UIControlState.Normal);
			btnComma = setup_digit_button (our_keyboard, 0, 3, true);
			btnComma.SetTitle(",", UIControlState.Normal);

			UIImageView under_comma = new UIImageView (btnComma.Frame);
			under_comma.AutoresizingMask = btnComma.AutoresizingMask;
			under_comma.Image = btnComma.BackgroundImageForState(UIControlState.Normal);
			our_keyboard.InsertSubviewBelow (under_comma, btnComma);

			nfloat limit = our_keyboard.Frame.Y;
			//float button_height = btnDigits [1].Frame.Height;
			AppDelegate.create_table_background(View, 0, limit - BottomPanelView.PANEL_HEIGHT, UIViewAutoresizing.FlexibleHeight);
			AppDelegate.create_table_cover(View, btnDigits [1], limit - BottomPanelView.PANEL_HEIGHT);

			{
				CGRect rr = viewConversionHint.Frame;
				rr.Y = limit - BottomPanelView.PANEL_HEIGHT - rr.Height;
				viewConversionHint.Frame = rr;
			}

			panelButtons = BottomPanelView.create_bottom_panel (View, limit - BottomPanelView.PANEL_HEIGHT, BottomPanelView.PANEL_HEIGHT);

			btnHelp = BottomPanelView.create_help_button( panelButtons, "help");
			btnPlusMinus = BottomPanelView.create_bottom_button( panelButtons, "plus_minus");
			btnAccount = BottomPanelView.create_bottom_button( panelButtons, "account");
			btnCurrency = BottomPanelView.create_bottom_button( panelButtons, "currency");

			panel_calc = BottomPanelView.create_bottom_panel (View, limit - BottomPanelView.PANEL_HEIGHT - CALC_BUTTON_HEIGHT, CALC_BUTTON_HEIGHT);
			btnCalcPlus = create_calc_button( panel_calc, "+");
			btnCalcMinus = create_calc_button( panel_calc, "-");
			btnCalcMultiply = create_calc_button( panel_calc, "×");
			btnCalcDivide = create_calc_button( panel_calc, "÷");
			btnCalcEq = create_calc_button( panel_calc, "=");
			panel_calc.Hidden = true;

			btnHelp.TouchUpInside += (sender, e) => {
				LayoutForHelp lh = new LayoutForHelp(NavigationController, this.our_keyboard.Frame.Y + this.btnDigits[8].Frame.Y);
				// From bottom, special case above keyboard
				UIView taphold_view = lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, btnDelete, 0, i18n.get ("HelpTapHold"), LayoutForHelp.BubleType.BUTTON, false);
				if( btnAccount.Enabled )
					lh.create_tail(btnAccount, 0, taphold_view);
				lh.create_tail(btnCurrency, 0, taphold_view);
				// From bottom
				lh.last_pos_from_bottom = lh.view_y_to_dark_view_y(btnPlusMinus, 0);
				if( btnPlusMinus.Enabled )
					lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, btnPlusMinus, 0, i18n.get ("HelpPlusMinus"), LayoutForHelp.BubleType.BUTTON, false);
				if( !viewConversionHint.Hidden ) {
					DateTime date;
					bool up = AppDelegate.app.docs.exchange_rate_update_on(out date);
					lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, viewConversionHint, 0, i18n.get ("HelpExchangeRate").Replace("{hours_ago}", up ? ((long)Math.Floor(DateTime.UtcNow.Subtract(date).TotalHours)).ToString() : i18n.get ("ConversionNA")), LayoutForHelp.BubleType.BUTTON, false); // Culture ok
				}
				lh.show ();
			};
			btnComma.TouchUpInside += (sender, e) => {
				amount_enter.comma_click();
				update_view();
			};
			btnPlusMinus.TouchUpInside += (sender, e) => {
				amount_enter.minus_click();
				update_view();
			};
			currency_hold_timer = new TimerForButton (btnCurrency, delegate {
				sum.currency = AppDelegate.app.docs.next_selected_currency (sum.currency, other_sum_for_convert.currency);
				CurrencyFormat our_currency = CurrencyFormat.get_currency (sum.currency);
				amount_enter.maximum_precision = our_currency.get_maximum_precision ();
				amount_enter.float_behavior = our_currency.float_behavior ();
				update_view ();
			}, delegate {
				//btnCurrency.CancelTracking (null);
				NavigationController.PushViewController (CurrencySelect.create_or_reuse(sum.currency, (cs,cur)=>{
					sum.currency = cur;
					CurrencyFormat our_currency = CurrencyFormat.get_currency (sum.currency);
					amount_enter.maximum_precision = our_currency.get_maximum_precision ();
					amount_enter.float_behavior = our_currency.float_behavior ();
					update_view();
				}), true);
			});
			account_hold_timer = new TimerForButton (btnAccount, delegate {
				sum.account = doc.next_selected_account(sum.account);
				update_view();
			}, delegate {
				//btnAccount.CancelTracking(null);
				NavigationController.PushViewController(AccountSelect.create_or_reuse( doc, i18n.get ("AccountTitle"), sum.account, "-1", false, (acs,acc)=>{
					sum.account = acc;
					update_view();
				}), true);
			});
			delete_hold_timer = new TimerForButton (btnDelete, delegate {
				if( !amount_enter.del() ) // nothing to delete, delete last operation from calc
				{
					if (calc_ops.Count != 0) {
						int pos = calc_ops.Count - 1;
						amount_enter.from_amount (DBExpense.to_decimal(calc_amounts_1000[pos]), allowed_sign);
						calc_ops.RemoveAt (pos);
						calc_amounts_1000.RemoveAt (pos);
					}
				}
				update_view();
			}, delegate {
				amount_enter.clear();
				calc_ops.Clear();
				calc_amounts_1000.Clear();
				update_view();
			});

			btnCalcPlus.TouchUpInside += (sender, e) => {
				calc_add_operation(CalcOpEnum.PLUS);
				update_view();
			};
			btnCalcMinus.TouchUpInside += (sender, e) => {
				calc_add_operation(CalcOpEnum.MINUS);
				update_view();
			};
			btnCalcMultiply.TouchUpInside += (sender, e) => {
				calc_add_operation(CalcOpEnum.MULTIPLY);
				update_view();
			};
			btnCalcDivide.TouchUpInside += (sender, e) => {
				calc_add_operation(CalcOpEnum.DIVIDE);
				update_view();
			};
			btnCalcEq.TouchUpInside += (sender, e) => {
				calc_add_operation(CalcOpEnum.NONE);
				update_view();
			};

			Utility.fix_rtl_label (labelAmount);
			Utility.fix_rtl_label (labelCalc);
			Utility.fix_rtl_label (labelAccountName);
			Utility.fix_rtl_label (labelAccountValue);
			Utility.fix_rtl_label (labelConversionHint);
			Utility.fix_rtl_view (viewConversionHint);
			Utility.fix_rtl_view (imgSeparator);
		}
		// 1. (e)
		// 2. a +- (e)           = -> 1 +- -> 2 */ -> 4
		// 3. a */ (e)           anything -> fold all
		// 4. a +- b */ (e)      = -> fold all, +- -> fold all, */ -> fold */ only
		private void calc_add_operation(CalcOpEnum op)
		{
			bool is_negative;
			long am_1000 = DBExpense.from_decimal( amount_enter.to_amount (out is_negative) );

			bool weak = op == CalcOpEnum.PLUS || op == CalcOpEnum.MINUS || op == CalcOpEnum.NONE;
			while (calc_ops.Count != 0) {
				int pos = calc_ops.Count - 1;
				CalcOpEnum op2 = calc_ops [pos];
				bool weak2 = op2 == CalcOpEnum.PLUS || op2 == CalcOpEnum.MINUS;
				long am2 = calc_amounts_1000[pos];
				if (weak || !weak2) {
					long result = 0;
					switch (op2) {
					case CalcOpEnum.PLUS:
						result = am2 + am_1000;
						break;
					case CalcOpEnum.MINUS:
						result = am2 - am_1000;
						break;
					case CalcOpEnum.MULTIPLY:
						result = am2 * am_1000 / 1000;
						break;
					case CalcOpEnum.DIVIDE:
						result = am_1000 == 0 ? 0 : (am2 * 1000 / am_1000);
						break;
					}
					calc_ops.RemoveAt (pos);
					calc_amounts_1000.RemoveAt (pos);
					am_1000 = result;
					continue;
				}
				break;
			}
			if (op == CalcOpEnum.NONE) {
				amount_enter.from_amount (DBExpense.to_decimal(am_1000), allowed_sign);
				return;
			}
			amount_enter.clear();
			amount_enter.negative = !weak ? false : am_1000 <= 0;
			calc_amounts_1000.Add( am_1000 );
			calc_ops.Add( op );
		}
		public void save_amount()
		{
			amount_enter.truncate_value ();
			bool is_negative;
			sum.amount_1000 = DBExpense.from_decimal( amount_enter.to_amount (out is_negative) );
		}
		public void update_view()
		{
			CurrencyFormat our_currency = CurrencyFormat.get_currency (sum.currency);
			// TODO - ensure first currency is not selected if second_sum
			bool is_negative;
			long am_1000 = DBExpense.from_decimal( amount_enter.to_amount (out is_negative) );
			labelAmount.Text = our_currency.format_amount_for_enter (am_1000, amount_enter.value_dot, amount_enter.get_value_str_ac().Length);
			// We use explicit colors to solve 0 problem
			labelAmount.TextColor = is_negative ? AppDelegate.app.negative_color : AppDelegate.app.positive_color;
			if (!String.IsNullOrEmpty(other_sum_for_convert.currency)) {
				long cam_1000 = 0;
				long other_am_1000 = Math.Abs(other_sum_for_convert.amount_1000);
				if (AppDelegate.app.docs.convert_currency (ref cam_1000, sum.currency, other_am_1000, other_sum_for_convert.currency)) {
					CurrencyFormat other_currency = CurrencyFormat.get_currency (other_sum_for_convert.currency);
					string src = other_currency.format_approximate_amount (other_am_1000);
					string dst = our_currency.format_approximate_amount (cam_1000);
					labelConversionHint.Text = i18n.ReplaceFirst( i18n.ReplaceFirst( i18n.get ("AmountConverted"), "%@", src), "%@", dst);
				} else {
					labelConversionHint.Text = i18n.get ("AmountNotConverted");
				}
				viewConversionHint.Hidden = false;
			} else {
				labelConversionHint.Text = "";
				viewConversionHint.Hidden = true;
			}
			DBAccount acc = doc.get_account (sum.account);
			if (acc != null) {
				labelAccountName.Text = acc.name;
				List<DBAccountBalance> ab = doc.get_balance (sum.account);
				long aam_1000 = DBAccountBalance.find_balance_1000 (ab, sum.currency);
				labelAccountValue.Text = our_currency.format_amount_precise (aam_1000);
				labelAccountValue.TextColor = AppDelegate.app.color_for_amount (aam_1000);
				NameValueCell.align_name_label_to_value_label (labelAccountName, labelAccountValue);
			}
			btnAccount.Enabled = (doc.sorted_accounts.Count > 1);
			btnPlusMinus.Enabled = (allowed_sign == 0);
			btnComma.Hidden = !(our_currency.get_maximum_precision () > 0);
			NavigationItem.RightBarButtonItem.Enabled = am_1000 != 0;

			CalcOpEnum op = calc_ops.Count != 0 ? calc_ops[calc_ops.Count - 1] : CalcOpEnum.NONE;

			//AppDelegate.decorate_bottom_button (btnCalcPlus, "plus_minus", op == CalcOpEnum.PLUS);
			//AppDelegate.decorate_bottom_button (btnCalcMinus, "plus_minus", op == CalcOpEnum.MINUS);
			//AppDelegate.decorate_bottom_button (btnCalcMultiply, "plus_minus", op == CalcOpEnum.MULTIPLY);
			//AppDelegate.decorate_bottom_button (btnCalcDivide, "plus_minus", op == CalcOpEnum.DIVIDE);
			string calc_str = "";
			for (int i = 0; i != calc_ops.Count; ++i) {
				string val = our_currency.format_approximate_amount (calc_amounts_1000 [i]);
				switch (calc_ops[i]) {
					case CalcOpEnum.PLUS:
					calc_str += val + " + ";
					break;
					case CalcOpEnum.MINUS:
					calc_str += val + " - ";
					break;
					case CalcOpEnum.MULTIPLY:
					calc_str += val + " × ";
					break;
					case CalcOpEnum.DIVIDE:
					calc_str += val + " ÷ ";
					break;
				}
			}
			labelCalc.Text = calc_str;
			NameValueCell.align_name_label_to_value_label (labelCalc, labelAmount);
		}
		public override void ViewWillAppear (bool animated)
		{
			Console.WriteLine ("AmountVC.ViewWillAppear");
			base.ViewWillAppear (animated);
			update_view ();
		}
		public override void ViewWillDisappear (bool animated)
		{
			Console.WriteLine ("AmountVC.ViewWillDisappear");
			base.ViewWillDisappear (animated);
		}
		/*public override void ViewDidDisappear (bool animated)
		{
			Console.WriteLine ("AmountVC.ViewDidDisappear");
			base.ViewDidDisappear (animated);
		}*/
		public override void ViewWillLayoutSubviews ()
		{
			base.ViewWillLayoutSubviews ();
			BottomPanelView.layout (panelButtons, btnHelp);
			BottomPanelView.layout (panel_calc, null);
		}
	}
}

