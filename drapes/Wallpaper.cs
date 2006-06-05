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
using Gdk;
using Gnome;
using Vfs = Gnome.Vfs;
using Mono.Posix;
using Res = Drapes.ResolutionProperties;
using Config = Drapes.Config;

namespace Drapes.ResolutionProperties
{
	public enum Aspect
	{
		ASPECT_43,
		ASPECT_WIDE,
		ASPECT_OTHER
	}
	
	public class Resolution
	{
		internal int		x;
		internal int		y;
		internal Aspect		aspect;
		internal string		name;
	
		public Resolution(int x, int y, Aspect	aspect, string name)
		{
			this.x = x;
			this.y = y;
			this.aspect = aspect;
			this.name = name;
		}
	}

	public static class ResolutionList
	{
		public readonly static Resolution[] List =
		{
			new Resolution (640,	480,	Aspect.ASPECT_43,	"VGA"),
			new Resolution (800,	600,	Aspect.ASPECT_43,	"SVGA"),
			new Resolution (1024,	768,	Aspect.ASPECT_43,	"XGA"),
			new Resolution (1158,	864,	Aspect.ASPECT_43, 	"XGA+"),
			new Resolution (1280,	768,	Aspect.ASPECT_WIDE,	"WXGA"),
			new Resolution (1280,	800,	Aspect.ASPECT_WIDE,	"WXGA"),
			new Resolution (1280,	1024,	Aspect.ASPECT_43,	"SXGA"),
			new Resolution (1400,	1050,	Aspect.ASPECT_43,	"SXGA+"),
			new Resolution (1440,	900,	Aspect.ASPECT_WIDE,	"WSXGA"),
			new Resolution (1600,	1200,	Aspect.ASPECT_43,	"UXGA"),
			new Resolution (1680,	1050,	Aspect.ASPECT_WIDE,	"WSXGA+"),
			new Resolution (1920,	1200,	Aspect.ASPECT_WIDE,	"WUXGA"),
			new Resolution (2048,	1536,	Aspect.ASPECT_43,	"QXGA"),
			new Resolution (2560,	1600,	Aspect.ASPECT_WIDE,	"WQXGA")
		};
	
		public static string ResolutionName(int x, int y)
		{
			foreach (Resolution r in List)
			{
				if (r.x == x && r.y == y)
					return r.name;
					
				// they should be in order
				if (r.x > x)
					break;
			}
		
			return "Unknown";
		}
		
		public static Aspect AspectType (int x, int y)
		{
			foreach (Resolution r in List)
			{
				if (r.x == x && r.y == y)
					return r.aspect;
				
				// they should be in order
				if (r.x > x)
					break;
			}
	
			return Aspect.ASPECT_OTHER;
		}
	}
}

namespace Drapes  
{
	public class Wallpaper
	{
		// info about this wallpaer
		private		string				filename;
		private		string				name;
		// low level file info
		private		string 				mime;
		private		System.DateTime		mtime = DateTime.MinValue;
		// options
		internal	int					w,h;
		private		bool				removed;
		private		bool				enabled;
		// thumbnail
		internal	Pixbuf				thumb;
		// initlized
		private		bool				init;
		//
		private		Config.Style		style;
				
		public Wallpaper()
		{
			removed = false;
			w = 0;
			h = 0;
			
			init = false;
		}
		
		public Wallpaper(string file)
		{
			LoadFile(file);
		}
		
		public bool LoadFile(string file)
		{			
			filename = file;
			// Load all the file settings
			ForceLoadAttr();
			
			init = true;
			return true;
		}
		
		// Dosen't really load a file just stores it's filename;
		// if we can grab the cached size data from the xml
		// instead of loading every single one maybe we can save
		// sometime.
		public bool LoadFileDelayed(string file)
		{
			filename = file;
			// Not fully loaded 
			init = false;
			
			return true;
		}
		
		public bool ForceLoadAttr()
		{
			// Don't waste our resources if it has been removed from the list
			if (!removed) {
	
				Vfs.Uri uri = new Vfs.Uri(Gnome.Vfs.Uri.GetUriFromLocalPath(filename));
				Vfs.FileInfo fi = new Vfs.FileInfo(uri);
				
				if (!uri.Exists) {
					// mark it as removed so it dosen't get listed as an option, ever
					removed = true;
					// set filename to null, so it dosen't get saved on quit
					filename = null;
					
					Console.WriteLine("Cannot find file: {0}", uri.ToString());
					// No sence doing anything else
					return false;
				}
				
				if (mtime != DateTime.MinValue) {
					// We got info we need, save our selves sometime
					if (mtime == fi.Mtime && w == 0 && h == 0)
						return true; 
				} else	// save mtime
					mtime = fi.Mtime;
				
				// Get mimetype
				mime = uri.MimeType.Description;	//	Gtk# bug
				
				// Not loaded
				if (w == 0 || h == 0) { 
					Pixbuf t = new Gdk.Pixbuf(filename);
					w = t.Width;
					h = t.Height;
					t.Dispose();
				}
				
				// Try to generate a thumbnail
				CreateThumnail();
			}
			
			// We're done
			init = true;
			return true;
		}
		
		public string File
		{
			get {
				if (filename == null)
					return "";
				
				return filename;
			}
		}
		
		public bool	Initlized
		{
			get {
				return init;
			}
		}
		
		public int Width
		{
			get {
				return w;
			}
		}
		
		public int Height
		{
			get {
				return h;
			}
		}
		
		public System.DateTime Mtime
		{
			get {
				return this.mtime;
			}
			set {
				this.mtime = value;
			}
		
		}
		
		public bool	Enabled
		{
			get {
				return enabled;
			}
			set {
				enabled = value;
			}
		}
		
		public double AspectRatio
		{
			get {
				return Convert.ToDouble(w) / Convert.ToDouble(h);
			}
		}
		
		public Res.Aspect Aspect
		{
			get {
				return Res.ResolutionList.AspectType(w, h);
			}
		}
		
		public string Name
		{
			get {
				// if no name is set return filename
				if (name == null) {
					Vfs.FileInfo fi = new Vfs.FileInfo(Vfs.Uri.GetUriFromLocalPath(filename));
				
					return fi.Name;
				}
				
				return name;
			}
			set {
				name = value;
			}
		}
		
		public bool Deleted
		{
			get {
				return removed;
			}
			set {
				removed = value;
			}
		}
		
		public string Mime
		{
			get {
				// return mime;
				return mime;
			}
		}
		
		public bool MatchScreen()
		{
			if (Gdk.Screen.Default.Width == w && Gdk.Screen.Default.Height == h)
				return true;
				
			return false;
		}
		
		private bool HasCurrentThumbnail()
		{
			Gnome.ThumbnailFactory t = new Gnome.ThumbnailFactory(ThumbnailSize.Normal);
			
			// failed thumb nails count
			if (t.HasValidFailedThumbnail(filename, mtime))
				return true;
			
			// one exists	
			string existing = t.Lookup(filename,  mtime);
			if (existing != null)
				return true;
		
			// didn't find one
			return false;
		}
		
		private bool CreateThumnail()
		{
			// don't waste time if one exists
			if (HasCurrentThumbnail() == true)
				return true;

			ThumbnailFactory t = new ThumbnailFactory(ThumbnailSize.Normal);
			Vfs.Uri uri = new Vfs.Uri(filename);
			
			// can we attempt to create it?
			if (!t.CanThumbnail(filename, uri.MimeType.ToString(), mtime)) {
				Console.WriteLine("Cannot create thumbnail for: {0}", filename);
				return false;
			}
			
			// Generate and save thumbnail
			Pixbuf tmp = t.GenerateThumbnail(filename, uri.MimeType.ToString());
			t.SaveThumbnail(tmp, filename, mtime);
			
			// get rid of the tempoary thumb
			tmp.Dispose();
			
			return true;
		}
		
		// For some memory saving goodness
		public void DisposeThumb()
		{
			if (thumb != null)
				thumb.Dispose();

			thumb = null;
		}
		
		// Grab thumbnail
		public Pixbuf Thumbnail()
		{
			string		existing;
			
			// Totaly ignore removed wps
			if (removed)
				return null;
			
			// we got a cashed thumbnail
			if (thumb != null)
				return thumb;
			
			if (HasCurrentThumbnail() == false)
				return null;
			
			ThumbnailFactory t = new ThumbnailFactory(ThumbnailSize.Normal);
			
			// Grab the thumb
			existing = t.Lookup(filename,  mtime);
			thumb = new Pixbuf(existing);
				
			// Figure out the scale for previews
			int x, y;
			switch (Res.ResolutionList.AspectType(w,h)) {
				case Res.Aspect.ASPECT_43:
					x = 64;
					y = 64;
					break;
				case Res.Aspect.ASPECT_WIDE:
					x = 64;
					y = 36;
					break;
				case Res.Aspect.ASPECT_OTHER:
				default:
					x = 64;
					y = 64;
					break;
			}
			
			// do the acctual scaling
			thumb = Gnome.Thumbnail.ScaleDownPixbuf(thumb, x, y);
			return thumb;
		}
	}
}
