using Gtk;

namespace Drapes
{
    public class About : AboutDialog
	{
        public About() : base()
        {
            this.Comments = "Wallpaper managment application for the GNOME desktop";
            this.Copyright = "Copyright © 2006-2007 Milosz Tanski";
            this.License = "Licensed under the GNU General Public License Version 2\n"
                + "\n"
                + "Drapes is free software; you can redistribute it and/or\n"
                + "modify it under the terms of the GNU General Public License\n"
                + "as published by the Free Software Foundation; either version 2\n"
                + "of the License, or (at your option) any later version.\n"
                + "\n"
                + "Drapes is distributed in the hope that it will be useful,\n"
                + "but WITHOUT ANY WARRANTY; without even the implied warranty of\n"
                + "MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the\n"
                + "GNU General Public License for more details.\n"
                + "\n"
                + "You should have received a copy of the GNU General Public License\n"
                + "along with this program; if not, write to the Free Software\n"
                + "Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA\n"
                + "02110-1301, USA.\n";
            this.LogoIconName = "drapes";
            this.Version = CompileOptions.Version;
            this.Website = "http://drapes.mindtouchsoftware.com";
            
            this.ShowAll();
        }
	}
}
