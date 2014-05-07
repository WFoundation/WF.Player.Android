///
/// WF.Player.Android - A Wherigo Player for Android, which use the Wherigo Foundation Core.
/// Copyright (C) 2012-2014  Dirk Weltz <web@weltz-online.de>
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Lesser General Public License as
/// published by the Free Software Foundation, either version 3 of the
/// License, or (at your option) any later version.
/// 
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU Lesser General Public License for more details.
/// 
/// You should have received a copy of the GNU Lesser General Public License
/// along with this program.  If not, see <http://www.gnu.org/licenses/>.
///

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using WF.Player.Core;
using WF.Player.Core.Engines;
using Android.Gms.Common;
using Android.Graphics;

namespace WF.Player.Game
{
	#region GameMapScreen

	public class GameMapScreen : SupportMapFragment
	{
		GameController ctrl;
		UIObject activeObject;
		float zoom = 16f;
		bool headingOrientation = false;
		Thing thing;
		View mapView;
		GoogleMap map;
		RelativeLayout layoutButtons;
		ImageButton btnCenter;
		ImageButton btnOrientation;
		ImageButton btnMapType;
		Dictionary<int, GroundOverlay> overlays = new Dictionary<int, GroundOverlay> ();
		Dictionary<int, Marker> markers = new Dictionary<int, Marker> ();
		Dictionary<int, Polygon> zones = new Dictionary<int, Polygon> ();
		Polyline distanceLine;
		string[] properties = {"Name", "Icon", "Active", "Visible", "ObjectLocation"};


		#region Constructor

		public GameMapScreen(GameController ctrl, UIObject obj)
		{
			this.ctrl = ctrl;
			this.activeObject = obj;
		}

		#endregion

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			mapView = base.OnCreateView (inflater, container, savedInstanceState);

			// Save ScreenController for later use
			ctrl = ((GameController)this.Activity);

			RelativeLayout view = new RelativeLayout(ctrl);
			view.AddView(mapView, new RelativeLayout.LayoutParams(-1, -1));

//			var view = inflater.Inflate(Resource.Layout.ScreenMap, container, false);
//
//			mapView = new MapView(ctrl);
//			mapView.OnCreate(savedInstanceState);
//
			map = this.Map;
//
//			// See http://stackoverflow.com/questions/19541915/google-maps-cameraupdatefactory-not-initalized
//			if (map != null)
//				MapsInitializer.Initialize(ctrl);
//
//			//			mapView = view.FindViewById<MapView>(Resource.Id.mapview);
//
//			var layout = view.FindViewById<LinearLayout>(Resource.Id.layoutMap);
//
//			layout.AddView(mapView);
//
			map.MapType = GoogleMap.MapTypeNormal;
			map.MoveCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(Main.GPS.Location.Latitude, Main.GPS.Location.Longitude), (float)Main.Prefs.GetDouble("MapZoom", 16)));
			map.MyLocationEnabled = true;
			map.BuildingsEnabled = true;
			map.UiSettings.ZoomControlsEnabled = true;

			CreateZones();

			layoutButtons = new RelativeLayout(ctrl);

			var lp = new RelativeLayout.LayoutParams(62, 62);
			lp.LeftMargin = 20;
			lp.TopMargin = 20;

			btnCenter = new ImageButton(ctrl);
			btnCenter.SetImageResource(Resource.Drawable.ic_button_center);
			btnCenter.SetBackgroundResource(Resource.Drawable.apptheme_btn_default_holo_light);

			//view.AddView(btnCenter, lp);

			lp = new RelativeLayout.LayoutParams(62, 62);
			lp.LeftMargin = 20;
			lp.TopMargin = 80;

			btnOrientation = new ImageButton(ctrl);
			btnOrientation.SetImageResource(Resource.Drawable.ic_button_layer);
			btnOrientation.SetBackgroundResource(Resource.Drawable.apptheme_btn_default_holo_light);

			//view.AddView(btnOrientation, lp);

			//			layout.AddView(layoutButtons);
//			LinearLayout layoutMap = view.FindViewById<LinearLayout> (Resource.Id.layoutMap);
//
//			mapView = new MapView(this);
//
//			layoutMap.AddView(mapView);

			return view;
		}

		/// <summary>
		/// Raised, when this fragment gets focus.
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			// Update all zones
			CreateZones();

			// Update distance line
			UpdateDistanceLine();

			ctrl.Engine.PropertyChanged += OnPropertyChanged;

			if (activeObject != null) {
				activeObject.PropertyChanged += OnDetailPropertyChanged;
			}

			map.CameraChange += OnCameraChange;
			map.MyLocationButtonClick += OnMyLocationButtonClick;
			// Use the common location listener, so that we have everywhere the same location
			map.SetLocationSource(Main.GPS);
			Main.GPS.AddLocationListener(OnLocationChanged);
		}

		/// <summary>
		/// Raised, when this fragment stops.
		/// </summary>
		public override void OnStop()
		{
			base.OnStop();

			//			mapView.OnPause();

			ctrl.Engine.PropertyChanged -= OnPropertyChanged;

			if (activeObject != null) {
				activeObject.PropertyChanged -= OnDetailPropertyChanged;
			}

			map.CameraChange -= OnCameraChange;
			map.MyLocationButtonClick -= OnMyLocationButtonClick;
			Main.GPS.RemoveLocationListener(OnLocationChanged);
		}

		void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName.Equals("ActiveVisibleZones")) {
				CreateZones ();
			}
		}

		void OnDetailPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			UpdateDistanceLine ();
		}

		void OnLocationChanged (object sender, WF.Player.Location.LocationChangedEventArgs e)
		{
			UpdateDistanceLine();
		}

		void OnCameraChange (object sender, GoogleMap.CameraChangeEventArgs e)
		{
			Main.Prefs.SetDouble("MapZoom", e.P0.Zoom);
		}

		void OnMyLocationButtonClick (object sender, GoogleMap.MyLocationButtonClickEventArgs args)
		{
			string[] items = new string[] {GetString(Resource.String.menu_screen_map_location_mylocation), GetString(Resource.String.menu_screen_map_location_game)};

			// Show selection dialog for location
			AlertDialog.Builder builder = new AlertDialog.Builder(ctrl);
			builder.SetTitle(Resource.String.menu_screen_map_location_header);
			builder.SetItems(items.ToArray(), delegate(object s, DialogClickEventArgs e) {
				if (e.Which == 0)
					FocusOnLocation();
				if (e.Which == 1)
					FocusOnGame();
			});
			AlertDialog alert = builder.Create();
			alert.Show();
		}

		#endregion

		#region Map handling

		/// <summary>
		/// Creates all visible zones.
		/// </summary>
		void CreateZones ()
		{
			// Remove all active zones
			while (zones.Count > 0) {
				zones.ElementAt (0).Value.Remove ();
				zones.Remove (zones.ElementAt (0).Key);
			}
			// Now create all zones new
			foreach (Zone z in ctrl.Engine.ActiveVisibleZones) {
				CreateZone (z);
			}
		}

		/// <summary>
		/// Creates a visible zone.
		/// </summary>
		/// <param name="z">The z coordinate.</param>
		void CreateZone (Zone z)
		{
			// Create new polygon for zone
			PolygonOptions po = new PolygonOptions ();
			foreach (var zp in z.Points) {
				po.Points.Add (new LatLng (zp.Latitude, zp.Longitude));
			}
			po.InvokeStrokeColor (Color.Argb (160, 255, 0, 0));
			po.InvokeStrokeWidth (2);
			po.InvokeFillColor (Color.Argb (80, 255, 0, 0));
			// Add polygon to list of active zones
			zones.Add (z.ObjIndex, map.AddPolygon (po));
		}

		/// <summary>
		/// Draw line from active location to active object, if it is a zone.
		/// </summary>
		void UpdateDistanceLine ()
		{
			if (activeObject != null && activeObject is Zone) {
				// Delete line
				if (distanceLine != null) {
					distanceLine.Remove ();
					distanceLine = null;
				}
				// Draw line
				PolylineOptions po = new PolylineOptions();
				po.Points.Add (new LatLng (Main.GPS.Location.Latitude, Main.GPS.Location.Longitude));
				po.Points.Add (new LatLng (((Zone)activeObject).Points[0].Latitude, ((Zone)activeObject).Points[0].Longitude)); //.ObjectLocation.Latitude, ((Zone)activeObject).ObjectLocation.Longitude));
				po.InvokeColor(Color.Cyan);
				po.InvokeWidth(2);
				distanceLine = map.AddPolyline(po);
			}
		}

		/// <summary>
		/// Focuses the on whole game.
		/// </summary>
		void FocusOnGame()
		{
			double latNorth, latSouth;
			double longWest, longEast;

			// Get correct latitude
			if (ctrl.Engine.Bounds.Right > ctrl.Engine.Bounds.Left) {
				latNorth = ctrl.Engine.Bounds.Right;
				latSouth = ctrl.Engine.Bounds.Left;
			} else {
				latNorth = ctrl.Engine.Bounds.Left;
				latSouth = ctrl.Engine.Bounds.Right;
			}

			// Get correct latitude
			if (ctrl.Engine.Bounds.Top > ctrl.Engine.Bounds.Bottom) {
				longWest = ctrl.Engine.Bounds.Bottom;
				longEast = ctrl.Engine.Bounds.Top;
			} else {
				longWest = ctrl.Engine.Bounds.Top;
				longEast = ctrl.Engine.Bounds.Bottom;
			}

			CameraUpdate cu = CameraUpdateFactory.NewLatLngBounds(new LatLngBounds(new LatLng(latSouth, longWest), new LatLng(latNorth, longEast)), 10);

			map.MoveCamera(cu);
		}

		/// <summary>
		/// Focuses the on active location.
		/// </summary>
		void FocusOnLocation()
		{
			CameraUpdate cu = CameraUpdateFactory.NewLatLng(new LatLng(Main.GPS.Location.Latitude, Main.GPS.Location.Longitude));

			map.MoveCamera(cu);
		}

		#endregion

	}

}
