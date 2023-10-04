
#
# Create the Plane 1 unassigned.hex file, with filler glyphs.
# Create filler glyphs in areas where Unicode scripts are not assigned;
# these are placed in directory "all/" so they are not included with
# the main font build, which reads "plane01/*.hex".
#
plane01-all.hex: plane01.hex plane01-space.hex plane01-nonprinting.hex \
	plane01-unassigned.hex plane01-noscript.hex
	sort -u *.hex > plane01-all.hex

plane01-unassigned.hex: unassigned-ranges.txt
	set -e && \
	   grep "^[0-9A-Fa-f]" unassigned-ranges.txt | \
	   awk 'NF == 1 {print "../../bin/unihexgen " $$1 " " $$1;} \
		NF == 2 {print "../../bin/unihexgen " $$1 " " $$2;}' | \
	   /bin/sh -s > plane01-unassigned.hex

plane01-noscript.hex: noscript-ranges.txt
	set -e && \
	   grep "^[0-9A-Fa-f]" noscript-ranges.txt | \
	   awk 'NF == 1 {print "../../bin/unihexgen " $$1 " " $$1;} \
		NF == 2 {print "../../bin/unihexgen " $$1 " " $$2;}' | \
	   /bin/sh -s > plane01-noscript.hex

clean:

distclean: clean
	\rm -rf plane01-all.hex

.PHONY: clean distclean
