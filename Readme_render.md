# Image rendering

**TextPaint** in **WorkMode=4** can render text/ANSI file into image or movie\. Renderer uses the same appearance properties as display when **WinUse=1** or **WinUse=2**\.

Rendering movie is possible for ANSI files, when used **ANSIRead=1**\. Some existing ANSI files saves simple animation, which can be shown be progressive processing characters slowly\.

In this mode, **TextPaint** behavies as command line application without user interface\. The generated images will be in PNG format\. While you render movie, actually it will be saved PNG image sequence in specified directory\. You will have to use other software to merge image sequence into single movie file\.

# Rendering parameters

The following parameters are used only for rendering\. Like other parameters, the parameters can be in **Config\.txt** file or provided in command line:


* **RenderFile** \- Image file name for rendering single image \(**RenderStep=0**\) or directory for rendering movie \(**RenderStep>0**\)
* **RenderStep** \- Number of processed character between movie frames\. To render single image, use **RenderStep=0**\. Rendering movie requires **ANSIRead=1**\.
* **RenderOffset** \- Offset in steps when rendering movie\. For example, if **RenderStep=10** and **RenderOffset=4**, the frames will be generated after step no: 4th, 14th, 24th and so on\.
* **RenderCursor** \- Draw cursor on rendered image\. Possible value are **0** or **1**\.
* **RenderFrame** \- Number of steps per one frame \(unit\) when you render file containing time markers\.
* **RenderType** \- Type of rendered file \(case insensitive\):
  * **PNG** or every other than listed below\- Render to PNG picture file\(s\)\.
  * **TXT** or **TEXT** \- Render to text file\(s\) without saving color and font size\.
  * **ANS** or **ANSI** \- Render to ANSI file\(s\) containing text with used color and font size\.
  * **XB** or **XBIN** \- Convert XBIN file to ANSI file instead of rendering, extract font and palette if contains\.
  * **BIN** \- Convert raw binary file to ANSI file instead of rendering\.
  * **CONV** or **CONVERT** \- Convert any text file from one encoding to other encoding, without text parsing and analyzing\.
* **RenderSliceW** \- The width of sliced picture\. Use **RenderSliceW=0** for disable slicing horizontally\.
* **RenderSliceH** \- The height of sliced picture\. Use **RenderSliceH=0** for disable slicing vertically\.
* **RenderSliceX** \- The horizontal offset, works only if **RenderSliceW>0**\.
* **RenderSliceY** \- The horizontal offset, works only if **RenderSliceH>0**\.
* **RenderLeading** \- Number of dummy leading ANSI processing steps before the file contents\. It can be used for create additional blank frames before ANSI animation\.
* **RenderTrailing** \- Number of dummy trailing ANSI processing steps after the file contents\. It can be used for create additional repeating frames after ANSI animation\.
* **RenderBlinkPeriod** \- Number of frames for one of the blinking state, the full blinking period \(including the base and alternate state\) equals thice the value\. Works when the **RenderStep>0** only\.
* **RenderBlinkOffset** \- The Offset of the blinking cycle, which will begin the rendered animation\.

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


* **Encodings/STD437\_BIN\.txt** \- Based on **STD437\.txt** \(code page 437\), but the control characters are mapped into standard glyphs\. This encoding is suitable for files with IBM standard character set and font other than DOS font\.
* **Encodings/DOS437\_BIN\.txt** \- Based on **DOS437\.txt** \(slightly modified code page 437\), but the control characters are mapped into standard glyphs\. This encoding is suitable for files with character set similar to standard IBM character set\.
* **Encodings/ASCII\_BIN\.txt** \- Similar to ASCII and ISO\-8859\-1 encodings, all printable characters are mapped to the same numbers, but control characters are mapped to character within **2400h**\-**241Fh**\. This encoding is suitable for files with non\-standard or unknown character set\.

If XBIN file contains own font, which will be used in editor or viewer, there is not difference, which encoding will be used\. The only difference is in font layout\.

The ANSI file will be saved without end\-of\-line marks, but will be saved with spaces from beginning to end of every line\. This file can be displayed or rendered correctly regardless **ANSIDOS** parameter, but the **ANSIWidth** must match screen width saved in original XBIN file \(in most cases, the XBIN file is dumped from screen with 80 columns\)\. If the original XBIN file contains the SAUCE information, the unmodified SAUCE information will be saved in he ANSI file\.

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

The conversion wil generate the **\*\.ans** file only\. If the original BIN file contains the SAUCE information, the unmodified SAUCE information will be saved in he ANSI file\.

# Conversion between encodings

TextPaint allows to convert text files between encoding by **RenderType=CONVERT**\. The **ANSIRead** and **ANSIWrite** parameters will be ignored\. You have to specify the following parameters:


* **RenderFile** \- Destination text file\.
* **FileReadEncoding** \- Encoding of source file \(code page number or encoding name or encoding file\)\.
* **FileWriteEncoding** \- Encoding of destination file \(code page number or encoding name or encoding file\)\.

The text content will not be parsed or analyzed\. You can use this command for any text file\.

# Command examples

There is examples with parameters can be invoked in command line, but the parameters can also be in **Config\.txt** file\.

Create single image of plan text file using black characters on white backgrounf with size 80x25:

```
TextPaint /Path/File.txt RenderFile=/Path/Image.png ANSIRead=0 ANSIWidth=80 ANSIHeight=25 ColorNormal=F0
```

Create single image of ANSI file with default screen size:

```
TextPaint /Path/File.txt RenderFile=/Path/Image.png ANSIRead=1 ANSIWidth=0 ANSIHeight=0
```

Create movie from ANSI animation by creating frame on 5th character, 20th character, 35th character and so on:

```
TextPaint /Path/File.txt RenderFile=/Path/MovieFolder ANSIRead=1 RenderStep=15 RenderOffset=5
```

Create movie from ANSI animation recorded by TextPaint using 1000 characters per rendered frame and 100 characters per one time marker unit:

```
TextPaint /Path/File.txt RenderFile=/Path/MovieFolder ANSIRead=1 RenderStep=1000 RenderOffset=0 RenderFrame=100
```

Convert XBIN \(which contains font and color palette\) file to ANSI file using encoding from file and open generated ANSI in editor:

```
TextPaint /Path/File.bin RenderFile=/Path/FileANSI RenderType=XBIN WorkMode=4 FileReadEncoding="Encodings/BIN_ISO.txt"
TextPaint /Path/FileANSI.ans WorkMode=0 WinFontName=/Path/FileANSI.png WinPaletteFile=/Path/FileANSI.txt
```

Convert text file from standard DOS 437 code page to UTF\-8:

```
TextPaint /Path/Text437.txt RenderFile=/Path/TextUTF8.txt RenderType=CONVERT FileReadEncoding="437" FileWriteEncoding="utf-8"
```

# Slicing file

The rendered file \(TEXT, ANSI or PNG\) can be sliced into parts horizontally and vertically\. This feature is usable especially, when you want to prepare old\-school style slideshow using **TextPaint**\. In this case, you can prepare all slides in single TEXT or ANSI file\. Assuming, that the single slide size is 80x25, the slide may have the following layout:


* The first slide will occupy lices from 0 to 24\.
* The second slide will occupy lices from 25 to 49\.
* The third slide will occupy lices from 50 to 74\.
* The n\-th slide will occupy lices from `(n-1)*25` to `(n*25)-1`\.

Of course, you can use any existing ANSI or TXT file for slicing\.

Without slicing, the final file size will be measured when rendering\. You can slice horizontally, vertically and in both directions \(produces horizontal and vertical slice matrix\)\.

For vertical slice, you have to set the **RenderSliceH** parameter as single slice height\. This slice dimension does not have to be equal to **ANSIHeight**\. Also, you can set the vertical offset by parameter **RenderSliceY**\. This value indicates, where will be sliced the first place\. If **RenderSliceY** is negative or equals at least **ANSIHeight**, the **RenderSliceY** will be automaticaly changed by adding or subtracting the **RenderSizeY** value\.

Horizontal slicing works independally of vertical slicing and can be controlled by **RenderSliceW** and **RenderSliceX** values\. The meaning is analogous to vertical slicing\.

The number of slices will be measured during rendering\.

If you configure both vertical and horizontal slicing, there will be generatef slicing matrix\. Assume, that rendered file without slicing has the size 120x70\. Assume, that the slicing parameters are following:


* **RenderSliceW**=80
* **RenderSliceH**=25
* **RenderSliceX**=0
* **RenderSliceY**=0

In thys case, there will be generated 2 slices horizontally and 3 slices vertically\. In total, there will be generated 6 files, due to 6 slices in 2x3 matrix\.

# Terminal recording and rendering examples

Assume, that desired display speed is 2500 characters per second\. There are example commands for record, view and render terminal session at the speed\.

To get this speed, you can use following command for telnet session with session recording:

```
TextPaint "localhost" WorkMode=2 TerminalFile=Example.ans **TerminalStep=100** **TerminalTimeResolution=40**
```

The speed meets the following formula:

```
Speed in characters by second: (1000 / TerminalTimeResolution) * TerminalStep
```

The file will contain timing markers, where single marker has value equals to session time millisecond divided by **TerminalTimeResolution**\. In this example, one second contains 25 timing units\. This is only timing of data portions received only, its not affect the text display smooth, but you have to remember the **TerminalTimeResolution** value used for recording the file for correct playing\. To create ledd resource\-demanding, but less accurate recording \(12\.5 timing units per second\) at the same speed, you should execute this command:

```
TextPaint "localhost" WorkMode=2 TerminalFile=Example.ans **TerminalStep=200** **TerminalTimeResolution=80**
```

For display and play the file, which is saved terminal session, you can use the following command:

```
TextPaint Example.ans WorkMode=1 **FileDelayTime=40** **FileDelayStep=100** **FileDelayFrame=100**
```

```
Speed in characters by second: (1000 / FileDelayTime) * FileDelayStep
```

The **FileDelayFrame** value should be the same as **TerminalStep** value used for recording session\. Otherwise, the pauses between data portions, which occurred while waiting for user reactions, will be too short or too long\. If the file is not a terminal session record created in **TextPaint**, the **FileDelayFrame** value does not affect the result\.

For smoother playing at the same speed \(but is more resource demanding\) you can execute the following command:

```
TextPaint Example.ans WorkMode=1 **FileDelayTime=20** **FileDelayStep=50** **FileDelayFrame=100**
```

For render the file, you can execute this command:

```
TextPaint Example.ans WorkMode=4 RenderFile=ExampleRender **RenderStep=100 RenderFrame=100**
```

This command will generate serie of frames\. The original frame rate for playing the sequence, you can calculate the frame rate:

```
Frames per second: [Characters per second] / RenderStep
```

To get the same movie rendered in 50 frames per second, you should execute:

```
TextPaint Example.ans WorkMode=4 RenderFile=ExampleRender **RenderStep=50 RenderFrame=100**
```

The **RenderFrame** means the speed of timing units\. It should be the same as **TerminalStep** used in terminal session\. If the file is not a terminal session record created in **TextPaint**, the **RenderFrame** value does not affect the result\.

# Create files of 8\-bit encodings

Using **WorkMode=4**, you can generate files of all 8\-bit encodings supported by your system\. In order to do this, you have to provide **?ENCODING?** as text/ANSI file name and use **RenderFile** parameter to provide folder\. Other parameters does not affect creating the files in this case and will be ignored\.

```
TextPaint ?ENCODING? WorkMode=4 RenderFile=DotNetEncodingFolder
```

Every file will have codepage numer as name with **\.txt** extension\. inside the file, there will be following parameters:


* **Codepage** \- Codepage number\.
* **Name** \- The name of encoding, if exists\.
* **AlternativeName** \- Other name of encoding if this encoding has two different names\.

Below the parameters, there are hexadecimal byte values as further parameters with assigned characters\. Such file can be used as custom encoding without any modification\.




