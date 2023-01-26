# ANSI in TextPaint

TextPaint can handle ANSI escape codes, which allows to get colored text using colors from palette of standard 16 items, which was used in DOS operating system and ANSI\-compatible text terminals\. In 1990s the ANSI files was popular format of colorful ASCII\-art, which was called as ANSI\-art\.

Some ANSI files contains simple animation achieved by manipulating cursor position and character writing out of the line\-by\-line order\. Such files was animated where was displayed slowly\. Most BBS and similar services provides about 2400 bits per second\.

TextPaint can handle ANSI files and provides other work modes, which also uses the same ANSI interpreter\.

## Displaying animations

The **WorkMode=2** is usable for displaying simple animations by progressive and slowly processing data from file\. You can also start the server on choosen port, which serves the same contents of file\. This feature can be used for comparing displaying file with other Telnet client\.

## Telnet client

TextPaint can act as Telnet client in **WorkMode=3**\. it is usable for test various cases with existing Telnet services\. It is recommended to run Telnet server and TextPaint on the same machine, because data is sent as plain text and the Telnet protocol is obsolete\. Because TextPaint is not intented to be replacement of other terminal emulators, the TextPaint does not support and will not support other protocols such as SSH\. The true telnet data can be achievet using network sniffer\. Because some network sniffers does not work on localhost connection, you can use virtual machine or other computer in the same network to test data exchange with reading real transfered data\.

# Additional settings

The **Config\.txt** contains parameters, which are related with ANSI interpreter or network features\. Like other parameters, every parameter in **Config\.txt** can be overrided with parameter in command line\.

The parameters, which is not described below, are described in **Readme\.md** file\.

## ANSI\-related settings

The following parameters are especially related to creating or interpreting ANSI data and affects in **WorkMode=0**, **WorkMode=1** and **WorkMode=2**:


* **ANSIRead** \- Use ANSI interpreter instead of plain text file on file reading in **WorkMode=0**, this parameter affects both reading file in application running or pressing **F8** key:
  * **0** \- Read file as plain text file without ANSI interpreter\.
  * **1** \- Read file as ANSI file using ANSI interpreter\. In file contains animation, there will be read final display\.
* **ANSIReadCR** \- Reaction by the CR character while reading ANSI file in **WorkMode=0** or **WorkMode=1**\. This parameter can fix incorrectly written text or ANSI files:
  * **0** \- Process as CR character only \(recommended\)\.
  * **1** \- Process as CRLF character sequence \(recommended with **ANSIReadLF=2**\)\.
  * **2** \- Ommic CR character\.
* **ANSIReadLF** \- Reaction by the LF character while reading ANSI file in **WorkMode=0** or **WorkMode=1**\. This parameter can fix incorrectly written text or ANSI files:
  * **0** \- Process as LF character only \(recommended\)\.
  * **1** \- Process as CRLF character sequence \(recommended with **ANSIReadCR=2**\)\.
  * **2** \- Ommic LF character\.
* **ANSIWidth** \- Width of ANSI virtual screen \(not to be confused with **WinW** parameter\)\. If not set, the default is **80**\.
* **ANSIHeight** \- Height of ANSI virtual screen \(not to be confused with **WinH** parameter\)\. If not set, the default is **24** \(when **ANSIDOS=0**\) or **25** \(when **ANSIDOS=1**\)\.
* **ANSIBufferAbove** \- Use extending buffer above the screen, affects in **WorkMode=0** and **WorkMode=4**\.
* **ANSIBufferBelow** \- Use extending buffer below the screen, affects in **WorkMode=0** and **WorkMode=4**\.
* **ANSIDOS** \- Use DOS behavior instead of standard VT100/ANSI\-derivative behavior, there are some differences, which affects correctness of ANSI display depending on source\.
* **ANSIIgnoreBlink** \- Ignore blink attribute while interpreting ANSI data\. Some ANSI files was created assuming that terminal displays blinking text as steady text on bright background\. Some other ANSI files was created assuming terminal displays text as blinking and this attribute should be ignored while TextPaint does not support other attributes than colors of background and text\.
* **ANSIIgnoreBold** \- Ignore bold attribute while interpreting ANSI data\.
* **ANSIIgnoreConcealed** \- Ignore concealed \(hidden, invisible\) attribute while interpreting ANSI data\.
* **ANSIReverseMode** \- Mode for reverse color:
  * **0** \- Before bold and blink \- compatible with DOS, usable in most cases\.
  * **1** \- After bold and blink \- with **ANSIIgnoreBlink=1** is more close to original VT100 terminal\.
* **ANSIPrintBackspace** \- Print backspace character \(while **ANSIDOS=1**\) or ignore backspace character \(while **ANSIDOS=0**\) instead of moving cursor backward\.
* **ANSIWrite** \- Save colors as ANSI escape codes using key **F7** in **WorkMode=0**:
  * **0** \- Write file as plain text, ommiting colors\.
  * **1** \- Write file as ANSI text including color definition\.
* **ANSIWriteBold** \- Use bold attribute instead of color attribute for save high intensity foreground color, work only while **ANSIWrite=1**\.
* **ANSIWriteBlink** \- Use blink attribute instead of color attribute for save high intensity background color, work only while **ANSIWrite=1**\.
* **ANSICharsDOS** \- List of 32 replacement character codes, to use as printable of characters from **00h** to **31h** while **DOSMODE=1**\.
* **ANSICharsVT100** \- List of 32 replacement character codes, to use as VT100 graphics characters from **5Fh** to **7Eh**\.
* **ANSICharsVT52** \- List of 32 replacement character codes, to use as VT52 graphics characters from **5Fh** to **7Eh**\.
* **ANSIScrollSmooth** \- Display smooth scroll when smooth scroll state is set\. This parameter may have following values:
  * **0** \- Do not use smooth scroll\.
  * **1** \- No text movement, delay only\.
  * **2** \- Move text by half of height\.\.
  * **3** \- Move text by quarter of height\.
  * **4** \- Move text by eighth of height\.
* **ANSIScrollChars** \- Number of characters, which smooth scroll durates\. Use 0 to turn off smooth scroll\.
* **ANSIScrollBuffer** \- Buffer arriving characters while scrolling:
  * **0** \- Smooth scrolling pauses character loading, scrolling extens text display time by scrolling time\.
  * **1** \- Smooth scrolling does not pause character loading, the buffered characters will be processed instantly after scrolling end\.

## File and server settings

The parameters affects only in **WorkMode=1**, which purpose is displaying animation on serving file contents via network:


* **FileDelayStep** \- Number of steps processed within single cycle\.
* **FileDelayOffset** \- Number of steps processed within the first cycle\.
* **FileDelayTime** \- Time in milliseconds between displaying two portions of steps\.
* **ServerPort** \- Number od network port, on which the serwer will wait for connection\. In order to not use connection, set **ServerPort=0**\.
* **ServerEncoding** \- Character encoding used to send data via network if **ServerPort>0**\.

## Terminal settings

The parameters affects only in **WorkMode=2** while connecting or working as Telnet client:


* **TerminalEncoding** \- Character encoding used to aquire characters from Telnet service\.
* **TerminalName** \- Terminal name sent to server, when server asks for client terminal name\. In some cases service can require certain terminal name or service behavior can vary depending on provided terminal name\.
* **TerminalFile** \- If not blank, there is the file, to which the terminal display will be dumped with time markers\. This file can be played directly using **WorkMode=1** or rendered using **WorkMode=4**\.
* **TerminalTimeResolution** \- The number of milliseconds between two display cycles\. Decreasing this value induces the more fluenty terminal display, but causes more CPU usage during working\.
* **TerminalStep** \- The maximum number of processing steps within single display cycle\. The **0** value means unlimited number\. This parameter can be used to simulate low receiving transfer speed or for smooth scroll simulation\.
* **TerminalKeys** \- The code of terminal keyboard codes configuration, consists of 4 digits\. Described on **Terminal Client** chapter\.

# DOS vs\. standard terminal

Older ANSI files uses features of DOS terminal, which works slightly differently than standard VTx/ANSI terminal\. The differences cannot be accomodate with each other, because the same data sequence \(text with escape codes\) giver different result\. In the table below, there are all differences:

| Feature | ANSIDOS=0 | ANSIDOS=1 |
| --- | --- | --- |
| Text wrapping\. | After writing character at the last column, cursor remains at the same line\. The cursor jumps into beginning of next line only after writing one more character and this character will be written at the first column\. | After writing character at the last column, cursor immediately jumps into beginning of next line\. |
| Characters from **00h** to **1Fh**, excluding from **08h** to **0Dh**, **1Ah**, **1Bh**\. | Character will be ignored\. | Character will be written using assigned printable character according standard DOS character glyph for control characters\. |
| Character **08h** \- backspace | Move cursor left one column\. | Write character **25D8h**\. |
| Character **09h** \- horizontal tab | Move cursor right to the nearest multiply of 6 column\. | Write character **25CBh**\. |
| Character **0Bh** \- vertical tab | Move cursor down one line\. | Write character **2642h**\. |
| Character **0Ch** \- form feed | Same as **0Ah**\. | Write character **2640h**\. |
| Character **7Fh** | Ignore\. | Write character **7Fh**\. |
| Sequence **1Bh D** | Move cursor one line up, scroll if necessary\. | Ignore\. |
| Sequence **1Bh M** | Move cursor one line down, scroll if necessary\. | Enter into music state\. |
| Sequence **1Bh \[ x L** | Scroll **x** lines up\. | Ignore\. |
| Sequence **1Bh \[ x M** | Scroll **x** lines down\. | Enter into music state\. |
| Sequence **1Bh \[ x C** | Move cursor right, if **x** exceedes distance to the last column, leave cursor at the last column\. | Move cursor right, if **x** exceedes distance to the last column, jump to begining of next line and move remaining value from begining of next line\. |
| Sequence **1Bh \[ 2 J** | Clear screen only\. | Clear screen and move cursor to upper left corner\. |

## Music state

Some ANSI files uses ANSI music, which generates simple sounds through PC speaker\. Sequence **1Bh M** or **1Bh \[ x M** informs the application, that following printable characters will define sound sequence and are not to write on the screen\. The last character of such sequence is **0Eh** and demands the application to exit from the music state\. If **03h** or **0Eh** occurs without entering into music state, the character will be printed or ignored according the **ANSIDOS** parameter\.

When **ANSIDOS=0**, the music state is not available\. Generating the sound is not implemented, because there is very rarely used and is not necessary to view or edit ANSI file\.

## DOS control character assignment

Characters from 00h to 1Fh are control characters and should be treated as non\-printable characters\. DOS has ability to print control character in some cases and it was used in some ANSI files\.

| Byte value | Character name | Default printable character |
| --- | --- | --- |
| 00h | NUL | 0020h |
| 01h | SOH | 263Ah |
| 02h | STX | 263Bh |
| 03h | ETX | 2665h |
| 04h | EOT | 2666h |
| 05h | ENQ | 2663h |
| 06h | ACK | 2660h |
| 07h | BEL | 2022h |
| 08h | BS | 25D8h |
| 09h | HT | 25CBh |
| 0Ah | LF | 25D9h |
| 0Bh | VT | 2642h |
| 0Ch | FF | 2640h |
| 0Dh | CR | 266Ah |
| 0Eh | SO | 266Bh |
| 0Fh | SI | 263Ch |
| 10h | DLE | 25BAh |
| 11h | DC1 | 25C4h |
| 12h | DC2 | 2195h |
| 13h | DC3 | 203Ch |
| 14h | DC4 | 00B6h |
| 15h | NAK | 00A7h |
| 16h | SYN | 25ACh |
| 17h | ETB | 21A8h |
| 18h | CAN | 2191h |
| 19h | EM | 2193h |
| 1Ah | SUB | 2192h |
| 1Bh | ESC | 2190h |
| 1Ch | FS | 221Fh |
| 1Dh | GS | 2194h |
| 1Eh | RS | 25B2h |
| 1Fh | US | 25BCh |

The characters from **00h** to **1Fh** from the ANSI file can be printed in exception of **0Ah**, **0Dh**, **1Ah** and **1Bh**\.

The printable character assignment can be changed by **ANSICharsDOS** setting\.

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
* List of last processed characters filling whole screen line, character number above 128 does not equal to byte values and depends on **FileReadEncoding** parameter\.

If you press the **\`** or **~** key, there will be displayed the information in 4 states:


* Display characters above 1Bh as characters and below 1Bh as numbers\. The space 20h are displayed as **replacement character**, which looks as negative question mark to increase readibility\.
* Display characters from 1Bh to 0x7E as characters and other characters as numbers\. The space 20h are displayed as **replacement character**, which looks as negative question mark to increase readibility\.
* Display all characters as numbers\.
* Number of current step and minimum/maximum number of dummy steps added by **1Bh \[ 1; P1 V** \(time marker\) sequence\. The negative number means, that the time presented by time marker represents ealier number of steps\. In such case, the display can be slightly glitched\. You have to increase the **FileDelayStep** parameter to solve this problem\.

Every press **Tab** key forces screen to repaint, so if screens displays glitches, press the **Tab** or **\`** or **~** key to repaint\.

# Telnet client

TextPaint provides simple Telnet client, whis can be used to test ANSI interpreter compatibility with VT100/ANSI terminal\. Telnet client is available in **WorkMode=2** and you have to provide addres in place of file name in command line\. For example, you can provide **localhost:23** if you have working local telnet server\.

In most cases, this is recommended to set **ANSIWidth** to **80** and **ANSIHeight** to **24**\. If you not specify, the default width and height will be used\.

After running **TextPaint**, choose and press key, which will be used as escape key\. The escape key means the key, which provides access to functions instead of sending keystroke to server\. The escape key can be changed during the session\. After pressing the key, the session will begin\.

During the session, if you press the selected escape key, the information window will popup and you will can use special keys:


* **Esc** \- Return to terminal \(close information\)
* **Enter** \- Change escape key
* **Tab** \- Move information window
* **Backspace** \- Quit from application
* **Number 1 to 9** \- Send **F1** to **F9** keys, useful, when application has problem with capturing function key\.
* **Number 0** \- Send **F10** key\.
* **Space** \- Send other conrol codes instead of function keys by pressing numbers\. If enabled, the numbers sends as following:
  * **Number 1** \- NUL \(00h\)\.
  * **Number 2** \- ESC \(1Bh\)\.
  * **Number 3** \- PS \(1Ch\)\.
  * **Number 4** \- GS \(1Dh\)\.
  * **Number 5** \- RS \(1Eh\)\.
  * **Number 6** \- US \(1Fh\)\.
* **\[** \- Send **F11** key\.
* **\]** \- Send **F12** key\.
* **Letter** \- Send letter with Ctrl modifier, like Ctrl\+X
* / \- Connect od disconnect, depending on current state
* **\\** \- Send screen size to server, some servers requires such information\.
* **\+** or **=** or **\-** or **\*** \- Change codes for function keys, arrows, and position keys\.

## Telnet client limitations

**TextPaint** has some limitations compared to other Telnet/SSH software:


* Screen size change on server demand \(like switching between 80 and 132 columns mode\) is not possible\.
* If server demand some information via escape sequence, the client will not answer \(this feature is necessary very rarely\)\.
* Limited compatibility with VT100/VT102/ANSI terminals, the compatibility is sufficient in most cases\.
* Telnet is the only communication protocol supported\. It is recommended to use server and TextPaint on the same machine \(localhost connection\) or within your LAN\.

## Key codes

The telnet client has several control code sets for some special keys to achieve compatibility with most services\. The initial setting is defined by **TerminalKeys** setting\. this parameter consists of 4 digits:


1. Arrows \- **0** or **1**
2. Functions from **F1** to **F4** \- **0** or **1**
3. Functions from **F5** to **F12** \- **0** or **1**
4. Navigation keys \- **0** or **1** or **2**

There is keys and codes, which will sent to server for each possible setting:

| Key | Number of digits | Value = 0 | Value = 1 | Value = 2 |
| --- | --- | --- | --- | --- |
| Up Arrow | 1 | 1Bh \[ A | 1Bh O A | N/A |
| Down Arrow | 1 | 1Bh \[ B | 1Bh O B | N/A |
| Right Arrow | 1 | 1Bh \[ C | 1Bh O C | N/A |
| Left Arrow | 1 | 1Bh \[ D | 1Bh O D | N/A |
| F1 | 2 | 1Bh \[ 1 1 ~ | 1Bh O P | N/A |
| F2 | 2 | 1Bh \[ 1 2 ~ | 1Bh O Q | N/A |
| F3 | 2 | 1Bh \[ 1 3 ~ | 1Bh O R | N/A |
| F4 | 2 | 1Bh \[ 1 4 ~ | 1Bh O S | N/A |
| F5 | 3 | 1Bh \[ 1 5 ~ | 1Bh O T | N/A |
| F6 | 3 | 1Bh \[ 1 7 ~ | 1Bh O U | N/A |
| F7 | 3 | 1Bh \[ 1 8 ~ | 1Bh O V | N/A |
| F8 | 3 | 1Bh \[ 1 9 ~ | 1Bh O W | N/A |
| F9 | 3 | 1Bh \[ 2 0 ~ | 1Bh O X | N/A |
| F10 | 3 | 1Bh \[ 2 1 ~ | 1Bh O Y | N/A |
| F11 | 3 | 1Bh \[ 2 3 ~ | 1Bh O Z | N/A |
| F12 | 3 | 1Bh \[ 2 4 ~ | 1Bh O \[ | N/A |
| Insert | 4 | 1Bh \[ 2 ~ | 1Bh \[ 2 ~ | 1Bh \[ 2 ~ |
| Delete | 4 | 1Bh \[ 3 ~ | 1Bh \[ 3 ~ | 1Bh \[ 3 ~ |
| Home | 4 | 1Bh \[ 1 ~ | 1Bh O H | 1Bh \[ H |
| End | 4 | 1Bh \[ 4 ~ | 1Bh O F | 1Bh \[ F |
| Page Up | 4 | 1Bh \[ 5 ~ | 1Bh \[ 5 ~ | 1Bh \[ 5 ~ |
| Page Down | 4 | 1Bh \[ 6 ~ | 1Bh \[ 6 ~ | 1Bh \[ 6 ~ |

During session, you can change the key assignment\. Press the escape key to show the information window\. If you press the **\+** or **=** key, you will mowe the marker across the digits in cycle\. If you press the **\-** or **\*** key, you will change the digit across all possible values\. Then, if you close the information window by **Esc** key, the key assignment will be changed immediately\.

## Recording and playing session

You can run Telnet client with session recording to Session\.ans using 100ms resolution \(10 frames per second\) with unlimited data per step:

```
TextPaint.exe localhost WorkMode=2 TerminalStep=0 TerminalTimeResolution=100 TerminalFile=Session.ans
```

After recording, you can play the file using the same time step and use high number of process step per frame\. The displaying tempo should be similar to original session:

```
TextPaint.exe Session.ans WorkMode=1 FileDelayTime=100 FileDelayStep=10000 FileDelayFrame=10000
```

You can render this session to movie \(picture sequence\), to achieve original session tempo, you have to play this 10 frames per second:

```
TextPaint.exe Session.ans WorkMode=4 ANSIRead=1 RenderFile=MoviePath RenderStep=10000 RenderFrame=10000
```

# Escape sequences

TextPaint has implemented limited set of escape sequences used to ANSI data processing and interpreting\. The sequences are following and enough for most files and many telnet services\.

## The non\-standard sequences

The sequences consists of constant number of character in exception by **1Bh \]**, which ends by **07h**\.

| Escape sequence | XTERM name | Meaning |
| --- | --- | --- |
| 1Bh \( P1 |   | Character set for bank 0: If P1 = **0** or P1 = **2**, then semigraphical, else use standard\. |
| 1Bh \) P1 |   | Character set for bank 1: If P1 = **0** or P1 = **2**, then semigraphical, else use standard\. |
| 1Bh \* P1 |   | Character set for bank 2: If P1 = **0** or P1 = **2**, then semigraphical, else use standard\. |
| 1Bh \+ P1 |   | Character set for bank 3: If P1 = **0** or P1 = **2**, then semigraphical, else use standard\. |
| 1Bh n | LS2 | Use character bank 2\. |
| 1Bh o | LS3 | Use character bank 3\. |
| 1Bh c | RIS | Reset terminal\. |
| 1Bh \# 0 |   | Ignore\. |
| 1Bh \# 1 |   | Ignore\. |
| 1Bh \# 2 |   | Ignore\. |
| 1Bh \# 3 | DECDHL | Set current line font to double width and double height, display upper part\. |
| 1Bh \# 4 | DECDHL | Set current line font to double width and double height, display lower part\. |
| 1Bh \# 5 | DECSWL | Set current line font to normal width and normal height\. |
| 1Bh \# 6 | DECDWL | Set current line font to double width and normal height\. |
| 1Bh \# 7 |   | Ignore\. |
| 1Bh \# 8 | DECALN | Fill screen with **E** character\. |
| 1Bh \# 9 |   | Ignore\. |
| 1Bh \] \.\.\. 07h |   | Ignore\. |
| 1Bh 6 | DECBI | Move line left by deleting first character\. |
| 1Bh 7 | DECSC | Save current cursor position and text attributes\. |
| 1Bh 8 | DECRC | Restore saved cursor position and text attributes\. |
| 1Bh 9 | DECFI | Move line right by inserting space before first character\. |
| 1Bh D \- **ANSIDOS=0** | IND | Move cursor down by 1 step, scroll screen if needed\. |
| 1Bh D \- **ANSIDOS=1** |   | Ignore\. |
| 1Bh M \- **ANSIDOS=0** | RI | Move cursor up by 1 step, scroll screen if needed\. |
| 1Bh M \- **ANSIDOS=1** |   | Enter to music state, ignore following characters\. |
| 1Bh E | NEL | Move cursor down by 1 step, scroll screen if needed and move cursor to first column\. |
| 1Bh H | HTS | Add current column to tabulator list |
| 08h | BS | Move cursor one step left\. |
| 09h \- **ANSIDOS=0** | TAB | Move cursor right to nearest multiply of 8 or defined tab stop\. |
| 0Ah | LF | Move cursor one line down, scroll screen if needed\. |
| 0Bh \- **ANSIDOS=0** | VT | Move cursor one line down, scroll screen if needed\. |
| 0Ch \- **ANSIDOS=0** | FF | Move cursor one line down, scroll screen if needed\. |
| 0Dh | CR | Move cursor to first column\. |
| 0Eh \- **ANSIDOS=1** |   | Exit from music state if in music state, otherwise, write character\. |
| 0Eh \- **ANSIDOS=0** | SO | Use character bank 1\. |
| 0Fh \- **ANSIDOS=0** | SI | Use character bank 0\. |
| 1Ah | SUB | Break data processing, ignore in **WorkMode=2**\. |

When **ANSIDOS=1**, every character between **01h** to **1Fh** will be written as standard character in exception of characters: **08h**, **0Ah**, **0Dh**, **1Ah** and **1Bh**\.

When **ANSIDOS=0**, every character between **01h** to **1Fh** will be ignored in exception of characters: **08h**, **09h**, **0Ah**, **0Bh**, **0Dh**, **1Ah** and **1Bh**\.

## The standard sequences

Every standard sequence begins with **1Bh** followed by \[ character and ends with any letter or the character **@** or **\`**:

The standard sequences with **?** charater and without parameters:

| Escape sequence | XTERM name | Meaning |
| --- | --- | --- |
| 1Bh \[ ? 2 l | DECRST / DECANM | Enter into VT52 mode\. |
| 1Bh \[ ? 3 h | DECSET / DECCOLM | Clear screen and move cursor to top left corner of cursor area\. |
| 1Bh \[ ? 3 l | DECRST / DECCOLM | Clear screen and move cursor to top left corner of cursor area\. |
| 1Bh \[ ? 4 h | DECSET / DECSCLM | Enable smooth scrolling |
| 1Bh \[ ? 4 l | DECRST / DECSCLM | Disable smooth scrolling |
| 1Bh \[ ? 6 h | DECSET / DECOM | Enable origin mode and move cursor to top left corner of cursor area\. |
| 1Bh \[ ? 6 l | DECRST / DECOM | Disable origin mode and move cursor to top left corner of screen\. |
| 1Bh \[ ? 7 h | DECSET / DECAWM | Disable screen wrapping\. |
| 1Bh \[ ? 7 l | DECRST / DECAWM | Enable screen wrapping\. |
| 1Bh \[ ? 69 h | DECSET / DECLRMM | Enable left and right margin\. |
| 1Bh \[ ? 69 l | DECRST / DECLRMM | Disable left and right margin\. |

The standard sequences without **?** character and may contains parameters\. When parameter is ommited, the default value is **1**:

| Escape sequence | XTERM name | Meaning |
| --- | --- | --- |
| 1Bh \[ \! p | DECSTR | Reset terminal state including clearing the screen\. |
| 1Bh \[ s | SCOSC | Save current cursor position and text attributes\. |
| 1Bh \[ u | SCORC | Restore saved cursor position and text attributes\. |
| 1Bh \[ 4 h | SM / IRM | Enable inserting mode\. |
| 1Bh \[ 4 l | RM / IRM | Disable inserting mode\. |
| 1Bh \[ 20 h | SM / LNM | Enable new line mode \(**WorkMode=2** only\)\. |
| 1Bh \[ 20 l | RM / LNM | Disable new line mode \(**WorkMode=2** only\)\. |
| 1Bh \[ 0 J | ED | Clear screen from cursor to bottom right corner\. |
| 1Bh \[ 1 J | ED | Clear screen from top left corner to cursor\. |
| 1Bh \[ 2 J | ED | Clear screen and move cursor to top left corner of screen\. |
| 1Bh \[ 0 K | EL | Clear current line from cursor to right edge\. |
| 1Bh \[ 1 K | EL | Clear current line from left edge to cursor\. |
| 1Bh \[ 2 K | EL | Clear current line from left edge to right edge\. |
| 1Bh \[ P1 ; P2 H | CUP | Move cursor to column P1 and line P2, move cursor to be within cursor area\. |
| 1Bh \[ P1 ; P2 f | HVP | Move cursor to column P1 and line P2, move cursor to be within cursor area\. |
| 1Bh \[ P1 A | CUU | Move cursor up through P1 steps\. |
| 1Bh \[ P1 B | CUD | Move cursor down through P1 steps\. |
| 1Bh \[ P1 C | CUF | Move cursor right through P1 steps\. |
| 1Bh \[ P1 D | CUB | Move cursor left through P1 steps\. |
| 1Bh \[ P1 d | VPA | Move cursor to P1 line\. |
| 1Bh \[ P1 e | VPR | Move cursor down through P1 lines\. |
| 1Bh \[ P1 \` | HPA | Move cursor to P1 column\. |
| 1Bh \[ P1 a | HPR | Move cursor right through P1 columns\. |
| 1Bh \[ P1 E | CNL | Move cursor to the first column and P1 lines down\. |
| 1Bh \[ P1 F | CPL | Move cursor to the first column and P1 lines up\. |
| 1Bh \[ P1 G | CHA | Move cursor to P1 column\. |
| 1Bh \[ P1 S | SU | Scroll cursor area P1 times down\. |
| 1Bh \[ P1 T | SD | Scroll cursor area P1 times up\. |
| 1Bh \[ P1 ; P2 r | DECSTBM | Define cursor area as from P1 line to P2 line, move cursor to be within cursor area\. |
| 1Bh \[ P1 ; P2 s | DECSLRM | Define left and right margin\. |
| 1Bh \[ P1 L \- **ANSIDOS=0** | IL | Scroll cursor area P1 times up\. |
| 1Bh \[ P1 L \- **ANSIDOS=1** |   | Ignore\. |
| 1Bh \[ P1 M \- **ANSIDOS=0** | DL | Scroll cursor area P1 times down\. |
| 1Bh \[ P1 M \- **ANSIDOS=1** |   | Enter to music state, ignore following characters\. |
| 1Bh \[ \.\.\. m | SGR | Set attributes, number of parameters can vary\. |
| 1Bh \[ P1 @ | ICH | Insert space and move text being right to be cursor P1 times\. |
| 1Bh \[ P1 P | DCH | Delete character and move text being right to be cursor P1 times\. |
| 1Bh \[ P1 X | ECH | Delete P1 characters right to be cursor without text movement\. |
| 1Bh \[ 0 g | TBC | Remove current column from tabulator list\. |
| 1Bh \[ 3 g | TBC | Clear tabulator list\. |
| 1Bh \[ P1 I | CHT | Forward tabulation P1 times\. |
| 1Bh \[ P1 Z | CBT | Backward tabulation P1 times\. |
| 1Bh \[ P1 b | REP | Repeat last printed character P1 times\. |
| 1Bh \[ P1 Space @ | SL | Move left P1 columns\. |
| 1Bh \[ P1 Space A | SR | Move right P1 columns\. |

## The request\-response sequences

Some sequences does not changing the terminal working, but received from server in **WorkMode=2**, induces response to server:

| Request | XTERM name | Response |
| --- | --- | --- |
| 1Bh \[ 0 c | Primary DA | 1Bh \[ ? 6 c |
| 1Bh \[ 5 n | DSR | 1Bh \[ 0 n |
| 1Bh \[ 6 n | DSR / CPR | 1Bh \[ YY ; XX R where YY and XX are current cursor position |
| 1Bh \[ > 0 c | Secondary DA | 1Bh \[ > 0 ; 1 0 ; 0 c |
| 1Bh \[ = 0 c | Tertiary DA | 1Bh P \! &#124; 0 0 0 0 0 0 0 0 1Bh \\ |
| 1Bh \[ 0 x | DECREQTPARM | 1Bh \[ 2 ; 1 ; 1 ; 1 1 2 ; 1 1 2 ; 1 ; 0 x |
| 1Bh \[ 1 x | DECREQTPARM | 1Bh \[ 3 ; 1 ; 1 ; 1 1 2 ; 1 1 2 ; 1 ; 0 x |

## The same\-meaning escape sequences

There are other supported escape sequences, which are the same meaining as standard escape sequences:

| Sequence | Equivalent |
| --- | --- |
| 1Bh \[ H | 1Bh \[ 1 ; 1 H |
| 1Bh \[ J | 1Bh \[ 0 J |
| 1Bh \[ K | 1Bh \[ 0 K |
| 1Bh \[ c | 1Bh \[ 0 c |
| 1Bh \[ > c | 1Bh \[ > 0 c |
| 1Bh \[ = c | 1Bh \[ = 0 c |
| 1Bh \[ g | 1Bh \[ 0 g |

## The TextPaint escape sequences

There is escape sequences usable in TextPaint only and should be ignored in other software terminals:

| Sequence | Meaning |
| --- | --- |
| 1Bh \[ 0; P1; P2 V | Set font width to P1 and font height to P2, the P1 and P2 are not explicity font size, it encodes the font size and character part number\. |
| 1Bh \[ 1; P1 V | Wait \(process some dummy steps\) to be processed P1 multiplied by defined constant\. It can be treated as time marker\. Used for terminal recording and playing\. |

The font size dimension \(width or height\) defines simultaneiusly character size \(number of modules\) and displayed character part \(horizontally and vertically\):


* **0** \- size 1, whole character\.
* **1** \- size 2, first part\.
* **2** \- size 2, second part\.
* **3** \- size 3, first part\.
* **4** \- size 3, second part\.
* **5** \- size 3, third part\.
* **6** \- size 4, first part\.
* **7** \- size 4, second part\.
* **8** \- size 4, third part\.
* **9** \- size 4, fourth part\.

The maximum supported font size is 16, so the maximum possible number is 135, which means font size 16 and displayed 16th part\. Currently, **TextPaint** can read and write file with various font sizes, but adjusting font size in editor is not implemented yet\.

## Text attributes 

During processing ANSI data, there will be used internal text attributes with following default values:


* Foreground = \-1 \(means color not defined\)
* Background = \-1 \(means color not defined\)
* Bold = false
* Blink = false
* Inverse = false

The attributes can be changed with the **1Bh \[ \.\.\. m** escape sequence\. The number of parameters can vary and has following meaning:

| Parameter | Meaning |
| --- | --- |
| 0 | Set all parameters to default values |
| 00 | Set all parameters to default values |
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
| 5 | Blink = true |
| 25 | Blink = false |
| 7 | Inverse = true |
| 27 | Inverse = false |
| 38 followed with 5 | Set the foreground color to the next number \(after **5**\) |
| 48 followed with 5 | Set the background color to the next number \(after **5**\) |

The parameters **38** or **48** followed by **5** sets the color from the 256\-color palette\. The TextPaint supports standard 16\-colors only\. The other colors are mapped as following:


* **Colors from 0 to 15** \- **0**, **1**, **2**, **3**, **4**, **5**, **6**, **7**, **8**, **9**, **A**, **B**, **C**, **D**, **E**, **F**\.
* **Colors from 16 to 231** \- The color number is achieved from RGB color model using formula `((B + 6*G + 36*R) + 16)` and are mapped by following rules:
  * Each component value are interpreted as low \(**0**, **1**, **2**\) or high value \(**3**, **4**, **5**\), achieving 8 possible colors\.
  * If the sum of component values gives at least **8** \(all possible sums are between **0** and **15**\), the color is high intenity \(from **8** to **F**\)\.
  * If the sum of component values gives at greatest **7** \(all possible sums are between **0** and **15**\), the color is low intenity \(from **0** to **7**\)\.
* **Colors from 232 to 255** \- **0**, **0**, **0**, **0**, **8**, **8**, **8**, **8**, **7**, **7**, **7**, **7**, **F**, **F**, **F**, **F**\.

The whole 256 to 16 color map:

```
00-0F: 0 1 2 3 4 5 6 7 8 9 A B C D E F
10-33: 0 0 0 4 4 4 0 0 0 4 4 4 0 0 0 4 4 4 2 2 2 6 6 E 2 2 2 6 E E 2 2 2 E E E
34-57: 0 0 0 4 4 4 0 0 0 4 4 4 0 0 0 4 4 C 2 2 2 6 E E 2 2 2 E E E 2 2 A E E E
58-7B: 0 0 0 4 4 4 0 0 0 4 4 C 0 0 0 4 C C 2 2 2 E E E 2 2 A E E E 2 A A E E E
7C-9F: 1 1 1 5 5 D 1 1 1 5 D D 1 1 1 D D D 3 3 B F F F 3 B B F F F B B B F F F
A0-C3: 1 1 1 5 D D 1 1 1 D D D 1 1 9 D D D 3 B B F F F B B B F F F B B B F F F
C4-E7: 1 1 1 D D D 1 1 9 D D D 1 9 9 D D D B B B F F F B B B F F F B B B F F F
E8-FF: 0 0 0 0 0 0 8 8 8 8 8 8 7 7 7 7 7 7 F F F F F F
```

**TextPaint** does not support other attributes than color, so every attribute set will be converted to color when text is diaplayed \(**WorkMode=1** or **WorkMode=2**\) or loaded into editor \(**WorkMode=0**\)\. If background or foreground is not defined \(is set as **\-1**\), the colors from **ColorNormal** parameter will be used\.

## Font size

The sequence 1Bh **\[ 0; P1; P2 V** changes the font size, where P1 is font width and P2 is font height\. The P1/P2 value can be from 0 do 10 as following:

| Value | Size | Display character part |
| --- | --- | --- |
| 0 | 1 | 1 of 1 |
| 1 | 2 | 1 of 2 |
| 2 | 2 | 2 of 2 |
| 3 | 3 | 1 of 3 |
| 4 | 3 | 2 of 3 |
| 5 | 3 | 3 of 3 |
| 6 | 4 | 1 of 4 |
| 7 | 4 | 2 of 4 |
| 8 | 4 | 3 of 4 |
| 9 | 4 | 4 of 4 |

# The VT52 mode

The terminal and ANSI parser has VT52 mode, which can be entered by **1Bh \[ ? 2 l** sequence\. In this mode, there are another escape sequence set\. Each sequence concsists of 2 character with some exceptions:

| Sequence | Meaning |
| --- | --- |
| 1Bh F | Enable semigraphic character set \(different from standard VTx\)\. |
| 1Bh G | Disable semigraphic character set \(different from standard VTx\)\. |
| 1Bh < | Exit from VT52 mode\. |
| 1Bh A | Move cursor up\. |
| 1Bh B | Move cursor down\. |
| 1Bh C | Move cursor right\. |
| 1Bh D | Move cursor left\. |
| 1Bh H | Move cursor to upper left corner\. |
| 1Bh Y XX YY | Move cursor to XX column and YY row, when XX and YY are single character\. |
| 1Bh d | Clear the screen part from upper left corner to cursor\. |
| 1Bh J | Clear the screen part from cursor to lower right corner\. |
| 1Bh E | Move cursor to upper left corner and clear whole screen\. |
| 1Bh Z | Request for **1Bh / Z** response, only in **WorkMode=3**\. |
| 1Bh I | Move cursor up or scroll screen backward\. |
| 1Bh K | Clear current line from cursor to end\. |
| 1Bh b P1 | Not implemented 3\-character sequence\. |
| 1Bh c P1 | Not implemented 3\-character sequence\. |




