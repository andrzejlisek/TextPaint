
#
# Create the Plane 0 CSUR unassigned.hex file, with filler glyphs.
# Create filler glyphs in areas where Unicode scripts are not assigned;
# these are placed in directory "all/" so they are not included with
# the main font build, which can read "plane00csur/*.hex" as an alternate.
#
plane00csur-all.hex: plane00csur.hex plane00csur-spaces.hex \
	             plane00csur-nonprinting.hex plane00csur-unassigned.hex
	set -e && \
	   grep "^[0-9A-Fa-f]" noscript-ranges.txt | \
	   awk 'NF == 1 {print "../../bin/unihexgen " $$1 " " $$1;} \
		NF == 2 {print "../../bin/unihexgen " $$1 " " $$2;}' | \
	   /bin/sh -s > noscript.hex
	sort -u *.hex > plane00csur-all.hex

plane00csur-unassigned.hex: unassigned-ranges.txt
	set -e && \
	   grep "^[0-9A-Fa-f]" unassigned-ranges.txt | \
	   awk 'NF == 1 {print "../../bin/unihexgen " $$1 " " $$1;} \
		NF == 2 {print "../../bin/unihexgen " $$1 " " $$2;}' | \
	   /bin/sh -s > plane00csur-unassigned.hex

clean:

distclean: clean
	\rm -f plane00csur-all.hex

.PHONY: clean distclean
