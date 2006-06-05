#!/bin/bash

aclocal
automake --gnu --add-missing
autoconf
./configure $@
