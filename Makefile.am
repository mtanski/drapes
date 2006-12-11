SUBDIRS = \
	drapes			\
	scripts			\
	data			\
	help			\
	po				\
	man

check_SCRIPTS_in =          \
    intltool-extract.in     \
    intltool-merge.in		\
	intltool-update.in

doc_files = 				\
	xmldocs.make			\
	omf.make				\
	gnome-doc-utils.make

deb_files =				\
    debian/changelog    \
    debian/copyright    \
    debian/control      \
    debian/rules        

EXTRA_DIST =			\
	$(check_SCRIPTS_in)	\
	$(deb_files)		\
	$(doc_files)

DISTCLEANFILES =		\
	intltool-extract	\
	intltool-update		\
	intltool-merge		\
	*cdbs*				\
	gnome-doc-utils.make

DISTCHECK_CONFIGURE_FLAGS = --disable-scrollkeeper
