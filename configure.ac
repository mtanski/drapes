AC_INIT(drapes/Main.cs)
AC_CANONICAL_SYSTEM

VERSION="0.4.90"
ASM_VERSION="$VERSION.*"

AM_INIT_AUTOMAKE(drapes, $VERSION)
AC_SUBST(ASM_VERSION)

AM_MAINTAINER_MODE

AC_PROG_INSTALL

dnl Find pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

MONO_REQUIRED_VERSION=1.1.15
PKG_CHECK_MODULES(UNMANAGED_DEPENDENCIES_MONO,mono >= $MONO_REQUIRED_VERSION, has_mono=true, has_mono=false)
PKG_CHECK_MODULES(UNMANAGED_DEPENDENCIES_MINT,mint >= $MONO_REQUIRED_VERSION, has_mint=true, has_mint=false)

AC_PATH_PROG(MONO, mono)
AC_PATH_PROG(MCS, mcs)

if test "x$MONO" = "x" ; then
  AC_PATH_PROG(MINT, mint)
  if test "x$MINT" = "x" ; then
	AC_MSG_ERROR([Can't find "mono" or "mint" in your PATH])
  fi
fi

AC_SUBST(PATH)
AC_SUBST(LD_LIBRARY_PATH)

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
    fi
fi

GTKSHARP_REQUIRED=2.8
PKG_CHECK_MODULES(GTKSHARP,
	gtk-sharp-2.0 >= $GTKSHARP_REQUIRED \
	gnome-sharp-2.0 >= $GTKSHARP_REQUIRED \
	gnome-vfs-sharp-2.0 >= $GTKSHARP_REQUIRED \
	glade-sharp-2.0 >= $GTKSHARP_REQUIRED \
	gconf-sharp-2.0 >= $GTKSHARP_REQUIRED)
AC_SUBST(GTKSHARP_LIBS)

AC_OUTPUT([
Makefile
drapes/Makefile
drapes/AssemblyInfo.cs
scripts/drapes
scripts/Makefile
docs/Makefile
])

echo ""
echo "Configuration summary"
echo ""
echo "   * Installation prefix = $prefix"
echo "   * C# compiler = $CSC"
echo ""
