﻿using System;
using System.Reflection;
using ExtendedCells.Forms.Plugin.Abstractions;
using ExtendedMap.Forms.Plugin.Abstractions.Models;
using ExtendedMap.Forms.Plugin.Abstractions.Services;
using SVG.Forms.Plugin.Abstractions;
using Xamarin.Forms;

namespace ExtendedMap.Forms.Plugin.Abstractions
{
	public class CustomMapContentView : ContentView
	{
		public CustomMapContentView (ExtendedMap extendedMap)
		{
			_extendedMap = extendedMap;

			_mapGrid = new RelativeLayout {BindingContext = this};

			Content = _mapGrid;
		}

		private const uint COLLAPSE_ANIMATION_SPEED = 400;
		private const uint EXPAND_ANIMATION_SPEED = 400;
		private double _minimizedFooterY;
		private double _expandedFooterY;
		private double _pageHeight;
		//		private readonly Grid _mapGrid;
		private readonly RelativeLayout _mapGrid;
		private readonly ExtendedMap _extendedMap;
		private FooterMode _footerMode;

		public FooterMode FooterMode {
			get { return _footerMode; }
			set {
				_footerMode = value;

				switch (value) {
				case FooterMode.Expanded:
					ExpandFooter ();
					break;
				case FooterMode.Minimized:
					MinimizeFooter ();
					break;
				default:
					HideFooter ();
					break;
				}
			}
		}

		private View _mapGridFooterRow {
			get {
				var footerRow = _mapGrid.Children [1];

				return footerRow;
			}
		}

		protected override void OnSizeAllocated (double width, double height)
		{
			//If the pageSize values have not been set yet, set them
			if (Math.Abs (_pageHeight) < 0.001 && height > 0) {
				_pageHeight = Bounds.Height;
				const double collapsedMapHeight = 0.37;
				const double expandedMapHeight = 0.86;
				const double expandedFooterHeight = 0.63;

				_minimizedFooterY = _pageHeight * expandedMapHeight;
				_expandedFooterY = _pageHeight * collapsedMapHeight;

				_mapGrid.Children.Add (
					_extendedMap,
					Constraint.RelativeToParent ((parent) => {
						return (parent.Width * 0);
					}),
					Constraint.RelativeToParent ((parent) => {
						return (parent.Height * 0);
					}),
					Constraint.RelativeToParent ((parent) => {
						return (parent.Width * 1);
					}),
					Constraint.RelativeToParent ((parent) => {
						return (parent.Height * 1);
					})
				);

				_mapGrid.Children.Add (
					CreateFooter (),
					Constraint.RelativeToParent ((parent) => {
						return (parent.Width * 0);
					}),
					Constraint.RelativeToParent ((parent) => {
						return (parent.Height * expandedMapHeight);
					}),
					Constraint.RelativeToParent ((parent) => {
						return (parent.Width * 1);
					}),
					Constraint.RelativeToParent ((parent) => {
						return (parent.Height * 1);
					})
				);


				FooterMode = FooterMode.Hidden;
//				var footerHeight = height * expandedFooterHeight;

//				_mapGrid.RowDefinitions [0].Height = new GridLength (height * expandedMapHeight);
//				_mapGrid.RowDefinitions [1].Height = new GridLength (footerHeight);
//
//				//The rows need to be added in this order for win phone for the footer to display on top of the map
//				_mapGrid.Children.Add (_extendedMap, 0, 0);
//				_mapGrid.Children.Add (CreateFooter (), 0, 1);
//
//				Grid.SetRowSpan (_extendedMap, 2);
//
//				_mapGridFooterRow.GestureRecognizers.Add (new TapGestureRecognizer { Command = new Command (ToogleFooter) });
//
//				FooterMode = FooterMode.Hidden;
			}

			base.OnSizeAllocated (width, height);
		}
//
		private void ToogleFooter ()
		{
			FooterMode = FooterMode == FooterMode.Expanded ? FooterMode.Minimized : FooterMode.Expanded;
		}

		private void HideFooter ()
		{
			var footerOldBounds = _mapGridFooterRow.Bounds;
			var footerNewBounds = new Rectangle (footerOldBounds.X, _pageHeight, footerOldBounds.Width, footerOldBounds.Height);

			_mapGridFooterRow.LayoutTo (footerNewBounds, EXPAND_ANIMATION_SPEED, Easing.SinIn);
		}

		private void ExpandFooter ()
		{
			var footerOldBounds = _mapGridFooterRow.Bounds;
			var footerNewBounds = new Rectangle (footerOldBounds.X, _expandedFooterY, footerOldBounds.Width,
				                      footerOldBounds.Height);

			_mapGridFooterRow.LayoutTo (footerNewBounds, EXPAND_ANIMATION_SPEED, Easing.SinIn);

			_extendedMap.CameraFocusYOffset = 1000;
			_extendedMap.CenterOnPosition = _extendedMap.SelectedPin.Position;
		}

		private void MinimizeFooter ()
		{
			var footerOldBounds = _mapGridFooterRow.Bounds;
			var footerNewBounds = new Rectangle (footerOldBounds.X, _minimizedFooterY, footerOldBounds.Width,
				                      footerOldBounds.Height);

			_mapGridFooterRow.LayoutTo (footerNewBounds, COLLAPSE_ANIMATION_SPEED, Easing.SinIn);

			_extendedMap.CameraFocusYOffset = 500;
			_extendedMap.CenterOnPosition = _extendedMap.SelectedPin.Position;
		}

		#region UI Creation

		private ContentView CreateFooter ()
		{
		var footerMasterLayout = new RelativeLayout ();

		var placeNameLabel = new Label {
					Text = "Pin Label Shows Here",
					TextColor = Color.Black,
				};
	
		Device.OnPlatform (iOS: () => placeNameLabel.FontSize = 20,
			Android: () => placeNameLabel.FontSize = 20,
			WinPhone: () => placeNameLabel.FontSize = 24);

		placeNameLabel.BindingContext = _extendedMap;

		placeNameLabel.SetBinding<ExtendedMap> (Label.TextProperty, vm => vm.SelectedPin.Label);

		var addressLabel = new Label {
			Text = "Address Shows Here",
			TextColor = Color.Gray,
		};

		Device.OnPlatform (iOS: () => addressLabel.FontSize = 14,
			Android: () => addressLabel.FontSize = 14,
			WinPhone: () => addressLabel.FontSize = 18);

		addressLabel.BindingContext = _extendedMap;
		addressLabel.SetBinding<ExtendedMap> (Label.TextProperty, vm => vm.SelectedPin.Address);

			footerMasterLayout.Children.Add (
				placeNameLabel,
				Constraint.RelativeToParent ((parent) => {
					return (parent.Width * 0.05);
				}),
				Constraint.RelativeToParent ((parent) => {
					return (parent.Height * 0.12);
				}),
				Constraint.RelativeToParent ((parent) => {
					return (parent.Width * 0.9);
				}),
				Constraint.RelativeToParent ((parent) => {
					return (parent.Height * 1);
				})
			);

			footerMasterLayout.Children.Add (
				addressLabel,
				Constraint.RelativeToParent ((parent) => {
					return (parent.Width * 0.05);
				}),
				Constraint.RelativeToParent ((parent) => {
					return (parent.Height * 0.52);
				}),
				Constraint.RelativeToParent ((parent) => {
					return (parent.Width * 0.9);
				}),
				Constraint.RelativeToParent ((parent) => {
					return (parent.Height * 1);
				})
			);

			var tappableRegion = new StackLayout { BackgroundColor = Color.Transparent };

			tappableRegion.GestureRecognizers.Add (new TapGestureRecognizer {
								Command = new Command (ToogleFooter)
							});

			footerMasterLayout.Children.Add (
				tappableRegion,
				Constraint.RelativeToParent ((parent) => {
					return (parent.Width * 0);
				}),
				Constraint.RelativeToParent ((parent) => {
					return (parent.Height * 0);
				}),
				Constraint.RelativeToParent ((parent) => {
					return (parent.Width * 0.85);
				}),
				Constraint.RelativeToParent ((parent) => {
					return (parent.Height * 1);
				})
			);


		var navigationImageButton = CreateImageButton ("navigate-icon.svg", (200),
							                            (200), () => {
							var selectedPin = _extendedMap.SelectedPin;
							DependencyService.Get<IPhoneService> ().LaunchNavigationAsync (new NavigationModel {
								Latitude = selectedPin.Position.Latitude,
								Longitude = selectedPin.Position.Longitude,
								DestinationAddress = selectedPin.Address,
								DestinationName = selectedPin.Label
							});
						});
			

		footerMasterLayout.Children.Add (
			navigationImageButton,
			Constraint.RelativeToParent ((parent) => {
				return (parent.Width * 0.8);
			}),
			Constraint.RelativeToParent ((parent) => {
				return (parent.Height * -0.4);
			}),
			Constraint.RelativeToParent ((parent) => {
				return (parent.Width * 0.16);
			}),
			Constraint.RelativeToParent ((parent) => {
				return (parent.Height * 0.7);
			})
		);


		var footerHeight = 0.23;

		Device.OnPlatform (iOS: () => footerHeight = 0.16,
			Android: () => footerHeight = 0.16,
			WinPhone: () => footerHeight = 0.23);

		var footerLayout = new RelativeLayout ();

		footerLayout.Children.Add (
			footerMasterLayout,
			Constraint.RelativeToParent ((parent) => {
				return (parent.Width * 0);
			}),
			Constraint.RelativeToParent ((parent) => {
				return (parent.Height * 0);
			}),
			Constraint.RelativeToParent ((parent) => {
				return (parent.Width * 1);
			}),
			Constraint.RelativeToParent ((parent) => {
				return (parent.Height * footerHeight);
			})
		);

			var footerDetailsHeight = Math.Round (1 - footerHeight, 2);

			footerLayout.Children.Add (CreateFooterDetails (footerDetailsHeight),
				Constraint.RelativeToParent ((parent) => {
					return (parent.Width * 0);
				}),
				Constraint.RelativeToParent ((parent) => {
					return (parent.Height * footerHeight);
				}),
				Constraint.RelativeToParent ((parent) => {
					return (parent.Width * 1);
				}),
				Constraint.RelativeToParent ((parent) => {
					return (parent.Height * footerDetailsHeight);
				})
			);

//			footerMainGrid.Children.Add (CreateFooterDetails (footerHeight, footerTopSectionHeight), 0, 1);
//
//			footerMainGrid.Children [1].GestureRecognizers.Add (new TapGestureRecognizer {
//				Command = new Command (ToogleFooter)
//			});

			return new ContentView { Content = footerLayout, BackgroundColor = Color.Red };
		}

		private ScrollView CreateFooterDetails (double footerDetailsHeight)
		{
			var footerDetailsGrid = new Grid {
				RowDefinitions = new RowDefinitionCollection {
					new RowDefinition {
						Height = new GridLength (0.23, GridUnitType.Star)
					},
					new RowDefinition {
						Height = new GridLength (0.25, GridUnitType.Star)
					},
					new RowDefinition {
						Height = new GridLength (0.3, GridUnitType.Star)
					},
					new RowDefinition {
						Height = new GridLength (0.22, GridUnitType.Star)
					},
				},
				ColumnDefinitions = new ColumnDefinitionCollection {
					new ColumnDefinition {
						Width = new GridLength (0.025, GridUnitType.Star)
					},
					new ColumnDefinition {
						Width = new GridLength (0.95, GridUnitType.Star)
					},
					new ColumnDefinition {
						Width = new GridLength (0.025, GridUnitType.Star)
					},
				},
				RowSpacing = 10,
				Padding = new Thickness (0, 0, 0, 0)
			};

			var relativeLayout = new RelativeLayout ();
			//todo : change height from 200 to relative
			relativeLayout.Children.Add (CreateActionButtonsGrid (200),
				Constraint.RelativeToParent ((parent) => {
					return (parent.Width * 0);
				}),
				Constraint.RelativeToParent ((parent) => {
					return (parent.Height * 0);
				}),
				Constraint.RelativeToParent ((parent) => {
					return (parent.Width * 1);
				}),
				Constraint.RelativeToParent ((parent) => {
					return (parent.Height * 0.15);
				})
			);


//			footerDetailsGrid.Children.Add (CreateActionButtonsGrid (footerTopSectionHeight), 1, 0);
//			footerDetailsGrid.Children.Add (CreateScheduleGrid (), 1, 1);
//			footerDetailsGrid.Children.Add (CreateOtherView (), 1, 2);

			return new ScrollView {
				Content = relativeLayout
			};
		}

		private Grid CreateActionButtonsGrid (double footerTopSectionHeight)
		{
			var actionButtonsGrid = new Grid {
				RowDefinitions = new RowDefinitionCollection {
					new RowDefinition {
						Height = new GridLength (1, GridUnitType.Star)
					}
				},
				ColumnDefinitions = new ColumnDefinitionCollection {
					new ColumnDefinition {
						Width = new GridLength (0.2, GridUnitType.Star)
					},
					new ColumnDefinition {
						Width = new GridLength (0.25, GridUnitType.Star)
					},
					new ColumnDefinition {
						Width = new GridLength (0.1, GridUnitType.Star)
					},
					new ColumnDefinition {
						Width = new GridLength (0.25, GridUnitType.Star)
					},
					new ColumnDefinition {
						Width = new GridLength (0.2, GridUnitType.Star)
					},
				},
				BackgroundColor = Color.White
			};

			var callImageButton = CreateImageButton ("phone-icon.svg", "CALL", (footerTopSectionHeight / 2),
				                      (footerTopSectionHeight / 2), (view, o) => {
				var phoneNumber = _extendedMap.SelectedPin.PhoneNumber;
				var name = _extendedMap.SelectedPin.Label;
				DependencyService.Get<IPhoneService> ().DialNumber (phoneNumber, name);
			});


			var shareImageButton = CreateImageButton ("share-icon.svg", "SHARE", (footerTopSectionHeight / 2),
				                       (footerTopSectionHeight / 2), (view, o) => {
				var selectedPin = _extendedMap.SelectedPin;
	    var shareText =
		    string.IsNullOrEmpty(_extendedMap.ShareText)
			    ? string.Format("Let's meet at {0},{1}",
				    selectedPin.Label, selectedPin.Address)
			    : _extendedMap.ShareText;
				DependencyService.Get<IPhoneService> ().ShareText (shareText);
			});

			actionButtonsGrid.Children.Add (callImageButton, 1, 0);
			actionButtonsGrid.Children.Add (shareImageButton, 3, 0);

			return actionButtonsGrid;
		}

		private Grid CreateScheduleGrid ()
		{
			var scheduleGrid = new Grid {
				RowDefinitions = new RowDefinitionCollection {
					new RowDefinition {
						Height = new GridLength (1, GridUnitType.Star)
					}
				},
				ColumnDefinitions = new ColumnDefinitionCollection {
					new ColumnDefinition {
						Width = new GridLength (1, GridUnitType.Star)
					},
				},
				BackgroundColor = Color.White
			};

			var listview = new ListView ();

			//Don't allow selection
			listview.ItemSelected += (sender, e) => {
				listview.SelectedItem = null;
			};

			var itemTemplate = new DataTemplate (typeof(ExtendedTextCell));

			itemTemplate.SetValue (ExtendedTextCell.LeftColumnWidthProperty, new GridLength (1.0, GridUnitType.Star));
			itemTemplate.SetBinding (ExtendedTextCell.LeftTextProperty, "Day");
			itemTemplate.SetValue (ExtendedTextCell.LeftTextColorProperty, Color.Black);
			itemTemplate.SetBinding (ExtendedTextCell.LeftDetailProperty, "HoursOfOperation");
			itemTemplate.SetValue (ExtendedTextCell.LeftDetailColorProperty, Color.Gray);

			listview.ItemTemplate = itemTemplate;
			listview.BindingContext = _extendedMap;
			listview.SetBinding<ExtendedMap> (ListView.ItemsSourceProperty, vm => vm.SelectedPin.ScheduleEntries);

			scheduleGrid.Children.Add (listview, 0, 0);

			return scheduleGrid;
		}

		private View CreateOtherView ()
		{
			var contentView = new ContentView { BackgroundColor = Color.White };

			var listview = new ListView ();

			//Don't allow selection
			listview.ItemSelected += (sender, e) => {
				var url = e.SelectedItem as ExtraDetailModel;

				if (url != null && url.Value.Contains ("www")) {
					DependencyService.Get<IPhoneService> ().OpenBrowser (url.Value);
				}

				listview.SelectedItem = null;
			};

			var itemTemplate = new DataTemplate (typeof(ExtendedTextCell));

			itemTemplate.SetValue (ExtendedTextCell.LeftColumnWidthProperty, new GridLength (0.85, GridUnitType.Star));
			itemTemplate.SetValue (ExtendedTextCell.RightColumnWidthProperty, new GridLength (0.15, GridUnitType.Star));
			itemTemplate.SetBinding (ExtendedTextCell.LeftTextProperty, "Key");
			itemTemplate.SetValue (ExtendedTextCell.LeftTextColorProperty, Color.Black);
			itemTemplate.SetBinding (ExtendedTextCell.LeftDetailProperty, "Value");
			itemTemplate.SetValue (ExtendedTextCell.LeftDetailColorProperty, Color.Gray);

			listview.ItemTemplate = itemTemplate;
			listview.BindingContext = _extendedMap;
			listview.SetBinding<ExtendedMap> (ListView.ItemsSourceProperty, vm => vm.SelectedPin.Others);


			contentView.Content = listview;

			return contentView;
		}

		private ContentView CreateImageButton (string buttonImage, double height, double width,
		                                       Action tappedCallback)
		{
			var grid = new Grid {
				RowDefinitions = new RowDefinitionCollection {
					new RowDefinition {
						Height = new GridLength (1, GridUnitType.Star)
					},
				},
				ColumnDefinitions = new ColumnDefinitionCollection {
					new ColumnDefinition {
						Width = new GridLength (1, GridUnitType.Star)
					},

				},
				BackgroundColor = Color.Transparent,
				HorizontalOptions = LayoutOptions.Center,
				RowSpacing = 0
			};

			var navImage = new SvgImage {
				SvgPath = string.Format ("ExtendedMap.Forms.Plugin.Abstractions.Images.{0}", buttonImage),
				SvgAssembly = typeof(CustomMapContentView).GetTypeInfo ().Assembly,
				HorizontalOptions = LayoutOptions.Center,
				HeightRequest = height,
				WidthRequest = width
			};

			grid.GestureRecognizers.Add (new TapGestureRecognizer{ Command = new Command (tappedCallback) });

			grid.Children.Add (navImage, 0, 0);

			return new ContentView { Content = grid };
		}

		private ContentView CreateImageButton (string buttonImage, string buttonText, double height, double width,
		                                       Action<View, Object> tappedCallback)
		{
			var grid = new Grid {
				RowDefinitions = new RowDefinitionCollection {
					new RowDefinition {
						Height = new GridLength (1, GridUnitType.Star)
					},
      
				},
				ColumnDefinitions = new ColumnDefinitionCollection {
					new ColumnDefinition {
						Width = new GridLength (1, GridUnitType.Star)
					},

				},
				BackgroundColor = Color.Transparent,
				HorizontalOptions = LayoutOptions.Center,
				RowSpacing = 0
			};

			var navImageGrid = new Grid {
				RowDefinitions = new RowDefinitionCollection {
					new RowDefinition {
						Height = new GridLength (1, GridUnitType.Star)
					}
				},
				ColumnDefinitions = new ColumnDefinitionCollection {
         
					new ColumnDefinition {
						Width = new GridLength (1, GridUnitType.Star)
					},
         

				}
			};

			var navImage = new SvgImage {
				SvgPath = string.Format ("ExtendedMap.Forms.Plugin.Abstractions.Images.{0}", buttonImage),
				SvgAssembly = typeof(CustomMapContentView).GetTypeInfo ().Assembly,
				HeightRequest = height,
				WidthRequest = width
			};

			grid.GestureRecognizers.Add (new TapGestureRecognizer (tappedCallback));

			navImageGrid.Children.Add (navImage, 0, 0);

			var label = new Label {
				Text = buttonText,
				Font = Font.SystemFontOfSize (16),
				TextColor = Colors.DarkBlue,
				HorizontalOptions = LayoutOptions.Center
			};

			grid.Children.Add (navImageGrid, 0, 0);
			grid.Children.Add (label, 0, 1);

			return new ContentView { Content = grid };
		}

		#endregion UI Creation
	}

	public enum FooterMode
	{
		Expanded,
		Minimized,
		Hidden
	}
}