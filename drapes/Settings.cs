/*
Copyright (C) 2006 Milosz Tanski

This file is part of Drapes.

Drapes is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.

Drapes is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Drapes; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.IO;
using Mono.Posix;
using GConf;
using Gtk;
using Config = Drapes.Config;

namespace Drapes.Config
{
	public class TimeDelay
	{
		public static string String(Delay d)
		{
			switch (d) {
				case Delay.MIN_5:
					return "5 minutes";
				case Delay.MIN_10:
					return "10 minutes";
				case Delay.MIN_15:
					return "15 minutes";
				case Delay.MIN_20:
					return "20 minutes";
				case Delay.MIN_30:
					return "30 minutes";
				case Delay.MIN_45:
					return "45 minutes";
				case Delay.MIN_60:
					return "1 hour";
				case Delay.MIN_90:
					return "1 hour 30 minutes";
				case Delay.MIN_120:
					return "2 hours";
				case Delay.MIN_NEVER:
				default:
					return "Never";
			}
		} 
	
		public static int Int32(Delay d)
		{
			switch (d) {
				case Delay.MIN_5:
					return 5;
				case Delay.MIN_10:
					return 10;
				case Delay.MIN_15:
					return 15;
				case Delay.MIN_20:
					return 20;
				case Delay.MIN_30:
					return 30;
				case Delay.MIN_45:
					return 45;
				case Delay.MIN_60:
					return 60;
				case Delay.MIN_90:
					return 90;
				case Delay.MIN_120:
					return 120;
				case Delay.MIN_NEVER:
				default:
					return 0;
			}
		}
	
		// delay	
		public enum Delay {
			MIN_5,		MIN_10,		MIN_15,		MIN_20,
			MIN_30,		MIN_45,		MIN_60,		MIN_90,
			MIN_120,	MIN_NEVER
		}
	}

	// Styles
	public enum Style {
		STYLE_CENTER,
		STYLE_FILL,
		STYLE_SCALE,
		STYLE_ZOOM,
		STYLE_TILED				// wtf? welcome back 1995
	}

	// Settings class
	public class Settings
	{
		GConf.Client client;
		
		// configuration keys stored in gconf
		const string GCONF_APP_PATH = "/apps/drapes";
		const string GCONF_KEY_TIMER = GCONF_APP_PATH + "/timedelay";
		const string GCONF_KEY_MONITOR = GCONF_APP_PATH + "/monitor";
		const string GCONF_KEY_MONITOR_DIR = GCONF_APP_PATH + "/monitor_directory";
		const string GCONF_KEY_SO_START =  GCONF_APP_PATH + "/on_startup";
		const string GCONF_KEY_SHUFFLE = GCONF_APP_PATH + "/enable_timed_shuffle";
		//
		ToggleButton		cbtStartSwitch;
		HScale				scaleTimer;
		ToggleButton		cbtMonitor;
		FileChooserButton	fcbDir;
		ComboBox			cmbStyle;
		
		// Constructor
		public Settings()
		{
			client = new GConf.Client();
			
			// GConf key change notifiers
			client.AddNotify(GCONF_KEY_SO_START, GConfKeyChange);
			client.AddNotify(GCONF_KEY_TIMER, GConfKeyChange);
			client.AddNotify(GCONF_KEY_MONITOR, GConfKeyChange);
			client.AddNotify(GCONF_KEY_MONITOR_DIR, GConfKeyChange);
			client.AddNotify(Defaults.Gnome.PictureOptionsKey, GConfKeyChange);
		}
		
		// For setting up GConf key monitoring
		public void ShuffleOnStartWidget(ToggleButton t)
		{
			cbtStartSwitch = t;
		}
		
		public void MonitorEnabledWidget(ToggleButton t)
		{
			cbtMonitor = t;
		}
		
		public void MonitorDirectoryWidget(FileChooserButton f)
		{
			fcbDir = f;
		}
		
		public void SwitchDelayWidget(HScale s)
		{
			scaleTimer = s;
		}

		public void SwitchStyleWidget(ComboBox c)
		{
			cmbStyle = c;
		}
		
		// Load a randomwallaper on start
		public bool ShuffleOnStart
		{
			get {
				bool val = Defaults.ShuffleOnStart;
				
				try {
					val = (bool) client.Get(GCONF_KEY_SO_START);
				} catch (NoSuchKeyException e) {		// key dosen't exist?
					client.Set(GCONF_KEY_SO_START, Defaults.ShuffleOnStart);
				} catch (InvalidCastException e) {		// got overwriten
					client.Set(GCONF_KEY_SO_START, Defaults.ShuffleOnStart);
				}
			
				return val;
			}
			
			set {
				client.Set(GCONF_KEY_SO_START, value);
			}		
		}

		public TimeDelay.Delay SwitchDelay
		{
			get {
				TimeDelay.Delay val = Defaults.SwitchDelay;
				
				try {
					val = (TimeDelay.Delay) client.Get(GCONF_KEY_TIMER);
				} catch (NoSuchKeyException e) {
					client.Set(GCONF_KEY_TIMER, (int) Defaults.SwitchDelay);
				} catch (InvalidCastException e) {
					client.Set(GCONF_KEY_TIMER, (int) Defaults.SwitchDelay);
				}
				
				return val;
			}
			
			set {
				client.Set(GCONF_KEY_TIMER, (int) value);
			}
		}

		public bool MonitorEnabled
		{
			get {
				bool val = Defaults.MonitorEnabled;
				
				try {
					val = (bool) client.Get(GCONF_KEY_MONITOR);
				} catch (NoSuchKeyException e) {
					client.Set(GCONF_KEY_MONITOR, Defaults.MonitorEnabled);
				} catch (InvalidCastException e) {
					client.Set(GCONF_KEY_MONITOR, Defaults.MonitorEnabled);
				}

				return val;
			}
			
			set {
				client.Set(GCONF_KEY_MONITOR, value);
			}
		}
		
		public string MonitorDirectory
		{
			get  {
				string val = "";
			
				try {
					val = (string) client.Get(GCONF_KEY_MONITOR_DIR);
				} catch (NoSuchKeyException e) {
					client.Set(GCONF_KEY_MONITOR_DIR, Defaults.MonitorDirectory);
				} catch (InvalidCastException e) {
					client.Set(GCONF_KEY_MONITOR_DIR, Defaults.MonitorDirectory);
				}
				
				return val;
			}
			
			set {
				client.Set(GCONF_KEY_MONITOR_DIR, value);
			}
		}
		
		public Config.Style Style {
			get {
				string val = "";
				
				try {
					val = (string) client.Get(Defaults.Gnome.PictureOptionsKey);
				} catch (NoSuchKeyException e) {
					client.Set(Defaults.Gnome.PictureOptionsKey, "centered");
				}
				
				switch (val) {
					case "stretched":
						return Config.Style.STYLE_FILL;
					case "scaled":
						return Config.Style.STYLE_SCALE;
					case "zoom":
						return Config.Style.STYLE_ZOOM;
					case "centered":
					default :
						return Config.Style.STYLE_CENTER;
				}
			
			}
		
			set {
				switch (value) {
					case Config.Style.STYLE_CENTER:
						client.Set(Defaults.Gnome.PictureOptionsKey, "centered");
						break;
					case Config.Style.STYLE_FILL:
						client.Set(Defaults.Gnome.PictureOptionsKey, "stretched");
						break;
					case Config.Style.STYLE_SCALE:
						client.Set(Defaults.Gnome.PictureOptionsKey, "scaled");
						break;
					case Config.Style.STYLE_ZOOM:
						client.Set(Defaults.Gnome.PictureOptionsKey, "zoom");
						break;
					case Config.Style.STYLE_TILED:
					default:
						client.Set(Defaults.Gnome.PictureOptionsKey, "wallpaper");
						break;
				}
			}
		}	
		
		public string Wallpaper
		{
			get  {
				string val = "";
			
				try {
					val = (string) client.Get(Defaults.Gnome.WallpaperKey);
				} catch (NoSuchKeyException e) {
					client.Set(Defaults.Gnome.WallpaperKey, "");
				} catch (InvalidCastException e) {
					client.Set(Defaults.Gnome.WallpaperKey, "");
				}
				
				return val;
			}
			
			set {
				client.Set(Defaults.Gnome.WallpaperKey,  value);
			}
		}
		
		public bool ShuffleEnabled
		{
			get {
				bool val = Defaults.ShuffleEnabled;
				
				try {
					val = (bool) client.Get(GCONF_KEY_SHUFFLE);
				} catch (NoSuchKeyException e) {
					client.Set(GCONF_KEY_SHUFFLE, Defaults.ShuffleEnabled);
				} catch (InvalidCastException e) {
					client.Set(GCONF_KEY_SHUFFLE, Defaults.ShuffleEnabled);
				}
				
				return val;
			}
			
			set {
				client.Set(GCONF_KEY_SHUFFLE, value);
			}
		}

		// This functions needs to be cleaned up... right now it's lame
		public bool AutoStart
		{
			get
			{
				string as_file = Path.Combine(Defaults.Gnome.AutoStartDir, "drapes.desktop");
				return File.Exists(as_file);
			}

			set
			{
				string as_file = Path.Combine(Defaults.Gnome.AutoStartDir, "drapes.desktop");

				try {
					// only bother if needed 
					if (AutoStart != value) {

						if (value == true) {
							// Create path need be
							if (!Directory.Exists(Defaults.Gnome.AutoStartDir))
								Directory.CreateDirectory(Defaults.Gnome.AutoStartDir);
							
							StreamWriter sr = new StreamWriter(as_file);
						
							// content of the .desktop file
							sr.WriteLine("[Desktop Entry]");
							sr.WriteLine("Name=Drapes");
							sr.WriteLine("Encoding=UTF-8");
							sr.WriteLine("Version=1.0");
							sr.WriteLine("Exec=drapes");
							sr.WriteLine("X-GNOME-Autostart-enabled=true");
							
							sr.Close();
							
						} else {
							File.Delete(as_file);
						}
					}
				} catch (Exception e) {
					Console.WriteLine("Cannot toggle autostart, reason: {0}", e.Message);
				}
			}
		}
		
		public void GConfKeyChange (object sender, NotifyEventArgs args)
		{
			switch (args.Key) {
				case GCONF_KEY_SO_START:
					if (cbtStartSwitch != null)
						cbtStartSwitch.Active = (bool) args.Value; 
					break;
				case GCONF_KEY_TIMER:
					if (scaleTimer != null)
						scaleTimer.Value = Convert.ToDouble(args.Value);
					break;	
				case GCONF_KEY_MONITOR:
					if (cbtMonitor != null)
						cbtMonitor.Active = (bool) args.Value;
					DrapesApp.WpList.FileSystemMonitor = (bool) args.Value;
					break;
				case GCONF_KEY_MONITOR_DIR:
					if (fcbDir != null)
						fcbDir.SetCurrentFolderUri((string) args.Value);
					DrapesApp.WpList.ChangeMonitorDir();
					break;
				case Defaults.GCONF_STYLE_OPTIONS:
					if (cmbStyle != null)
						cmbStyle.Active = Convert.ToInt32(Style);
					break;
				default:
					Console.WriteLine("Unknown GConf key: {0}", args.Key);
					break;
			}
		}
	}
		
	// The default settings
	public static class Defaults
	{
		private static string monDir = "Documents";
		internal const string GCONF_STYLE_OPTIONS = "/desktop/gnome/background/picture_options";
	
		// Incase we ever want to change it, I guess.
		static public string ApplicationName
		{
			get { return "Desktop Drapes";	}
		}
		
		static public bool ShuffleOnStart
		{
			get { return false; }
		}
		
		static public TimeDelay.Delay SwitchDelay
		{
			get { return TimeDelay.Delay.MIN_15; }
		}
		
		static public bool MonitorEnabled
		{
			get { return false; }
		}
	
		// Default Directory to monitor
		static public string MonitorDirectory
		{
			get {
				return Path.Combine(Environment.GetEnvironmentVariable("HOME"), monDir);
			}
		}

		// We do we store out wallpaper list		
		static public string DrapesWallpaperList
		{
			get {
				return Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".gnome2/drapes.xml");
			}
		}
		
		static public bool ShuffleEnabled
		{
			get { return true; }
		}
		
		// Settings that belong orginaly to Gnome, not us
		public class Gnome
		{
			// Gnome wallaper list, for initial imports 
			static public string WallpaperListFile
			{
				get {
					return Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".gnome2/backgrounds.xml");
				}
			}

			// The user autostart path (i think it's a freedesktop spec path)
			static public string AutoStartDir
			{
				get {
					return Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".config/autostart");
				}
			}
		
			// GConf Wallaper keys
			static public string WallpaperKey
			{
				get {
					return "/desktop/gnome/background/picture_filename";
				}
			}
			
			// GConf Wallpaper drawing enabled
			static public string DrawWallpaperKey
			{
				get {
					return "/desktop/gnome/background/draw_background";
				}
			}
			
			// Wallpaper option key
			static public string PictureOptionsKey
			{
				get {
					return GCONF_STYLE_OPTIONS;
				}
			}
		}
	}
}
