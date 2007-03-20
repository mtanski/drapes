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
using Mono.Unix;
using GConf;
using Gtk;
using Config = Drapes.Config;

namespace Drapes.Config
{
	public static class Delay
	{
		public static string ToText(DelayEnum d)
		{
			switch (d) {
				case DelayEnum.MIN_5:
					return Catalog.GetString("5 minutes");
				case DelayEnum.MIN_10:
					return Catalog.GetString("10 minutes");
				case DelayEnum.MIN_15:
					return Catalog.GetString("15 minutes");
				case DelayEnum.MIN_20:
					return Catalog.GetString("20 minutes");
				case DelayEnum.MIN_30:
					return Catalog.GetString("30 minutes");
				case DelayEnum.MIN_45:
					return Catalog.GetString("45 minutes");
				case DelayEnum.MIN_60:
					return Catalog.GetString("1 hour");
				case DelayEnum.MIN_90:
					return Catalog.GetString("1 hour 30 minutes");
				case DelayEnum.MIN_120:
					return Catalog.GetString("2 hours");
				case DelayEnum.MIN_NEVER:
				default:
					return Catalog.GetString("Never");
			}
		}
	
		public static int Int32(DelayEnum d)
		{
			switch (d) {
				case DelayEnum.MIN_5:
					return 5;
				case DelayEnum.MIN_10:
					return 10;
				case DelayEnum.MIN_15:
					return 15;
				case DelayEnum.MIN_20:
					return 20;
				case DelayEnum.MIN_30:
					return 30;
				case DelayEnum.MIN_45:
					return 45;
				case DelayEnum.MIN_60:
					return 60;
				case DelayEnum.MIN_90:
					return 90;
				case DelayEnum.MIN_120:
					return 120;
				case DelayEnum.MIN_NEVER:
				default:
					return 0;
			}
		}
	
		// delay
		public enum DelayEnum {
			MIN_5,		MIN_10,		MIN_15,		MIN_20,
			MIN_30,		MIN_45,		MIN_60,		MIN_90,
			MIN_120,	MIN_NEVER
		}
	}
        
    public static class Style
    {
        public static StyleEnum FromText(string name)
        {
            switch (name) {
                case "stretched":
                    return StyleEnum.STYLE_FILL;
                case "scaled":
                    return StyleEnum.STYLE_SCALE;
                case "zoom":
                    return StyleEnum.STYLE_ZOOM;
                case "centered":
                    return StyleEnum.STYLE_CENTER;
                case "wallpaper":
                    return StyleEnum.STYLE_TILED;
                case "none":
                default :
                    return StyleEnum.STYLE_NONE;
            }
        }
        
        public static string ToText(StyleEnum src) {
            switch (src) {
                case StyleEnum.STYLE_CENTER:
                    return "centered";
                case StyleEnum.STYLE_FILL:
                    return "stretched";
                case StyleEnum.STYLE_SCALE:
                    return "zoom";
                case StyleEnum.STYLE_TILED:
                    return "wallpaper";
                case StyleEnum.STYLE_ZOOM:
                    return "zoon";
                case StyleEnum.STYLE_NONE:
                default:
                    return "none";
            }
        }
        
    
        // Styles
        public enum StyleEnum {
            STYLE_CENTER = 0,
            STYLE_FILL = 1,
            STYLE_SCALE = 2,
            STYLE_TILED = 3,
            STYLE_ZOOM = 4,             // wtf? welcome back 1995
            // 5 - spacer in the menu
            STYLE_NONE = 6
        }
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

        // Current internal settings
        Delay.DelayEnum     _timeDelay;
        bool                _monitorEnabled;
        string              _monitorDirectory;
        bool                _startup;
        bool                _shuffle;
        Style.StyleEnum     _style;

        // dosen't get saved
        bool                debug = false;
		
		// Constructor
		public Settings()
		{
			client = new GConf.Client();

            // loads initial settings
            try {
                _timeDelay = (Delay.DelayEnum) client.Get(GCONF_KEY_TIMER);
            } catch (Exception) {
                _timeDelay = Defaults.SwitchDelay;
            }

            try {
                _monitorEnabled = (bool) client.Get(GCONF_KEY_MONITOR);
            } catch (Exception) {
                _monitorEnabled = Defaults.MonitorEnabled;
            }

            try {
                _monitorDirectory = (string) client.Get(GCONF_KEY_MONITOR_DIR);
            } catch (Exception) {
                _monitorDirectory = Defaults.MonitorDirectory;
            }

            try {
                _startup = (bool) client.Get(GCONF_KEY_SO_START);
            } catch (Exception) {
				_startup = Defaults.ShuffleOnStart;
            }
			
			try {
				_shuffle = (bool) client.Get(GCONF_KEY_SHUFFLE);
			} catch (Exception) {
				_shuffle = Defaults.ShuffleEnabled;
			}
            
            try {
                _style = Config.Style.FromText((String) client.Get(Defaults.GCONF_STYLE_OPTIONS));
            } catch (Exception) {
                _style = 0;
            }

            // setup key change callbacks
			client.AddNotify(GCONF_APP_PATH, this.GConfKeyChange);
			client.AddNotify(GCONF_KEY_MONITOR, this.GConfKeyChange);
			client.AddNotify(GCONF_KEY_MONITOR_DIR, this.GConfKeyChange);
			client.AddNotify(GCONF_KEY_SHUFFLE, this.GConfKeyChange);
			client.AddNotify(GCONF_KEY_TIMER, this.GConfKeyChange);
            client.AddNotify(Defaults.GCONF_STYLE_OPTIONS, this.GConfKeyChange);

		}
        
        public event BoolSettingChangeHandler      ShuffleOnStartChanged;
        public event TimeSettingsChangeHandler     SwitchDelayChanged;
        public event BoolSettingChangeHandler      MonitorEnabledChanged;
        public event StringSettingChangeHandler    MonitorDirectoryChanged;
        public event StyleSettingsChangeHander     StyleChanged;
        

        public bool Debug
        {
            get {
                return debug;
            }

            set {
                debug = value;
            }
        }
		
		// Load a randomwallaper on start
		public bool ShuffleOnStart
		{
			get {
                return _startup;
			}
			
			set {
                if (_startup == value)
                    return;

                _startup = value;
				client.Set(GCONF_KEY_SO_START, value);
                if (this.ShuffleOnStartChanged != null)
                    this.ShuffleOnStartChanged(this, new SettingsChangeEvent<bool> (value, !value, SettingsChangeSrc.Application));
			}
		}

		public Delay.DelayEnum SwitchDelay
		{
			get {
                return _timeDelay;
			}
			
			set {
                // stop feeding me crap input
                if (value > Delay.DelayEnum.MIN_NEVER || value < Delay.DelayEnum.MIN_5)
                    value = Defaults.SwitchDelay;
                
                if (_timeDelay == value)
                    return;
                
                Delay.DelayEnum old = _timeDelay;
                _timeDelay = value;
				client.Set(GCONF_KEY_TIMER, (int) value);
                if (this.SwitchDelayChanged != null)
                    this.SwitchDelayChanged(this, new SettingsChangeEvent<Delay.DelayEnum> (_timeDelay, old, SettingsChangeSrc.Application));
			}
		}

		public bool MonitorEnabled
		{
			get {
                return _monitorEnabled;
			}
			
			set {
                // ignore same values
                if (_monitorEnabled == value)
                    return;
                
                client.Set(GCONF_KEY_MONITOR, value);
                if (this.MonitorEnabledChanged != null)
                    this.MonitorEnabledChanged(this, new SettingsChangeEvent<bool> (value, !value, SettingsChangeSrc.Application));
			}
		}
		
		public string MonitorDirectory
		{
			get  {
                switch (_monitorDirectory) {
                    case "":
                    case "unset":
                        return null;
                }
                
                return _monitorDirectory;
			}
			
			set {
                // ignore the same values
                if (_monitorDirectory == value)
                    return;
                
                if (Directory.Exists(value) == false) {
                    if (this.Debug == true)
                        Console.WriteLine("Monitor directory: {0} dosen't exist.", value);
                    
                    _monitorDirectory = null;
                    DrapesApp.WpList.FileSystemMonitor = false;
                } else {
                    string oldDir = _monitorDirectory;
                    _monitorDirectory = value;
                    client.Set(GCONF_KEY_MONITOR_DIR, value);
                    if (this.MonitorDirectoryChanged != null)
                        this.MonitorDirectoryChanged(this, new SettingsChangeEvent<string> (_monitorDirectory, oldDir, SettingsChangeSrc.Application));
                }
			}
		}
		
		public Style.StyleEnum Style {
			get {
                return _style;
			}
		
			set {
                if (_style == value)
                    return;
                
                Style.StyleEnum oldValue = _style;
                _style = value;
                client.Set(Defaults.Gnome.PictureOptionsKey, Config.Style.ToText(_style));
                
                if (this.StyleChanged != null)
                    this.StyleChanged(this, new SettingsChangeEvent<Style.StyleEnum> (_style, oldValue, SettingsChangeSrc.Application));
			}
		}
		
		public string Wallpaper
		{
			get  {
				string val = "";
			
				try {
					val = (string) client.Get(Defaults.Gnome.WallpaperKey);
				} catch (NoSuchKeyException) {
					client.Set(Defaults.Gnome.WallpaperKey, "");
				} catch (InvalidCastException) {
					client.Set(Defaults.Gnome.WallpaperKey, "");
				}
				
				return val;
			}
			
			set {
				client.Set(Defaults.Gnome.WallpaperKey,  value);
			}
		}
		
		public event BoolSettingChangeHandler SuffleEnabledChanged;
		public bool ShuffleEnabled
		{
			get {
				return _shuffle;
			}
			
			set {
				if (_shuffle == value)
					return;
					
				_shuffle = value;
				client.Set(GCONF_KEY_SHUFFLE, value);
				this.MonitorEnabledChanged(this, new SettingsChangeEvent<bool> (value, !value, SettingsChangeSrc.Application));
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
					Console.WriteLine(Catalog.GetString("Cannot toggle autostart, reason: {0}"), e.Message);
				}
			}
		}
		
        public void GConfKeyChange(object sender, NotifyEventArgs args)
        {
            switch (args.Key) {
                case GCONF_KEY_SO_START:
                    if (_startup == (bool) args.Value)
                        break;
                    
                    _startup = (bool) args.Value;
                    if (this.ShuffleOnStartChanged != null)
                        this.ShuffleOnStartChanged(this, new SettingsChangeEvent<bool>(_startup, !_startup, SettingsChangeSrc.Gconf));
                    
                    break;
                case GCONF_KEY_TIMER:
                    if (_timeDelay == (Delay.DelayEnum) args.Value)
                        break;
                    
                    int newTime = (int) args.Value;
                    if (newTime > (int) Delay.DelayEnum.MIN_NEVER || newTime < (int) Delay.DelayEnum.MIN_5) {
                        client.Set(GCONF_KEY_TIMER, (int) _timeDelay);
                        break;
                    }
                    
                    Delay.DelayEnum oldTime = _timeDelay;
                    _timeDelay = (Delay.DelayEnum) args.Value;
                    
                    if (this.SwitchDelayChanged != null)
                        this.SwitchDelayChanged(this, new SettingsChangeEvent<Delay.DelayEnum>(_timeDelay, oldTime, SettingsChangeSrc.Gconf));
                    
                    break;
                case GCONF_KEY_MONITOR:
                    // debounce, and ignore the same values
                    if (_monitorEnabled == (bool) args.Value)
                        break;
                    
                    _monitorEnabled = (bool) args.Value;
                    if (this.MonitorEnabledChanged != null)
                        this.MonitorEnabledChanged(this, new SettingsChangeEvent<bool>(_monitorEnabled, !_monitorEnabled, SettingsChangeSrc.Gconf));
                    
                    break;
                case GCONF_KEY_MONITOR_DIR:
                    // debounce, and ignore same values
                    if (_monitorDirectory == (string) args.Value)
                        break;
                    
                    if (Directory.Exists((string) args.Value) == false) {
                        if (_monitorDirectory == null)
                            return;
                        
                        if (DrapesApp.Cfg.Debug == true)
                            Console.WriteLine("Directory {0} dosen't exist", (string) args.Value);
                        _monitorDirectory = null;
                        DrapesApp.WpList.FileSystemMonitor = false;
                        break;
                    }
                    
                    string oldDir = _monitorDirectory;
                    _monitorDirectory = (string) args.Value;
                    if (this.MonitorDirectoryChanged != null)
                        this.MonitorDirectoryChanged(this, new SettingsChangeEvent<string>(_monitorDirectory, oldDir, SettingsChangeSrc.Gconf));
                    
                    break;
                case Defaults.GCONF_STYLE_OPTIONS:
                    Config.Style.StyleEnum oldStyle = _style;
                    _style = Config.Style.FromText((String) args.Value);
                    if (this.StyleChanged != null)
                        this.StyleChanged(this, new SettingsChangeEvent<Config.Style.StyleEnum>(_style, oldStyle, SettingsChangeSrc.Gconf));
                    break;
                default:
                    if (debug == true)
                        Console.WriteLine(Catalog.GetString("Unknown GConf key: {0}"), args.Key);
                    
                    break;
            }
        }
    }
		
	// The default settings
	public static class Defaults
	{
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
		
        static public Delay.DelayEnum SwitchDelay
		{
			get { return Delay.DelayEnum.MIN_15; }
		}
		
		static public bool MonitorEnabled
		{
			get { return false; }
		}
	
		// Default Directory to monitor
		static public string MonitorDirectory
		{
			get {
                string path;

                if (Directory.Exists(Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Photos"))) {
                    path = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Photos");
                } else if (Directory.Exists(Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Documents"))) {
                    path = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Documents");
                } else
                    path = Environment.GetEnvironmentVariable("HOME");

				return path;
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
    
    public enum SettingsChangeSrc {
        Application,
        Gconf
    }
    
    public delegate void BoolSettingChangeHandler(object sender, SettingsChangeEvent<bool> args);
    public delegate void StringSettingChangeHandler(object sender, SettingsChangeEvent<string> args);
    public delegate void TimeSettingsChangeHandler(object sender, SettingsChangeEvent<Delay.DelayEnum> args);
    public delegate void StyleSettingsChangeHander(object sender, SettingsChangeEvent<Style.StyleEnum> args);
    
    public class SettingsChangeEvent<T> : EventArgs
    {
        T _oldVal;
        T _val;
        SettingsChangeSrc _src;
        
        public SettingsChangeEvent(T current, T old, SettingsChangeSrc source)
        {
            _val = current;
            _oldVal = old;
            _src = source;
        }
        
        
        public T OldValue {
            get {
                return _oldVal;
            }
        }
        
        public T Value {
            get {
                return _val;
            }
        }
        
        public SettingsChangeSrc Source {
            get {
                return _src;
            }
        }
    }
}
