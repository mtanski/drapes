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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using Drapes;

namespace Drapes
{

	public class WallPaperList : IEnumerable <Wallpaper>
	{	
		private OrderedDictionary list = new OrderedDictionary();
		private Hashtable enabled = new Hashtable();
		// For "later" processing
		private Queue processing = new Queue();
		
		public WallPaperList(string file)
		{
			LoadList(file);
		}
		
		public bool LoadList(string file)
		{
			// Our file went MIA
			if (!File.Exists(file)) {
				Console.WriteLine("No such file: {0}", file);
				return false;
			}
			
			// We got it open it our selfs because we need to close the stream explicitly,
			// mono dosen't close the stream on a XmlReader.Close, we Sharing Violation exception
			FileStream fs;
			try {	
				fs = new FileStream(file, FileMode.Open, FileAccess.Read);
			} catch (Exception e) {
				Console.WriteLine("Unable to open file {0} because:", file, e.Message);
				return false;
			}
			
			// Gadzilion settings
			XmlReaderSettings s = new XmlReaderSettings();
			s.IgnoreComments = true;
			s.IgnoreProcessingInstructions = true;
			s.IgnoreWhitespace = true;
			s.ProhibitDtd = false;
			// the XmlReader object 
			XmlReader xml = XmlReader.Create(fs, s);
		
			try {
				// Proces the Xml document
				while (xml.Read()) {
					if (xml.NodeType.Equals(XmlNodeType.Element)) {
						if (xml.Name == "wallpaper") {
							XmlReader SubTree = xml.ReadSubtree();
							Wallpaper Cur = new Wallpaper();
							
							string tmp;
							// deleted from the list
							tmp = xml.GetAttribute("deleted");
							if (tmp != null)
								Cur.Deleted = Convert.ToBoolean(tmp);
							else
								Cur.Deleted = false;
									
							tmp = xml.GetAttribute("enabled");
							if (tmp != null)
								Cur.Enabled = Convert.ToBoolean(tmp);
							else
								Cur.Enabled = true;
							
							// Process subtree
							string node = null;
							while (SubTree.Read())
							{
								// root node
								if (SubTree.NodeType.Equals(XmlNodeType.Element))
								{
									switch (SubTree.Name) {
										case "wallpaper":		// skip root node
										case "filename":
										case "x":
										case "y":
										case "name":
										case "mtime":
											node = SubTree.Name;
											break;
										default:
											node = null;
											Console.WriteLine("Unknow element: {0}", SubTree.Name);
											break;
									}
								}
								
								// node data
								if (SubTree.NodeType.Equals(XmlNodeType.Text))
								{
									string v = SubTree.Value;
									switch (node) {
										case "x":
											Cur.w = Convert.ToInt32(v);
											break;
										case "y":
											Cur.h = Convert.ToInt32(v);
											break;	
										case "name":
											Cur.Name = v;
											break;
										case "mtime":
											long ftutc = Convert.ToInt64(v);
											Cur.Mtime = DateTime.FromFileTimeUtc(ftutc);
											break;
										case "filename":
											if (v != "(none)")
												Cur.LoadFileDelayed(v);
											break;
										default:
											break;
									}
								}
							}
							
							// ignore non image wallpapers (eg. color and gradient crap)
							if (Cur.File.Length > 0)
								Append(Cur);
							
						}
					}
				}
			} catch (System.Xml.XmlException e) {
				Console.WriteLine("Something bad happened las time, opening as far as we can...");
			}

			// Cleanup on isle 5
			xml.Close();
			fs.Close();
		
			return true;
		}
		
		public bool SaveList(string file)
		{
            Console.WriteLine(file);
            
			XmlTextWriter xml = new XmlTextWriter(file, null);
			xml.Formatting = Formatting.Indented;
			xml.Namespaces = false;
			
			// headers
			xml.WriteStartDocument();
			xml.WriteDocType("wallpapers", "SYSTEM", "gnome-wp-list.dtd", null);
			xml.WriteStartElement(null, "wallpapers", null);
			
			// process the wallpaer list
			foreach(Wallpaper w in this) {
					
				// don't save non existing wallpapers
				if (w.Name.Length < 1)
					continue;
			
				xml.WriteStartElement("wallpaper");

				// main atributes
				xml.WriteAttributeString("deleted", w.Deleted.ToString());	
				xml.WriteAttributeString("enabled", w.Enabled.ToString());
				
				// values
				xml.WriteElementString("name", w.Name.ToString());
				xml.WriteElementString("filename", w.File.ToString());
				
				// style TODO
				xml.WriteElementString("x", w.Width.ToString());
				xml.WriteElementString("y", w.Height.ToString());
				
				// correr case where this may not yet be loaded (importing old gnome wp settings), and quiting right away
				if (w.Mtime != DateTime.MinValue) {
					long time = w.Mtime.ToFileTimeUtc();
					xml.WriteElementString("mtime", time.ToString());
				}
						
				xml.WriteEndElement();
			}
			
			// End
			xml.WriteEndElement();
			xml.WriteEndDocument();
			
			//
			xml.Close();
			
			Console.WriteLine("Wallpaper list file {0} saved", file);
			return true;
		}
		
		public int NumberBackgrounds
		{
			get {
				return list.Count;
			}
		}
		
		public Wallpaper this[int index]
		{
			get {
				try {
					return (Wallpaper) list[index];
				} catch (ArgumentOutOfRangeException e) {
					return null;
				}
			}
		}

		public Wallpaper this[string key]
		{
			get {
				return (Wallpaper) list[key];
			}
		}

		// This is the worst part of the app
		Random r = new Random();
		public Wallpaper Random(string old)
		{
			// Cannot do anything with no wallpapers
			if (list.Count == 0)
				return null;
			
			// save ourselves a lot of voodoo	
			if (enabled.Count == 1)
				return (Wallpaper) list[0];

			Wallpaper w;
			
			do {
				// what if all of them are deselected
				if (enabled.Count == 0) {
					// just pick any one
					int i = list.Count - 1;
					i = (int) Math.Round(i * r.NextDouble());
					w = (Wallpaper) list[i];
				} else {
					string[] keys = new string[enabled.Count];
					enabled.Keys.CopyTo(keys, 0);
					
					int i = enabled.Count - 1;
					i = Convert.ToInt32(r.NextDouble() * i);
					w = (Wallpaper) list[keys[i]];
				}
			// Make sure we always get a diffrent one
			} while (old == w.File);
			
			return w;
		}
		
		// Wrapper for the enabled propery of Wallaper
		public void SetEnabled(string file, bool val)
		{
			Wallpaper w = (Wallpaper) list[file];

			// deleted wallpapers have no rights
			if (w.Deleted)
				return;
			
			if (val)
				enabled.Add(w.File, w);
			else
				enabled.Remove(w.File);
			
			w.Enabled = val;
		}

		public void SetDelete(string file)
		{
			if (file == null)
				return;
			
			Wallpaper w = (Wallpaper) enabled[file];
			w.Deleted = true;
				
			// Remove from rotation
			enabled.Remove(file);
		}
				
		public void Append(Wallpaper w)
		{
			// add for later processing
			processing.Enqueue(w);
		}

		public bool DelayedLoader()
		{
			// do we have anything to do
			if (processing.Count == 0)
				return true;

			// Add it to the list of wallpapers
			Wallpaper w = (Wallpaper) processing.Dequeue();

			// Load attributes if not deleted
			if (w.Deleted == false)
				w.ForceLoadAttr();

			// Readded a file
			if (list.Contains(w.File)) {
				Wallpaper old = (Wallpaper) list[w.File];
				list[w.File] = w;

				// When adding a wallpaper a second time make it enabled
				if (enabled.ContainsKey(w.File) == false)
					enabled.Add(w.File, w);

				// Add it the list of wallpapers if it was previously removed
				if (old.Deleted == true && DrapesApp.ConfigWindow != null)
					DrapesApp.ConfigWindow.AddWallpaper(w.File);
				
			} else {
				list.Add(w.File, w);

				if (w.Deleted == false) {
					// add it to the rotation
					if (w.Enabled == true)
						enabled.Add(w.File, w);
					
					// Should we add it to the window
					if (DrapesApp.ConfigWindow != null)
						DrapesApp.ConfigWindow.AddWallpaper(w.File);
				}
			}

			return true;
		}
		
		// Thumb cleanup for some memory saving hotness (eg. not leaking)
		private int LastClean = 0;
		public bool ThumbCleanup()
		{
			// clean up done?
			if (NumberBackgrounds <= LastClean) {
				Console.WriteLine("Cleanup complete");
				LastClean = 0;
				return false;
			}
				
			// Halt cleanup when config window is brought up
			if (DrapesApp.ConfigWindow != null) {
				Console.WriteLine("Cleanup halted");
				LastClean = 0;
				return false; 
			}
			
			// Perform the cleanup
			(list[LastClean] as Wallpaper).DisposeThumb();
			
			LastClean++;
			return true;
		}

		// Enumeration magic
		public IEnumerator <Wallpaper> GetEnumerator()
		{
			foreach (DictionaryEntry entry in list)
			{
				yield return (entry.Value as Wallpaper);
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
	}
}
