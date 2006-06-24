TARGET = drapes.exe
CLEANFILES = drapes.exe 

MCS_FLAGS = -debug 
ASSEMBLIES = \
	-r:System.Xml				\
	-r:Mono.Posix				\
	-pkg:gconf-sharp-2.0		\
	-pkg:glade-sharp-2.0		\
	-pkg:gnome-vfs-sharp-2.0

RESOURCES = \
	-resource:$(top_srcdir)/data/drapes.glade

drapes_sources_in = AssemblyInfo.cs.in
drapes_sources = \
	ConfigMenu.cs 			\
	Main.cs					\
	Settings.cs				\
	Traylib.cs				\
	Wallpaper.cs			\
	WpList.cs

drapes_build_sources = $(drapes_sources)

drapeslibdir = $(libdir)/drapes
drapeslib_DATA = drapes.exe

$(TARGET): $(drapes_build_sources) $(top_srcdir)/data/drapes.glade
	$(CSC) $(MCS_FLAGS) $(ASSEMBLIES) $(RESOURCES)  \
	-target:exe -out:"$@" \
	$(drapes_build_sources)

EXTRA_DIST = $(drapes_build_sources)
