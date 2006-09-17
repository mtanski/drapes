serverdir = $(libdir)/bonobo/servers
server_in_files = DrapesApplet.server.in
server_DATA = $(server_in_files:.server.in=.server)
$(server_in_files): $(srcdir)/DrapesApplet.server.in.in Makefile
	sed -e 's,[@]bindir/[@],$(bindir),'       \
	    < $< > $@
@INTLTOOL_SERVER_RULE@

menuxmldir = $(datadir)/gnome-2.0/ui
menuxml_DATA = DrapesApplet.xml

desktopdir = $(datadir)/applications
desktop_in_files = drapes.desktop.in
desktop_DATA=$(desktop_in_files:.desktop.in=.desktop)
@INTLTOOL_DESKTOP_RULE@

schemadir = $(GCONF_SCHEMA_FILE_DIR)
schema_in_files = drapes.schemas.in
schema_DATA = $(schema_in_files:.schemas.in=.schemas)
@INTLTOOL_SCHEMAS_RULE@

if GCONF_SCHEMAS_INSTALL
install-data-local: $(schema_DATA)
	GCONF_CONFIG_SOURCE=$(GCONF_SCHEMA_CONFIG_SOURCE) \
	$(GCONFTOOL) --makefile-install-rule $(schema_DATA)
endif

EXTRA_DIST = \
	$(schema_in_files)	\
	$(server_in_files)	\
	$(menuxml_DATA)		\
	$(desktop_in_files)	\
	drapes.glade

CLEANFILES = \
	DrapesApplet.server		\
	DrapesApplet.server.in	\
	drapes.schemas			\
	drapes.desktop

DISTCLEANFILES = \
	*.gladep*				\
	*.glade.*

SUBDIRS = \
	images
