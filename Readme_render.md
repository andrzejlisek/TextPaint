# Image rendering

**TextPaint** in WorkMode=4 can render text/ANSI file into image or movie\. Renderer uses the same appearance properties as display when **WinUse=1** or **WinUse=2**\.

Rendering movie is possible for ANSI files, when used **ANSIRead=1**\. Some existing ANSI files saves simple animation, which can be shown be progressive processing characters slowly\.

In this mode, **TextPaint** behavies as command line application without user interface\. The generated images will be in PNG format\. While you render movie, actually it will be saved PNG image sequence in specified directory\. You will have to use other software to merge image sequence into single movie file\.

# Rendering parameters

The following parameters are used only for rendering\. Like other parameters, the parameters can be in **Config\.txt** file or provided in command line:


* **RenderFile** \- Image file name for rendering single image \(**RenderStep=0**\) or directory for rendering movie \(**RenderStep>0**\)
* **RenderStep** \- Number of processed character between movie frames\. To render single image, use **RenderStep=0**\. Rendering movie has additional requirements:
  * **ANSIRead=1** \- Rendering movie is possible for ANSI files only\.
  * **ANSIWidth>0** \- Rendering movie requires specified screen width\.
  * **ANSIHeight>0** \- Rendering movie requires specified screen height\.
* **RenderOffset** \- Offset in character sequence when rendering movie\. For example, if **RenderStep=10** and **RenderOffset=4**, the frames will be generated after character no: 4th, 14th, 24th and so on\.
* **RenderCursor** \- Draw cursor on rendered image\. Possible value are **0** or **1**\.

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
* **ColorNormal** \- Color of default background and foreground in all work modes\.
* **ANSIRead** \- Use ANSI interpreter instead of plain text text file on file reading\.
* **ANSIWidth** \- Define width of ANSI virtual screen\. If **ANSIWidth=0**, the screen width is unlimited in interpreting ANSI data and rendered image dimensions depends on file contents\.
* **ANSIHeight** \- Define height of ANSI virtual screen\. If **ANSIHeight=0**, the screen height is unlimited in interpreting ANSI data and rendered image dimensions depends on file contents\.

The parameter works only if **ANSIRead=1**:


* **ANSIReadCR** \- Reaction by the CR character while reading ANSI file\.
* **ANSIReadLF** \- Reaction by the LF character while reading ANSI file\.
* **ANSIDOS** \- Use DOS behavior instead of standard VT100/ANSI\-derivative behavior, there are some differences, which affects correctness of ANSI display depending on source\.
* **ANSIIgnoreBlink** \- Ignore blink attribute while interpreting ANSI data\.

Other parameter provided both in command line or in **Config\.txt** file, are ignored in **WorkMode=4**\.

# Examples

There is examples with parameters can be invoked in command line, but the parameters can also be in **Config\.txt** file\.

Create single image of plan text file using black characters on white backgrounf with size 80x25:

```
TextPaint.exe /Path/File.txt RenderFile=/Path/Image.png ANSIRead=0 ANSIWidth=80 ANSIHeight=25 ColorNormal=F0
```

Create single image of ANSI file with unlimited screen size:

```
TextPaint.exe /Path/File.txt RenderFile=/Path/Image.png ANSIRead=1 ANSIWidth=0 ANSIHeight=0
```

Create movie from ANSI animation by creating frame on 5th character, 20th character, 35th character and so on:

```
TextPaint.exe /Path/File.txt RenderFile=/Path/MovieFolder ANSIRead=1 ANSIWidth=80 ANSIHeight=25 RenderStep=15 RenderOffset=5
```




