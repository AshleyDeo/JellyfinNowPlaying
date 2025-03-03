using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Rainmeter;

namespace PluginJellyfinNowPlaying
{
	internal class JellyfinApiSession {
		public string UserName { get; set; }
		public string Client { get; set; }
		public JellyfinPlayState PlayState { get; set; }
		public JellyfinNowPlayingItem NowPlayingItem { get; set; }
	}

	internal class JellyfinNowPlayingItem {
		public string Name { get; set; }
		public string Type { get; set; }
		public string ParentLogoItemId { get; set; }
		public string ParentBackdropItemId { get; set; }
		public List<string> Artists { get; set; }
		public string Album { get; set; }
		public string Path { get; set; }
		public string ParentId { get; set; }
		public long RunTimeTicks { get; set; }
		public string ProductionYear { get; set; }
		public int IndexNumber { get; set; }
	}

	internal class JellyfinPlayState {
		public long PositionTicks { get; set; }
		public bool IsPaused { get; set; }
		public bool IsMuted { get; set; }
		public int VolumeLevel { get; set; }
		public string RepeatMode { get; set; }
	}

	internal class ApiSupportedCommands {
		public bool SetVolume { get; set; }

	}

	internal struct Data {
		public string Client;
		public string Title;
		public string Artist;
		public string Album;
		public string Cover;
		public string CoverUrl;
		public string Backdrop;
		public string BackdropUrl;
		public string Logo;
		public string LogoUrl;
		public string Duration;
		public string Position;
		public string Remaining;
		public int DurationSeconds;
		public int PositionSeconds;
		public int RemainingSeconds;
		public int Progress;
		public int State;
		public int Status;
		public int Rating;
		public int Repeat;
		public int Shuffle;
		public int Volume;

		internal void clear () {
			Client = null;
			Title = null;
			Artist = null;
			Album = null;
			Cover = null;
			CoverUrl = null;
			Backdrop = null;
			BackdropUrl = null;
			Logo = null;
			LogoUrl = null;
			Duration = null;
			Position = null;
			Remaining = null;
			DurationSeconds = 0;
			PositionSeconds = 0;
			RemainingSeconds = 0;
			Progress = 0;
			Status = 0;
			State = 0;
			Rating = 0;
			Repeat = 0;
			Shuffle = 0;
			Volume = 0;
		}
	}

	internal struct Image {
		public string DefaultPath;
		public string OutputLocation;
		public string LastDownloadedUrl;
		public string LastFailedUrl;

		internal void clear() {
			DefaultPath = null;
			OutputLocation = null;
			LastDownloadedUrl = null;
			LastFailedUrl = null;
		}

	}

	internal class Measure
	{
		internal enum MeasureType
		{
			Player,
			Status,
			Title,
			Artist,
			Album,
			Cover,
			CoverUrl,
			Backdrop,
			BackdropUrl,
			Logo,
			LogoUrl,
			Duration,
			Position,
			Remaining,
			DurationSeconds,
			PositionSeconds,
			RemainingSeconds,
			Progress,
			Volume,
			State,
			Rating,
			Repeat,
			Shuffle,
			/*
			 -- Jellyfin remote control of media through API not working (yet?) --
			SupportsMediaControl,
			SupportsRemoteControl*/
		}

		internal MeasureType Type = MeasureType.Player;
		internal API api;

		internal virtual void Dispose()
		{
		}

		internal virtual void Reload(Rainmeter.API api, ref double maxValue) 
		{
			this.api = api;

			string type = api.ReadString("Type", "");
			switch (type.ToLowerInvariant())
			{
				case "player":
					Type = MeasureType.Player;
					break;

				case "status":
					Type = MeasureType.Status;
					break;

				case "title":
					Type = MeasureType.Title;
					break;
					
				case "artist":
					Type = MeasureType.Artist;
					break;

				case "album":
					Type = MeasureType.Album;
					break;

				case "cover":
					Type = MeasureType.Cover;
					break;

				case "coverurl":
					Type = MeasureType.CoverUrl;
					break;

				case "backdrop":
					Type = MeasureType.Backdrop;
					break;

				case "backdropurl":
					Type = MeasureType.BackdropUrl;
					break;

				case "logo":
					Type = MeasureType.Logo;
					break;

				case "logourl":
					Type = MeasureType.LogoUrl;
					break;
					
				case "duration":
					Type = MeasureType.Duration;
					break;

				case "position":
					Type = MeasureType.Position;
					break;

				case "remaining":
					Type = MeasureType.Remaining;
					break;
					
				case "durationseconds":
					Type = MeasureType.DurationSeconds;
					break;

				case "positionseconds":
					Type = MeasureType.PositionSeconds;
					break;

				case "remainingseconds":
					Type = MeasureType.RemainingSeconds;
					break;
					
				case "progress":
					Type = MeasureType.Progress;
					break;

				case "volume":
					Type = MeasureType.Volume;
					break;

				case "state":
					Type = MeasureType.State;
					break;
					
				case "rating":
					Type = MeasureType.Rating;
					break;

				case "repeat":
					Type = MeasureType.Repeat;
					break;

				case "shuffle":
					Type = MeasureType.Shuffle;
					break;
					
				default:
					api.Log(API.LogType.Error, $"JellyfinNowPlaying.dll: Type={type} not valid");
					break;
			}
		}

		internal virtual double Update()
		{
			return 0.0;
		}

		internal virtual string GetString (MeasureType type) 
		{
			return null;
		}
	}

	internal class ParentMeasure : Measure {
		// This list of all parent measures is used by the child measures to find their parent.
		internal static List<ParentMeasure> ParentMeasures = new List<ParentMeasure>();

		internal string Name;
		internal IntPtr Skin;

		//Server Info
		internal string ServerUrl;
		internal string ApiKey;
		internal string UserName;

		internal Data data = default(Data);
		//internal HttpClient client = new HttpClient();

		//Download Images
		private static readonly string DefaultOutputLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Temp\\Rainmeter\\JellyfinNowPlaying\\";
		internal Image Cover = default(Image);
		internal Image Backdrop = default(Image);
		internal Image Logo = default(Image);

		internal ParentMeasure () {
			ParentMeasures.Add(this);
		}

		internal override void Dispose () {
			Cover.clear();
			Backdrop.clear();
			Logo.clear();
			ParentMeasures.Remove(this);
		}

		internal override void Reload (Rainmeter.API api, ref double maxValue) {
			base.Reload(api, ref maxValue);

			Name = api.GetMeasureName();
			Skin = api.GetSkin();

			ServerUrl = api.ReadString("ServerUrl", "http://localhost:8096");
			ApiKey = api.ReadString("ApiKey", "");
			UserName = api.ReadString("UserName", "");

			if (string.IsNullOrEmpty(ApiKey)) {
				api.Log(API.LogType.Error, "JellyfinNowPlaying.dll: ApiKey missing");
			}

			if (string.IsNullOrEmpty(UserName)) {
				api.Log(API.LogType.Error, "JellyfinNowPlaying.dll: UserName missing");
			}

			if (Type == MeasureType.Cover) {
				string DefaultPath = api.ReadPath("DefaultPath", "");
				if (DefaultPath.Length > 0) Cover.DefaultPath = DefaultPath;
			}

			if (Type == MeasureType.Backdrop) {
				string DefaultPath = api.ReadPath("DefaultPath", "");
				if (DefaultPath.Length > 0) Backdrop.DefaultPath = DefaultPath;
			}

			if (Type == MeasureType.Logo) {
				string DefaultPath = api.ReadPath("DefaultPath", "");
				if (DefaultPath.Length > 0) Logo.DefaultPath = DefaultPath;
			}
		}

		internal override double Update () {
			if (!string.IsNullOrEmpty(ApiKey)) {
				string url = $"{ServerUrl}/Sessions?ApiKey={ApiKey}";
				try {
					//HttpClient httpClient = new HttpClient();
					using (HttpClient client = new HttpClient()) {
						HttpResponseMessage response = client.GetAsync(url).Result;
						if (response.StatusCode == HttpStatusCode.OK) {
							api.Log(API.LogType.Debug, $"JellyfinNowPlaying.dll: Connection to Jellyfin Api: Success!!");
							string apiRes = response.Content.ReadAsStringAsync().Result;

							List<JellyfinApiSession> sessions = JsonConvert.DeserializeObject<List<JellyfinApiSession>>(apiRes);

							JellyfinApiSession session = sessions.Find(s => {
								return s.NowPlayingItem != null && s.NowPlayingItem.Type.Equals("Audio") && s.UserName.Equals(UserName);
							});

							if (session != null) {
								data.Status = 1;
								api.Log(API.LogType.Debug, $"JellyfinNowPlaying.dll: {UserName}'s session from {session.Client}");
								ParseApi(session, client);
							}
							else {
								api.Log(API.LogType.Notice, $"JellyfinNowPlaying.dll: {UserName}'s session not found!!");
								data.clear();
							}
						}
						else {
							api.Log(API.LogType.Error, $"JellyfinNowPlaying.dll: Connection Failed!!: {url}");
						}
					}
				}
				catch (Exception e) {
					api.Log(API.LogType.Error, $"JellyfinNowPlaying.dll: Error : {e.Message}");
				}
			}
			return GetValue(Type);
		}
		internal override string GetString (MeasureType type) {
			switch (type) {
				case MeasureType.Player:
					return data.Client;

				case MeasureType.Title:
					return data.Title;

				case MeasureType.Artist:
					return data.Artist;

				case MeasureType.Album:
					return data.Album;

				case MeasureType.Cover:
					return data.Cover;

				case MeasureType.CoverUrl:
					return data.CoverUrl;

				case MeasureType.Backdrop:
					return data.Backdrop;

				case MeasureType.BackdropUrl:
					return data.BackdropUrl;

				case MeasureType.Logo:
					return data.Logo;

				case MeasureType.LogoUrl:
					return data.LogoUrl;

				case MeasureType.Duration:
					return data.Duration;

				case MeasureType.Position:
					return data.Position;

				case MeasureType.Remaining:
					return data.Remaining;
			}

			return null;
		}

		internal double GetValue (MeasureType type) {
			switch (type) {
				case MeasureType.Status:
					return data.Status;

				case MeasureType.DurationSeconds:
					return data.DurationSeconds;

				case MeasureType.PositionSeconds:
					return data.PositionSeconds;

				case MeasureType.RemainingSeconds:
					return data.RemainingSeconds;

				case MeasureType.Progress:
					return data.Progress;

				case MeasureType.Volume:
					return data.Volume;

				case MeasureType.State:
					return data.State;

				case MeasureType.Rating:
					return data.Rating;

				case MeasureType.Repeat:
					return data.Repeat;

				case MeasureType.Shuffle:
					return data.Shuffle;
			}

			return 0.0;
		}

		internal void ParseApi (JellyfinApiSession session, HttpClient client) {
			api.Log(API.LogType.Debug, "JellyfinNowPlaying.dll: Parsing...");
			data.Client = (String.IsNullOrEmpty(session.Client)) ? "" : session.Client;
			data.Title = (String.IsNullOrEmpty(session.NowPlayingItem.Name)) ? "" : session.NowPlayingItem.Name;
			data.Artist = session.NowPlayingItem.Artists.Count == 0 ? "" : String.Join(", ", session.NowPlayingItem.Artists.ToArray());
			data.Album = (String.IsNullOrEmpty(session.NowPlayingItem.Album)) ? "" : session.NowPlayingItem.Album;
			data.DurationSeconds = (int)(session.NowPlayingItem.RunTimeTicks / 10000000.0);
			data.PositionSeconds = (int)(session.PlayState.PositionTicks / 10000000.0);
			data.RemainingSeconds = (int)((session.NowPlayingItem.RunTimeTicks - session.PlayState.PositionTicks) / 10000000.0);
			data.Progress = (int)(data.PositionSeconds * 100.0 / data.DurationSeconds);
			data.Duration = FormatTime(data.DurationSeconds);
			data.Position = FormatTime(data.PositionSeconds);
			data.Remaining = FormatTime(data.RemainingSeconds);
			data.State = session != null ? 1 : 0;
			data.Volume = session.PlayState.IsMuted ? 0 : session.PlayState.VolumeLevel;
			data.Rating = 0;
			data.Repeat = session.PlayState.RepeatMode.Equals("RepeatNone") ? 0 : session.PlayState.RepeatMode.Equals("RepeatAll") ? 1 : 2;
			data.Shuffle = 0;
			if (!string.IsNullOrEmpty(session.NowPlayingItem.ParentId)) {
				data.CoverUrl = $"{ServerUrl}/Items/{session.NowPlayingItem.ParentId}/Images/Primary?fillHeight=600&fillWidth=600&ApiKey={ApiKey}";
			} else {
				api.Log(API.LogType.Debug, "JellyfinNowPlaying.dll: No Cover");
				data.CoverUrl = "";
				Cover.OutputLocation = "";
			}
			if (!string.IsNullOrEmpty(session.NowPlayingItem.ParentBackdropItemId)) {
				data.BackdropUrl = $"{ServerUrl}/Items/{session.NowPlayingItem.ParentBackdropItemId}/Images/Backdrop?maxWidth=378&ApiKey={ApiKey}";
			} else {
				api.Log(API.LogType.Debug, "JellyfinNowPlaying.dll: No Backdrop");
				data.BackdropUrl = "";
				Backdrop.OutputLocation = "";
			}
			if (!string.IsNullOrEmpty(session.NowPlayingItem.ParentLogoItemId)) {
				data.LogoUrl = $"{ServerUrl}/Items/{session.NowPlayingItem.ParentLogoItemId}/Images/Logo?maxHeight=32&maxWidth=171&quality=90&ApiKey={ApiKey}";
			} else {
				api.Log(API.LogType.Debug, "JellyfinNowPlaying.dll: No Logo");
				data.LogoUrl = "";
				Logo.OutputLocation = "";
			}
			DownloadImage(session, client);
			data.Cover = Cover.OutputLocation.Length > 0 ? Cover.OutputLocation : Cover.DefaultPath;
			api.Log(API.LogType.Debug, $"JellyfinNowPlaying.dll: Album art: {data.Cover}");
			data.Backdrop = Backdrop.OutputLocation.Length > 0 ? Backdrop.OutputLocation : Backdrop.DefaultPath;
			api.Log(API.LogType.Debug, $"JellyfinNowPlaying.dll: Backdrop: {data.Backdrop}");
			data.Logo = Logo.OutputLocation.Length > 0 ? Logo.DefaultPath : Logo.OutputLocation;
			api.Log(API.LogType.Debug, $"JellyfinNowPlaying.dll: Logo: {data.Logo}");
			api.Log(API.LogType.Debug, $"JellyfinNowPlaying.dll: Parsing...Finished");
		}

		internal string FormatTime(int sec) {
			int hours = 0;
			int mins = 0;
			int secs = 0;
			if (sec >= 60) {
				mins = sec / 60;
				secs = sec % 60;
			} else return $"{mins.ToString().PadLeft(2, '0')}:{sec.ToString().PadLeft(2, '0')}";

			if (mins >= 60) {
				hours = mins / 60;
				mins = mins % 60;
			} else return $"{mins.ToString().PadLeft(2, '0')}:{secs.ToString().PadLeft(2, '0')}";

			return $"{hours.ToString().PadLeft(2, '0')}:{mins.ToString().PadLeft(2, '0')}:{secs.ToString().PadLeft(2, '0')}";
		}

		internal void DownloadImage (JellyfinApiSession session, HttpClient client) {
			api.Log(API.LogType.Debug, $"JellyfinNowPlaying.dll: Downloading Images...");
			Directory.CreateDirectory(DefaultOutputLocation);
			if (data.CoverUrl.Length > 0 && !data.CoverUrl.Equals(Cover.LastFailedUrl) && !data.CoverUrl.Equals(Cover.LastDownloadedUrl)) {
				HttpResponseMessage response = client.GetAsync(data.CoverUrl).Result;
				api.Log(API.LogType.Debug, $"JellyfinNowPlaying.dll: Downloading album art >> Response status code: {response.StatusCode}");

				if (response.StatusCode == HttpStatusCode.OK) { //Save image to folder
					Cover.OutputLocation = $"{DefaultOutputLocation}{UserName}-cover.webp"; //Name image file
					using (var inputStream = response.Content.ReadAsStreamAsync().Result) {
						using (var fileStream = new FileStream(Cover.OutputLocation, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)) {
							inputStream.CopyTo(fileStream);
						}
					}
					Cover.LastDownloadedUrl = data.CoverUrl;
					api.Log(API.LogType.Debug, "JellyfinNowPlaying.dll: Downloading album art...Finished");
				}
				else {
					Cover.LastFailedUrl = data.CoverUrl;
					Cover.OutputLocation = "";
					api.Log(API.LogType.Error, $"JellyfinNowPlaying.dll: Unable to get album art. Response status code: {response.StatusCode}");
				}
			}
			if (data.BackdropUrl.Length > 0 && !data.BackdropUrl.Equals(Backdrop.LastFailedUrl) && !data.BackdropUrl.Equals(Backdrop.LastDownloadedUrl)) {
				HttpResponseMessage response = client.GetAsync(data.BackdropUrl).Result;
				api.Log(API.LogType.Debug, $"JellyfinNowPlaying.dll: Downloading artist backdrop >> Response status code: {response.StatusCode}");

				if (response.StatusCode == HttpStatusCode.OK) {
					Backdrop.OutputLocation = $"{DefaultOutputLocation}{UserName}-backdrop.webp";
					using (var inputStream = response.Content.ReadAsStreamAsync().Result) {
						using (var fileStream = new FileStream(Backdrop.OutputLocation, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)) {
							inputStream.CopyTo(fileStream);
						}
					}
					Backdrop.LastDownloadedUrl = data.BackdropUrl;
					api.Log(API.LogType.Debug, "JellyfinNowPlaying.dll: Downloading artist backdrop...Finished");
				}
				else {
					Backdrop.LastFailedUrl = data.BackdropUrl;
					Backdrop.OutputLocation = "";
					api.Log(API.LogType.Error, $"JellyfinNowPlaying.dll: Unable to get artist backdrop. Response status code: {response.StatusCode}");
				}
			}
			if (data.LogoUrl.Length > 0 && !data.LogoUrl.Equals(Logo.LastFailedUrl) && !data.LogoUrl.Equals(Logo.LastDownloadedUrl)) {
				HttpResponseMessage response = client.GetAsync(data.LogoUrl).Result;
				api.Log(API.LogType.Debug, $"JellyfinNowPlaying.dll: Downloading artist logo >> Response status code: {response.StatusCode}");

				if (response.StatusCode == HttpStatusCode.OK) {
					Logo.OutputLocation = $"{DefaultOutputLocation}{UserName}-logo.webp";
					using (var inputStream = response.Content.ReadAsStreamAsync().Result) {
						using (var fileStream = new FileStream(Logo.OutputLocation, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)) {
							inputStream.CopyTo(fileStream);
						}
					}
					Logo.LastDownloadedUrl = data.LogoUrl;
					api.Log(API.LogType.Debug, "JellyfinNowPlaying.dll: Downloading artist logo...Finished");
				}
				else {
					Logo.LastFailedUrl = data.LogoUrl;
					Logo.OutputLocation = "";
					api.Log(API.LogType.Error, $"JellyfinNowPlaying.dll: Unable to get artist logo. Response status code: {response.StatusCode}");
				}
			}
			//api.Log(API.LogType.Debug, "JellyfinNowPlaying.dll: Downloading Images...Finished");

		}
	}

	internal class ChildMeasure : Measure
	{
		private ParentMeasure ParentMeasure = null;

		internal override void Reload(Rainmeter.API api, ref double maxValue)
		{
			base.Reload(api, ref maxValue);

			string parent = api.ReadString("UseInfo", "");
			IntPtr skin = api.GetSkin();

			ParentMeasure = null;
			foreach (ParentMeasure parentMeasure in ParentMeasure.ParentMeasures)
			{
				if (parentMeasure.Skin.Equals(skin) && parentMeasure.Name.Equals(parent))
				{
					ParentMeasure = parentMeasure;
				}
			}

			if (ParentMeasure == null) {
				api.Log(API.LogType.Error, $"JellyfinNowPlaying.dll: Info from [{parent}] not valid");
			}
			else
			{
				if (Type == MeasureType.Cover) {
					string DefaultPath = api.ReadPath("DefaultPath", "");
					if (DefaultPath.Length > 0) ParentMeasure.Cover.DefaultPath = DefaultPath;
				}

				if (Type == MeasureType.Backdrop) {
					string DefaultPath = api.ReadPath("DefaultPath", "");
					if (DefaultPath.Length > 0) ParentMeasure.Backdrop.DefaultPath = DefaultPath;
				}

				if (Type == MeasureType.Logo) {
					string DefaultPath = api.ReadPath("DefaultPath", "");
					if (DefaultPath.Length > 0) ParentMeasure.Logo.DefaultPath = DefaultPath;
				}
			}
		}

		internal override double Update()
		{
			if (ParentMeasure != null)
			{
				return ParentMeasure.GetValue(Type);
			}

			return 0.0;
		}

		internal override string GetString (MeasureType type)
		{
			if (ParentMeasure != null)
			{
				return ParentMeasure.GetString(type);
			}

			return null;
		}
	}

	public static class Plugin
	{
		static string PluginName = "JellyfinNowPlaying";

		[DllExport]
		public static void Initialize(ref IntPtr data, IntPtr rm)
		{
			Rainmeter.API api = new Rainmeter.API(rm);

			string parent = api.ReadString("UseInfo", "");
			Measure measure;
			if (String.IsNullOrEmpty(parent))
			{
				measure = new ParentMeasure();
			}
			else
			{
				measure = new ChildMeasure();
			}

			data = GCHandle.ToIntPtr(GCHandle.Alloc(measure));
		}

		[DllExport]
		public static void Finalize(IntPtr data)
		{
			Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
			measure.Dispose();
			GCHandle.FromIntPtr(data).Free();
		}

		[DllExport]
		public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
		{
			Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
			measure.Reload(new Rainmeter.API(rm), ref maxValue);
		}

		[DllExport]
		public static double Update(IntPtr data)
		{
			Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
			return measure.Update();
		}

		[DllExport]
		public static IntPtr GetString (IntPtr data) {
			Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
			string value = measure.GetString(measure.Type);

			if (value != null) {
				return Marshal.StringToHGlobalAuto(value);
			}
			return IntPtr.Zero;
		}
	}
}
