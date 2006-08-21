bonobodir = $(libdir)/bonobo/servers
bonobo_DATA = DrapesApplet.server 

DrapesApplet.server: $(srcdir)/DrapesApplet.server.in Makefile
	sed -e 's,[@]bindir/[@],$(bindir),'       \
	    < $< > $@

menuxmldir = $(datadir)/gnome-2.0/ui
menuxml_DATA=DrapesApplet.xml

@INTLTOOL_DESKTOP_RULE@
desktopdir = $(datadir)/applications
desktop_in_files=drapes.desktop.in
desktop_DATA=$(desktop_in_files:.desktop.in=.desktop)

schemadir = @GCONF_SCHEMA_FILE_DIR@
schema_DATA = drapes.schemas

@INTLTOOL_SCHEMAS_RULE@

if GCONF_SCHEMAS_INSTALL
install-data-local: $(schema_DATA)
	GCONF_CONFIG_SOURCE=$(GCONF_SCHEMA_CONFIG_SOURCE) $(GCONFTOOL) --makefile-install-rule $(srcdir)/$(schema_DATA);
endif

EXTRA_DIST = \
	DrapesApplet.server.in	\
	drapes.schemas		\
	$(bonobo_in_files)	\
	$(menuxml_DATA)		\
	$(desktop_in_files)	\
	drapes.glade

CLEANFILES = \
	drapes.desktop

DISTCLEANFILES = \
	*.gladep			\
	DrapesApplet.server	\
	drapes.desktop
