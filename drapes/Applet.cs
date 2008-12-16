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
using IO = System.IO;

namespace Drapes
{
	public class DrapesApplet : Gnome.PanelApplet
	{
		private AppletWidget			AppletIcon = null;
		private Gnome.BonoboUIVerb[]	MenuVerbs = null;
		
		public DrapesApplet(IntPtr raw)
			: base(raw)
		{

		}
		
		public override string IID
		{
			get { return "OAFIID:DrapesApplet"; }
		}

		public override string FactoryIID
		{
			get { return "OAFIID:DrapesApplet_Factory"; }
		}

		public override void Creation()
		{
			// Filename of the menu xml file
			string XmlFile = IO.Path.Combine(CompileOptions.GnomeXmlUiDir, "DrapesApplet.xml");
            
			// Create the applet icon
			AppletIcon = new AppletWidget(AppletStyle.APPLET_PANEL, (int) this.Size);
			Add(AppletIcon);
			ShowAll();
			
			MenuVerbs = new []
			{
				new Gnome.BonoboUIVerb("Switch", SwitchWallpaper),
				new Gnome.BonoboUIVerb("Shuffle", null),
				new Gnome.BonoboUIVerb("Preferences", ShowPreferences),
			};

			this.SetupMenu (XmlFile, MenuVerbs);
		}

		private void SwitchWallpaper()
		{
			AppletIcon.ToggleSwitch();
		}

		private void ShowPreferences()
		{
			AppletIcon.ShowPrefrences();
		}
        
        private void ShowHelp()
        {
            DrapesApp.OpenHelp(null, this.Screen);
        }

		public static void Run(ref string[] args)
		{
			Gnome.Program p = new Gnome.Program("drapes",
				CompileOptions.Version, Gnome.Modules.UI, args);
			
			Gnome.PanelAppletFactory.Register(typeof(DrapesApplet));
		}
	}
}
