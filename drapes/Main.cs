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
using Gtk;
using Vfs = Gnome.Vfs;
using Egg;
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
		private				Gnome.Program			Program;
        private             Gnome.Client            Client;
		
		private DrapesApp(string[] args)
		{
			// Initialize the interntionalization bits
			Catalog.Init("drapes", CompileOptions.GnomeLocaleDir);
			
			// 
			Program = new Gnome.Program("Drapes", CompileOptions.Version, Gnome.Modules.UI, args);

            Client = new Gnome.Client();
            Client.SaveYourself += Shutdown;
			
			// Load settings for us
			Cfg = new Config.Settings();

			// Process application arguments
			ProcessArgs(args);
	
			// If Monitor is enabled, make sure the dir exists as well
			if (Cfg.MonitorEnabled) {
				Vfs.Uri d = new Vfs.Uri(Cfg.MonitorDirectory);
				
				// Don't create anything if it exists
				if (d.Exists)
					Vfs.Directory.Create(d, Vfs.FilePermissions.UserAll);
			}
	
			// Check if we already have file with wallpapers, else assume first start
			Vfs.Uri cfg = new Vfs.Uri(Config.Defaults.DrapesWallpaperList);
			if (!cfg.Exists) {
				Console.WriteLine(Catalog.GetString("Importing Gnome's background list"));
				WpList = new WallPaperList(Config.Defaults.Gnome.WallpaperListFile);
				
				// Lets save it in our own format
				WpList.SaveList(Config.Defaults.DrapesWallpaperList);
			} else {
				Console.WriteLine(Catalog.GetString("Opening wallpaper list"));
				WpList = new WallPaperList(Config.Defaults.DrapesWallpaperList);
			}
			
			// Wallpaper switcher (check every 10 seconds or so)
			GLib.Timeout.Add(20000, TimerSwitcher);
			// Waite a couple secons before doing first random, otl et it lazy load somethings
			GLib.Timeout.Add(5000, OnstartSwitch);
			
			// tray Icon
			if (AppletStyle == AppletStyle.APPLET_TRAY) {
				new AppletWidget(AppletStyle, null);
				this.Program.Run();
			} else
				_Gnome.PanelAppletFactory.Register(typeof (DrapesApplet));
		}
		
		static DateTime LastSwitch = DateTime.Now;
		private bool TimerSwitcher()
		{
			// Autoswitching tempoarly disabled
			if (!Cfg.ShuffleEnabled)
				return true;
			
			// Never switch the wallpaper
			if (Cfg.SwitchDelay == Config.TimeDelay.Delay.MIN_NEVER)
				return true;
			
			DateTime next = LastSwitch.AddMinutes(Config.TimeDelay.Int32(Cfg.SwitchDelay));
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

        private void Shutdown(object sender, Gnome.SaveYourselfArgs args)
        {
            Console.WriteLine(Catalog.GetString("Notified of shutdown"));
            
            if (WpList != null)
                WpList.SaveList(Config.Defaults.DrapesWallpaperList);
        }
	
		private void ProcessArgs(string[] Args)
		{
			foreach (string cur in Args) {
				switch (cur) {
				case "--panel-applet":
					AppletStyle = AppletStyle.APPLET_PANEL;
					break;
				default:
					Console.WriteLine(Catalog.GetString("Sorry unknow argument: {0}"), cur);
					break;
				}
			}
			
		}
	}
}
