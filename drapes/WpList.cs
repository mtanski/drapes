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
using System.Xml;
using Drapes;

namespace Drapes
{

	public class WallPaperList : IEnumerable <Wallpaper>
	{	
		private ArrayList	list = new ArrayList();
		private Hashtable	elist = new Hashtable();
		
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
			
			// Gadzilion settings
			XmlReaderSettings s = new XmlReaderSettings();
			s.IgnoreComments = true;
			s.IgnoreProcessingInstructions = true;
			s.IgnoreWhitespace = true;
			s.ProhibitDtd = false;
			// the XmlReader object 
			XmlReader xml = XmlReader.Create(file, s);
		
			try {
				// Proces the Xml document
				while (xml.Read()) {
					if (xml.NodeType.Equals(XmlNodeType.Element)) {
						if (xml.Name == "wallpaper") {
							XmlReader SubTree = xml.ReadSubtree();
							Wallpaper Cur = new Wallpaper();
							
							string tmp;
							bool deleted, enabled;
							
							// deleted from the list
							tmp = xml.GetAttribute("deleted");
							if (tmp != null)
								deleted = Convert.ToBoolean(tmp);
							else
								deleted = false;
									
							tmp = xml.GetAttribute("enabled");
							if (tmp != null)
								enabled = Convert.ToBoolean(tmp);
							else
								enabled = true;
							
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
							if (Cur.File.Length > 0) {
								list.Add(Cur);
								
								// set some properties thought wrappers
								SetDeleted(list.Count-1, deleted);
								SetEnabled(list.Count-1, enabled);
							}
						}
					}
				}
			} catch (System.Xml.XmlException e) {
				Console.WriteLine("Something bad happened lastime, opening as far as we can");
			}

			xml.Close();	
		
			return true;
		}
		
		public bool SaveList(string file)
		{
			XmlTextWriter xml = new XmlTextWriter(file, null);
			xml.Formatting = Formatting.Indented;
			xml.Namespaces = false;
			
			// headers
			xml.WriteStartDocument();
			xml.WriteDocType("wallpapers", "SYSTEM", "gnome-wp-list.dtd", null);
			xml.WriteStartElement(null, "wallpapers", null);
			
			// process the wallpaer list
			foreach(Wallpaper w in list) {
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
				long time = w.Mtime.ToFileTimeUtc();
				xml.WriteElementString("mtime", time.ToString());
						
				xml.WriteEndElement();
			}
			
			// End
			xml.WriteEndElement();
			xml.WriteEndDocument();
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
		
		Random r = new Random();
		public Wallpaper Random(string old)
		{
			// Cannot do anything with no wallpapers
			if (list.Count == 0)
				return null;
			
			// save ourselves a lot of voodoo	
			if (list.Count == 1)
				return (Wallpaper) list[0];
		
			int i;
			Wallpaper w;
			
			do {
				double scale = r.NextDouble();
				// what if all of them are deselected
				if (elist.Count == 0) {
					// just pick any enabled one
					i = list.Count - 1;
					i = (int) Math.Round(i * scale);
				} else {
					ICollection keys = elist.Keys;
					// this is beyond ghey
					int[] l = new int[keys.Count];
					i = elist.Count - 1;
					keys.CopyTo(l, 0);
					i = l[(int) Math.Round(scale * i)];
				}
				w = (Wallpaper) list[i];
			// Make sure we always get a diffrent one
			} while (old == w.File);
			
			return w;
		}
		
		// Wrapper for the enabled propery of Wallaper
		public void SetEnabled(int index, bool val)
		{
			// Deleted entries have no rights
			if ((list[index] as Wallpaper).Deleted)
				return;
		
			(list[index] as Wallpaper).Enabled = val;
			
			if (val == true) {
				try {
					elist.Add(index, null);
				} catch(System.ArgumentException e) {}
			} else {
				try {
					elist.Remove(index);
				} catch (System.ArgumentException e) {}
			}
		}
		
		public void SetDeleted(int index, bool val)
		{
			// bring it back to life
			(list[index] as Wallpaper).Deleted = val;
		
			if (val == true) {
				try {
					elist.Remove(index);
				} catch (System.ArgumentException e) {}
			} else {
				// if undeleted, make it enabled
				(list[index] as Wallpaper).Enabled = true;
				
				try {
					elist.Add(index, null);
				} catch (System.ArgumentException e) {}
			}
		}
		
		public void Append(Wallpaper w)
		{
			list.Add(w);
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
				
			// Don't atempt to cleanup when
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
			foreach (Wallpaper w in list)
			{
				yield return w;
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
	}
}
