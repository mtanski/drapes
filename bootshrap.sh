#!/bin/bash

# fuck it, just use the gnome autogen
REQUIRED_AUTOMAKE_VERSION="1.9" REQUIRED_AUTOCONF_VERSION="2.53" USE_COMMON_DOC_BUILD="yes" gnome-autogen.sh $@
