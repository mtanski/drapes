SUBDIRS = \
	drapes			\
	scripts			\
	data			\
	docs			\
	po

check_SCRIPTS_in =          \
    intltool-extract.in     \
    intltool-merge.in		\
	intltool-update.in

EXTRA_DIST =			\
	$(check_SCRIPTS_in)	\
	debian/changelog	\
	debian/copyright	\
	debian/control		\
	debian/rules

DISTCLEANFILES =		\
	intltool-extract	\
	intltool-update		\
	intltool-merge		\
	*cdbs*
