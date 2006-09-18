pixmapdir = $(datadir)/pixmaps
pixmap_DATA = drapes.png

hicolordir = $(datadir)/icons/hicolor

tango_icons = 				\
	drapes-16.png			\
	drapes-22.png			\
    drapes-24.png			\
	drapes-32.png			\
	drapes.png				\
	drapes.svg

install-data-local:
	@-$(mkinstalldirs) $(DESTDIR)$(hicolordir)/scalable/apps
	$(INSTALL_DATA) $(srcdir)/drapes.svg $(DESTDIR)$(hicolordir)/scalable/apps/drapes.svg
	@-$(mkinstalldirs) $(DESTDIR)$(hicolordir)/16x16/apps
	$(INSTALL_DATA) $(srcdir)/drapes-16.png $(DESTDIR)$(hicolordir)/16x16/apps/drapes.png
	@-$(mkinstalldirs) $(DESTDIR)$(hicolordir)/22x22/apps
	$(INSTALL_DATA) $(srcdir)/drapes-22.png $(DESTDIR)$(hicolordir)/22x22/apps/drapes.png
	@-$(mkinstalldirs) $(DESTDIR)$(hicolordir)/24x24/apps
	$(INSTALL_DATA) $(srcdir)/drapes-24.png $(DESTDIR)$(hicolordir)/24x24/apps/drapes.png
	@-$(mkinstalldirs) $(DESTDIR)$(hicolordir)/32x32/apps
	$(INSTALL_DATA) $(srcdir)/drapes-32.png $(DESTDIR)$(hicolordir)/32x32/apps/drapes.png
	@-$(mkinstalldirs) $(DESTDIR)$(hicolordir)/48x48/apps
	$(INSTALL_DATA) $(srcdir)/drapes.png $(DESTDIR)$(hicolordir)/48x48/apps/drapes.png

gtk_update_icon_cache = gtk-update-icon-cache -f -t $(datadir)/icons/hicolor

install-data-hook:
	@-if test -z "$(DESTDIR)"; then \
		echo "Updating Gtk icon cache."; \
		$(gtk_update_icon_cache); \
	else \
		echo "*** Icon cache not updated.  After install, run this:"; \
		echo "***   $(gtk_update_icon_cache)"; \
	fi

uninstall-hook:
	rm -f $(DESTDIR)$(hicolordir)/scalable/apps/drapes.svg
	rm -f $(DESTDIR)$(hicolordir)/16x16/apps/drapes.png
	rm -f $(DESTDIR)$(hicolordir)/22x22/apps/drapes.png
	rm -f $(DESTDIR)$(hicolordir)/24x24/apps/drapes.png
	rm -f $(DESTDIR)$(hicolordir)/32x32/apps/drapes.png
	rm -f $(DESTDIR)$(hicolordir)/48x48/apps/drapes.png

noinst_DATA = 				\
	drapes-16.xcf			\
	drapes-22.xcf			\
	drapes-32.svg

EXTRA_DIST = $(noinst_DATA) $(pixmap_DATA) $(tango_icons)
