# Bitmap fonts

The **TextPaint** application supports the bitmap fonts in windows mode\. Such font is saved as bitmap file, which is provided as **WinFontName** parameter in **Config\.txt** file\. Using bitmap fonts instead of system font or console mode, has following advantages:


* Use any custom characters in unicode range\.
* Such font is truely fixed\-width\.
* Achieve old\-school look, for example like DOS or some 8\-bit computers with screen\.
* Modifying glyphs are easy, just edit bitmap in favorite image editor\.
* Avoid breaking and overlaying semigraphical glyphs\.

Using bitmap fonts has also disadvantages:


* The cell width and height must be integer multiply of glyph width and height\.
* When you use large font file \(with many pages\), opening application may take a while\.

# Bitmap font format

The bitmap font is organized in pages, single page consists of 256 characters\. Every page has number from 0 to 255, but unicode defines 17 planes from 0 to 16, so in fact, the page number are from 0 to 4351\. Bitmap can use any color mode, but the color will be thresholded into two colors, black and white, so every pixel will be readed as black or white, depending on pixel brightness\.

The bitmap width determines the font glyph and must match the formula `(W * 256) + 16)`, where **W** is the glyph width\. For example, for width 8 pixels, the bitmap must have 2064 pixels width\. The glyph height is determined by bitmap height\. The bitmap height is simply glyph height multiplied by number of pages defined in the bitmap\. The number of defined pages may vary depending on glyphs, which will be defined\.

Every row consists of binary encoded page number and glyph images of this page\. The page number code occupies 16 pixels from left and is a binary code, when black means 0 and white means 1\. The code height must be the same as glyph height\. On the glyphs part, the black \(dark\) color means 0 \(background\) and the white \(bright\) color means 1 \(foreground\)\.

The page order in the bitmap is not important\.

# Duospace fonts

The duospace font is the fixed\-width font, which has some characters in double width\. Some bitmap fonts, like Unifont and Unscii has double\-width characters\.

If duospace mode is enabled, double character in **WorkMode=0**, **WorkMode=1** and **WorkMode=2** generates actually two characters, where the first character is the desired character and second character is the additional space for the first character\.

When you use the bitmap font, you can use additional bitmap font, where characters has the same height and has double width\.

The duospace has the following settings:


* **DuospaceMode** \- Duospace usage as following:
  * **0** \- Do not use duospace\. All characters will be treated as single character\. The font specified as **DuospaceFontName** parameter will not be used\.
  * **1** \- Use duospace mode\. The font specified as **DuospaceFontName** parameter will be used for double characters\.
  * **0** \- Do not use duospace, but cell width will be double\. The font specified as **DuospaceFontName** parameter will be loaded and all characters will be used as single\.
* **DuospaceFontName** \- The bitmap font file, which will be used for double characters\. The height must be the same as height of font specified as **WinFontName** parameter and width must be double as the width of the **WinFontName** font\. This mode in character writing works the same as **DuospaceMode=0**\.
* **DuospaceDoubleChars** \- The double character list for force duospace characters in the one of two cases:
  * **WinUse=0** \- Use some characters as duospace\.
  * **WinFontName** is not a bitmap file \- The listed characters will be act as duospace characters\.
  * Blank or undefined characters in bitmap font specified as **DoublespaceFontName** \- the characters will not be displayed, but these will act as double character\.

TextPaint can not determine, if desired character is double\-width, but some console/terminal applications, displays certain characters by occupying two cells\. TextPaint can supports duospace font in console by relying on the double character list only\. TextPaint or operating system can not check, if certain character is really single or double\. The duospace character will affect the working logic, not the character display\.

# Double character list

The double character list has the list of hexadecimal numbers and ranges of these numbers\. The range is presented as two numbers separated by two dots\. Fore example, the entry `F200..F2FF` means, that every charactr between **F200** and **F2FF** is double character\. You can use the character list also with the **DuospaceFontName** together, but the duospace font format will have the higher priority\.

# Supplied bitmap fonts

The **TextPaint** supplies example bitmap fonts in **Fonts** subfolder, which can be used\. In the bitmaps, there was applied the following color convention:


* **White on black** \- Original glyphs existing in source\.
* **Yellow on blue** \- Painted or corrected glyphs\.
* **Green on purple** \- Original glyphs existing in source, but the glyph placement was changed to fix errors\.

The example fonts are generated based on three sources, disinguished by font file name prefix:


* **Dos** \- [https://int10h\.org/oldschool\-pc\-fonts/fontlist/](https://int10h.org/oldschool-pc-fonts/fontlist/ "https://int10h.org/oldschool-pc-fonts/fontlist/") \- Standard EGA and VGA fonts, which looks like a standard DOS font\. There are added some additional glyphs:
  * **Page 23** \- Some math/technical characters for partial **DEC Technical** support**\.**
  * **Page 25** \- Border heavy variant, quadrant boxes, four triangle glyphs from **25E2h** to **25E5h**, some geometrical glyphs\. The glyphs **2580h**, **2581h** and **2584h** are slightly corrected in some variants\.
  * **Page 28** \- Braille dots\.
  * **Page 2E** \- Reverser question mark\.
  * **Page 1FB** \- Semigraphical shapes from **1FB00h** to **1FBAFh**, including **1FB93h**\.
* **Unscii** \(duospace\) \- [http://viznut\.fi/unscii/](http://viznut.fi/unscii/ "http://viznut.fi/unscii/") \- All fonts from Unscii project, without any modyfication and additional glyphs but correction of **2509h** glyph in all 8x8 versions\. This fonts contains non\-standard characters, some glyph placement \(page **25h** and **1FBh**\) are manually corrected, because some glyph positions was swapped related to unicode standard\.
* **Unifont** \(duospace\) \- [https://unifoundry\.com/unifont/](https://unifoundry.com/unifont/ "https://unifoundry.com/unifont/") \- Two versions of Unifont with added **1FB93h** glyph\. This font provides the most unicode coverage\.
* **Small** \- [https://opengameart\.org/content/4x4\-px\-bitmap\-font](https://opengameart.org/content/4x4-px-bitmap-font "https://opengameart.org/content/4x4-px-bitmap-font") \- Small font, which is manually extended with many semigraphical glyphs and can be used to view large ASCII/ANSI arts\. The readibility of letters and digits is poor due to small character size\.
* **Amiga** \- [https://github\.com/rewtnull/amigafonts](https://github.com/rewtnull/amigafonts "https://github.com/rewtnull/amigafonts") \- Fonts suitable for some ASCII\-art or ANSI\-art files, which was created on Amiga computers\.
* **VT** \- [https://www\.masswerk\.at/nowgobang/2019/dec\-crt\-typography](https://www.masswerk.at/nowgobang/2019/dec-crt-typography "https://www.masswerk.at/nowgobang/2019/dec-crt-typography") \- The font, which is used in real DEC VT220 terminal, extended with some semigraphical glyphs\.
* **Xterm** \- Fonts captured from the XTERM application without modification\.

If the font is the duospace font, there are additional file with the **duospace** word in the name\. For every duospace font, there also supplied the double character list\.

# TextPaint tools

Textpaint has some additional tools, which can be usable for generating fonts and test purposes\. The tools can be invoked in **WorkMode=4** and use special name as the filename for render\. The name starts with **?TOOL** PREFIX and ends with **?** suffix\. Each tool has specified other parameters\.

## Create files of 8\-bit encodings

The **?TOOL\_ENCODING?** tool creates the bunch of text files, which are equivalend encodings as the every one\-byte encoding \(the single byte into another single byte\)\. The non\-one\-byte encodings such as Unicode or Chinese will not be processed\. The tool has the following parameter:


* **EncodingDir** \- The directory for encoding files\.

For encode into DotNetEncodingFolder, you have execute the command:

```
TextPaint ?ENCODING? WorkMode=4 EncodingDir=DotNetEncodingFolder
```

Every file will have codepage numer as name with **\.txt** extension\. inside the file, there will be following parameters:


* **Codepage** \- Codepage number\.
* **Name** \- The name of encoding, if exists\.
* **AlternativeName** \- Other name of encoding if this encoding has two different names\.

Below the parameters, there are hexadecimal byte values as further parameters with assigned characters\. Such file can be used as custom encoding without any modification\.

# Create HEX file from the terminal font display

There are several tools, which allow to create own fonts based on display in other terminal applications\. The terminal software must meet these conditions:


* The screen size in characters is minimum 80x24\.
* The font display is binary\. Attemping to capture antialiased or vector font will cause distorions\.
* If the software uses double\-width characters, these characters must be also binary display\. The colorful emoji characters can not be captured\.

You also need any software, which records screen or window into uncompressed or losslessly compressed movie or image serie\. If this software records into movie file, you will have to convert the movie into bitmap image serie\.

The font capture can be done within the several steps\. The maximum possible cell width is 16 pixels\.

## Step 1: Create display file

Use the tool **?TOOL\_FONTDISP?** for create character display animation using the several parameters:


* **AnsiFile** \- The animation ANSI file, in which the animation will be stored\.
* **PageFirst** \- The first page as hexadecimal number\. Single page is the 256 characters\.
* **PageLast** \- The Last page as hexadecimal number\. Single page is the 256 characters\.
* **Interval** \- The time units between capture mark state change\. The whole page duration is the three times\.
* **Break** \- The time units before and after animation display\.
* **DelayType** \- The delay type for use the Interval and Break time:
  * **0** \- Use time marker, the TextPaint native command\.
  * **1** \- Use repeated **1Bh 1 ; 1 ; H** string, supported in all terminals, provided the data processing is throttled\.
* **Char1** \- The character hexadecimal number for high marker state\. The default is **20**\.
* **Char0** \- The character hexadecimal number for lov marker state\. The default is **2588**\.
* **ColorChar1** \- Color for capture character foreground color\.
* **ColorChar0** \- Color for capture character background color\.
* **ColorBack1** \- Color for background character foreground color\.
* **ColorBack0** \- Color for background character background color\.

For capture whole plane 2, white on black characters and red on blue background and save the animation into CharDisplay\.ans, you can run the command:

```
TextPaint ?TOOL_FONTDISP? WorkMode=4 PageFirst=200 PageLast=2FF Interval=3000 Break=5000 Char0=20 Char1=2588 ColorChar1=7 ColorChar0=0 ColorBack1=1 ColorBack0=4 AnsiFile=CharDisplay.ans
```

## Step 2: Prepare file display

Run the terminal/console application and run the TextPaint for prepare animation play inside the console as following example:

```
TextPaint.exe CharDisplay.ans WorkMode=1 WinUse=0 ANSIRead=1 FileDelayTime=40 FileDelayStep=500
```

Then press the enter and test the animation display\. You can change animation speed by pressing the **\[** or **\]** key\. At the top of screen, you have to see the page number in binary format and marker, which appears and disappears\.

After testing displat, rewind into beginning, by pressing the **Home** key\.

## Step 3: Record the display

Run the your screen recording software and configure it for capture the whole terminal window and record framerate must be al least as high as for record several frames for every animation state \(without marker and with marker\)\.

Then, start the recording and run the animation\. If animation finishes, stop the screen recording\.

If the recording software creates movie file, convert it into bitmap image serie, optimal is the PNG format\. The file names are not important\.

## Step 4: Filter images

The image serie consists several images per page and can contains some area outside the terminal screen\. You have to filter the images using the **?TOOL\_FONTFILTER?** tool with the following parameters:


* **FrameX** \- The horizontal coordinate of the terminal screen in pixels from the left edge\.
* **FrameY** \- The vertical coordinate of the terminal screen in pixels from the top edge\.
* **CellW** \- The cell width, where CellW\*80 equals the terminal screen\.
* **CellH** \- The cell height, where CellW\*24 equals the terminal screen\.
* **CellX** \- The X coordinate inside cell\. Usually, it should be equal to **CellW/2**\.
* **CellY** \- The Y coordinate inside cell\. Usually, it should be equal to **CellY/2**\.
* **RawDirectory** \- The directory containing unfiltered images created in previous step\.
* **FilteredDirectory** \- The directory, where will be saved the filteres image\.
* **FrameFirst** \- The first frame number of file in file list\. The file name will not affect and the first recorded frame is 0\.
* **FrameFirst** \- The last frame number of file in file list\. The file name will not affect\. If the number is greater or equals the number of files, the number will be automatically decreased\.
* **FrameTest** \- The parameter with boolean value:
  * **1** \- Create only first and last frame for test the parameters\.
  * **0** \- Create filtered image serie\.

The example of image filtering for test for font 8x16:

```
TextPaint ?TOOL_FONTFILTER? WorkMode=4 FrameX=10 FrameY=10 CellW=8 CellH=16 CellX=4 CellY=8 FrameFirst=100 FrameLast=20000 RawDirectory=ImgCap FilteredDirectory=ImgFilter FrameTest=1
```

In this case, there will be created the three images\. You have to chech meeting the following conditions:


* The **Test1\.png** should show the screen during break directly before animation\.
* The **Test3\.png** should show the screen during break directly after animation\.
* The **Test2\.png** should exactly the terminal whole screen displaying some page\. At the top, there drawn points on the markers, and the points should match the scree markers \(page number and display state\)\.

If the display is correct, delete the test files and run the same command with **FrameTest=0**\. There should be generate exactly the same number of bitmap files as number of displayed pages\. The files with **difference** word in name should not be generated\.\. There are possible errors:


* Some pages are skipped\.
* There are files with **difference** word in name\.

In such case, repeat the step 3 after one of these changes:


* Decrease animation display speed\.
* Increase the record frame rate speed\.

If these changes not helps, repeat the step 2 with increasing the **Interval** parameter and repeat the step 3\.

## Step 5: Create HEX file

After image filtering, you have to parse these images using the **?TOOL\_FONTPARSE?** tool with the following parameters:


* **FilteredDirectory** \- The same as **FilteredDirectory** in previous step\.
* **CellW** \- The same as **CellW** in previous step\.
* **CellH** \- The same as **CellH** in previous step\.
* **BlankChars** \- The list of hexadecimal numbers of characters, which shoud be treated as blank characters\. These character glyphs will be saved into hex as skip characters for converting Hex into bitmap\.
* **HexFile** \- The fine, where will be sotred the hex file\.

The command example, where 20, 1000, 1100 and 3000 character glyphs will be treated as blank:

```
TextPaint.exe ?TOOL_FONTPARSE? WorkMode=4 CellW=8 CellH=16 FilteredDirectory=ImgFilter HexFile=Font.hex BlankChars=20,1000,1100,3000
```

In these file, there are additionally saved the glyphs of four characters, which should not be exists in font bitmap\. Frequently, not\-exisiting glyphs are presented as question mark or rectangle\.

# Create bitmap from HEX files

The font source can consists of several HEX files, so the **?TOOL\_FONTHEX?** uses the file list instead of single hex file\. This tool uses the following parameters:


* **CellW** \- Cell width for single character\.
* **CellH** \- Cell height\.
* **InputFileList** \- The file list of hex files\.
* **FontSingle** \- The image file for single width characters\.
* **FontDouble** \- The image file for double width characters\.
* **DoubleList** \- The double character list described Double character list chapter\.
* **OutputHex** \- The single HEX file, which will contain only this characters, which are currently included in image files\.
* **DrawPageNumber** \- Binary option as following:
  * **0** \- Do not draw hexadecimal page number, the font is usable with TextPaint\.
  * **1** \- Draw hexadecimal page number at the right edge for view and test\. The font is not usable with TextPaint\.
* **AllCharsInDouble** \- Binary option as following:
  * **0** \- The FontDouble image will contain the double characters only\.
  * **1** \- The FontDouble image will contain all characters, the single characters will be stretched\.

## Font list file

The font list file is the text file, where one line indicates the single hex file\. There are several special commands:


* **\#** \- Line leaded with **\#** will be ignored\. You can use it for comments\.
* **\{** \- Begin block comment consisting of several lines\.
* **\}** \- End block comment consisting of several lines\.
* **\*** \- The directory path or file name prefix\. The text will be concatenated with every file name after the line\.
* **:** \- The background and foreground of character glyphs drawn after the lines\. The command consists of 6 values, indicating two RGB colors, for example:
  * **:0:0:0:255:255:255** \- White character on black background \(default colors\)\.
  * **:0:0:255:255:255:0** \- Yellow character on blue background\.
* **@First** \- After the command, if several files contains the same character, the first character will be used \(default mode\)\.
* **@Last** \- After the command, if several files contains the same character, the last character will be used\.

## Hex file

Every hex file, should contain the lines consisting the character number as hexadecimal followed by **:** character and byte serie as hexadecimal\. The blank character are without the character number and can be placed somewhere within the file\.

The blank character means, that every char with the same shape will be ignored and will not be drawn in the bitmap file\.

The lines without **:** character are ignored and can be treated as comments\.

## Command example

The following command reads the FileLinst\.txt and created two bitmap files and double character list:

```
TextPaint ?TOOL_FONTHEX? WorkMode=4 CellW=8 CellH=16 InputFileList=FileList.txt FontSingle=Font.png FontDouble=Font_double.png DoubleList=Font_double.txt DrawPageNumber=0 AllCharsInDouble=0
```

If the Hex file does not contain the single width character, the **Font\.png** file will not be created\. If the Hex file does not contain the double width character, the **Font\_double\.png** file will not be created and the **Font\_double\.txt** will be blank\. If you want to not create one of these file, do not provide the file name\.

# Supplied HEX sources

In the **Hex** subdirectory, there are supplied Unscii, Unifont and Xterm HEX files with appropriate file lists and additional hex files, which corrects these some characters\.

## Unscii

The original unmodified HEX files of version 2\.1 are downloaded from the address: [http://viznut\.fi/unscii/](http://viznut.fi/unscii/ "http://viznut.fi/unscii/")\. There are the additional **Unscii\_correct\_08\.hex** and **Unscii\_correct\_16\.hex** files, which corrects the incorrect characters in the original hex files\.

## Unifont

The original unmodified HEX files of version 15\.0\.06 are downloaded from the address: [https://unifoundry\.com/pub/unifont/unifont\-15\.0\.06/](https://unifoundry.com/pub/unifont/unifont-15.0.06/ "https://unifoundry.com/pub/unifont/unifont-15.0.06/")\. There are the additional **Unifont\_correct\.hex** file, which supplies one lacking 1FB93h character\. The double character set has the standard and Japan version, both version are usable with the same single character set\.

## Xterm

There are several Xterm fonts captured from Xterm terminal using TextPaint\. The blank characters are removed, no other modifications or corrections was done\.




