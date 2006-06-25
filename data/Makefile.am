desktopdir = $(datadir)/applications
desktop_in_files=drapes.desktop.in
desktop_DATA=$(desktop_in_files:.desktop.in.in=.desktop)

%desktop.in: %.desktop.in
	sed -e "s/\@VERSION\@/$(VERSION)/" < $< > $@

@INTLTOOL_DESKTOP_RULE@

schema_in_files = drapes.schemas.in
schemadir = $(GCONF_SCHEMA_FILE_DIR)
schema_DATA = $(schema_in_files:.schemas.in=.schemas)

@INTLTOOL_SCHEMAS_RULE@

if GCONF_SCHEMAS_INSTALL
install-data-local:
        if [ -z "$(DESTDIR)" ]; then \
                GCONF_CONFIG_SOURCE="" $(GCONFTOOL) --makefile-install-rule $(builddir)$(schema_DATA); \
        fi
endif

EXTRA_DIST = \
	$(schema_in_files)	\
	$(desktop_in_files)	\
	$(gladedir_data)

