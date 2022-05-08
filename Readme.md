# Overview

**TextPaint** is not designed to use as plain text editor, there is designed to create or modify text as a simple graphic or scheme using ANSI or Unicode characters\. You can create texts such as organizational charts, simple circuit schemes, algorithm flow, tables, to use it where destination medium supports text printed using fixed\-width font\.

The text can be both monochrome saved as plain text or color saved as text with ANSI escape commands\. You can view or edit many existing ASCII\-art or ANSI\-art files\.

This application works in system console, but it can also work as graphical window, which simulates the console\. You can resize the console or window, but application will be repainted after pressing any key\. Application will not be repainted after resizing while the character selector is shown, but the application will be repainted after closing the character table and pressing any key\.

**TextPaint** also provides simple cipher, which user Vigenere algorithm\. You can encipher some text fragments using password\.

## Other functions

**TextPaint** has other function than text editor, called as work modes\. Other work modes uses the same ANSI interpreter and was implemented to test various cases\.

There are available following work modes, choosen by **WorkMode** parameter:


* **WorkMode=0** \- Text editor, the main purpose of **TestPaint**, which is described in **Readme\.md** file\.
* **WorkMode=1** \- ANSI viewer with progressive printing and server\. This mode can be used to view animation created by progressive placing characters and is described in **Readme\_ANSI\.md** file\.
* **WorkMode=2** \- Telnet client, which is parially compatible with VT100, ANSI and derivative terminal emulators\. Details about this mode are described in **Readme\_ANSI\.md** file\.
* **WorkMode=3** \- Encoding list display and keycode test\. The only purpose is displaying supported encoding list and code/name of every pressed key, which is needed to implement additional functions and test the **TextPaint** in various platforms\. This mode is described in **Readme\_keys\.md** file\.
* **WorkMode=4** \- Render text into bitmap image detailed in **Readme\_render\.md** file\. In this mode, **TextPaint** does not run any interface, it behavies as simple command line application\.

# Application running

Input file name as command line parameter\. If you not give parameter or give blank file name, the **Config\.txt** file will be opened to allow editing the configuration file\.

```
TextPaint.exe FileNameWithPath.txt
```

If you use **Mono** project \(required in Linux\), you can run application with this command:

```
mono TextPaint.exe FileNameWithPath.txt
```

You can also override configuration options, which can be used in **Config\.txt** file\. From second parameter, application accepts configuration parameters profided with **=** character\. For example, to force running in 40x15 window, run this command:

```
TextPaint.exe FileNameWithPath.txt WinUse=2 WinW=40 WinH=15
```

In work mode 3, in place of file name, you have to provide server address with port\. if the port equals to 23, it can be ommited\.

```
TextPaint.exe 192.168.1.100:23 WorkMode=3
```

All options are described in **Configuration file** chapter\.

## Application files

**TextPaint** is supplied with the following files and directories:


* **Samples** \- Sample text files\.
* **Fonts** \- Bitmap fonts\.
* **Config\.txt** \- Main configuration file\.
* **Fav\.txt** \- Favorite character definitions\.
* **Info\.txt** \- Context information\.
* **TextPaint\.exe** \- Application binary executable\.

# Functionality and interface

TextPaint can work in one of the 4 states\. The default state is **state 1**\. In all states, you can use these keys:


* **Arrows** \- Move cursor horizontally or vertically\.
* **Home, End, PageUp**, **PageDown** \- Move cursor diagonally\.
* **Tab** \- Change cursor visualization type, one of four modes\. This function may help to find, where is the text cursor in **state 3** or **state 4**\.
* **F1** \- Switch to **state 1** \(edit text\) or show information about **state 1**\.
* **F2** \- Switch to **state 2** \(edit characters\) or show information about **state 2**\.
* **F3** \- Switch to **state 3** \(character paint\) or show information about **state 3**\.
* **F4** \- Switch to **state 4** \(pixel paint\) or show information about **state 4**\.
* **F7** \- Save file\. If the cipher is enabled, you will have to input cipher password twice\. If password is not the same, you will have to input password one more time\.
* **F8** \- Reload file\. If the cipher is enabled, you will have to input cipher password used to save the file\.
* **F9** \- Open character selection to draw\.
* **F12** \- Quit TextPaint\.
* **Z** \- Undo \(not works in **state 1** and **state 2**\)\.
* **X** \- Redo \(not works in **state 1** and **state 2**\)\.

To undo or redo editing text or characters \(**state 1** or **state 2**\), you can undo/redy by following procedure:


1. Switch temporarily to **state 2** or **state 3**\.
2. Press **Z** or **X** key to undo or redo\.
3. Switch back to **state 0** or **state 1**\.

The other available keys depends on the current working state\. The unnecessary spaces and lines are automatically trimmed during editing\. You can write on the grey aread, then necessary lines or spaces will be added automatically\.

The colors are following:


* **White text on black background** \- Actual text\.
* **Dark gray** \- Space beyond end of line\.
* **Bright gray** \- Space below the last line\.
* **Black text in white background** \- Status bar\.

## Context information

When you press the **F1**, **F2**, **F3**, **F4** key one more time, after switchint to the state, there will be displayed short information about selected state\. Use arrow keys to scroll the text\.

To close information, press **Esc** or any **F** key \(**F1**\-**F12**\)\. The informations are read from the **Info\.txt** file\.

## Status bar

At the bottom of console window, there is the status bar, with black text on white background\. The status bar in **state 1**, **state 2** and **state 3** has the following informations from left to right:


1. Cursor position \(row and column\)\. The character between numbers indicated current text area:
   * **Colon** \- Cursor inside text line\.
   * **Semicolon** \- Cursor beyond end of text line, but not below the last line\.
   * **Comma** \- Cursor below the last line of text\.
2. Character number and glyph under the cursor\.
3. Number of background and foreground color under the cursor, the hyphen indicates, that color is not defined \(used default\)\.
4. State indicator:
   * **Text** \- Write text \(**state 1**\), followed by direction indicator:
     * **R** \- **Right** \- From left to right\.
     * **RD** \- **Right/Down** \- From up\-left to down\-right\.
     * **D** \- **Down** \- From up to down\.
     * **DL** \- **Down/Left** \- From up\-right to down\-left\.
     * **L** \- **Left** \- From right to left\.
     * **LU** \- **Left/Up** \- From down\-right to up\-left\.
     * **U** \- **Up** \- From down to up\.
     * **UR** \- **Up/Right** \- From down\-left to up\-right\.
   * **Char** \- Write character \(**state 2**\), followed by direction indicator the same as in the **state 1**\.
   * **Rect** \- Character drawing \(**state 3**\), rectangle shape\.
   * **Dia** \- Character drawing \(**state 3**\), diamond shape\.
5. Elements in **state 3** only:
   1. Shape size\.
   2. Character set used to draw figure in **state 3**:
      * Single character: Character code and glyph\.
      * Character set: Name defined in **Config\.txt** file\.
6. Insert/delete mode:
   1. **H\-block** \- Insert or delete text inside one line, at right from the cursor \- moves text horizontally\.
   2. **V\-block** \- Insert or delete text inside one column, below the cursor \- moves text vertically\.
   3. **H\-line** \- Insert or delete columns \- moves text horizontally\.
   4. **V\-line** \- Insert or delete rows \- moves text vertically\.

The status bar in **state 4** has different layout, which is described in subchapter about **state 4**\.

# Character and color selector

**TextPaint** has two similar selectors\. In order to enter into selector, press **F9** key and switch between selectors by pressing **F3** key\.

## Character selector

In the **state 3** and **state 4**, you can draw using selected character\. The current drawing character and character code is shown on the status bar when the character is used to draw\. In the **state 1**, **state 2** and **state 3**, if you insert space \(in mode **H\-block** and **V\-block** only\), the selected character will be inserted\. You can invoke the character selector in every state by pressing the **F9** key\.

At the top of character table there will be display the character hexadecimal code and character glyph\. The code consists of 4 or 5 digits\. The plane 16 forces code length to be 6 digits, but is used very rarely, so the leading **10** digits for plane 16 is replaced by **G** to get 5\-digital code\. The **G** digit is outside of hexadecimal digit set and means 16\. For example, the **G1234** has the same meaning as **101234**\.

The character selector can work in two states:


* **Ordinary** \- Browse all characters\.
* **Favorite** \- The 256 places of favorite characters, 95 of them can be assigned to alphanumeric keys\.

You can use the following keys while the character table is shown:


* **Arrows** \- Select character on the current page\. You can go beyond the current page range to switch the page\.
* **Page up**, **Page down** \- Flip one page up or down \(go ahead 256 characters\)\.
* **Home**, **End** \- Flip 16 pages up or down \(go ahead 4096 characters\)\.
* **F1**, **F2** \- Switch the plane \(go ahead 65536 characters\)\.
* **F3** \- Switch to **color selector**\.
* **Insert** \- Switch between ordinary and favorite state:
  * **Ordinary**: If the pinted character exists in favorite set, there will be pointed\.
  * **Favorite**: There will be pointed the same character, which is pointed in favorite state\.
* **Delete** \- Go to character being under the cursor\.
* **Backspace**:
  * **Ordinary**: Select character for save as favorite\.
  * **Favorite**: Select character or put character on pointed place\.
* **Tab** \- Move character selector window from corner to corner\. You can use this if the window covers some text or window is bigger than screen\.
* **Esc** \- Close without changing selected character\.
* **Enter** \- Change selected character and close\.
* **Any alphanumeric key**:
  * **Ordinary**: Save pointed character as favorite under pressed key\.
  * **Favorite**: Go to place assigned with pressed key\.

## Color selector

You can turn character selector into color selector or turn color selector into character chelector by pressing **F3**\. In color selector, you can select the color and input text mode\. The text and colors are treated as two independed layers, which can be modifed simultaneously or separately\.

While the color selector is showm, you can use the following keys:


* **Up arrow**, **Down arrow** \- Select background color\.
* **Left arrow**, **Right arrow** \- Select foreground color\.
* **Page up**, **Page down** \- Number of columns for ANSI file load\. Use if file is loaded incorrectly, applicable when **ANSIRead=1**\. Value 0 means unlimited\.
* **Home**, **End** \- Number of rows for ANSI file load\. Use if file is loaded incorrectly, applicable when **ANSIRead=1**\. Value 0 means unlimited\.
* **F1** \- Switch between CR character processing while loading file, when when **ANSIRead=1**:
  * **0** \- Treat as CR character \(default\)\.
  * **1** \- Treat as CR\+LF sequence\.
  * **2** \- Ignore CR character\.
* **F2** \- Switch between LF character processing while loading file, when when **ANSIRead=1**:
  * **0** \- Treat as LF character \(default\)\.
  * **1** \- Treat as CR\+LF sequence\.
  * **2** \- Ignore LF character\.
* **F3** \- Switch to **character selector**\.
* **Insert** \- Switch between edit mode within three states:
  * **Text \+ color** \- Edit text and color simultaneously\.
  * **Text** \- Edit text only without color changing\.
  * **Color** \- Edit color only without text changing\.
* **Delete** \- Go to color being under the cursor\.
* **Tab** \- Move color selector window from corner to corner\. You can use this if the window covers some text or window is bigger than screen\.
* **Esc** \- Close without changing selected color\.
* **Enter** \- Change selected color and close\.

You can change text without changing colors or colors without changing the text\. In order to do, switch to appropriate mode using **Insert** key\. The text and colors are independed layes\.

## ANSI file loading

Many ANSI files are prepared for specified number of columns, in most cases for 80 columns and unlimited number of rows\. If you set other number of columns or use unlimited number of columns \(**ANSIWidth=0**\), the file may be loaded incorrectly\. To correct this, enter into color selector, change number of columns or rows using **Page up**, **Page down**, **Home** or **End** keys and press **Enter** key\. Then, you have to reload file using **F8** keys\.

Sometimes, the number of rows can also be specified for particular file and the file can be loaded incorrectly, when number of rows are unlimited\. The most popular numbers of rows in such case are **24** and **25**\.

If there file loaded with **ANSIRead=1** is actually plain text file, the file can contain different end\-of\-line character\. If the file is loaded incorrectly, you can change parsing of CR \(**0Dh**\) and LF \(**0Ah**\) character in color selector\. You can press **F1** or **F2** during color selector to change way of the character processing, the current state is diaplayed as two digits on the top of character selector\. Example settings:


* 00 \- Standard process of CR and LF characters, suitable for all ANSI files and text files using CR\+LF like files created in DOS and Windows\.
* 21 \- Use LF character as CR\+LF \(EOL\), suitable for plain text files created on Unix and Linux, which uses LF as EOL\.
* 12 \- Use CR character as CR\+LF \(EOL\), suitable for plain text files created on systems, which uses CR as EOL\.
* 11 \- Use every CR character and every LF character as EOL separately\.

# Work states

**TextPaint** in work mode 0 works in one of 4 states, which you can swich by pressing **F1**, **F2**, **F3**, or **F4** key\. In order to display help text about keys available in the current state, press the same function key again\. For example, in order to get help about state 2, press **F2** while applicationworks in state 2\.

## State 1 \- Write text

In the **state 1**, you can input alphanumeric characters like in every standard text editor, but there are few differences\.

You can use the following functional keys:


* **Esc** \- Change writing direction\.
* **Backspace** \- Move cursor in opposite direction \(this not delete the char\)\.
* **Insert** \- Insert line or character\.
* **Delete** \- Delete line or character\.
* **Enter** \- Change Insert/Delete mode\.

### Undo and redo

You can undo or redo writing, but the direct function not exists\. To undo or redo, you have to switch to **state 3** or **state 4** by pressing **F3** or **F4**\. Then, you can undo or redo by pressing **Z** or **X**\. After undo or redo, you can return to **state 1** by pressing **F1**\.

### Insert and delete

The **Enter**, **Insert** and **Delete** keys has different function than standard text editor\.

By pressing **Enter**, you can choose between four insert/delete modes:


* **H\-block** \- Insert or delete text inside one line, at right from the cursor \- moves text horizontally\.
* **V\-block** \- Insert or delete text inside one column, below the cursor \- moves text vertically\.
* **H\-line** \- Insert or delete columns \- moves text horizontally\.
* **V\-line** \- Insert or delete rows \- moves text vertically\.

## State 2 \- Write characters

**State 2** works exactly by the same way as **state 1** with one difference: If you press alphanumeric key, there will be written favorite characters assignet to the keys instead of ordinary characters\. You can view and change the assigment in character selector\.

The state is most usable when you frequently write specified characters, because you can write the character just by pressing assigned key\.

## State 3 \- Character drawing

**State 3** is intended to the following actions:


* Draw line using frame characters\.
* Undo and redo\.
* Draw the rectangle or diamond on the selected place without painting line manually\.
* Copy and paste text\.
* Manually write frame elements\.
* Insert or delete larger portion of spaces\.

In the **state 3**, you have the following functions available:


* **W**,**S**,**A**,**D** \- Change figure size\.
* **Q**,**E** \- Change cursor position within the rectangle or diamond\.
  * **Q** \- Move counter clockwise\.
  * **E** \- Move clockwise\.
* **1** \- Change figure shape one of 9\.
* **2** \- Change character set\.
* **3** \- Draw hollow frame\.
* **4** \- Draw filled frame\.
* **5** \- Start or stop drawing line by moving cursor\.
* **C** \- Copy the text being inside the figure\.
* **V** \- Paste the text into the area inside the figure\.
* **Insert** \- Insert line or character \(works on rectangle shape only\)\.
* **Delete** \- Delete line or character \(works on rectangle shape only\)\.
* **Enter** \- Change Insert/Delete mode\.
* **I** or **Numpad Plus** \- Write vertical line\.
* **K** or **Numpad Minus** \- Write horizontal line\.
* **Space** or **Numpad 0** \- Write selected character\.

### Writing frame elements

Apart from keys listed above, you can write frame elements \(corners, tees and crosses\) using some letter keys and numpad digits as in the following scheme:

```
  T---Y---U         T         7---8---9         7
  |   |   |        / \        |   |   |        / \
  |   |   |       G   Y       |   |   |       4   8
  |   |   |      / \ / \      |   |   |      / \ / \
  G---H---J     B   H   U     4---5---6     1   5   9
  |   |   |      \ / \ /      |   |   |      \ / \ /
  |   |   |       N   J       |   |   |       2   6
  |   |   |        \ /        |   |   |        \ /
  B---N---M         M         1---2---3         3
```

For example, the **T** key or **Numpad 7** writes the upper left corner as shown above\. The diamond shape is treated as rectangle rotated clockwise\.

### Insert and delete

The insert and delete function works by the same way as in **state 1** and **state 2**, but there is few differences:


* This function works only, when rectangle shape is selected
* In **H\-block** and **H\-line** modes, the figure width determines number of inserted or deleted columns once\.
* In **V\-block** and **V\-line** modes, the figure height determines number of inserted or deleted columns once\.
* In **H\-block** mode, there can be modified few rows according to figure height\.
* In **V\-block** mode, there can be modified few columns according to figure width\.

## State 4 \- Pixel drawing

The **state 4** is purposed to draw pictures using semigraphical characters splitted into pixels\. Some characters offers pixels by splitting for example to 2x2 pixels per character\. You can paint by character or by pixels as defined in configuration file\. In the state, there are two cursors:


* **Main cursor** \- The cursor, which is moved by **Arrow** keys and **Home**/**End**/**PageUp**/**PageDown** keys\. Pressing these keys causes movement of both cursors\.
* **Size cursor** \- The second cursor, which is connected with main cursor\. This cursor determines figure size for drawing lines, rectangles, ellipses, and copying/pasting picture\.

During **state 4**, there are following functions by keys:


* **W**,**S**,**A**,**D** \- Move size cursor\.
* **I**,**K**,**J**,**L** \- Move picture between cursors\.
* **1** \- Change paint configuration:
  * The first configuration uses one pixel per character and allows to select character for foreground and background in character selector\.
  * The other configurations are defined in **Config\.txt** file and may use several pixels per character\. Every pixel configuration inside character is mapped to appropriate character\.
* **2** \- Change default color\. This option defines, how will be treated characters other than characters used in current paint configuration\.
* **3** \- Draw one pixel \(in hollow mode\) or flood fill \(in fill mode\)\.
* **4** \- Draw line between cursors\.
* **5** \- Draw rectangle between cursors\.
* **6** \- Draw ellipse between cursors\.
* **P** \- Turn pencil on/off\.
* **M** \- Change picture move mode\. The mode is used while moving using **I**,**K**,**J**,**L** keys:
  * **Repeat** \- The edge of moved picture will be repeated\.
  * **Roll** \- The edge will be fillled with edge from the other side of moved picture\.
  * **Background** \- The edge will be filled with background color\.
  * **Foreground** \- The edge will be filled with foreground color\.
  * **Flip/Rotate** \- Flip or rotate instead of movement\. Use keys as following:
    * **I** \- Flip vertically\.
    * **K** \- Flip horizontally\.
    * **J** \- Rotate counter clockwise \- works only when width equals to height\.
    * **L** \- Rotate clockwise \- works only when width equals to height\.
* **N** \- Invert colors between cursors\.
* **C** \- Copy picture between cursors as text to clipboard\. This function uses characters selected as foreground and background color in first paint configuration\.
* **V** \- Paste picture between cursors from flipboard\. The clipboard bust contain text using characters selected as foreground and background color in first paint configuration\.
* **Q**,**E** \- Move cursors around rectangle corners determined by this cursors:
  * **Q** \- Move counter clockwise\.
  * **E** \- Move clockwise\.
* **R** \- Change paint color wrom there options:
  * **Background** \- Use background color\.
  * **Foreground** \- Use foreground color\.
  * **Negative** \- Changes to opposite color while drawing\.
* **F** \- Change paint fill option for drawing pixels, rectangles and ellipses:
  * **Hollow** \- Draw pixels and hollow figured\.
  * **Fill** \- Draw floodfill and filled figures\.

### Status bar

In **state 4**, the status bar has different layout, described from left to right:


1. Main cursor position and size in pixels \(not in characters\)\. The size sign \(plus or minus\) indicates direction from main cursor to size cursor\.
2. The **state 4** indicator with default color and pencil state:
   * **Pxl\-B** \- The default color is background, pencil is off\.
   * **Pxl\-F** \- The default color is foreground, pencil is off\.
   * **Pxl\-B\*** \- The default color is background, pencil is on\.
   * **Pxl\-F**\* \- The default color is foreground, pencil is on\.
3. The paint configuration:
   * The first configuration \(one pixel per character\) is indicated by codes of used characters, separated by slash\.
   * The other configurations are indicated by configuration name defined in **Config\.txt**\.
4. Drawing color and fill mode:
   1. **Fore\-H** \- Foreground color, hollow\.
   2. **Back\-H** \- Background color, hollow\.
   3. **Nega\-H** \- Negative color, hollow\.
   4. **Fore\-F** \- Foreground color, fill\.
   5. **Back\-F** \- Background color, fill\.
   6. **Nega\-F** \- Negative color, fill\.
5. Picture move mode:
   1. **Repeat** \- The edge of moved picture will be repeated\.
   2. **Roll** \- The edge will be fillled with edge from the other side of moved picture\.
   3. **Back** \- The edge will be filled with background color\.
   4. **Fore** \- The edge will be filled with foreground color\.
   5. **FlipRot** \- Flip or rotate instead of movement\.

### Cursor display

Console can display cursor over the character, not over the pixel\. If you select configuration with pixels smaller than character, you will move pixel by pixel, when you press cursor keys\. The visible cursor will by over this character, which contains pointed pixel\. To point a specified pixel, you have to observe the position on status bar or count movement from last visible cursor movement\.

To check position, you can draw pixel or other figure \(key **3**, **4**, **5**, **6**\) and undo the drawing by pressing **Z**\.

# Configuration file

In the TextPaint directory there is the **Config\.txt** file, which allows to configure the application\. If you run TextPaint without parameters, this file will be open\. The configuration is read in the application run moment\. This file is organized as the **parameter=value** convention\. The blank lines, lines without **=** character and parameters other than listed below are ignored:


* **WinUse** \- Use window instead of console\. In certain systems and console configurations, some of unicode characters are not correctly rendered in the console\. If **WinUse** is set to **1** or **2**, the TextPain will work in the graphical window and all unicode characters should be rendered correctly\. The application usage is the same as in the console\. There are allowed the following values:
  * **0** \- Use console\.
  * **1** \- Use window with standard image rendering control\.
  * **2** \- Use window with non\-standard image rendering control\. Use this setting only if interface picture is not displayed or crashed when you use **WinUse=1**\.
* **WorkMode** \- Work mode of TextPaint:
  * **0** \- Text editor \- the fundamental mode and purpose of **TextPaint**\.
  * **1** \- ANSI viewer and file server \- described in **Readme\_ANSI\.md** file\.
  * **2** \- Telnet client \- described in **Readme\_ANSI\.md** file\.
  * **3** \- Encoding list and keycode test \- described in **Readme\_keys\.md** file\.
  * **4** \- Render text file or ANSI file into image or movie \- described in **Readme\_render\.md** file\.
* **Space** \- Characters \(hexadecimal numbers\), which will be treated as space\. The first character in the list is used as character being outside actual text\.
* **FileReadEncoding** \- Encoding used in file reading, in most cases, it should be **FileReadEncoding=utf\-8**\. You can display all supported encodings using **WorkMode=3**\.
* **FileWriteEncoding** \- Encoding used in file writing, in most cases, it should be **FileWriteEncoding=utf\-8**\. You can display all supported encodings using **WorkMode=3**\.
* **Frame1\_x** \- Character set for rectangle frame, the **x** is the number of set, starting from 0\. The value consists of 12 items separated by comma\.
  * The first item is the set name\.
  * The other items are the hexadecimal character codes, which are the frame elements\.
* **Frame2\_x** \- Character set for diamond frame\. It works like **Frame1\_x**\.
* **Pixel\_x** \- Name and pixels per character definition number **x** for **state 4**\. The first must be **Pixel\_1**\. The parameters has three values separated by comma:
  * Configuration name\.
  * Horizontal pixels per character\.
  * Vertical pixels per character\.
* **Pixel\_x\_y** \- Character list fragment for fumber **x**\. For example, the first character list for definition number 2 is defined by parameter **Pixel\_2\_1**\. The number of coder per one fragment and number of fragments are not important\. Only the total number of character codes are important and must be equal to 2 powered to number of pixels per character\. For example:
  * 2 x 2 pixels per character \- 16 characters\.
  * 2 x 3 pixels per character \- 64 characters\.

## Appearance settings

The settings affect the appearance of interface:


* **CursorDisplay** \- Display cursor character at cursor position\. In most cases, the value should be **CursorDisplay=1**\. Sometimes, the cursor can be invisible while moving due to inverting background and text colors in some console implementations\. Use **CursorDisplay=0** to not draw cursor character and may help in cursor visibility\.
* **ColorNormal** \- Color of default background and foreground in all work modes\.
* **ColorBeyondLine** \- Color of background indicating area beyond end of line, but not below the last line\.
* **ColorBeyondEnd** \- Color of background indicatin area below the last line\.
* **ColorCursor** \- Color of text cursor with guide lines in **WorkMode=0**\.
* **ColorStatus** \- Color of status bar in **WorkMode=0** and **WorkMode=1**\.
* **ColorPopup** \- Color of character selector and color selector in **WorkMode=0**, color of information window in **WorkMode=2**\.
* **BeyondLineColumn** \- Number of columns, which will be displayed using **ColorNormal** instead of **ColorBeyondLine** regardless being beyond of the text line\. This setting may improve readibility of some ANSI files\. If you use negative value, there will be used the same value as **ANSIWidth**\.

## Console settings

Settings related to console, affects when **WinUse=0** only\.


* **ConInputEncoding** \- Set encoding name or codepage for writing characters\. Use it to solve problems in entering characters, especially diacritic characters using keyboard\. Affects only when you use **WinUse=0**\.
* **ConOutputEncoding** \- Set encoding name or codepage for printing characters\. Use it to solve problems in displaying characters\. Affects only when you use **WinUse=0**\.
* **ConUseMemo** \- Use additional text memory in console \(not affects when **WinUse=1** or **WinUse=2**\)\. The application uses the console buffer move from the system API while scrolling the text\. You can set one of the following values:
  * **0** \- Use console buffer moving\. This setting is recommended, while all characters are displayed correctly while text scrolling\.
  * **1** \- Use console buffer moving with rewriting non\-ASCII characters\. Use this setting if some characters are displayed incorrectly during text scrolling although the same characters are displayed correctly during writing/painting using Textpaint functions\. This may slightly slow down the text rendering during scrolling\. 
  * **2** \- Do not use console buffer moving\. Use this setting only if the text rendering crashes during trying to scroll\. This setting causes, that every character on screen is rewritten during text scrolling\.

## Window settings

Settings related to window, affects when **WinUse=1** or **WinUse=2** only:


* **WinCellW** \- Character cell width in window\. If you use bitmap font, it will be rounded to integer multiply of font glyph width\.
* **WinCellH** \- Character cell height in window\. If you use bitmap font, it will be rounded to integer multiply of font glyph height\.
* **WinFontName** \- Font name or bitmap file in window, the name can be interpreted by one of three ways from highest priority:
  * **File name** \- Use bitmap font file\.
  * **Generic font name** \- Use one of three generic font names, the actual font depends on operating system:
    * **GenericSerif**
    * **GenericSansSerif**
    * **GenericMonospace**
  * **Font name** \- Use selected font, like **Arial**, **Courier**\.
* **WinFontSize** \- Font size in window\. This setting not affects the cell size\.
* **WinW** \- Number of columns\. To display whole status bar, it should be at least **WinW=40**\.
* **WinH** \- Number of rows\.
* **WinColorBlending** \- Enables color blending for some block characters\. This parameter affects to this characters:
  * **2591h**, **2592h**, **2593h**\.
  * From **1FB8Ch** to **1FB94h**\.
  * From **1FB9Ch** to **1FB9Fh**\.
* **WinPaletteR** \- The red component of all 16 colors\.
* **WinPaletteG** \- The green component of all 16 colors\.
* **WinPaletteB** \- The blue component of all 16 colors\.

The **WinColorBlending** uses other character and mixed foreground color with background color as in table below:

| Original character | Replacement character | Remarks |
| --- | --- | --- |
| 2591h | 2588h | Changed foreground color, mixed 1:3 |
| 2592h | 2588h | Changed foreground color |
| 2593h | 2588h | Changed foreground color, mixed 3:1 |
| 1FB8Ch | 258Ch | Changed foreground color |
| 1FB8Dh | 2590h | Changed foreground color |
| 1FB8Eh | 2580h | Changed foreground color |
| 1FB8Fh | 2584h | Changed foreground color |
| 1FB9Ch | 25E4h | Changed foreground color |
| 1FB9Dh | 25E5h | Changed foreground color |
| 1FB9Eh | 25E2h | Changed foreground color |
| 1FB9Fh | 25E3h | Changed foreground color |
| 1FB90h | 20h | Changed background color |
| 1FB91h | 2580h | Changed background color |
| 1FB92h | 2584h | Changed background color |
| 1FB93h | 258Ch | Changed background color |
| 1FB94h | 2590h | Changed background color |

The **WinPaletteR**, **WinPaletteG** and **WinPaletteB** defines the palette, which will be used to display 16 colors in window\. You can define arbitraty colors\. Each of the parameters must consist of 32 hexadecimal digits\. Each 2 digits defines value for one channer of single color\. Below, there are some examples\.

DOS 16\-color palette with dark yellow \(default palette\):

```
WinPaletteR=00AA00AA00AA00AA55FF55FF55FF55FF
WinPaletteG=0000AAAA0000AAAA5555FFFF5555FFFF
WinPaletteB=00000000AAAAAAAA55555555FFFFFFFF
```

DOS 16\-color palette with brown instead of dark yellow:

```
WinPaletteR=00AA00AA00AA00AA55FF55FF55FF55FF
WinPaletteG=0000AA550000AAAA5555FFFF5555FFFF
WinPaletteB=00000000AAAAAAAA55555555FFFFFFFF
```

Windows VGA palette:

```
WinPaletteR=00800080008000C080FF00FF00FF00FF
WinPaletteG=00008080000080C08000FFFF0000FFFF
WinPaletteB=00000000808080C080000000FFFFFFFF
```

## Cipher settings

Settings related to cipher are:


* **CipherMode** \- Chipher mode:
  * **0** \- No cipher\.
  * **1** \- Cipher with repeated key\.
  * **2** \- Cipher with autokey on plain text\.
  * **3** \- Cipher with autokey on ciphered text\.
* **CipherBegin** \- Character codes, which indicated begining of ciphered text\. It must be at least code to enable cipher\.
* **CipherEnd** \- Character codes, which indicated ending of ciphered text\. It must be at least code to enable cipher\.
* **CipherAlphabet** \- Full cipher alphabet provided as text\. It should be consists of standard characters\. Only characters existing in the alphabet can be ciphered and used i password\. The character order is important\.
* **CipherPassword** \- Password used instead of manually input at every save or every load file\.

## Other settings

The parameters described above are not the all available parameters\. The other parameters are related to ANSI interpreter and work modes other than 0\. These parameters are described in **Readme\_ANSI\.md** file\.

## Character encodings

Console and files uses some encoding for input and output charaters\. You can input codepage or encoding name\. If value consists of digits only, it will be treated as codepage number, otherwise, it will be threated as encoding name\. If you not set or set incorrect name, the system default encoding will be used\. The **ConInputEncoding** and **ConOutputEncoding** are not the same as **FileReadEncoding** and **FileWriteEncoding**\.

The most common encodings:

| Name | Codepage | Description |
| --- | --- | --- |
| us\-ascii | 20127 | 7\-bit ASCII |
| utf\-8 | 65001 | Unicode \- UTF\-8 |
| utf\-16 | 1200 | Unicode \- little endian |
| utf\-16BE | 1201 | Unicode \- big endian |

You can set any encoding supported by \.NET API function `System.Text.Encoding.GetEncoding(Int32)` and `System.Text.Encoding.GetEncoding(String)`, details can be found on the web, list of all supported encoding can be achieved on **WorkMode=3**\. This settings affects only console input/output\.

## Encoding from file

You can create custom 8\-byte encoding, which encodes every supported character as single byte\. Many 8\-bit encodings, including 437, 1252 codepages, were popular before Unicode was widespreaded\. Such encoding assigns one character to every number from 0 to 255 \(byte value\), the same character may be assigned to several byte values\.

The file has similar convention to **Config\.txt** file, so there are list of parameters with value for each parameter\. In encoding file, the parameter is the byte value denoted as hexadecimal number and the value is denoted as hexadecimal character number\.

Lines, which does not contain the **=** character and blank lines are ignored, so you can write comments and additional information, which will not be processed\. Other parameters, such as **Name**, will also be ignored\.

To use the file, you have to place file name with path in place of the encoding name/codepage\.

You can use the **WorkMode=3** to test encoding file or display character list of system encodings\. Details are in **Readme\_keys\.md** file\.

You can generate files from all system 8\-bit encodings using **WorkMode=4**, details are in **Readme\_render\.md** file\.

## Frame elements

The **Frame1\_x** and **Frame2\_x** parameters defines the frame character set as comma separated list\. The first item is the name\. From the second item to the last item there are 11 hexadecimal character codes, which defines the frame elements by following order \(the numbers 1 and 2 are line elements, the numbers from 3 to 11 are the corner and junction elements\):

```
    -     1     \
    |     2     /

3---4---5       3
|   |   |      / \
|   |   |     6   4
|   |   |    / \ / \
6---7---8   9   7   5
|   |   |    \ / \ /
|   |   |    10   8
|   |   |      \ /
9--10--11      11
```

As you can see, the diamond is treated as rectangle rorate by 45 degrees clockwise\. Every **Frame1\_x** and **Frame2\_x** must consist of 12 item including character set name\. Application reads the correctly defined numbers to the first incorrectly defined or missing number\.

# Text cipher

TextPaint can cipher some fragments of text using Vigenere's cipher using repeated key or autokey\. If enabled, some fragments of text file can be ciphered\. Cipher setting defines characters, which begins or ends the ciphered fragments\. The remaining contents will not be ciphered, so it makes to difficult to attack the ciphered text, because the knowledge about text information and text layout is useless to guess the used password\. To achieve most security, you should meeet the conditions:


* Text to be ciphered is a random character sequence, for example randomly\-generated passwords\.
* Cipher key is random character sequence\.
* Every character in text to be cipher and cipher key exists in cipher alphabet\.
* Cipher key is longer than whole text to be ciphered\.
* One key is used to cipher one file\.

You can enable or disable cipher in **Config\.txt** setting **CipherMode**, using one of following values:


* **0** \- No cipher\.
* **1** \- Cipher with repeated key\.
* **2** \- Cipher with autokey on plain text\.
* **3** \- Cipher with autokey on ciphered text\.

If value is **0**, the cipher will be disabled regardless other settings related with cipher\. To enable cipher, you can also set to non\-blank values the following settings:


* **CipherBegin**
* **CipherEnd**
* **CipherAlphabet**

The **CipherBegin** and **CipherEnd** defines the characters, which can begin or end the text\. For example, assume, that **CipherBegin=005B,007B** and **CipherEnd=005D,007D**\. In this case, if whole text is `abcde[fghij]klmno{pqrst}uvwxy`, only the `fghij` and `pqrst` will be ciphered, the remaining fragments will be plain\.

The **CipherAlphabet** defines whole set of the characters, which must exists in text to be ciphered\. The Vigenere cipher alhorithm bases on alphabet and assigns number for every character in the alphabet\. For example, the **CipherAlphabet** value can be a sequence of small or capital letters ordered alphabetically\. Consider the two cases:


* **CipherAlphabet=abcdefghijklmnopqrstuvwxyz**
* **CipherAlphabet=qwertyuiopasdfghjklzxcvbnm**

The two values are not the same, because different numbers will be assigned for the same characters\. If **CipherAlphabet** will blank, the cipher will be disabled\. The characters defined as **CipherBegin** and **CipherEnd** must not exists in cipher alphabet\.

## Using cipher

If cipher is enabled and meets all required conditions, everytime, if you open or reload file, you will be asked for the password\. The password will be used to decipher text\. If you provide blank password \(just pressing **Enter** without inputting any characters\), the text will be loaded as is, without deciphering\. If you input wrong password, the text will be deciphered incorrectly\.

If you provide wrong password, you can retry by pressing **F8** key \(reload file\) and input password again\.

When you save file by pressing **F7** key, you will be also asket for password and you have to input the password twice\. If the first and second password are not the same, you will be again asked for password\. To save file without ciphering, just press **Enter** key twice without inputting any characters\. At the moment, you can change the password by inputting new password, which differs from the password used to load file\.

If you input password containing characters, which not exist in cipher alphabet, these characters will be ignored\. From this rule, there is one exception, described in **Password from file** subchapter\.

## Password from file

You can use file instead of password to input very long password\. To do this, you have to input password according the rules:


* The first character is the chipher begin character\.
* The last character is the chipher end character\.
* The string between the first and the last character is the filename\.

Examples assuming that **CipherBegin=005B**, **CipherEnd=005D** and **CipherAlphabet=abcdefghijklmnopqrstuvwxyz**:


* **pass123** \- The password is **pass**\.
* **\[pas2s** \- The password is **pass**\.
* **3pass\]** \- The password is **pass**\.
* **\[pass123\.txt\]** \- The password is from file **pass123\.txt** existing in the current directory\.
* **\[C:\\Documents\\pass\.123\.txt\]** \- The password is from file **pass123\.txt** existing in the **C:\\Documents\\** directory\.

The file contents will be read as text file and the text will be used as password\.

## Several password for one file

You can cipher some text fragment using the first password and the another text fragment within the same file using the second password, which differs from the first password\. To get such effects, you have to do following steps:


1. Open file\.
2. Input all fragments of text to be ciphered\.
3. Save file using the first password\.
4. Reload file using the second password\.
5. Reinput the information to be ciphered using the second password\.
6. Save the file using the second password\.

At the time, if you load file using the first password, the first information will be shown as plain text and the second information will be unreadable\. If you open the file using the second password, the first information will be unreadable and the second information will be shown as plain text\.

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

## Bitmap font format

The bitmap font is organized in pages, single page consists of 256 characters\. Every page has number from 0 to 255, but unicode defines 17 planes from 0 to 16, so in fact, the page number are from 0 to 4351\. Bitmap can use any color mode, but the color will be thresholded into two colors, black and white, so every pixel will be readed as black or white, depending on pixel brightness\.

The bitmap width determines the font glyph and must match the formula `(W * 256) + 16)`, where **W** is the glyph width\. For example, for width 8 pixels, the bitmap must have 2064 pixels width\. The glyph height is determined by bitmap height\. The bitmap height is simply glyph height multiplied by number of pages defined in the bitmap\. The number of defined pages may vary depending on glyphs, which will be defined\.

Every row consists of binary encoded page number and glyph images of this page\. The page number code occupies 16 pixels from left and is a binary code, when black means 0 and white means 1\. The code height must be the same as glyph height\. On the glyphs part, the black \(dark\) color means 0 \(background\) and the white \(bright\) color means 1 \(foreground\)\.

The page order int he bitmap is not important\.

## Supplied examples

The **TextPaint** supplies example bitmap fonts, which can be used\. In the bitmaps, there was applied the following color convention:


* **White on black** \- Original glyphs existing in source\.
* **Yellow on blue** \- Painted or corrected glyphs\.
* **Green on purple** \- Original glyphs existing in source, but the glyph placement was changed to fix errors\.

Some fonts has glyph size 16x16, but these fonts has also 08x16 version\. Glyphs existing in 08x16 version are the same as in the 16x16 version\. In the 16x16 version there are more glyphs, which are not exist in 08x16 version\.

The example fonts are generated based on three sources, disinguished by font file name prefix:


* **Dos** \- [https://int10h\.org/oldschool\-pc\-fonts/fontlist/](https://int10h.org/oldschool-pc-fonts/fontlist/ "https://int10h.org/oldschool-pc-fonts/fontlist/") \- Standard EGA and VGA fonts, which looks like a standard DOS font\. There are added some additional glyphs:
  * **Page 25** \- Border heavy variant, quadrant boxes, four triangle glyphs from **25E2h** to **25E5h**\. The glyphs **2580h**, **2581h** and **2584h** are slightly corrected in some variants\.
  * **Page 28** \- Braille dots\.
  * **Page 1FB** \- Semigraphical shapes from **1FB00h** to **1FBAFh**, including **1FB93h**\.
* **Unscii** \- [http://viznut\.fi/unscii/](http://viznut.fi/unscii/ "http://viznut.fi/unscii/") \- All fonts from Unscii project, without any modyfication and additional glyphs but correction of **2509h** glyph in all 8x8 versions\. This fonts contains non\-standard characters, some glyph placement \(page **25h** and **1FBh**\) are manually corrected, because some glyph positions was swapped related to unicode standard\.
* **Unifont** \- [https://unifoundry\.com/unifont/](https://unifoundry.com/unifont/ "https://unifoundry.com/unifont/") \- Two versions of Unifont with manually added **1FB93h** glyph\. This font provides the most unicode coverage\. In both fonts, the 08x16 glyphs are exactly the same, so there exists single 08x16 Unifont variant\.
* **Small** \- [https://opengameart\.org/content/4x4\-px\-bitmap\-font](https://opengameart.org/content/4x4-px-bitmap-font "https://opengameart.org/content/4x4-px-bitmap-font") \- Small font, which is manually extended with many semigraphical glyphs and can be used to view large ASCII/ANSI arts\. The readibility of letters and digits is poor due to small character size\.
* **Amiga** \- [https://github\.com/rewtnull/amigafonts](https://github.com/rewtnull/amigafonts "https://github.com/rewtnull/amigafonts") \- Fonts suitable for some ASCII\-art or ANSI\-art files, which was created on Amiga computers\.




