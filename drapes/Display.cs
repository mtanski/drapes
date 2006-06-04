using System;
using System.Runtime.InteropServices;

namespace X11 
{
	public class XineramaScreen {
		int	width;
		int	height;
		
		int	x, y;

		// a Non existant
		public XineramaScreen()
		{
			//
			width = 0;
			height = 0;
			//
			x = 0;
			y = 0;
		}
		
		public XineramaScreen(Xlib.XineramaScreenInfo xinScreen)
		{	
			// Xinerama position
			width = xinScreen.width;
			height = xinScreen.height;
			//
			x = xinScreen.x_org;
			y = xinScreen.y_org;
		}
		
		public int Width {
			get {
				return width;
			}
		}
		
		public int Height {
			get {
				return height;
			}
		}
		
		public int OffsetX {
			get {
				return x;
			}
		}
		
		public int OffsetY {
			get {
				return y;
			}
		}
	}

	public class Screen
	{
		//
		IntPtr				x11Screen;
		int					screenNum;
		int					numMonitors;
		XineramaScreen[]	virtualScreens;
		
		// Will create a X11 Screen Object with the default X11 Display
		public Screen()
		{	
			x11Screen = Display.getDefaultScreen;
			classSetup();
		}
		
		// Will create a X11 Screen with a given Screen Pointer 
		public Screen(IntPtr ScreenPtr, int Num)
		{
			x11Screen = ScreenPtr;
			screenNum = Num; 
			
			classSetup();
		}
		
		private void classSetup()
		{
			IntPtr x11Display = Display.getDefaultDisplay;
		
			// We have a least one physical monitor per display (i hope)
			numMonitors = 1;

			// Figure the number of "Virtual" Xineram monitors present
			if (Xlib.XineramaIsActive(x11Display)) {
				int 	totMon = 0;
				IntPtr	xinStruct;
				
				xinStruct = Xlib.XineramaQueryScreens(x11Display, ref totMon);
				
				// Go thought the xinerama displays

				unsafe {
					Xlib.XineramaScreenInfo *xinPtr = (Xlib.XineramaScreenInfo*) xinStruct;
					
					// Find the number of Xinerama Screens on this Screens
					numMonitors = 0;	
					for (int i=0; i < totMon; i++) {
						if (xinPtr->screen_number == screenNum)
							numMonitors++;
						xinPtr++;
					}
				
					virtualScreens = new XineramaScreen[numMonitors];
					xinPtr = (Xlib.XineramaScreenInfo*) xinStruct;
				
					// Create Xinerama screens
					for (int i=0; i < totMon; i++) {
						if (xinPtr->screen_number == screenNum) {
							virtualScreens[i] = new XineramaScreen(xinPtr[i]);
						}
					}	
				}
			}
		}
		
		public int Width {
			get {
				return Xlib.XWidthOfScreen(x11Screen);
			}
		}
		
		public int Height {
			get {
				return Xlib.XHeightOfScreen(x11Screen);
			}
		}
		
		public int Depth {
			get {
				return Xlib.XDefaultDepthOfScreen(x11Screen);
			}
		}
		
		public XineramaScreen getXinScreen(int monitorNum)
		{
			return virtualScreens[monitorNum];
		}
		
		// Figure out the type of X11 display
		public x11ScreenType ScreenType {
			get {
				if (x11Screen == IntPtr.Zero)
					return x11ScreenType.SCREEN_NONE;
					
				if (numMonitors > 1)
					return x11ScreenType.SCREEN_XINERAMA;
					
				return x11ScreenType.SCREEN_STANDART;
			}
		}
		
		public enum x11ScreenType
		{
			SCREEN_STANDART,
			SCREEN_XINERAMA,
			SCREEN_NONE				// imposible screens
		}
		
		public int NumberMonitors
		{
			get {
				return numMonitors;
			}
		}
		
		public int ScreenNum {
			get {
				return screenNum;
			}
		}
	}

	public class Display
	{
		private	IntPtr		x11Display;
		private Screen[]	screens;
	
		public Display()
		{
			int			numX11Screens;
			IntPtr		gdkDisplay = Gdk.Display.Default.Handle;
		
			x11Display = GdkX11.gdk_x11_display_get_xdisplay(gdkDisplay);
			
			numX11Screens = Xlib.XScreenCount(x11Display);
			
			screens = new Screen[numX11Screens];
			for (int i = 0; i < Xlib.XScreenCount(x11Display); i++) {
				screens[i] = new Screen(Xlib.XScreenOfDisplay(x11Display, i), i);
			}
		}
		
		static public IntPtr getDefaultDisplay
		{
			get {
				return GdkX11.gdk_x11_display_get_xdisplay(Gdk.Display.Default.Handle);
			}
		}
		
		// Retrive the Number of avaible displays
		public int NumberOfScreens()
		{
			return Xlib.XScreenCount(x11Display);
		}
	
		// Returns the current (active) Screen
		public Screen DefaultScreen()
		{
			int			sNum;
			Screen 		s;
			
			// Retrive the default Screen number
			sNum = Xlib.XDefaultScreen(x11Display);
			// Retirve the Screen object for the default screen 
			s = GetScreen(sNum);
			
			return s;
		}
		
		public static IntPtr getDefaultScreen
		{
			get {
				int			sNum;
				IntPtr		x11Screen;
				IntPtr		x11Display;
				
				x11Display = getDefaultDisplay;
				sNum = Xlib.XDefaultScreen(x11Display);
				x11Screen = Xlib.XScreenOfDisplay(x11Display, sNum);
				
				return x11Screen;
			}
		}
		
		public Screen GetScreen(int ScreenNum)
		{	
			return screens[ScreenNum];
		}
	}
	
	// gdk-x11.so native bindings
	public class GdkX11 {
		[DllImport ("gdk-x11-2.0")]
		public static extern IntPtr gdk_x11_display_get_xdisplay (IntPtr gdkDisplay);
	}
	
	// XLib native bindings (plus Xinerama)
	public class Xlib {
		// Xlib
		[DllImport ("X11")]
		public extern static int XScreenCount(IntPtr x11Display);
		[DllImport ("X11")]
		public extern static int XDefaultScreen(IntPtr x11Display);
		[DllImport ("X11")]
		public extern static IntPtr XScreenOfDisplay(IntPtr x11Display, int scnNum);
		[DllImport ("X11")]
		public extern static int XWidthOfScreen(IntPtr x11Screen);
		[DllImport ("X11")]
		public extern static int XHeightOfScreen(IntPtr x11Screen);
		[DllImport ("X11")]
		public extern static int XDefaultDepthOfScreen(IntPtr x11Screen);
		// Xinerama structs
		public struct XineramaScreenInfo {
			public int		screen_number;
			public short	x_org;
			public short	y_org;
			public short	width;
			public short	height;
		};
		// Xinerama funcs
		[DllImport("Xinerama")]
		public extern static bool XineramaIsActive(IntPtr x11Display);
		[DllImport("Xinerama")]
		public extern static IntPtr XineramaQueryScreens(IntPtr x11Display, ref int XinNum);
	}
}
