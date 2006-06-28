WRAPPER = drapes

$(WRAPPER): $(srcdir)/$(WRAPPER).in Makefile
	sed -e 's,[@]pkglibdir/[@],$(pkglibdir),'		\
		< $< > $@
	chmod +x $(WRAPPER)

EXTRA_DIST = $(WRAPPER).in
bin_SCRIPTS = $(WRAPPER)

CLEANFILES = $(bin_SCRIPTS)
