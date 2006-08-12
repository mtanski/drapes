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
using Mono.Unix;
using Gtk;
using Egg;

namespace Drapes
{
	public enum AppletStyle
	{
		APPLET_TRAY,
		APPLET_PANEL
	}

	public class AppletWidget : Gtk.EventBox
	{
		private Egg.TrayIcon		NotificationIcon;
		private AppletStyle			AppletStyle;
		private Gtk.Image			Icon;
		private Tooltips			Tooltips;
		
		public AppletWidget(AppletStyle style)
		{
			// Tooltips
			Tooltips = new Tooltips();
			Tooltips.SetTip(this, Catalog.GetString("Deskop Drapes, click to switch wallpaper"), null);
			
			// Create the icon
			Icon = new Image((string) null, IconSize.Menu);
			Add(Icon);

			// Set enabled status
			Enabled = DrapesApp.Cfg.ShuffleEnabled;

			// Keep track of what kind of applet we are
			// if we are a tray icon, then register a notification area widget
			this.AppletStyle = style;
			if (this.AppletStyle == AppletStyle.APPLET_TRAY)
				CreateNotifyIcon();

			// What shall we do 
			ButtonPressEvent += ButtonPress;
			
			// Show the tray
			ShowAll();
		}

		private void CreateNotifyIcon()
		{
			NotificationIcon = new Egg.TrayIcon(Config.Defaults.ApplicationName);
			NotificationIcon.Add(this);
			NotificationIcon.ShowAll();
		}

		public AppletStyle AppletType
		{
			get {
				return AppletStyle;
			}
		}
			
		public bool Enabled
		{
			set {
				if (value == true) {
					this.Icon.SetFromStock(Stock.ColorPicker, IconSize.Menu);
				} else {
					this.Icon.SetFromStock(Stock.Cancel, IconSize.Menu);
				}
			}
		}

		public void ToggleSwitch()
		{
			DrapesApp.SwitchWallpaper();
		}

		public void ToggleShuffleCheck()
		{
			DrapesApp.Cfg.ShuffleEnabled = !DrapesApp.Cfg.ShuffleEnabled;
			Enabled = DrapesApp.Cfg.ShuffleEnabled;
		}
		
		public void ShowPrefrences()
		{
			if (DrapesApp.ConfigWindow == null)
				DrapesApp.ConfigWindow = new ConfigWindow();
			else
				DrapesApp.ConfigWindow.RaiseWindow();
		}

		// Handles the button clicks on our evenbox
		private void ButtonPress(object sender, ButtonPressEventArgs args)
		{
			switch (args.Event.Button) {
			case 1:	// left click
				ToggleSwitch();
				break;
			case 3: // right click
				if (this.AppletStyle == AppletStyle.APPLET_PANEL)
					break;
				
				// Create, show & connect popup menu
				Menu popup = ShowPopupMenu();
				popup.ShowAll();
				popup.Popup(null, null, TrayPositionFunc, args.Event.Button, args.Event.Time);
				break;
			default:
				// do nothing
				break;
			}
		}

		Menu ShowPopupMenu()
		{
			Menu popup = new Menu();

			// Switch to another wallpaper
			ImageMenuItem next = new ImageMenuItem(Catalog.GetString("_Switch"));
			next.Image = new Image(Stock.GoForward, IconSize.Menu);
			next.Activated += delegate (object sender, EventArgs args)
				{
					ToggleSwitch();
				};
			
			// Enabled
			CheckMenuItem enabled = new Gtk.CheckMenuItem(Catalog.GetString("Shuffle periodicaly"));
			enabled.Active = DrapesApp.Cfg.ShuffleEnabled;
			enabled.Toggled += delegate (object sender, EventArgs args)
				{
					ToggleShuffleCheck();
				};
					
			
			// Configuration
			ImageMenuItem config = new ImageMenuItem(Catalog.GetString("Prefrences"));
			config.Image = new Image(Stock.Preferences, IconSize.Menu);
			config.Activated += delegate (object sender, EventArgs args)
				{
					ShowPrefrences();
				};
			
			// Quit
			ImageMenuItem quit = new ImageMenuItem(Catalog.GetString("_Quit"));
			quit.Image = new Image(Stock.Quit, IconSize.Menu);
			quit.Activated += delegate (object sender, EventArgs args)
				{
					DrapesApp.Quit();
				};
			
			// Add the items to the menu (separators for easy distinctions)
			popup.Add(next);
			popup.Add(new SeparatorMenuItem());
			popup.Add(enabled);
			popup.Add(config);
			popup.Add(new SeparatorMenuItem());
			popup.Add(quit);

			return popup;
		}

		// used the potition fuction from nm-applet as an example
		public void TrayPositionFunc (Menu menu, out int x, out int y, out bool push_in)
		{
			int MenuPosY;
			int PanelW, PanelH;
			
			// Get size of the menu
			Gtk.Requisition ms = menu.Requisition;
			
			// X from orgin of TrayIcon, y will need tweaking
			this.GdkWindow.GetOrigin(out x, out MenuPosY);
			this.GdkWindow.GetSize(out PanelW, out PanelH);
			
			// Do the acctual positioning
			if (MenuPosY + ms.Height >= this.Screen.Height)
				y =  MenuPosY - ms.Height;			// panel bottom
			else
				y = MenuPosY + PanelH ; 			// panel top
	
			// We want it to say inside the current screen		
			push_in = true;
		}
	}
}
