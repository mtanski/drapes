AC_INIT(drapes/Main.cs)
AC_CANONICAL_SYSTEM

VERSION="0.4.96"
ASM_VERSION="$VERSION.*"

AM_INIT_AUTOMAKE(drapes, $VERSION)
AC_SUBST(ASM_VERSION)

AM_MAINTAINER_MODE

AC_PROG_INTLTOOL([0.21])

AC_PROG_INSTALL

dnl Find pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

dnl Mono
MONO_REQUIRED_VERSION=1.1.13
PKG_CHECK_MODULES(UNMANAGED_DEPENDENCIES_MONO,mono >= $MONO_REQUIRED_VERSION, has_mono=true, has_mono=false)
PKG_CHECK_MODULES(UNMANAGED_DEPENDENCIES_MINT,mint >= $MONO_REQUIRED_VERSION, has_mint=true, has_mint=false)

if test "x$has_mono" = "xtrue"; then
    AC_PATH_PROG(RUNTIME, mono, no)
    AC_PATH_PROG(CSC, gmcs, no)
    LIB_PREFIX=.so
    LIB_SUFFIX=
else
    if test "x$has_mint" = "xtrue"; then
	AC_PATH_PROG(RUNTIME, mint, no)
	AC_PATH_PROG(CSC, gmcs, no)
	LIB_PREFIX=.so
	LIB_SUFFIX=
    else
      AC_MSG_ERROR([You need to install mono])
    fi
fi

CS="C#"
if test "x$CSC" = "xno" ; then
    AC_MSG_ERROR([No $CS compiler found])
fi

AC_SUBST(RUNTIME)
AC_SUBST(CSC)

dnl Gtk#

GTKSHARP_REQUIRED=2.8
PKG_CHECK_MODULES(GTKSHARP,
	gtk-sharp-2.0 >= $GTKSHARP_REQUIRED \
	gnome-sharp-2.0 >= $GTKSHARP_REQUIRED \
	gnome-vfs-sharp-2.0 >= $GTKSHARP_REQUIRED \
	glade-sharp-2.0 >= $GTKSHARP_REQUIRED \
	gconf-sharp-2.0 >= $GTKSHARP_REQUIRED)
AC_SUBST(GTKSHARP_LIBS)

dnl Gconf
AC_PATH_PROG(GCONFTOOL, gconftool-2, no)
AM_GCONF_SOURCE_2

dnl internatilization
ALL_LINGUAS="`cat "$srcdir/po/LINGUAS" | grep -v '^#'`"
GETTEXT_PACKAGE=drapes
AC_SUBST(GETTEXT_PACKAGE)
AC_DEFINE_UNQUOTED(GETTEXT_PACKAGE, "$GETTEXT_PACKAGE", [Gettext Package])
AM_GLIB_GNU_GETTEXT
AC_SUBST([CONFIG_STATUS_DEPENDENCIES],['$(top_srcdir)/po/LINGUAS'])

dnl gnome documentation stuffs
AC_CONFIG_MACRO_DIR([m4])
GNOME_DOC_INIT

AC_OUTPUT([
Makefile
drapes/Makefile
scripts/Makefile
help/Makefile
data/Makefile
data/images/Makefile
po/Makefile.in
])

echo ""
echo "Configuration summary"
echo ""
echo "   * Installation prefix = $prefix"
echo "   * C# compiler = $CSC"
echo ""
