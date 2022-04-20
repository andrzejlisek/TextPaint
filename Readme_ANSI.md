# ANSI in TextPaint

TextPaint can handle ANSI escape codes, which allows to get colored text using colors from palette of standard 16 items, which was used in DOS operating system and ANSI\-compatible text terminals\. In 1990s the ANSI files was popular format of colorful ASCII\-art, which was called as ANSI\-art\.

Some ANSI files contains simple animation achieved by manipulating cursor position and character writing out of the line\-by\-line order\. Such files was animated where was displayed slowly\. Most BBS and similar services provides about 2400 bits per second\.

TextPaint can handle ANSI files and provides other work modes, which also uses the same ANSI interpreter\.

## Displaying animations

The work mode 2 is usable for displaying simple animations by progressive and slowly processing data from file\. You can also start the server on choosen port, which serves the same contents of file\. This feature can be used for comparing displaying file with other Telnet client\.

## Telnet client

TextPaint can act as Telnet client in work mode 3\. it is usable for test various cases with existing Telnet services\. It is recommended to run Telnet server and Textpaint on the same machine, because data is sent as plain text and the Telnet protocol is obsolete\. Because TextPaint is not intented to be replacement of other terminal emulators, the TextPaint does not support and will not support other protocols such as SSH\. The true telnet data can be achievet using network sniffer\. Because some network sniffers does not work on localhost connection, you can use virtual machine or other computer in the same network to test data exchange with reading real transfered data\.

# Additional settings

The **Config\.txt** contains parameters, which are related with ANSI interpreter or network features\. Like other parameters, every parameter in **Config\.txt** can be overrided with parameter in command line\.

The parameters, which is not described below, are described in **Readme\.md** file\.

## ANSI\-related settings

The following parameters are especially related to creating or interpreting ANSI data and affects in work mode 0, 1 and 2:


* **ANSIRead** \- Use ANSI interpreter instead of plain text text file on file reading in **WorkMode=0**, this parameter affects both reading file in application running or pressing **F8** key:
  * **0** \- Read file as plain text file without ANSI interpreter\.
  * **1** \- Read file as ANSI file using ANSI interpreter\. In file contains animation, there will be read final display\.
* **ANSIWrite** \- Save colors as ANSI escape codes using key **F7** in **WorkMode=0**:
  * **0** \- Write file as plain text, ommiting colors\.
  * **1**\- Write file as ANSI text including color definition\.
* **ANSIWidth** \- Define width of ANSI virtual screen \(not to be confused with **WinW** parameter\)\. If 0, the screen width is unlimited in interpreting ANSI data, but some files uses screen wrapping and can be displayed incorrectly while **ANSIWidth** has another value\.
* **ANSIHeight** \- Define height of ANSI virtual screen \(not to be confused with **WinH** parameter\)\. If 0, the screen height is unlimited in interpreting ANSI data, but some files uses screen scrolling and can be displayed incorrectly while **ANSIHeight** has another value\.
* **ANSICR** \- Reaction by the CR character while reading ANSI file in **WorkMode=0** or **WorkMode=1**\. This parameter can fix incorrectly written text or ANSI files:
  * **0** \- Process as CR character only \(recommended\)\.
  * **1** \- Process as CRLF character sequence \(recommended with **ANSILF=2**\)\.
  * **2** \- Ommic CR character\.
* **ANSILF** \- Reaction by the LF character while reading ANSI file in **WorkMode=0** or **WorkMode=1**\. This parameter can fix incorrectly written text or ANSI files:
  * **0** \- Process as LF character only \(recommended\)\.
  * **1** \- Process as CRLF character sequence \(recommended with **ANSICR=2**\)\.
  * **2** \- Ommic LF character\.
* **ANSIDOS** \- Use DOS behavior instead of standard VT100/ANSI\-derivative behavior, there are some differences, which affects correctness of ANSI display depending on source\.
* **ANSIIgnoreBlink** \- Ignore blink attribute while interpreting ANSI data\. Some ANSI files was created assuming that terminal displays blinking text as steady text on bright background\. Some other ANSI files was created assuming terminal displays text as blinking and this attribute should be ignored while TextPaint does not support other attributes than colors of background and text\.

## File and server settings

The parameters affects only in **WorkMode=1**, which purpose is displaying animation on serving file contents via network:


* **FileDelayChars** \- Number of characters displayed once between two delayes\.
* **FileDelayTime** \- Time in milliseconds between displaying two portions of characters\.
* **ServerPort** \- Number od network port, on which the serwer will wait for connection\. In order to not use connection, set value as **0**\.
* **ServerEncoding** \- Character encoding used to send data via network if **ServerPort** parameter has value other than **0**\.

## Terminal settings

The parameters affects only in **WorkMode=2** while connecting or working as Telnet client:


* **TerminalEncoding** \- Character encoding used to aquire characters from Telnet service\.
* **TerminalName** \- Terminal name sent to server, when server asks for client terminal name\. In some cases service can require certain terminal name or service behavior can vary depending on provided terminal name\.
* **TerminalVTFuncKeys** \- Sequence, which will be sent by pressing function keys:
  * **0** \- ANSI sequence\.
  * **1** \- VT100 sequence\.
* **TerminalVTArrowKeys** \- Sequence, which will be sent by pressing arrow keys:
  * **0** \- ANSI sequence\.
  * **1** \- VT100 sequence\.

# DOS vs\. standard terminal

Older ANSI files uses features of DOS terminal, which works slightly differently than standard VT/ANSI terminal\. The differences cannot be accomodate with each other, because the same data sequence \(text with escape codes\) giver different result\. In the table below, there are all differences:

| Feature | ANSIDOS=0 | ANSIDOS=1 |
| --- | --- | --- |
| Text wrapping\. | After writing character at the last column, cursor remains at the same line\. The cursor jumps into beginning of next line only after writing one more character and this character will be written at the first column\. | After writing character at the last column, cursor immediately jumps into beginning of next line\. |
| Characters from **00h** to **1Fh**, excluding **08h**, **09h**, **0Ah**, **0Bh**, **0Dh**, **1Ah**, **1Bh**\. | Character will be ignored\. | Character will be written using assigned printable character according standard DOS character glyph for control characters\. |
| Character **09h** \- horizontal tab | Move cursor right to the nearest multiply of 6 column\. | Write character **25CBh**\. |
| Character **0Bh** \- vertical tab | Move cursor down one line\. | Write character **2642h**\. |
| Sequence **1Bh D** | Move cursor one line up, scroll if necessary\. | Ignore\. |
| Sequence **1Bh M** | Move cursor one line down, scroll if necessary\. | Enter into music state\. |
| Sequence **1Bh \[ x L** | Scroll **x** lines up\. | Ignore\. |
| Sequence **1Bh \[ x M** | Scroll **x** lines down\. | Enter into music state\. |
| Sequence **1Bh \[ x C** | Move cursor right, if **x** exceedes distance to the last column, leave cursor at the last column\. | Move cursor right, if **x** exceedes distance to the last column, jump to begining of next line and move remaining value from begining of next line\. |

## Music state

Seldom ANSI files used ANSI music, which generates simple sounds through PC speaker\. Sequence **1Bh M** or **1Bh \[ x M** informs the application, that following printable characters will define sound sequence and are not to write on the screen\. The last character of such sequence is **03h** or **0Eh** and demands the application to exit from the music state\. If **03h** or **0Eh** occurs without entering into music state, the character will be printed or ignored according the **ANSIDOS** parameter\.

When **ANSIDOS=0**, the music state is not available\. Generating the sound is not implemented and will not be implemented, because there is very rarely used and is not necessary to view or edit ANSI file\.

## DOS control character assignment

Characters from 00h to 1Fh are control characters and should be treated as non\-printable characters\. DOS has ability to print control character in some cases and it was used in some ANSI files\.

| Number | Printable char |
| --- | --- |
| 00h | 0020h |
| 01h | 263Ah |
| 02h | 263Bh |
| 03h | 2665h |
| 04h | 2666h |
| 05h | 2663h |
| 06h | 2660h |
| 07h | 2022h |
| 08h | 25D8h |
| 09h | 25CBh |
| 0Ah | 25D9h |
| 0Bh | 2642h |
| 0Ch | 2640h |
| 0Dh | 266Ah |
| 0Eh | 266Bh |
| 0Fh | 263Ch |
| 10h | 25BAh |
| 11h | 25C4h |
| 12h | 2195h |
| 13h | 203Ch |
| 14h | 00B6h |
| 15h | 00A7h |
| 16h | 25ACh |
| 17h | 21A8h |
| 18h | 2191h |
| 19h | 2193h |
| 1Ah | 2192h |
| 1Bh | 2190h |
| 1Ch | 221Fh |
| 1Dh | 2194h |
| 1Eh | 25B2h |
| 1Fh | 25BCh |

# File display and server

The work mode 1 allows to display any file using sequentially displaying character to view simple ANSI animations or graphics like while downoading in real time\. After running this **TextPaint** with **WorkMode=1** parameter, you will see information about functions available during displaying\. At this state, you can change current file by writing full path of another file\. The file name with path will be written also, if you drop file icon into window\.

To start displaying file, press **Enter**, in order to quit **TextPaint**, press **Tab**\.

After pressing **Enter**, if parameter **ServerPort** has value other than **0**, applcation will wait for connection from network client\. As network client you can use other terminal emulator\. If **ServerPort** is set to **0**, the step will be skipped\.

After this, screen will be cleared\. You can use the following keys:


* **Esc** \- Return to information screen\.
* **Tab** \- Show or hide status bar
* **Enter** \- Start or stop automatic display according the **FileDelayChars** and **FileDelayTime** parameters\.
* **Space** \- Process 1 character \(not to be confused with 1 byte, especially when handling UTF\-8 or UTF\-16 file\)\.
* **Backspace** \- Process as many characters as set as **FileDelayChars** parameter\.

In order to quit application, ples the **Esc** key to return to information and press **Tab** key to quit\.

## Status bar

During display, you can show or hide the status bar by pressing **Tab** key, which contains following informations:


* Number of processed characters from file\.
* Number of all characters of file\.
* List of last processed character number filling whole screen line, character number above 128 does not equal to byte values and depends on **FileReadEncoding** parameter\.\.

Every press **Tab** key forces screen to repaint, so if screens displays glitches, press the **Tab** key to repaint\.

# Telnet client

TextPaint provides simple Telnet client, whis can be used to test ANSI interpreter compatibility with VT100/ANSI terminal\. Telnet client is available in **WorkMode=2** and you have to provide addres in place of file name in command line\. For example, you can provide **localhost:23** if you have working local telnet server\.

In most cases, this is recommended to set **ANSIWidth** to **80** and **ANSIHeight** to **24**\.

After running **TextPaint**, choose and press key, which will be used as escape key\. The escape key means the key, which provides access to functions instead of sending keystroke to server\. The escape key can be changed during the session\. After pressing the key, the session will begin\.

During the session, if you press the selected escape key, the information window will popup and you will can use special keys:


* **Esc** \- Return to terminal \(close information\)
* **Enter** \- Change escape key
* **Tab** \- Move information window
* **Backspace** \- Quit from application
* **Number 1 to 9** \- Send **F1** to **F9** keys, useful, when application has problem with capturing function key\.
* **Number 0** \- Send **F10** key\.
* **\[** \- Send **F11** key\.
* **\]** \- Send **F12** key\.
* **Letter** \- Send letter with Ctrl modifier, like Ctrl\+X
* / \- Connect od disconnect, depending on current state
* **\\** \- Send screen size to server, some servers requires such information\.

## Telnet client limitations

**TextPaint** has some limitations compared to ther Telnet/SSH software:


* Screen size change on server demand \(like switching between 80 and 132 columns mode\) is not possible\.
* If server demand some information via escape sequence, the client will not answer \(this feature is necessary very rarely\)\.
* Limited compatibility with VT or XTERM terminal series\.
* SSH protocol is not supported\.

# Escape sequences

TextPaint has implemented limited set of escape sequences used to ANSI data processing and interpreting\. The sequences are following and enough for most files and many telnet services\.

The non\-standard sequences:

| Escape sequence | Meaning |
| --- | --- |
| 1Bh \# 0 | Ignore\. |
| 1Bh \# 1 | Ignore\. |
| 1Bh \# 2 | Ignore\. |
| 1Bh \# 3 | Ignore\. |
| 1Bh \# 4 | Ignore\. |
| 1Bh \# 5 | Ignore\. |
| 1Bh \# 6 | Ignore\. |
| 1Bh \# 7 | Ignore\. |
| 1Bh \# 8 | Fill screen with **E** character\. |
| 1Bh \# 9 | Ignore\. |
| 1Bh \] \.\.\. 07h | Ignore\. |
| 1Bh 7 | Save current cursor position and text attributes\. |
| 1Bh 8 | Restore saved cursor position and text attributes\. |
| 1Bh D \- **ANSIDOS=0** | Move cursor down by 1 step, scroll screen if needed\. |
| 1Bh D \- **ANSIDOS=1** | Ignore\. |
| 1Bh M \- **ANSIDOS=0** | Move cursor up by 1 step, scroll screen if needed\. |
| 1Bh M \- **ANSIDOS=1** | Enter to music state, ignore following characters\. |
| 1Bh E | Move cursor down by 1 step, scroll screen if needed and move cursor to first column\. |
| 03h \- **ANSIDOS=1** | Exit from music state if in music state, otherwise, write character\. |
| 08h | Move cursor one step left\. |
| 09h \- **ANSIDOS=0** | Move cursor right to nearest multiply of 6\. |
| 0Ah | Move cursor one line down, scroll screen if needed\. |
| 0Bh \- **ANSIDOS=0** | Move cursor one line down, scroll screen if needed\. |
| 0Dh | Move cursor to first column\. |
| 14h \- **ANSIDOS=1** | Exit from music state if in music state, otherwise, write character\. |
| 1Ah | Break data processing, ignore in **WorkMode=2**\. |

When **ANSIDOS=1**, every character between 01h to 1Fh will be written as standard character in exception of characters: 08h, 0Ah, 0Dh, 1Ah and 1Bh\.

When **ANSIDOS=0**, every character between 01h to 1Fh will be ignored in exception of characters: 08h, 09h, 0Ah, 0Bh, 0Dh, 1Ah and 1Bh\.

Every standard sequence begins with 1Bh followed by \[ character and ends with one of the characters:


* Digit\.
* Lowercase letter\.
* Uppercase letter\.
* **>** character\.

The standard sequences with **?** charater and without parameters:

| Escape sequence | Meaning |
| --- | --- |
| 1Bh \[ ? 3 h | Clear screen and move cursor to top left corner of cursor area\. |
| 1Bh \[ ? 3 l | Clear screen and move cursor to top left corner of cursor area\. |
| 1Bh \[ ? 6 h | Move cursor to top left corner of cursor area\. |
| 1Bh \[ ? 6 l | Reset cursor area and move cursor to top left corner of cursor area\. |
| 1Bh \[ ? 7 h | Turn off screen wrapping |
| 1Bh \[ ? 7 l | Turn on screen wrapping |

The standard sequences without **?** character and may contains parameters\. When parameter is ommited, the default value is **1**:

| Escape sequence | Meaning |
| --- | --- |
| 1Bh \[ s | Save current cursor position and text attributes\. |
| 1Bh \[ u | Restore saved cursor position and text attributes\. |
| 1Bh \[ H | Move cursor to top left corner\. |
| 1Bh \[ J &#124; 1Bh \[ 0 J | Clear screen from cursor to bottom right corner\. |
| 1Bh \[ 1 J | Clear screen from top left corner to cursor\. |
| 1Bh \[ 2 J | Clear screen and move cursor to top left corner of screen\. |
| 1Bh \[ K &#124; 1Bh \[ 0 K | Clear current line from cursor to right edge\. |
| 1Bh \[ 1 K | Clear current line from left edge to cursor\. |
| 1Bh \[ 2 K | Clear current line from left edge to right edge\. |
| 1Bh \[ P1 ; P2 H &#124; 1Bh \[ P1 ; P2 f | Move cursor to column P1 and line P2, move cursor to be within cursor area\. |
| 1Bh \[ P1 A | Move cursor up through P1 steps\. |
| 1Bh \[ P1 B | Move cursor down through P1 steps\. |
| 1Bh \[ P1 C | Move cursor right through P1 steps\. |
| 1Bh \[ P1 D | Move cursor left through P1 steps\. |
| 1Bh \[ P1 d | Move cursor to P1 line\. |
| 1Bh \[ P1 e | Move cursor down P1 through P1 steps\. |
| 1Bh \[ P1 E | Move cursor to the first column and P1 lines down\. |
| 1Bh \[ P1 F | Move cursor to the first column and P1 lines up\. |
| 1Bh \[ P1 G | Move cursor to P1 column\. |
| 1Bh \[ P1 S | Scroll cursor area P1 times down\. |
| 1Bh \[ P1 T | Scroll cursor area P1 times up\. |
| 1Bh \[ P1 ; P2 r | Define cursor area as from P1 line to P2 line, move cursor to be within cursor area\. |
| 1Bh \[ P1 L \- **ANSIDOS=0** | Scroll cursor area P1 times up\. |
| 1Bh \[ P1 L \- **ANSIDOS=1** | Ignore\. |
| 1Bh \[ P1 M \- **ANSIDOS=0** | Scroll cursor area P1 times down\. |
| 1Bh \[ P1 M \- **ANSIDOS=1** | Enter to music state, ignore following characters\. |
| 1Bh \[ \.\.\. m | Set attributes, number of parameters can vary\. |
| 1Bh \[ P1 P | Delete character and move text being right to be cursor P1 times\. |
| 1Bh \[ P1 X | Delete P1 characters right to be cursor without text movement\. |

Every standard sequence, which is not in the table will be ignored\.

## Text attributes

During processing ANSI data, there will be used internal text attributes with following default values:


* Foreground = \-1 \(means color not defined\)
* Background = \-1 \(means color not defined\)
* Bold = false
* Underline = false
* Inverse = false
* Blink1 = false
* Blink2 = false

The attributes can be changed with the **1Bh \[ \.\.\. m** escape sequence\. The number of parameters can vary and has following meaning:

| Parameter | Meaning |
| --- | --- |
| 0 | Set all parameters to default values |
| 39 | Foreground = \-1 |
| 30 | Foreground = 0 |
| 31 | Foreground = 1 |
| 32 | Foreground = 2 |
| 33 | Foreground = 3 |
| 34 | Foreground = 4 |
| 35 | Foreground = 5 |
| 36 | Foreground = 6 |
| 37 | Foreground = 7 |
| 90 | Foreground = 8 |
| 91 | Foreground = 9 |
| 92 | Foreground = A |
| 93 | Foreground = B |
| 94 | Foreground = C |
| 95 | Foreground = D |
| 96 | Foreground = E |
| 97 | Foreground = F |
| 49 | Background = \-1 |
| 40 | Background = 0 |
| 41 | Background = 1 |
| 42 | Background = 2 |
| 43 | Background = 3 |
| 44 | Background = 4 |
| 45 | Background = 5 |
| 46 | Background = 6 |
| 47 | Background = 7 |
| 100 | Background = 8 |
| 101 | Background = 9 |
| 102 | Background = A |
| 103 | Background = B |
| 104 | Background = C |
| 105 | Background = D |
| 106 | Background = E |
| 107 | Background = F |
| 1 | Bold = true |
| 22 | Bold = false |
| 4 | Underline = true |
| 24 | Underline = false |
| 5 | Blink1 = true |
| 25 | Blink1 = false |
| 6 | Blink2 = true |
| 26 | Blink2 = false |
| 7 | Inverse = true |
| 27 | Inverse = false |

**TextPaint** does not support other attributes than color, so every attribute set will be converted to color when text is diaplayed \(**WorkMode=1** or **WorkMode=2**\) or loaded into editor \(**WorkMode=0**\)\. If background or foreground is not defined \(is set as **\-1**\), the colors from **ColorNormal** parameter will be used\.




