# VTx Character mapping

Original VTx terminals operates on 7\-bit data or 8\-bit data, so the character map consists of 256 items including control and special characters\. The map is splitted into two parts consisting 96 printable characters, called as **GL** and **GR**\.

The DOS system uses the character maps \(called as code page\) in the different way, which can be treated as encoding \(usually IBM 437\) and the code page is never changed during animation playback or terminal session\. The ISO 8859\-1 and Windows\-1252 character maps are used very rarely in original DOS ANSI files\.

In both cases, TextPaint remaps the characters to the Unicode encoding, so the character banks are used for first 256 characters only \(excluding control characters\) after this remapping\.

## Character banks

For the **GL** and **GR** separately, there can be selected one of the four banks called as **G0**, **G1**, **G2** and **G3** by following commands:

| Escape sequencte | XTERM name | Meaning |
| --- | --- | --- |
| 0Fh | LS0 | Assign **G0** to **GL**\. |
| 0Eh | LS1 | Assign **G1** to **GL**\. |
| 1Bh n | LS2 | Assign **G2** to **GL**\. |
| 1Bh o | LS3 | Assign **G3** to **GL**\. |
| 1Bh ~ | LS1R | Assign **G1** to **GR**\. |
| 1Bh \} | LS2R | Assign **G2** to **GR**\. |
| 1Bh &#124; | LS3R | Assign **G3** to **GR**\. |
| 1Bh N | SS2 | Assign **G2** to **GR** for single character\. |
| 1Bh O | SS3 | Assign **G3** to **GR** for single character\. |

Notes: Assignment G0 to GR is not possible\. These command not works when **ANSIDOS=1**\.

## Designate character set into bank

| Escape sequence | Meaning |
| --- | --- |
| 1Bh \( Code | Designate **G0** as Type\-1 Code character set\. |
| 1Bh \) Code | Designate **G1** as Type\-1 Code character set\. |
| 1Bh \* Code | Designate **G2** as Type\-1 Code character set\. |
| 1Bh \+ Code | Designate **G3** as Type\-1 Code character set\. |
| 1Bh \- Code | Designate **G1** as Type\-2 Code character set\. |
| 1Bh \. Code | Designate **G2** as Type\-2 Code character set\. |
| 1Bh / Code | Designate **G3** as Type\-2 Code character set\. |

Note: Designation G0 as any Type\-2 character set is not possible\. These command not works when **ANSIDOS=1**\.

## Possible character sets

For each bank, you can use one of the many character sets, which are splitted into two types:


* Type 1:
  * Bases on US ASCII or DEC Supplemental character set\.
  * The DEC Supplemental character set is similat to ISO\-8859\-1, but differs in several characters\.
  * At the 20h there is a space and at the 7Ch there is control character, so there are 94 glyphs\.
  * Some sets requires the National Replacement Character option in Enabled state, otherwise the set works the same as US ASCII\.
* Type 2:
  * Bases on ISO 8859\-2 called as Latin\-1\.
  * The Latin\-1 characters are the same as characters from A0h to FFh in Unicode standard\.
  * All characters \(including 20h and 7Ch\) has any glyph, so there are 96 glyphs\.

Some character glyphs and mappings to Unicode are not documented and originally was not used\. Such characters are mapped into FFFDh character named as "replacement character"\. Some sets has several equivalent codes\.

| Type | Code | Name | Remarks | Based on | DEC serie introduced |
| --- | --- | --- | --- | --- | --- |
| 1 | B | US ASCII | Default for G0 and G1 bank, base for some other sets | US ASCII | VT100 |
| 1 | 1 | DEC Alternate character ROM standard characters | The same as US ASCII | US ASCII | VT100 |
| 1 | A | British |   | US ASCII | VT100 |
| 1 | > | DEC Technical |   | Ambiguous | VT300 |
| 1 | 0 | DEC Special graphics and line drawing |   | US ASCII | VT100 |
| 1 | 2 | DEC Alternate character ROM special graphics | The same as DEC Special graphics and line drawing | US ASCII | VT100 |
| 1 | < | DEC Supplemental | Default for G2 and G3 bank when ANSIDOS=2, base for some other sets | DEC Supplemental | VT200 |
| 1 | %5 | DEC Supplemental Graphic | The same as DEC Supplemental | DEC Supplemental | VT300 |
| 1 | 4 | Dutch | Depends on NRC | US ASCII | VT200 |
| 1 | 5 or C | Finnish | Depends on NRC | US ASCII | VT200 |
| 1 | R or f | French | Depends on NRC | US ASCII | VT200 |
| 1 | Q or 9 | French Canadian | Depends on NRC | US ASCII | VT200 |
| 1 | K | German | Depends on NRC | US ASCII | VT200 |
| 1 | Y | Italian | Depends on NRC | US ASCII | VT200 |
| 1 | \` or 6 or E | Norwegian/Danish | Depends on NRC | US ASCII | VT200 |
| 1 | %6 | Portuguese | Depends on NRC | US ASCII | VT300 |
| 1 | Z | Spanish | Depends on NRC | US ASCII | VT200 |
| 1 | 7 or H | Swedish | Depends on NRC | US ASCII | VT200 |
| 1 | = | Swiss | Depends on NRC | US ASCII | VT200 |
| 1 | %2 | Turkish | Depends on NRC | US ASCII | VT500 |
| 1 | "> | Greek | Depends on NRC, replaced all lower\-case letters | US ASCII | VT500 |
| 1 | %= | Hebrew | Depends on NRC, replaced all lower\-case letters | US ASCII | VT500 |
| 1 | &4 | Cyrillic \(DEC\) |   | Ambiguous | VT500 |
| 1 | "? | Greek \(DEC\) |   | DEC Supplemental | VT500 |
| 1 | "4 | Hebrew \(DEC\) |   | DEC Supplemental | VT500 |
| 1 | %0 | Turkish \(DEC\) |   | DEC Supplemental | VT500 |
| 1 | &5 | Russian | Not implemented, actually the same as US ASCII | US ASCII | VT500 |
| 1 | %3 | SCS | Not implemented, actually the same as US ASCII | US ASCII | VT500 |
| 2 | A | ISO Latin\-1 | ISO\-8859\-1 code page, default for G2 and G3 bank, base for some other sets | ISO Latin\-1 | VT300 |
| 2 | B | ISO Latin\-2 Supplemental | ISO\-8859\-2 code page | ISO Latin\-1 | VT500 |
| 2 | L | ISO Latin\-Cyrillic | ISO\-8859\-5 code page | ISO Latin\-1 | VT500 |
| 2 | F | ISO Greek Supplemental | ISO\-8859\-7 code page | ISO Latin\-1 | VT500 |
| 2 | H | ISO Hebrew Supplemental | ISO\-8859\-8 code page | ISO Latin\-1 | VT500 |
| 2 | M | ISO Latin\-5 Supplemental | ISO\-8859\-9 code page | ISO Latin\-1 | VT500 |

## Command examples

There ase some example codes for character set designation:

| Command | Meaning |
| --- | --- |
| 1Bh \( B | Designate C0 as US ASCII\. |
| 1Bh \* % 5 | Designate C2 as DEC Supplemental Graphic\. |
| 1Bh \. A | Designate C2 as ISO Latin\-1\. |




