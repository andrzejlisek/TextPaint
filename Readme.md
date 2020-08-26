# Overwiev

TextPaint is not designed to use as plain text editor, there is designed to create or modify text as a simple graphic or scheme using ANSI or Unicode characters\. You can create texts such as organizational charts, simple circuit schemes, algorithm flow, tables to use it where destination medium supports text printed using fixed\-width font\.

This application works in system console, but it can work as graphical window, which simulates the console\. You can resize the console or window, but application will be repainted after pressing any key\. Application will not be repainted after resizing while the character table is shown, but the application will be repainted after closing the character table and pressing any key\.

# Application running

Input file name as command line parameter\. If you not give parameter, the Config\.txt file will be opened to allow editing the configuration file\.

```
TextPaint.exe FileNameWithPath.txt
```

# Functionality and states

TextPaint can work in one of the 5 states\. The default state is state 0\. In all states, you can use these keys:


* **Arrows** \- Move cursor horizontally or vertically\.
* **PageUp**, **PageDown**, **Home**, **End** \- Move cursor diagonally\.
* **Tab** \- Change cursor visualization type, one of four states\.
* **Esc** \- Switch to state 0 \(edit text\) or change writing direction\.
* **F1** \- Switch to state 1 \(draw frame\)\.
* **F2** \- Switch to state 2 \(draw character\)\.
* **F3** \- Switch to state 3 \(draw rectangle\)\.
* **F4** \- Switch to state 4 \(draw diamond\) or change diamond variant\.
* **F7** \- Save file\.
* **F8** \- Reload file\.
* **F9** \- Open character selection to draw\.
* **F12** \- Quit TextPaint\.

The other available keys depends on the current working state\. The unnecessary spaces and lines are automatically trimmed during editing\. The text is presented as white characters on the black background, the space beyond end of line is dark grey, the space beyond the last line is bright grey\. You can write on the grey aread, then necessary lines or spaces will be added automatically\.

## Choosing the draw character

In the states 2, 3 and 4, you can draw using selected character\. The current drawing character and character code is shown on the status bar after **Draw** word\.

In every state, you can change the draw character\. To do this, press the **F11** key to display the character table\. At the top of character table there will be display the character hexadecimal dose and character glyph\. The Conde consists of 4 digits, the first two digits are the page number, the last two digits are the character number on the specified page\. You can use the following keys while the character table is shown:


* **Arrows** \- Select character on the current page\. You can go beyond the current page range to switch the page\.
* **Page up**, **Page down** \- Switch one page up or down\.
* **Home**, **End** \- Switch 16 pages up or down\.
* **Insert**, **Delete** \(both keys has the same function\):
  * Inside the FAV page: Jump to the same character in the table\.
  * Outside the FAV page: If the character exists on the FAV page, jump to the first ossurence of the character on the FAV page\.
* **Enter** \- Close the character table and change the draw character to the selected character\.
* **Esc** \- Close the character table without changing the draw character\.

Between the FF and 00 page, there is the FAV page, which contains the characters defined in the configuration file\.

## State 0 \- Edit text

In the state 0, you can input alphanumeric characters like in every ordinary text editor, but the functional keys has another meaning\. You can use the following functional keys:


* **Esc** \- Change writing direction \(change in four\-state cycle\)\.
* **Backspace** \- Move cursor in opposite direction \(this not delete the char\)\.
* **Insert** \- Insert space into text as described in state 3\.
* **Delete** \- Delete one character as described in state 3\.
* **Enter** \- Change Insert/Delete working mode as described in state 3\.

## State 1 \- Draw frame

In the state 1, you can draw frames or line using semigraphical characters by moving the cursor\. To select the frame character set, you have to press 1 key to select rectangle character set or press 2 key to select diamond character set\. The character set names are displayed on the status bar\. After select character set, move cursor do draw line\.

The line corner and junction characters will be automatically placed if appropiate characters in neighbor cells are from the same character set\.

If you to move cursor to other place without drawing, switch to state 0 or 3 or 4, move the cursor, return to the state 1\. The last selected character sets will be remembered\.

## State 2 \- Draw character

This state is very similar to state 1\. The only exception is drawing one character independently on the neighbor characters and the cursor movement direction\. Move the cursor to draw the character\.

If you erase or overlay the frame characters, the neighbor frame characters may be replaced to other frame characters to automatically correct the frames when the frame is drew using the same character set, as you selected in this state by pressing 1 or 2 key\. It is recommended to leave the None/None when you not want to affect the neighbor characters\.

You can pause the drawing by temporally switching to state 0 or 3 or 4\.

## State 3 and 4 \- Draw rectangle or diamond

State 3 and 4 are intended to the following actions:


* Draw the rectangle or diamond on the selected place without painting line manually\.
* Copy and paste text\.
* Manually write frame elements\.
* Move already written text by inserting or deleting cells or lines like on the state 0 \(state 3 only\)\.

Move the cursor and set the cursor size, after this press **1** or **2** or **3** to place the figure\. The cursor show the shape, which will be placed, when the **1** or **2** or **3** will be pressed\. Like in the other states, you can use the **Tab** key you can visualize straight or diagonal line without affecting the figure shape\.

In the 3 and 4 states, there are available the following keys:


* **W**, **S**, **A**, **D** \- Change rectangle size, which will be displayed as cursor\.
* **Q**, **E** \- Change cursor position without shape movement and size changing\.
* **1** \- Change frame character set\.
* **2** \- Draw frame using character set\.
* **3** \- Draw frame using character selected to draw\.
* **4** \- Fill using character selected to draw\.
* **C** \- Copy the text embraced by rectangle\.
* **V** \- Paste the copied text \(the clipboard will be cleared when size or state is changed\)\.
* **T**, **Y**, **U**, **G**, **H**, **J**, **B**, **N**, **M**, **I**, **K** \- Draw frame element at the cursor\. The keys without the last two keys creates the square shape on the PC keyboard\. You can use also the **numpad digit**s, **numpad\+** and **numpad\-** keys instead of the mentioned letter keys\. The last two keys in each set \(**I**, **K**, **numpad\+** and **numpad\-**\) writes the vertical or horizontal frame element\.
* **Space** or **numpad 0** \- Draw space \(clear character\) at the cursor\.
* **Insert** \(state 3 only\) \- Insert spaces into text as rectangle width or height\.
* **Delete** \(state 3 only\) \- Delete text as rectangle width or height\.
* **Enter** \(state 3 only\) \- Change Insert/Delete working mode, one of four modes:
  * **H\-block** \- Insert or delete once rectangle width \- move text horizontally\.
  * **V\-block** \- Insert or delete once rectangle height \- move text vertically\.
  * **H\-line** \- Insert or delete columns as rectangle width \- move text horizontally\.
  * **V\-line** \- Insert or delete rows as rectangle height\- move text vertically\.
* **F4** \(state 4 only\) \- Change shape variant, the clipboard will be cleared\.

# Configuration file

In the TextPaint directory there is the **Config\.txt** file, which allows to configure the application\. If you run TextPaint without parameters, this file will be open\. The configuration is read during the application running\. This file is organized as the **parameter=value** convention\. The blank lines, lines without **=** character and parameters other than listed below are ignored:


* **WinUse** \- Use window instead of console\. In certain systems and console configurations, some of unicode characters are not correctly rendered in the console\. If **WinUse** is set to **1** or **2**, the TextPain will work in the graphical window and all unicode characters should be rendered correctly\. The application usage is the same as in the console\. There are allowed the following values:
  * **0** \- Use console\.
  * **1** \- Use window with standard image rendering control\.
  * **2** \- Use window with non\-standard image rendering control\. Use this setting only if interface picture is not displayed or crashed when you use **WinUse=1**\.
* **FavChar** \- Favorite characters hexadecimal codes separated by commas, maximum 256 items\. You can define the favorite characters, which will be shown on the FAV page on the character table\. The first FAV character is the default draw character\.
* **Frame1\_x** \- Character set for rectangle frame, the **x** is the number of set, starting from 0\. The value consists of 12 items separated by comma\. The first item is the set name, the other items are the hexadecimal character codes, which are the frame elements\.
* **Frame2\_x** \- Character set for diamond frame\. It works like **Frame1\_x**\.

Settings related to console, affects when **WinUse=0** only\.


* **ConInputEncoding** \- Set encoding name or codepage for writing characters\. Use it to solve problems in entering characters, especially diacritic characters using keyboard\. Affects only when you use **WinUse=0**\.
* **ConOutputEncoding** \- Set encoding name or codepage for printing characters\. Use it to solve problems in displaying characters\. Affects only when you use **WinUse=0**\.
* **ConUseMemo** \- Use additional text memory in console \(not affects when **WinUse** is set to **1** or **2**\)\. The application uses the console buffer move from the system API while scrolling the text\. You can set one of the following values:
  * **0** \- Use console buffer moving\. This setting is recommended, while all characters are displayed correctly while text scrolling\.
  * **1** \- Use console buffer moving with rewriting non\-ASCII characters\. Use this setting if some characters are displayed incorrectly during text scrolling although the same characters are displayed correctly during writing/painting using Textpaint functions\. This may slightly slow down the text rendering during scrolling\. 
  * **2** \- Do not use console buffer moving\. Use this setting only if the text rendering crashes during trying to scroll\. This setting causes, that every character on screen is rewritten during text scrolling\.

Settings related to window, affects when **WinUse=1** or **WinUse=2** only:


* **WinCellW** \- Character cell width in window\.
* **WinCellH** \- Character cell height in window\.
* **WinFontName** \- Font name in window\.
* **WinFontSize** \- Font size in window\.

## Console encoding

Console uses some endoding for input and output charaters\. You can input codepage or encoding name\. If value consists of digits only, it will be treated as codepage number, otherwise, it will be threated as encoding name\. If you not set or set incorrect name, the system default encoding will be used\.

The most common encodings:

| Name | Codepage | Description |
| --- | --- | --- |
| us\-ascii | 20127 | 7\-bit ASCII |
| utf\-8 | 65001 | Unicode \- UTF\-8 |
| utf\-16 | 1200 | Unicode \- little endian |
| utf\-16BE | 1201 | Unicode \- big endian |

You can set any encoding supported by \.NET API function `System.Text.Encoding.GetEncoding(Int32)` and `System.Text.Encoding.GetEncoding(String)`, details can be found on the web\. This settings affects only console input/output\. Regardless of this setting, the text files edited using Textpaint must use UTF\-8 encoding\.

## Frame elements

The **Frame1\_x** and **Frame2\_x** parameters defines the frame character set as comma separated list\. The first item is the name\. From the second item to the last item there are 11 hexadecimal character codes, which defines the frame elements by following order \(the numbers 1 and 2 are line elementa, the numbers from 3 to 11 are the corner and junction elements\):

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




