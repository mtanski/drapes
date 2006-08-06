namespace _Gnome {

	using System;
	using System.IO;
	using System.Collections;
	using System.Reflection;
	using System.Runtime.InteropServices;

	public abstract class PanelApplet : Gtk.EventBox {

		~PanelApplet()
		{
			Dispose();
		}

		[Obsolete]
		protected PanelApplet(GLib.GType gtype) : base(gtype) {}
		public PanelApplet(IntPtr raw) : base(raw) {}

		[DllImport("panel-applet-2")]
		static extern IntPtr panel_applet_new();

		public PanelApplet () : base (IntPtr.Zero)
		{
			if (GetType () != typeof (PanelApplet)) {
				CreateNativeObject (new string [0], new GLib.Value[0]);
				return;
			}
			Raw = panel_applet_new();
		}

		[GLib.CDeclCallback]
		delegate void ChangeBackgroundSignalDelegate (IntPtr arg0, int arg1, ref Gdk.Color arg2, IntPtr arg3, IntPtr gch);

		static void ChangeBackgroundSignalCallback (IntPtr arg0, int arg1, ref Gdk.Color arg2, IntPtr arg3, IntPtr gch)
		{
			GLib.Signal sig = ((GCHandle) gch).Target as GLib.Signal;
			if (sig == null)
				throw new Exception("Unknown signal GC handle received " + gch);

			_Gnome.ChangeBackgroundArgs args = new _Gnome.ChangeBackgroundArgs ();
			args.Args = new object[3];
			args.Args[0] = (_Gnome.PanelAppletBackgroundType) arg1;
			args.Args[1] = arg2;
			args.Args[2] = GLib.Object.GetObject(arg3) as Gdk.Pixmap;
			_Gnome.ChangeBackgroundHandler handler = (_Gnome.ChangeBackgroundHandler) sig.Handler;
			handler (GLib.Object.GetObject (arg0), args);

		}

		[GLib.CDeclCallback]
		delegate void ChangeBackgroundVMDelegate (IntPtr applet, int type, ref Gdk.Color color, IntPtr pixmap);

		static ChangeBackgroundVMDelegate ChangeBackgroundVMCallback;

		static void changebackground_cb (IntPtr applet, int type, ref Gdk.Color color, IntPtr pixmap)
		{
			PanelApplet applet_managed = GLib.Object.GetObject (applet, false) as PanelApplet;
			switch ((PanelAppletBackgroundType)type) {
			case PanelAppletBackgroundType.ColorBackground:
				applet_managed.OnChangeBackground ((PanelAppletBackgroundType)type, color, null);
				break;
			case PanelAppletBackgroundType.PixmapBackground:
				applet_managed.OnChangeBackground ((PanelAppletBackgroundType)type, Gdk.Color.Zero, (Gdk.Pixmap) GLib.Object.GetObject(pixmap));
				break;
                        default:
				applet_managed.OnChangeBackground ((PanelAppletBackgroundType)type, Gdk.Color.Zero, null);
				break;
                        }
		}

		private static void OverrideChangeBackground (GLib.GType gtype)
		{
			if (ChangeBackgroundVMCallback == null)
				ChangeBackgroundVMCallback = new ChangeBackgroundVMDelegate (changebackground_cb);
			OverrideVirtualMethod (gtype, "change_background", ChangeBackgroundVMCallback);
		}

		[GLib.DefaultSignalHandler(Type=typeof(_Gnome.PanelApplet), ConnectionMethod="OverrideChangeBackground")]
		protected virtual void OnChangeBackground (_Gnome.PanelAppletBackgroundType type, Gdk.Color color, Gdk.Pixmap pixmap)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (4);
			GLib.Value[] vals = new GLib.Value [4];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (type);
			inst_and_params.Append (vals [1]);
			vals [2] = new GLib.Value (color);
			inst_and_params.Append (vals [2]);
			vals [3] = new GLib.Value (pixmap);
			inst_and_params.Append (vals [3]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal("change_background")]
		public event _Gnome.ChangeBackgroundHandler ChangeBackground {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "change_background", new ChangeBackgroundSignalDelegate(ChangeBackgroundSignalCallback));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "change_background", new ChangeBackgroundSignalDelegate(ChangeBackgroundSignalCallback));
				sig.RemoveDelegate (value);
			}
		}

		[GLib.CDeclCallback]
		delegate void MoveFocusOutOfAppletSignalDelegate (IntPtr arg0, int arg1, IntPtr gch);

		static void MoveFocusOutOfAppletSignalCallback (IntPtr arg0, int arg1, IntPtr gch)
		{
			GLib.Signal sig = ((GCHandle) gch).Target as GLib.Signal;
			if (sig == null)
				throw new Exception("Unknown signal GC handle received " + gch);

			_Gnome.MoveFocusOutOfAppletArgs args = new _Gnome.MoveFocusOutOfAppletArgs ();
			args.Args = new object[1];
			args.Args[0] = (Gtk.DirectionType) arg1;
			_Gnome.MoveFocusOutOfAppletHandler handler = (_Gnome.MoveFocusOutOfAppletHandler) sig.Handler;
			handler (GLib.Object.GetObject (arg0), args);

		}

		[GLib.CDeclCallback]
		delegate void MoveFocusOutOfAppletVMDelegate (IntPtr frame, int direction);

		static MoveFocusOutOfAppletVMDelegate MoveFocusOutOfAppletVMCallback;

		static void movefocusoutofapplet_cb (IntPtr frame, int direction)
		{
			PanelApplet frame_managed = GLib.Object.GetObject (frame, false) as PanelApplet;
			frame_managed.OnMoveFocusOutOfApplet ((Gtk.DirectionType) direction);
		}

		private static void OverrideMoveFocusOutOfApplet (GLib.GType gtype)
		{
			if (MoveFocusOutOfAppletVMCallback == null)
				MoveFocusOutOfAppletVMCallback = new MoveFocusOutOfAppletVMDelegate (movefocusoutofapplet_cb);
			OverrideVirtualMethod (gtype, "move_focus_out_of_applet", MoveFocusOutOfAppletVMCallback);
		}

		[GLib.DefaultSignalHandler(Type=typeof(_Gnome.PanelApplet), ConnectionMethod="OverrideMoveFocusOutOfApplet")]
		protected virtual void OnMoveFocusOutOfApplet (Gtk.DirectionType direction)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (2);
			GLib.Value[] vals = new GLib.Value [2];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (direction);
			inst_and_params.Append (vals [1]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal("move_focus_out_of_applet")]
		public event _Gnome.MoveFocusOutOfAppletHandler MoveFocusOutOfApplet {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "move_focus_out_of_applet", new MoveFocusOutOfAppletSignalDelegate(MoveFocusOutOfAppletSignalCallback));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "move_focus_out_of_applet", new MoveFocusOutOfAppletSignalDelegate(MoveFocusOutOfAppletSignalCallback));
				sig.RemoveDelegate (value);
			}
		}

		[GLib.CDeclCallback]
		delegate void ChangeSizeSignalDelegate (IntPtr arg0, uint arg1, IntPtr gch);

		static void ChangeSizeSignalCallback (IntPtr arg0, uint arg1, IntPtr gch)
		{
			GLib.Signal sig = ((GCHandle) gch).Target as GLib.Signal;
			if (sig == null)
				throw new Exception("Unknown signal GC handle received " + gch);

			_Gnome.ChangeSizeArgs args = new _Gnome.ChangeSizeArgs ();
			args.Args = new object[1];
			args.Args[0] = arg1;
			_Gnome.ChangeSizeHandler handler = (_Gnome.ChangeSizeHandler) sig.Handler;
			handler (GLib.Object.GetObject (arg0), args);

		}

		[GLib.CDeclCallback]
		delegate void ChangeSizeVMDelegate (IntPtr applet, uint size);

		static ChangeSizeVMDelegate ChangeSizeVMCallback;

		static void changesize_cb (IntPtr applet, uint size)
		{
			PanelApplet applet_managed = GLib.Object.GetObject (applet, false) as PanelApplet;
			applet_managed.OnChangeSize (size);
		}

		private static void OverrideChangeSize (GLib.GType gtype)
		{
			if (ChangeSizeVMCallback == null)
				ChangeSizeVMCallback = new ChangeSizeVMDelegate (changesize_cb);
			OverrideVirtualMethod (gtype, "change_size", ChangeSizeVMCallback);
		}

		[GLib.DefaultSignalHandler(Type=typeof(_Gnome.PanelApplet), ConnectionMethod="OverrideChangeSize")]
		protected virtual void OnChangeSize (uint size)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (2);
			GLib.Value[] vals = new GLib.Value [2];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (size);
			inst_and_params.Append (vals [1]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal("change_size")]
		public event _Gnome.ChangeSizeHandler ChangeSize {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "change_size", new ChangeSizeSignalDelegate(ChangeSizeSignalCallback));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "change_size", new ChangeSizeSignalDelegate(ChangeSizeSignalCallback));
				sig.RemoveDelegate (value);
			}
		}

		[DllImport("panel-applet-2")]
		static extern void panel_applet_set_size_hints(IntPtr raw, out int size_hints, int n_elements, int base_size);

		public int SetSizeHints(int n_elements, int base_size) {
			int size_hints;
			panel_applet_set_size_hints(Handle, out size_hints, n_elements, base_size);
			return size_hints;
		}

		[DllImport("panel-applet-2")]
		static extern IntPtr panel_applet_get_type();

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = panel_applet_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}

		[DllImport("panel-applet-2")]
		static extern unsafe int panel_applet_gconf_get_int(IntPtr raw, IntPtr key, out IntPtr opt_error);

		public unsafe int GconfGetInt(string key) {
			IntPtr key_as_native = GLib.Marshaller.StringToPtrGStrdup (key);
			IntPtr error = IntPtr.Zero;
			int raw_ret = panel_applet_gconf_get_int(Handle, key_as_native, out error);
			int ret = raw_ret;
			GLib.Marshaller.Free (key_as_native);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

		[DllImport("panel-applet-2")]
		static extern unsafe void panel_applet_gconf_set_float(IntPtr raw, IntPtr key, double the_float, out IntPtr opt_error);

		public unsafe void GconfSetFloat(string key, double the_float) {
			IntPtr key_as_native = GLib.Marshaller.StringToPtrGStrdup (key);
			IntPtr error = IntPtr.Zero;
			panel_applet_gconf_set_float(Handle, key_as_native, the_float, out error);
			GLib.Marshaller.Free (key_as_native);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
		}

		[DllImport("panel-applet-2")]
		static extern bool panel_applet_get_locked_down(IntPtr raw);

		public bool LockedDown { 
			get {
				bool raw_ret = panel_applet_get_locked_down(Handle);
				bool ret = raw_ret;
				return ret;
			}
		}

		[DllImport("panel-applet-2")]
		static extern IntPtr panel_applet_get_preferences_key(IntPtr raw);

		public string PreferencesKey { 
			get {
				IntPtr raw_ret = panel_applet_get_preferences_key(Handle);
				string ret = GLib.Marshaller.PtrToStringGFree(raw_ret);
				return ret;
			}
		}

		[DllImport("panel-applet-2")]
		static extern int panel_applet_get_background(IntPtr raw, ref Gdk.Color color, IntPtr pixmap);

		public _Gnome.PanelAppletBackgroundType GetBackground(Gdk.Color color, Gdk.Pixmap pixmap) {
			int raw_ret = panel_applet_get_background(Handle, ref color, pixmap == null ? IntPtr.Zero : pixmap.Handle);
			_Gnome.PanelAppletBackgroundType ret = (_Gnome.PanelAppletBackgroundType) raw_ret;
			return ret;
		}

		[DllImport("panel-applet-2")]
		static extern unsafe IntPtr panel_applet_gconf_get_string(IntPtr raw, IntPtr key, out IntPtr opt_error);

		public unsafe string GconfGetString(string key) {
			IntPtr key_as_native = GLib.Marshaller.StringToPtrGStrdup (key);
			IntPtr error = IntPtr.Zero;
			IntPtr raw_ret = panel_applet_gconf_get_string(Handle, key_as_native, out error);
			string ret = GLib.Marshaller.PtrToStringGFree(raw_ret);
			GLib.Marshaller.Free (key_as_native);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

		[DllImport("panel-applet-2")]
		static extern unsafe void panel_applet_gconf_set_int(IntPtr raw, IntPtr key, int the_int, out IntPtr opt_error);

		public unsafe void GconfSetInt(string key, int the_int) {
			IntPtr key_as_native = GLib.Marshaller.StringToPtrGStrdup (key);
			IntPtr error = IntPtr.Zero;
			panel_applet_gconf_set_int(Handle, key_as_native, the_int, out error);
			GLib.Marshaller.Free (key_as_native);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
		}

		[DllImport("panel-applet-2")]
		static extern unsafe void panel_applet_add_preferences(IntPtr raw, IntPtr schema_dir, out IntPtr opt_error);

		public unsafe void AddPreferences(string schema_dir) {
			IntPtr schema_dir_as_native = GLib.Marshaller.StringToPtrGStrdup (schema_dir);
			IntPtr error = IntPtr.Zero;
			panel_applet_add_preferences(Handle, schema_dir_as_native, out error);
			GLib.Marshaller.Free (schema_dir_as_native);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
		}

		[DllImport("panel-applet-2")]
		static extern uint panel_applet_get_size(IntPtr raw);

		public uint Size { 
			get {
				uint raw_ret = panel_applet_get_size(Handle);
				uint ret = raw_ret;
				return ret;
			}
		}

		[DllImport("panel-applet-2")]
		static extern IntPtr panel_applet_gconf_get_full_key(IntPtr raw, IntPtr key);

		public string GconfGetFullKey(string key) {
			IntPtr key_as_native = GLib.Marshaller.StringToPtrGStrdup (key);
			IntPtr raw_ret = panel_applet_gconf_get_full_key(Handle, key_as_native);
			string ret = GLib.Marshaller.PtrToStringGFree(raw_ret);
			GLib.Marshaller.Free (key_as_native);
			return ret;
		}

		[DllImport("panel-applet-2")]
		static extern unsafe void panel_applet_gconf_set_string(IntPtr raw, IntPtr key, IntPtr the_string, out IntPtr opt_error);

		public unsafe void GconfSetString(string key, string the_string) {
			IntPtr key_as_native = GLib.Marshaller.StringToPtrGStrdup (key);
			IntPtr the_string_as_native = GLib.Marshaller.StringToPtrGStrdup (the_string);
			IntPtr error = IntPtr.Zero;
			panel_applet_gconf_set_string(Handle, key_as_native, the_string_as_native, out error);
			GLib.Marshaller.Free (key_as_native);
			GLib.Marshaller.Free (the_string_as_native);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
		}

		[DllImport("panel-applet-2")]
		static extern int panel_applet_get_flags(IntPtr raw);

		[DllImport("panel-applet-2")]
		static extern void panel_applet_set_flags(IntPtr raw, int flags);

		public new _Gnome.PanelAppletFlags Flags { 
			get {
				int raw_ret = panel_applet_get_flags(Handle);
				_Gnome.PanelAppletFlags ret = (_Gnome.PanelAppletFlags) raw_ret;
				return ret;
			}
			set {
				panel_applet_set_flags(Handle, (int) value);
			}
		}

		[DllImport("panel-applet-2")]
		static extern unsafe bool panel_applet_gconf_get_bool(IntPtr raw, IntPtr key, out IntPtr opt_error);

		public unsafe bool GconfGetBool(string key) {
			IntPtr key_as_native = GLib.Marshaller.StringToPtrGStrdup (key);
			IntPtr error = IntPtr.Zero;
			bool raw_ret = panel_applet_gconf_get_bool(Handle, key_as_native, out error);
			bool ret = raw_ret;
			GLib.Marshaller.Free (key_as_native);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

		[DllImport("panel-applet-2")]
		static extern unsafe double panel_applet_gconf_get_float(IntPtr raw, IntPtr key, out IntPtr opt_error);

		public unsafe double GconfGetFloat(string key) {
			IntPtr key_as_native = GLib.Marshaller.StringToPtrGStrdup (key);
			IntPtr error = IntPtr.Zero;
			double raw_ret = panel_applet_gconf_get_float(Handle, key_as_native, out error);
			double ret = raw_ret;
			GLib.Marshaller.Free (key_as_native);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

		[DllImport("panel-applet-2")]
		static extern unsafe void panel_applet_gconf_set_bool(IntPtr raw, IntPtr key, bool the_bool, out IntPtr opt_error);

		public unsafe void GconfSetBool(string key, bool the_bool) {
			IntPtr key_as_native = GLib.Marshaller.StringToPtrGStrdup (key);
			IntPtr error = IntPtr.Zero;
			panel_applet_gconf_set_bool(Handle, key_as_native, the_bool, out error);
			GLib.Marshaller.Free (key_as_native);
			if (error != IntPtr.Zero) throw new GLib.GException (error);
		}


		static PanelApplet ()
		{
			_GtkSharp.GnomeSharp.ObjectManager.Initialize ();
		}

		[DllImport ("panel-applet-2")]
		static extern void panel_applet_setup_menu (IntPtr handle, IntPtr xml, BonoboUIVerb[] items, IntPtr user_data);

		public void SetupMenu (string xml, BonoboUIVerb[] items)
		{
			BonoboUIVerb[] nulled_items = new BonoboUIVerb[items.Length + 1];
			Array.Copy (items, nulled_items, items.Length);
			nulled_items[items.Length] = new BonoboUIVerb (null, null);
			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (xml);
			panel_applet_setup_menu (Handle, native, nulled_items, IntPtr.Zero);
			GLib.Marshaller.Free (native);
		}

		public void SetupMenuFromFile(string FileName, BonoboUIVerb[] verbs)
		{		
			// Open the xml file describing the menu
			StreamReader menufile = new StreamReader(FileName);
			string xml = menufile.ReadToEnd();
			menufile.Close();

			SetupMenu(xml, verbs);
		}

		new void SetupMenuFromResource (Assembly asm, string resource, BonoboUIVerb [] verbs)
		{
			if (asm == null)
				asm = GetType ().Assembly;

			Stream stream = asm.GetManifestResourceStream (resource);
			if (stream != null) {
				StreamReader reader = new StreamReader (stream);
				String xml = reader.ReadToEnd ();
				reader.Close ();
				stream.Close ();

				SetupMenu (xml, verbs);
			}
		}		

		public abstract void Creation ();

		public abstract string IID {
			get;
		}

		public abstract string FactoryIID {
			get;
		}

	}
}
