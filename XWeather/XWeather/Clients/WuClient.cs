﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ServiceStack;

using XWeather.Domain;
using XWeather.Constants;

namespace XWeather.Clients
{
	public class WuClient
	{
		static WuClient _shared;

		public static WuClient Shared => _shared ?? (_shared = new WuClient ());


		public event EventHandler LocationAdded;
		public event EventHandler LocationRemoved;
		public event EventHandler UpdatedSelected;


		WuLocation _selected;
		public WuLocation Selected {
			get { return _selected; }
			set {

				foreach (var location in Locations) location.Selected = false;

				_selected = value;

				_selected.Selected = true;

				UpdatedSelected?.Invoke (this, EventArgs.Empty);
			}
		}


		public List<WuLocation> Locations { get; set; } = new List<WuLocation> ();

		public bool HasCurrent => Selected != null;


		JsonServiceClient _client;
		JsonServiceClient client => _client ?? (_client = new JsonServiceClient ());


		public async Task AddLocation (WuAcLocation location)
		{
			LocationAdded?.Invoke (this, EventArgs.Empty);

			var wuLocation = await getWuLocation (location);

			Locations.Add (wuLocation);

			LocationAdded?.Invoke (this, EventArgs.Empty);
		}


		public void RemoveLocation (WuLocation location)
		{
			Locations.Remove (location);

			LocationRemoved?.Invoke (this, EventArgs.Empty);
		}


		async Task<WuAcLocation> getCurrentLocation (double latitude, double longitude)
		{
			var location = await GetAsync<GeoLookup> ($"/q/{latitude},{longitude}");

			return location.ToWuAcLocation ();
		}


		public async Task GetLocations (string json, double latitude, double longitude)
		{
			var locations = json.GetLocations ();

			var oldCurrent = locations.FirstOrDefault (l => l.Current);

			if (oldCurrent != null) locations.Remove (oldCurrent);


			var newCurrent = await getCurrentLocation (latitude, longitude);

			if (newCurrent != null) locations.Add (newCurrent);


			// if the previous current was selected, or theres not one selected, select this one
			newCurrent.Selected |= oldCurrent?.Selected ?? false || !locations.Any (l => l.Selected);

			await GetLocations (locations);
		}


		public async Task GetLocations (List<WuAcLocation> locations)
		{
			var tasks = locations.Select (l => getWuLocation (l)).ToArray ();

			var wuLocations = await Task.WhenAll (tasks);

			Locations = new List<WuLocation> (wuLocations);

			Selected = Locations.FirstOrDefault (l => l.Selected) ?? Locations [0];
		}


		async Task<WuLocation> getWuLocation (WuAcLocation acLocation)
		{
			var location = new WuLocation (acLocation);

			location.Weather = await GetAsync<WuWeather> (acLocation.l);

			System.Diagnostics.Debug.WriteLine (location.Weather.ToJson ());

			location.Updated = DateTime.UtcNow;

			return location;
		}


		public Task<T> GetAsync<T> (string location)
			where T : WuObject, new()
		{
			try {

				var url = ApiKeys.WuApiKeyedQueryJsonFmt.Fmt (new T ().WuKey, location);

				System.Diagnostics.Debug.WriteLine (url);

				return client.GetAsync<T> (url);

			} catch (WebServiceException webEx) {

				System.Diagnostics.Debug.WriteLine ($"Exception processing Weather Underground request for {typeof (T).Name}\n{webEx.Message}");
				throw;

			} catch (Exception ex) {

				System.Diagnostics.Debug.WriteLine ($"Exception processing Weather Underground request for {typeof (T).Name}\n{ex.Message}");
				throw;
			}
		}


		public Task<byte []> GetRadarImageAsync (RadarBounds bounds)
		{
			try {

				var query = $"image.gif?maxlat={bounds.MaxLat}&maxlon={bounds.MaxLon}&minlat={bounds.MinLat}&minlon={bounds.MinLon}&width={bounds.Width}&height={bounds.Height}&rainsnow={1}&num={6}&delay={25}";

				var url = ApiKeys.WuApiKeyedQueryFmt.Fmt ("animatedradar", query);

				System.Diagnostics.Debug.WriteLine (url);

				return client.GetAsync<byte []> (url);

			} catch (WebServiceException webEx) {

				System.Diagnostics.Debug.WriteLine ($"Exception processing Weather Underground request for Radar Image\n{webEx.Message}");
				throw;

			} catch (Exception ex) {

				System.Diagnostics.Debug.WriteLine ($"Exception processing Weather Underground request for Radar Image\n{ex.Message}");
				throw;
			}
		}
	}
}