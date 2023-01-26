# Image rendering

**TextPaint** in **WorkMode=4** can render text/ANSI file into image or movie\. Renderer uses the same appearance properties as display when **WinUse=1** or **WinUse=2**\.

Rendering movie is possible for ANSI files, when used **ANSIRead=1**\. Some existing ANSI files saves simple animation, which can be shown be progressive processing characters slowly\.

In this mode, **TextPaint** behavies as command line application without user interface\. The generated images will be in PNG format\. While you render movie, actually it will be saved PNG image sequence in specified directory\. You will have to use other software to merge image sequence into single movie file\.

# Rendering parameters

The following parameters are used only for rendering\. Like other parameters, the parameters can be in **Config\.txt** file or provided in command line:


* **RenderFile** \- Image file name for rendering single image \(**RenderStep=0**\) or directory for rendering movie \(**RenderStep>0**\)
* **RenderStep** \- Number of processed character between movie frames\. To render single image, use **RenderStep=0**\. Rendering movie requires **ANSIRead=1**\.
* **RenderOffset** \- Offset in character sequence when rendering movie\. For example, if **RenderStep=10** and **RenderOffset=4**, the frames will be generated after character no: 4th, 14th, 24th and so on\.
* **RenderCursor** \- Draw cursor on rendered image\. Possible value are **0** or **1**\.
* **RenderFrame** \- Number of steps per one frame \(unit\) when you render file containing time markers\.
* **RenderType** \- Type of rendered file \(case insensitive\):
  * **PNG** or every other than listed below\- Render to PNG picture file\(s\)\.
  * **TXT** or **TEXT** \- Render to text file\(s\) without saving color and font size\.
  * **ANS** or **ANSI** \- Render to ANSI file\(s\) containing text with used color and font size\.
  * **XB** or **XBIN** \- Convert XBIN file to ANSI file instead of rendering, extract font and palette if contains\.
  * **BIN** \- Convert raw binary file to ANSI file instead of rendering\.

# Graphic parameters

The following parameters are usable in other work modes, but also affects the rendered image appearance:


* **WinCellW** \- Character cell width in window\. If you use the bitmap font, it will be rounded to integer multiply of font glyph width\.
* **WinCellH** \- Character cell height in window\. If you use the bitmap font, it will be rounded to integer multiply of font glyph height\.
* **WinFontName** \- Font name or bitmap file in window\. When the file of given name exists, there will be used bitmap file, otherwise, the value will be used as font name\.
* **WinFontSize** \- Font size in window\. This setting not affects the cell size\.
* **WinColorBlending** \- Enables color blending for some block characters\.
* **WinPaletteR** \- The red component of all 16 colors\.
* **WinPaletteG** \- The green component of all 16 colors\.
* **WinPaletteB** \- The blue component of all 16 colors\.
* **FileReadEncoding** \- Encoding used in file reading, in most cases, it should be **FileReadEncoding=utf\-8**\.
* **FileWriteEncoding** \- Encoding used in ANSI file writing while converting XBIN or BIN file to ANSI file\.
* **ColorNormal** \- Color of default background and foreground in all work modes\.
* **ANSIRead** \- Use ANSI interpreter instead of plain text text file on file reading\.
* **ANSIWidth** \- Define width of ANSI virtual screen\. If **ANSIWidth=0**, the default screen width will be used\.
* **ANSIHeight** \- Define height of ANSI virtual screen\. If **ANSIHeight=0**, the default screen height will be used\.

If **ANSIRead=1**, the ANSI\-related parameters works as described in **Readme\_ANSI\.md** file\. Parameters unusable in **WorkMode=4**\. provided both in command line or in **Config\.txt** file, are ignored\.

## Creating text frames

If you specify **RenderType=TEXT** or **RenderType=ANSI**, the rendering behavior will be the same, but there will be generate TXT/ANS files containing text instead of PNG files\. Such TXT/ANS file can be opened in **TextPaint** or other text viewer/editor\. This feature is usable for creating some art based on specified animation file\.

# Conversion XBIN file to ANSI file

The file format used in TextPaint is ANSI text file, which extension is usually **\.ans**, but there is possible to convert XBIN file file into ANSI text file\. Such XBIN files can be found on some repositories containing IBM\-compatible computer screen dump saved to files\. This conversion can be achieved using **RenderType=XBIN**\.

The XBIN file \(usually **\.xb** extension\) can contain the following elements with file extension, which TextPaint uses in conversion:


* Color palette used to define 16 colors \- saved to **\*\.txt** file\.
* Primary font glyphs set \- saved to **\*\.png** file\.
* Secondary font glyphs set \- saved to **\*\_\.png** file\.
* Compressed or uncompressed raw screen data, containing color and character \- saved to **\*\.ans** file\.

The palette and fonts can be ommited\. Secondary font feature were used very rarely\.

The XBIN file can contain the special characters \(including CR and LF\) used to display some glyphs, so it is highly recommended to use encoding file, which remapes every control character to printable character\. The write encoding uset to save ANSI file should be UTF\-8 in most cases\.

The sample encodings suitable for conversione XBIN to ANSI are:


* **Encodings/BIN\_DOS437\.txt** \- Based on **DOS437\.txt** \(slightly modified code page 437\), but the control characters are mapped into standard glyphs\. This encoding is suitable for files with character set similar to standard IBM character set\.
* **Encodings/BIN\_ASCII\.txt** \- Similar to ASCII and ISO\-8859\-1 encodings, all printable characters are mapped to the same numbers, but control characters are mapped to character within **2400h**\-**241Fh**\. This encoding is suitable for files with non\-standard or unknown character set\.

If XBIN file contains own font, which will be used in editor or viewer, there is not difference, which encoding will be used\. The only difference is in font layout\.

The ANSI file will be saved without end\-of\-line marks, but will be saved with spaces from beginning to end of every line\. This file can be displayed or rendered correctly regardless **ANSIDOS** parameter, but the **ANSIWidth** must match screen width saved in original XBIN file \(in most cases, the XBIN file is dumped from screen with 80 columns\)\.

After conversion, there will be printed the following informations:


1. File name containing color palette, or information, that color palette does not exist\.
2. File name containing primary and secondary font file, or information, that font does not exist\.
3. File name containing text data\.
4. Text width and height \(number of columns and lines\)\.

The palette and font files can be used as **WinPaletteFile** and **WinFontName** parameters respectively\.

# Conversion BIN file to ANSI file

The BIN file is the simple raw screen dump, where odd bytes are character number, and even bytes are color number \(combined background and foreground color\)\. This conversion can be performed using **RenderType=BIN**\.

Conversion BIN to ANSI has the same rules as conversion XBIN to ANSI with following exceptions:


* BIN file contains only text data\.
* Screen width and height is not specified in file and will not printed after conversion\.
* Every file byte will be processed\.

The conversion wil generate the **\*\.ans** file only\.

# Examples

There is examples with parameters can be invoked in command line, but the parameters can also be in **Config\.txt** file\.

Create single image of plan text file using black characters on white backgrounf with size 80x25:

```
TextPaint.exe /Path/File.txt RenderFile=/Path/Image.png ANSIRead=0 ANSIWidth=80 ANSIHeight=25 ColorNormal=F0
```

Create single image of ANSI file with default screen size:

```
TextPaint.exe /Path/File.txt RenderFile=/Path/Image.png ANSIRead=1 ANSIWidth=0 ANSIHeight=0
```

Create movie from ANSI animation by creating frame on 5th character, 20th character, 35th character and so on:

```
TextPaint.exe /Path/File.txt RenderFile=/Path/MovieFolder ANSIRead=1 RenderStep=15 RenderOffset=5
```

Create movie from ANSI animation recorded by TextPaint using 1000 characters per rendered frame and 100 characters per one time marker unit:

```
TextPaint.exe /Path/File.txt RenderFile=/Path/MovieFolder ANSIRead=1 RenderStep=1000 RenderOffset=0 RenderFrame=100
```

Convert XBIN \(which contains font and color palette\) file to ANSI file using encoding from file and open generated ANSI in editor:

```
TextPaint.exe /Path/File.bin RenderFile=/Path/FileANSI RenderType=XBIN WorkMode=4 FileReadEncoding="Encodings/BIN_ISO.txt"
TextPaint.exe /Path/FileANSI.ans WorkMode=0 WinFontName=/Path/FileANSI.png WinPaletteFile=/Path/FileANSI.txt
```

# Create files of 8\-bit encodings

Using **WorkMode=4**, you can generate files of all 8\-bit encodings supported by your system\. In order to do this, you have to provide **?ENCODING?** as text/ANSI file name and use **RenderFile** parameter to provide folder\. Other parameters does not affect creating the files in this case and will be ignored\.

Every file will have codepage numer as name with **\.txt** extension\. inside the file, there will be following parameters:


* **Codepage** \- Codepage number\.
* **Name** \- The name of encoding, if exists\.
* **AlternativeName** \- Other name of encoding if this encoding has two different names\.

Below the parameters, there are hexadecimal byte values as further parameters with assigned characters\. Such file can be used as custom encoding without any modification\.




