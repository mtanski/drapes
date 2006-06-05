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
using Gtk;
using Vfs = Gnome.Vfs;
using Egg;
using Drapes;
using Config = Drapes.Config;

public class DrapesApp
{
	public static void Main (string[] args)
	{
		new DrapesApp();
	}
	
	// List of all wallpapers
	private	WallPaperList		WpList;
	// The trayicon
	private	TrayIcon			TrayWidget;
	// Settings engine
	static internal Config.Settings		Cfg;
	// Configuration window
	internal static Drapes.ConfigWindow ConfigWindow;
	
	public DrapesApp()
	{
		// Gtk init
		Application.Init();
		Vfs.Vfs.Initialize();
		
		// Load settings for us
		Cfg = new Config.Settings();

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
			Console.WriteLine("Importing Gnome's background list");
			WpList = new WallPaperList(Config.Defaults.Gnome.WallpaperListFile);
		} else {
			Console.WriteLine("Opening wallpaper list");
			WpList = new WallPaperList(Config.Defaults.DrapesWallpaperList);
		}
		
		// tray Icon
		CreateTrayIcon();
		
		// Idle load the files
		GLib.Idle.Add(LazyFileLoader);
		
		// Wallpaper switcher (check every 10 seconds or so)
		GLib.Timeout.Add(20000, TimerSwitcher);
		// Waite a couple secons before doing first random, otl et it lazy load somethings
		GLib.Timeout.Add(5000, OnstartSwitch);
		
		// Go go go
		Application.Run();
	}
	
	// Borowed a lot of this from the mono wiki
	// Will replace it once Gtk 2.10 is out with real tray support
	private void CreateTrayIcon()
	{
		TrayWidget = new TrayIcon(Config.Defaults.ApplicationName);
		
		// event box for clickage
		Gtk.EventBox TrayBox = new EventBox();
		TrayBox.Add(new Image(Stock.MissingImage, IconSize.Menu));
		
		// our menu
		TrayBox.ButtonPressEvent += DrawTrayMenu;
		
		// add it to the tray
		TrayWidget.Add(TrayBox);
		
		// show it
		TrayWidget.ShowAll();
	}
	
	// used the potition fuction from nm-applet as an example
	public void TrayPositionFunc (Menu menu, out int x, out int y, out bool push_in)
	{
		int MenuPosY;
		int PanelW, PanelH;
		
		// Get size of the menu
		Gtk.Requisition ms = menu.Requisition;
		
		// X from orgin of TrayIcon, y will need tweaking
		TrayWidget.GdkWindow.GetOrigin(out x, out MenuPosY);
		TrayWidget.GdkWindow.GetSize(out PanelW, out PanelH);
		
		// Do the acctual positioning
		if (MenuPosY + ms.Height >= TrayWidget.Screen.Height)
			y =  MenuPosY - ms.Height;			// panel bottom
		else
			y = MenuPosY + PanelH ; 			// panel top

		// We want it to say inside the current screen		
		push_in = true;
	}
	
	private void DrawTrayMenu(object o, ButtonPressEventArgs args)
	{
		switch (args.Event.Button) {
			case 3:
				// Menu
				Menu popup = new Menu();
				
				// Switch to another one
				ImageMenuItem next = new ImageMenuItem("_Switch");
				next.Image = new Image(Stock.GoForward, IconSize.Menu);
				next.Activated += OnSwitchClick;
				
				// Enabled
				CheckMenuItem enabled = new Gtk.CheckMenuItem("Shuffle periodicaly");
				enabled.Active = Cfg.ShuffleEnabled;
				enabled.Toggled += OnShuffleChange;
				
				// Configuration
				ImageMenuItem config = new ImageMenuItem("Prefrences");
				config.Image = new Image(Stock.Preferences, IconSize.Menu);
				config.Activated += OnClickConfig;
				
				// Quit
				ImageMenuItem quit = new ImageMenuItem("_Quit");
				quit.Image = new Image(Stock.Quit, IconSize.Menu);
				quit.Activated += OnClickClose;
				
				// Add the items to the menu (separators for easy distinctions)
				popup.Add(next);
				popup.Add(new SeparatorMenuItem());
				popup.Add(enabled);
				popup.Add(config);
				popup.Add(new SeparatorMenuItem());
				popup.Add(quit);
				
				// This needs to come before the popup, so we get the right size
				popup.ShowAll();
				
				// Connect the menu
				popup.Popup(null, null, TrayPositionFunc, args.Event.Button, args.Event.Time);
				
				break;
			default:
				// do nothing
				break;
		}
	}
	
	// Force switch to random wallpaper
	private void OnSwitchClick(object o, EventArgs args)
	{
		Wallpaper w = WpList.Random(Cfg.Wallpaper);
	
		if (w != null)
			Cfg.Wallpaper = w.File;
	}
	
	DateTime LastSwitch = DateTime.Now;
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
		
		Wallpaper w = WpList.Random(Cfg.Wallpaper);
		if (w != null)
			Cfg.Wallpaper = w.File;
		
		LastSwitch = (DateTime) DateTime.Now;
	
		return true;
	}
	
	private bool OnstartSwitch()
	{
		if (!Cfg.ShuffleOnStart)
			return false;

		Wallpaper w = WpList.Random(Cfg.Wallpaper);
		if (w != null)
			Cfg.Wallpaper = w.File;
	
		LastSwitch = (DateTime) DateTime.Now;
		return false;
	}
	
	// Enable automatic wallpaper switch
	private void OnShuffleChange(object o, EventArgs args)
	{
		// Invert it's value
		Cfg.ShuffleEnabled = !Cfg.ShuffleEnabled;
	}
	
	// Handle disallow creation of multiple windoww
	private void OnClickConfig(object o, EventArgs args)
	{
		if (ConfigWindow == null)
			ConfigWindow = new Drapes.ConfigWindow(ref WpList);
		else
			ConfigWindow.RaiseWindow();
	}
	
	private void OnClickClose(object o, EventArgs args)
	{
		// Stop any idle handlers
		GLib.Idle.Remove(LazyFileLoader);
		GLib.Idle.Remove(WpList.ThumbCleanup);
	
		// Save changes to the list
		if (WpList != null)
			WpList.SaveList(Config.Defaults.DrapesWallpaperList);
		
		// Shutdown the vfs subsystem
		Vfs.Vfs.Shutdown();
		
		// Exit
		Application.Quit();
	}
	
	internal static int LastLoad = 0;
	public bool LazyFileLoader()
	{
		// Are we done loading
		if (LastLoad >= WpList.NumberBackgrounds) {
			Console.WriteLine("Done loading image info");
			return false;
		}
		
		// If it's already done (dunno how) skip it
		if (WpList[LastLoad].Initlized)
			return true;
		
		// Load more info about the image
		WpList[LastLoad].ForceLoadAttr();
		
		// Add it to the config window if open
		if (ConfigWindow != null)
			ConfigWindow.AddWallpaper(LastLoad);
		
		// Do the next one later
		LastLoad++;
		
		return true;
	}
}

