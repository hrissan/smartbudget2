using System;
using System.Drawing;
using Foundation;
using CoreGraphics;
using UIKit;
using System.Collections.Generic;
using SmartBudgetCommon;

namespace SmartBudgetiOS
{
	public partial class DrawVC : UIViewController
	{
		private Action<DrawVC, UIImage> save_action;
		private List<UIImage> undo_queue = new List<UIImage>();
		private bool eraser;
		private int undo_queue_pos;
		UIView panelButtons;
		UIImageView output_image;
		DrawArea draw_area;
		UIButton btnHelp;
		UIButton btnUndo;
		UIButton btnRedo;
		UIButton btnMarker;
		UIButton btnEraser;

		public DrawVC () : base ("DrawVC", null)
		{
			NavigationItem.Title = i18n.get ("ImageTitle");
			NavigationItem.RightBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Save, (sender, e) => {
				this.save_action.Invoke (this, this.undo_queue[this.undo_queue_pos]);
				Utility.dismiss_or_pop(this.NavigationController, true);
				//this.draw_area.destroy_all();
				this.draw_area.RemoveFromSuperview();
				//this.draw_area.Dispose();
				this.draw_area = null;
			});
			NavigationItem.LeftBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Cancel, (sender, e) => {
				Utility.dismiss_or_pop(this.NavigationController, true);
				//this.draw_area.destroy_all();
				this.draw_area.RemoveFromSuperview();
				//this.draw_area.Dispose();
				this.draw_area = null;
			});
		}
		void construct(UIImage initial_image, Action<DrawVC, UIImage> save_action)
		{
			this.save_action = save_action;
			eraser = false;
			undo_queue_pos = 0;
			undo_queue.Clear ();
			undo_queue.Add(initial_image);
		}
		~DrawVC()
		{
			Console.WriteLine ("~DrawVC");
		}
		private static Utility.ReuseVC<DrawVC> reuse = new Utility.ReuseVC<DrawVC> ();
		public static DrawVC create_or_reuse(UIImage initial_image, Action<DrawVC, UIImage> save_action)
		{
			DrawVC result = reuse.create_or_reuse();
			result.construct(initial_image, save_action);
			return result;
		}
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			if (!IsViewLoaded) {
				draw_area = Utility.free_view (draw_area);
				undo_queue.Clear ();
			}
		}
		private void draw_area_changed(DrawArea sender, EventArgs e)
		{
			undo_queue_pos += 1;
			if( undo_queue.Count > undo_queue_pos )
				undo_queue.RemoveRange(undo_queue_pos, undo_queue.Count - undo_queue_pos);
			undo_queue.Add(output_image.Image);
			update();
		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = AppDelegate.app.dark_background_color;
			UIView back = AppDelegate.create_table_background(View, 0, View.Bounds.Height - AppDelegate.BUTTON_ROW_HEIGHT, UIViewAutoresizing.FlexibleHeight);
			AppDelegate.create_table_cover(View, back, View.Bounds.Height - AppDelegate.BUTTON_ROW_HEIGHT);

			CGRect rr = drawPlace.Bounds;
			rr.Inflate (-1, -1);
			output_image = new UIImageView (rr);
			drawPlace.AddSubview (output_image);
			draw_area = new DrawArea (rr, output_image);
			drawPlace.AddSubview (draw_area);

			panelButtons = BottomPanelView.create_bottom_panel (View);

			btnHelp = BottomPanelView.create_help_button( panelButtons, "help");
			btnUndo = BottomPanelView.create_bottom_button( panelButtons, "undo");
			btnMarker = BottomPanelView.create_bottom_button( panelButtons, "marker");
			btnEraser = BottomPanelView.create_bottom_button( panelButtons, "eraser");
			btnRedo = BottomPanelView.create_bottom_button( panelButtons, "redo");

/*			WeakReference weak_this = new WeakReference (this);

			drawPlace.UserInteractionEnabled = true;

			UIPanGestureRecognizer pan_recognizer = new UIPanGestureRecognizer( (r)=>{
				Console.WriteLine("pan_recognizer {0} {1}", r.State, r.LocationInView(r.View));
				//if( r.State == UIGestureRecognizerState.Began ) {
				//	var strong_cell = weak_cell.Target as SimpleCell;
				//	if( strong_cell != null)
				//		strong_cell.taphold_action.Invoke(strong_cell);
				//}
			});
			drawPlace.AddGestureRecognizer (pan_recognizer);
			UITapGestureRecognizer tap_recognizer = new UITapGestureRecognizer( (r)=>{
				Console.WriteLine("tap_recognizer {0} {1}", r.State, r.LocationInView(r.View));
				//if( r.State == UIGestureRecognizerState.Began ) {
				//	var strong_cell = weak_cell.Target as SimpleCell;
				//	if( strong_cell != null)
				//		strong_cell.taphold_action.Invoke(strong_cell);
				//}
			});
			drawPlace.AddGestureRecognizer (tap_recognizer);*/

			btnHelp.TouchUpInside += (sender, e) => {
				LayoutForHelp lh = new LayoutForHelp(this.NavigationController, this.View.Bounds.Height - AppDelegate.BUTTON_ROW_HEIGHT);

				// From bottom
				lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, this.btnEraser, 0, i18n.get ("HelpEraser"), LayoutForHelp.BubleType.BUTTON);
				lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, this.btnMarker, 0, i18n.get ("HelpMarker"), LayoutForHelp.BubleType.BUTTON);
				lh.show ();
			};

			btnUndo.TouchUpInside += (sender, e) => {
				if( this.undo_queue_pos == 0)
					return;
				this.undo_queue_pos -= 1;
				this.draw_area.revertToImage ( this.undo_queue[this.undo_queue_pos] );
				this.update ();
			};
			btnRedo.TouchUpInside += (sender, e) => {
				if( this.undo_queue_pos >= this.undo_queue.Count - 1)
					return;
				this.undo_queue_pos += 1;
				this.draw_area.revertToImage ( this.undo_queue[this.undo_queue_pos] );
				this.update ();
			};
			btnMarker.TouchUpInside += (sender, e) => {
				this.eraser = false;
				this.update ();
			};
			btnEraser.TouchUpInside += (sender, e) => {
				this.eraser = true;
				this.update ();
			};
/*			draw_area.changed += (sender, e) => {
				DrawVC strong_this = weak_this.Target as DrawVC;
				if( strong_this == null )
					return;
				strong_this.draw_area_changed();
			};*/
			imgResult.Hidden = true; // Will live without it
		}
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			draw_area.changed += draw_area_changed;
			draw_area.revertToImage ( undo_queue[undo_queue_pos] );
			update ();
		}
		public override void ViewWillDisappear (bool animated)
		{
			if( draw_area != null )
				draw_area.changed -= draw_area_changed;
			base.ViewWillDisappear (animated);
		}
		public override void ViewWillLayoutSubviews ()
		{
			base.ViewWillLayoutSubviews ();
			BottomPanelView.layout (panelButtons, btnHelp);
		}
		void update()
		{
			btnUndo.Enabled = undo_queue_pos != 0;
			btnRedo.Enabled = undo_queue_pos < undo_queue.Count - 1;
			//UIImage hi = UIImage.FromBundle ("b4_backgound_high.png");
			//btnMarker.SetBackgroundImage(!eraser ? hi : null, UIControlState.Normal);
			//btnEraser.SetBackgroundImage(eraser ? hi : null, UIControlState.Normal);
			AppDelegate.decorate_bottom_button (btnMarker, "marker", !eraser);
			AppDelegate.decorate_bottom_button (btnEraser, "eraser", eraser);
			draw_area.brush_index = eraser ? 1 : 0;

			imgResult.Image = output_image.Image;
		}
	}
}