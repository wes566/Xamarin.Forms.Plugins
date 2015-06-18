﻿using System;
using Xamarin.Forms.Platform.iOS;
using Foundation;
using UIKit;
using Xamarin.Forms;
using CoreGraphics;
using KeyboardOverlap.Forms.Plugin.iOSUnified;
using Xamarin.Forms.Xaml;

[assembly: ExportRenderer (typeof(Page), typeof(KeyboardOverlapPageRenderer))]
namespace KeyboardOverlap.Forms.Plugin.iOSUnified
{
	public class KeyboardOverlapPageRenderer : PageRenderer
	{
		NSObject _keyboardShowObserver;
		NSObject _keyboardHideObserver;
		private bool _pageWasShiftedUp;
		private double _activeViewBottomEdge;

		public static void Init ()
		{
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			var page = Element as ContentPage;

			if (page != null) {
				var contentScrollView = page.Content as ScrollView;

				if (contentScrollView != null)
					return;

				RegisterForKeyboardNotifications ();
			}
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);

			UnregisterForKeyboardNotifications ();
		}

		void RegisterForKeyboardNotifications ()
		{
			if (_keyboardShowObserver == null)
				_keyboardShowObserver = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillShowNotification, OnKeyboardShow);
			if (_keyboardHideObserver == null)
				_keyboardHideObserver = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillHideNotification, OnKeyboardHide);
		}

		void UnregisterForKeyboardNotifications ()
		{
			if (_keyboardShowObserver != null) {
				NSNotificationCenter.DefaultCenter.RemoveObserver (_keyboardShowObserver);
				_keyboardShowObserver.Dispose ();
				_keyboardShowObserver = null;
			}

			if (_keyboardHideObserver != null) {
				NSNotificationCenter.DefaultCenter.RemoveObserver (_keyboardHideObserver);
				_keyboardHideObserver.Dispose ();
				_keyboardHideObserver = null;
			}
		}

		protected virtual void OnKeyboardShow (NSNotification notification)
		{
			if (!IsViewLoaded)
				return;
			
			var activeView = View.FindFirstResponder ();

			if (activeView == null)
				return;

			var keyboardFrame = UIKeyboard.FrameEndFromNotification (notification);
			var isOverlapping = activeView.IsKeyboardOverlapping (View, keyboardFrame);

			if (!isOverlapping)
				return;

			if (isOverlapping) {
				
				_activeViewBottomEdge = activeView.GetViewBottomEdgeY ();
				ShiftPageUp (keyboardFrame.Height, _activeViewBottomEdge);
			}
		}

		private void OnKeyboardHide (NSNotification notification)
		{
			if (!IsViewLoaded)
				return;

			var keyboardFrame = UIKeyboard.FrameBeginFromNotification (notification);

			if (_pageWasShiftedUp) {
				ShiftPageDown (keyboardFrame.Height, _activeViewBottomEdge);
			}
		}

		private void ShiftPageUp (nfloat keyboardHeight, double activeViewBottomEdge)
		{
			var pageFrame = Element.Bounds;

			var newY = pageFrame.Y + CalculateShiftByAmount (pageFrame.Height, keyboardHeight, activeViewBottomEdge);

			Element.LayoutTo (new Rectangle (pageFrame.X, newY,
				pageFrame.Width, pageFrame.Height));

			_pageWasShiftedUp = true;
		}

		private void ShiftPageDown (nfloat keyboardHeight, double activeViewBottomEdge)
		{
			var pageFrame = Element.Bounds;

			var newY = pageFrame.Y - CalculateShiftByAmount (pageFrame.Height, keyboardHeight, activeViewBottomEdge);

			Element.LayoutTo (new Rectangle (pageFrame.X, newY,
				pageFrame.Width, pageFrame.Height));

			_pageWasShiftedUp = false;
		}

		private double CalculateShiftByAmount (double pageHeight, nfloat keyboardHeight, double activeViewBottomEdge)
		{
			return (pageHeight - activeViewBottomEdge) - keyboardHeight;
		}
	}
}