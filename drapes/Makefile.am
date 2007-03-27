TARGET = drapes.exe

MCS_FLAGS = -debug 
ASSEMBLIES = \
	-r:System.Xml				\
	-r:Mono.Posix				\
	-pkg:gconf-sharp-2.0		\
	-pkg:glade-sharp-2.0		\
	-pkg:gnome-vfs-sharp-2.0

RESOURCES = \
	-resource:$(top_srcdir)/data/drapes.glade,drapes.glade

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e "s,[@]ASM_VERSION/[@],$(ASM_VERSION),"		\
	    -e "s,[@]datadir/[@],$(datadir),"				\
		-e "s,[@]prefix/[@],$(prefix),"					\
		-e "s,[@]version/[@],$(VERSION),"				\
		-e "s,[@]helpdir/[@],$(HELP_DIR),"				\
	    < $< > $@

drapes_sources_in = $(srcdir)/AssemblyInfo.cs.in
drapes_sources = \
	$(srcdir)/panelapplet/*.cs			\
	$(srcdir)/About.cs					\
	$(srcdir)/Applet.cs					\
	$(srcdir)/AppletWidget.cs			\
	$(srcdir)/ConfigMenuWidgets.cs		\
	$(srcdir)/ConfigMenu.cs				\
	$(srcdir)/Main.cs					\
	$(srcdir)/Settings.cs				\
	$(srcdir)/Traylib.cs				\
	$(srcdir)/Wallpaper.cs				\
	$(srcdir)/WpList.cs

drapes_build_sources = $(drapes_sources_in:.cs.in=.cs) $(drapes_sources)

drapeslibdir = $(libdir)/drapes
drapeslib_DATA = drapes.exe

$(TARGET): $(drapes_build_sources) $(top_srcdir)/data/drapes.glade
	$(CSC) $(MCS_FLAGS) $(ASSEMBLIES) $(RESOURCES)  \
	-unsafe+ -target:exe -out:"$@" \
	$(drapes_build_sources)

EXTRA_DIST = $(drapes_build_sources) $(srcdir)/AssemblyInfo.cs.in

CLEANFILES =	\
	$(TARGET)	\
	*.mdb

DISTCLEANFILES = \
	AssemblyInfo.cs
