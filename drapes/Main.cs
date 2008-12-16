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

// project created on 4/26/2006 at 8:40 PM
using System;
using Mono.Unix;
using Gdk;
using Gtk;
using Vfs = Gnome.Vfs;
using Drapes;
using Config = Drapes.Config;

namespace Drapes {
	
	public class DrapesApp
	{

		public static void Main (string[] args)
		{
			new DrapesApp(args);
		}
		
		// List of all wallpapers
		internal static		WallPaperList			WpList;
		// Settings engine
		internal static		Config.Settings			Cfg;
		// Configuration window
		internal static		Drapes.ConfigWindow		ConfigWindow;
		// Panel/Tray applet
		internal static		AppletStyle				AppletStyle;
		
		private DrapesApp(string[] args)
		{
			// Initialize the i18n bits
			Catalog.Init("drapes", CompileOptions.GnomeLocaleDir);
			
			Gtk.Application.Init("Drapes", ref args);

			// Load settings for us
			Cfg = new Config.Settings();

			// Process application arguments
			ProcessArgs(args);

			// If Monitor is enabled, make sure the dir exists as well
			if (Cfg.MonitorEnabled == true)
				// If it dosen't exist turn of monitoring
				if (Cfg.MonitorDirectory == null) {
					Cfg.MonitorEnabled = false;
                    Cfg.MonitorDirectory = "unset";
                }
	
			// Check if we already have file with wallpapers, else assume first start
			Vfs.Uri cfg = new Vfs.Uri(Config.Defaults.DrapesWallpaperList);
			if (!cfg.Exists) {
                if (Cfg.Debug == true)
                    Console.WriteLine(Catalog.GetString("Importing Gnome's background list: {0}"), Config.Defaults.Gnome.WallpaperListFile);

				WpList = new WallPaperList(Config.Defaults.Gnome.WallpaperListFile);
				
				// Lets save it in our own format
				WpList.SaveList(Config.Defaults.DrapesWallpaperList);
			} else {
                if (Cfg.Debug == true)
                    Console.WriteLine(Catalog.GetString("Opening wallpaper list: {0}"), Config.Defaults.DrapesWallpaperList);

				WpList = new WallPaperList(Config.Defaults.DrapesWallpaperList);
			}
			
			// Wallpaper switcher (check every 10 seconds or so)
			GLib.Timeout.Add(20000, TimerSwitcher);
			// Waite a couple secons before doing first random, otl et it lazy load somethings
			GLib.Timeout.Add(5000, OnstartSwitch);
			
			// tray Icon
			if (AppletStyle == AppletStyle.APPLET_TRAY) {
				new AppletWidget(AppletStyle, null);
				Gtk.Application.Run();
			} else
				_Gnome.PanelAppletFactory.Register(typeof (DrapesApplet));
		}
		
		static DateTime LastSwitch = DateTime.Now;
		private bool TimerSwitcher()
		{
			// Autoswitching temporarily disabled
			if (!Cfg.ShuffleEnabled)
				return true;
			
			// Never switch the wallpaper
			if (Cfg.SwitchDelay == Config.Delay.DelayEnum.MIN_NEVER)
				return true;
			
			DateTime next = LastSwitch.AddMinutes(Config.Delay.Int32(Cfg.SwitchDelay));
			// Are we there yet?
			if (DateTime.Now < next)
				return true;
			
			SwitchWallpaper();
		
			return true;
		}
		
		private bool OnstartSwitch()
		{
			if (!Cfg.ShuffleOnStart)
				return false;
	
			SwitchWallpaper();
		
			return false;
		}

		public static void SwitchWallpaper()
		{
			Wallpaper w = WpList.Random(Cfg.Wallpaper);
			if (w != null) {
				Cfg.Wallpaper = w.File;
								
				// Update the timer too
				LastSwitch = (DateTime) DateTime.Now;
				
				Console.WriteLine(Catalog.GetString("Wallpaper switch to: {0}"), w.File);
			} else {
				Console.WriteLine(Catalog.GetString("Wallpaper switch failed; No wallpapers?"));
			}
		}
		
		public static void Quit()
		{
            // The list knows how to handle it's own clean up
            WpList.Quit();
			
			// Shutdown the vfs subsystem
			Vfs.Vfs.Shutdown();
			
			// Exit
			Application.Quit();
		}
        
        public static void OpenHelp(string id, Gdk.Screen screen)
        {
            // default id
            if (id == null)
                id = "drapes-intro";
            
            if (Cfg.Debug == true)
                Console.WriteLine("Opening help file: ghelp:drapes");
            
            try {
                Gnome.Help.DisplayOnScreen("drapes", id, screen);
            } catch (Exception) {
                Gnome.Help.DisplayUriOnScreen("ghelp:drapes", screen);
            } finally {
                if (Cfg.Debug == true)
                    Console.WriteLine("Failed to open: ghelp:drapes");
            }
        }
        
        public static void OpenAbout()
        {
            new About();
        }

		private void ProcessArgs(string[] Args)
		{
			foreach (string cur in Args) {
				switch (cur) {
				case "--panel-applet":
					AppletStyle = AppletStyle.APPLET_PANEL;
					break;
                case "--debug":
                    Console.WriteLine("Running with extra debug output");
                    DrapesApp.Cfg.Debug = true;
                    break;
				default:
					Console.WriteLine(Catalog.GetString("Sorry unknow argument: {0}"), cur);
					break;
				}
			}
			
		}
	}
}
