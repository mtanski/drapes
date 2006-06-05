TARGET = drapes.exe
CLEANFILES = drapes.exe 

EXTRA_DIST = gui.glade

MCS_FLAGS = -debug 
ASSEMBLIES = \
	-r:System.Xml			\
	-r:Mono.Posix			\
	-pkg:gconf-sharp-2.0		\
	-pkg:glade-sharp-2.0		\
	-pkg:gnome-vfs-sharp-2.0

drapes_sources_in = AssemblyInfo.cs.in
drapes_sources = \
	ConfigMenu.cs 			\
	Main.cs				\
	Settings.cs			\
	Traylib.cs			\
	Wallpaper.cs			\
	WpList.cs

drapes_build_sources = $(drapes_sources)

drapeslibdir = $(libdir)/drapes
drapeslib_DATA = drapes.exe

$(TARGET): $(drapes_build_sources) gui.glade
	$(CSC) $(MCS_FLAGS) $(ASSEMBLIES)  \
	-target:exe -out:"$@" \
	$(drapes_build_sources)


