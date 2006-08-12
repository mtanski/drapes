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
using Mono.Posix;
using Gtk;
using Glade;
using Drapes;
using Config = Drapes.Config;
using Res = Drapes.ResolutionProperties;

namespace Drapes
{

	public class ConfigWindow
	{
	
		public ConfigWindow()
		{
			// Glade autoconnect magic
			Glade.XML gxml = new Glade.XML (null, "drapes.glade", "winPref", null);
			gxml.Autoconnect (this);
			
			// the window it self
			winPref.DeleteEvent += OnWindowDelete;
			
			// add/remove wallpaper buttons
			btnRemove.Clicked += onRemoveButtonClick;	
			btnAdd.Clicked += onAddButtonClick;
			
			// style selection
			cmbStyle.InsertText(0, "Centered");
			cmbStyle.InsertText(1, "Fill Screen");
			cmbStyle.InsertText(2, "Scaled");
			cmbStyle.InsertText(3, "Zoom");
			cmbStyle.InsertText(4, "Tiled");
			cmbStyle.Active = (int) DrapesApp.Cfg.Style;
			cmbStyle.Changed += onStyleChanged;

			// start on login button
			cbtAutoStart.Active = DrapesApp.Cfg.AutoStart;
			cbtAutoStart.Toggled += onAutoStartToggled;
		
			// Bottom butons
			btnClose.Clicked += onCloseButtonClick;
			
			// B: General Tab
			
			// CheckButton: Switch wallpaper on start
			cbtStartSwitch.Clicked += OnStartupChanged;
			cbtStartSwitch.Active = DrapesApp.Cfg.ShuffleOnStart;
			DrapesApp.Cfg.ShuffleOnStartWidget(cbtStartSwitch);
			
			// HScale: Wallaper switch timer
			Gtk.Adjustment adjTimer= new Gtk.Adjustment(1.0, 0.0, 9.0, 1.0, 1.0, 0.0);
			scaleTimer.Adjustment = adjTimer;		
			scaleTimer.ChangeValue += OnTimerChangeValueEvent;
			scaleTimer.FormatValue += TimerFormatValue;
			scaleTimer.Value = (double) DrapesApp.Cfg.SwitchDelay;
			DrapesApp.Cfg.SwitchDelayWidget(scaleTimer);
			
			// CheckButton: Monitor directory toggle
			cbtMonitor.Clicked += OnMonitorChanged;
			cbtMonitor.Active = DrapesApp.Cfg.MonitorEnabled;
			DrapesApp.Cfg.MonitorEnabledWidget(cbtMonitor);
			
			// The FileChooserButton
			fcbDir.Sensitive = DrapesApp.Cfg.MonitorEnabled;
			fcbDir.SetCurrentFolderUri(DrapesApp.Cfg.MonitorDirectory);
			fcbDir.CurrentFolderChanged += OnMonitorDirChanged;
			DrapesApp.Cfg.MonitorDirectoryWidget(fcbDir);

			// E: General Tab
			// B: Treeview wallpaper list
			
			// The one cell we'll be using
			Gtk.TreeViewColumn tvc = new Gtk.TreeViewColumn();
			tvc.Title = "selection";

			// Toggle cell renderer needs some special setup, it dosen't handle the toggle it self, it needs a call back
			Gtk.CellRendererToggle tg = new Gtk.CellRendererToggle();
			tg.Toggled += OnWallPaperToggle;
			tg.Xpad = 4;
			
			// Preview Image & Description
			Gtk.CellRendererPixbuf px = new Gtk.CellRendererPixbuf();
			Gtk.CellRendererText tx = new Gtk.CellRendererText();
			tx.Xpad = 4;
			px.Mode = CellRendererMode.Inert;
			tx.Mode = CellRendererMode.Inert;
			
			// Pack everything in one cell
			tvc.PackStart(tg, false);
			tvc.PackStart(px, false);
			tvc.PackStart(tx, true);
				
			// Our custom column rendering function
			tvc.SetCellDataFunc(tg, RenderList);
			tvc.SetCellDataFunc(px, RenderList);
			tvc.SetCellDataFunc(tx, RenderList);
			
			// Just one column that everything is shoved into
			tvBgList.AppendColumn(tvc);
			
			// Tree store
			// 1st we store the key of the wallpaper
			// 2nd we store the section name (if needed)
			tsEntries = new Gtk.TreeStore(typeof(string), typeof(string));
			
			// The various "sorters"
			tiMatch = tsEntries.AppendValues(null, "Perfect fit");
			tiAsp43 = tsEntries.AppendValues(null, "Regular 4:3");
			tiAspWide = tsEntries.AppendValues(null, "Widescreen");
			tiAspMisc = tsEntries.AppendValues(null, "Other");
			
			// Add wallpapers to the Config window
			foreach (Wallpaper w in DrapesApp.WpList)
				AddWallpaper(w.File);

			// We need a filter to get rid of all the empty sections
			tmfFilter = new Gtk.TreeModelFilter(tsEntries, null);
			tmfFilter.VisibleFunc = FilterEmptySections;
			
			// Double click on a row (switch wallpaper, or expand collapse category)
			tvBgList.RowActivated += onRowDoubleClick;

			// The filter is the "proxy" for the TreeView model 
			tvBgList.Model = tmfFilter;
				
			// Show everything
			tvBgList.ExpandAll();
			
			// E: Treeview wallpaper list
		}
		
		public void AddWallpaper(string key)
		{
			if (key == null)
				return;
				
			// skip non-intilized files
			if (DrapesApp.WpList[key].Initlized == false)
				return;
				
			// don't show deleted files
			if (DrapesApp.WpList[key].Deleted)
				return;
			
			// Perfectly matching resolution
			if (DrapesApp.WpList[key].MatchScreen()) {
				tsEntries.AppendValues(tiMatch, key, null);
			} else {
				switch (DrapesApp.WpList[key].Aspect) {
				case Res.Aspect.ASPECT_43:
					tsEntries.AppendValues(tiAsp43, key, null);
					break;
				case Res.Aspect.ASPECT_WIDE:
					tsEntries.AppendValues(tiAspWide, key, null);
					break;
				case Res.Aspect.ASPECT_OTHER:
				default:
					tsEntries.AppendValues(tiAspMisc, key, null);
					break;
				}
			}
			
			if (tmfFilter != null)
				tmfFilter.Refilter();
		}
		
		// The main window
		[Widget] Window				winPref;
		// Things in the general tab
		[Widget] HScale				scaleTimer;
		[Widget] Button				btnClose;
		[Widget] CheckButton		cbtAutoStart;
		[Widget] CheckButton		cbtStartSwitch;
		[Widget] CheckButton		cbtMonitor;
		[Widget] FileChooserButton	fcbDir;
		// Add/Remove Style
		[Widget] Button				btnAdd;
		[Widget] Button				btnRemove;
		[Widget] ComboBox			cmbStyle;
		// The Treeview
		[Widget] TreeView			tvBgList;
		Gtk.TreeStore				tsEntries;
		// the filter
		Gtk.TreeModelFilter			tmfFilter;
		// Diffrent "sections" of the treeview
		Gtk.TreeIter				tiMatch;
		Gtk.TreeIter				tiAsp43;
		Gtk.TreeIter				tiAspWide;
		Gtk.TreeIter				tiAspMisc;

		// Update gnome wallaper style
		private void onStyleChanged(object sender, EventArgs args)
		{
			DrapesApp.Cfg.Style = (Config.Style) (sender as Gtk.ComboBox).Active; 
		}

		private void onAutoStartToggled(object sender, EventArgs args)
		{
			DrapesApp.Cfg.AutoStart = (sender as Gtk.ToggleButton).Active;
		}

		private void ImportDirectory(string dir)
		{
			
		}

		private void ImportFiles(string[] inpfiles)
		{
			foreach (string file in inpfiles) {
				Wallpaper w = new Wallpaper();

				Console.WriteLine("Adding wallpaper file: {0}", file);
				
				// Delay load it, it'll get picked up automaticaly anywas
				w.LoadFileDelayed(file);
				w.Enabled = true;

				DrapesApp.WpList.Append(w);
			}
		}
		
		// Add more wallpapers
		private void onAddButtonClick (object sender, EventArgs args)
		{
			FileChooserDialog fc = new FileChooserDialog("Add wallpaper", winPref, FileChooserAction.Open);

			// Settings
			fc.LocalOnly = true;				// Only local files
			fc.SelectMultiple = true;			// Users can select multiple images at a time
			fc.Filter = new FileFilter();		// Filter
			fc.Filter.AddPixbufFormats();		// Add pixmaps
			
			// Add buttons
			fc.AddButton(Stock.Cancel, ResponseType.Cancel);
			fc.AddButton(Stock.Open , ResponseType.Ok);
			
			// Try to goto the monitor dir if monitoring is enabled, else goto documents
			if (DrapesApp.Cfg.MonitorEnabled == true)
				fc.SetUri(DrapesApp.Cfg.MonitorDirectory);
			else
				fc.SetUri(Environment.GetEnvironmentVariable("HOME") + "/Documents");
			
			// Show the dialog
			int r = fc.Run();
		
			// Process file
			if ((ResponseType) r == ResponseType.Ok)
				ImportFiles(fc.Filenames);
		
			// Get rid of the window
			fc.Destroy();
		}
		
		// Remove backgrouds from the list
		private void onRemoveButtonClick (object sender, EventArgs args)
		{
			// First we need to get a TreeSelection representing selected nodes
			TreeSelection 	sel = tvBgList.Selection;
			TreeModel		model;
			TreePath[]		paths;
			
			paths = sel.GetSelectedRows(out model);
			
			foreach (TreePath p in paths)
			{
				TreeIter		iter;
				// Real TreeStore entities
				TreePath		rPath;
				TreeStore		rModel;
				// wallpaper key
				string			key;
	
				// Get the real TreeStore path from TreeFilter
				rModel = (TreeStore) (model as TreeModelFilter).Model;
				rPath = (model as TreeModelFilter).ConvertPathToChildPath(p);
				
				// Retrive the TreeStore out of TreeFilter
				rModel.GetIter(out iter, rPath);
				
				// Get index
				key = (string) rModel.GetValue(iter, 0);

				// Cannot remove a section
				if (key == null)
					return;
					
				// Delete the wallpaper
				DrapesApp.WpList.SetDelete(key);
				
				// Remove the node from the acctual TreeStore
				rModel.Remove(ref iter);
	
				// Update our filter
				tmfFilter.Refilter();
			}
		}
		
		// If a user dosen't have any wallpaper to display in a section, don't show it
		private bool FilterEmptySections(TreeModel model, TreeIter iter)
		{
			string key = (string) model.GetValue(iter, 0);
			
			// we only want to filter out "sections" not individual wallpapers 
			if (key == null)
				return model.IterHasChild(iter);

			return true;
		}
		
		// This basicaly performs the rendering of each row (on a cell by cell basis)
		private void RenderList (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			string	key = (string) model.GetValue(iter, 0);
			string	h = (string) model.GetValue(iter, 1);
		
			// Root nodes
			if (key == null) {			
				if (cell is Gtk.CellRendererToggle) {
					(cell as Gtk.CellRendererToggle).Visible = false;
				} else if (cell is Gtk.CellRendererPixbuf) {
					(cell as Gtk.CellRendererPixbuf).Visible = false;
				} else if (cell is Gtk.CellRendererText) {
					(cell as Gtk.CellRendererText).Text = h;
					(cell as Gtk.CellRendererText).Sensitive = true;
				} else {
					Console.WriteLine("Unknown column");
				}
				
			// Sub nodes
			} else {
				if (cell is CellRendererToggle) {
					CellRendererToggle c = (CellRendererToggle) cell;
					
					c.Activatable = true;
					c.Visible = true;
					c.Active = DrapesApp.WpList[key].Enabled;
				} else if (cell is CellRendererPixbuf) {
					CellRendererPixbuf p = (CellRendererPixbuf) cell; 
					
					// this is a hack, so the whole column dosen't activate on sellection
					p.Mode = CellRendererMode.Activatable;
					
					p.Visible = true;
					p.Pixbuf = DrapesApp.WpList[key].Thumbnail();
					
					// Gray it out if the user diabled it
					p.Sensitive = DrapesApp.WpList[key].Enabled;
					
				} else if (cell is CellRendererText) {
					CellRendererText t = (CellRendererText) cell;
					
					string TextDesc;
					
					// Format the description text next to the image
					TextDesc = String.Format("<b>{0}</b>\n", DrapesApp.WpList[key].Name );
					TextDesc += String.Format("{0}\n", DrapesApp.WpList[key].Mime);
					TextDesc += String.Format("{0} x {1} pixels", DrapesApp.WpList[key].Width, DrapesApp.WpList[key].Height);
						
					t.Markup = TextDesc;
						
					// So the text dosen't overflow
					t.Ellipsize = Pango.EllipsizeMode.End;
					
					// Gray it out if the user disabled it
					t.Sensitive = DrapesApp.WpList[key].Enabled;
				} else {
					Console.WriteLine("Unknow column");
				}
			}
		}
		
		// User double clicked a row in the TreeView
		private void onRowDoubleClick (object sender, RowActivatedArgs args)
		{
			Console.WriteLine(sender);
			
			TreeView		tv = (TreeView) sender;
			TreeModelFilter	model = (TreeModelFilter) tv.Model;
			TreeIter		iter;
			
			model.GetIter(out iter, args.Path);
			
			string	key = (string) model.GetValue(iter, 0);
			
			if (key == null) {
				// Expand or collapse a row
				if (tv.GetRowExpanded(args.Path))
					tv.CollapseRow(args.Path);
				else
					tv.ExpandRow(args.Path, false);
			} else {	// Activate wallaper
				// Only switch if enabled
				if (DrapesApp.WpList[key].Enabled == true) {
					DrapesApp.Cfg.Wallpaper = DrapesApp.WpList[key].File;
					Console.WriteLine("Switching wallpaper to: {0}", DrapesApp.WpList[key].File);
				} else
					Console.WriteLine("Not activating {0}, disabled", DrapesApp.WpList[key].File);
			}
		}
		
		// User toggles a wallpaper in the list
		private void OnWallPaperToggle (object sender, ToggledArgs args)
		{
			TreeModelFilter	model = (Gtk.TreeModelFilter) tvBgList.Model;
			TreeIter		iter;
			
			model.GetIter(out iter, new Gtk.TreePath(args.Path));
			
			string key = (string) model.GetValue(iter, 0);

			// Switch the Wallpaper enabled
			DrapesApp.WpList.SetEnabled(key, !DrapesApp.WpList[key].Enabled);
		}
		
		// Clicked on cbtMonitor
		private void OnMonitorChanged (object sender, EventArgs args)
		{
			Gtk.CheckButton c = (Gtk.CheckButton) sender;
			
			// CConf settings
			DrapesApp.Cfg.MonitorEnabled = c.Active;

			// They can only select a directory to monitor, if the monitor toggle is clicked			
			fcbDir.Sensitive = c.Active;
		}
		
		private void OnMonitorDirChanged (object sender, EventArgs args)
		{
			Gtk.FileChooserButton d = (Gtk.FileChooserButton) sender;

			// Update GConf settings
			DrapesApp.Cfg.MonitorDirectory = d.Filename;
		}
		
		// Clicked on cbtStartSwitch
		private void OnStartupChanged (object sender, EventArgs args)
		{
			Gtk.CheckButton c = (Gtk.CheckButton) sender;
		
			// Gconf settings
			DrapesApp.Cfg.ShuffleOnStart = c.Active;
		}
		
		// Text formating for: cbtMonitor
		private void TimerFormatValue (object sender, FormatValueArgs args)
		{
			HScale	t = (Gtk.HScale) sender;
			args.RetVal = Config.TimeDelay.String((Config.TimeDelay.Delay) Convert.ToInt32(t.Value));
		}
		
		// change the value of cbtMonitor
		private void OnTimerChangeValueEvent (object sender, ChangeValueArgs args)
		{
			HScale	t = (HScale) sender;
			
			// Convert it to a TimeDelay and update GConf
			Config.TimeDelay.Delay d = (Config.TimeDelay.Delay) Convert.ToInt32(t.Value);
			DrapesApp.Cfg.SwitchDelay = d;
		}
		
		// Raise the focus of the window
		public void RaiseWindow()
		{
			winPref.Present();
		}
		
		private void onCloseButtonClick (object sender, EventArgs args)
		{
			DrapesApp.ConfigWindow  = null;
			winPref.Destroy();
			DrapesApp.WpList.ThumbCleanup();
		}
		
		void OnWindowDelete (object o, DeleteEventArgs args)
		{
			DrapesApp.ConfigWindow  = null;
			DrapesApp.WpList.ThumbCleanup();
		}
	}
}
