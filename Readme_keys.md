# Encoding list and keyboard test

The **WorkMode=3** has two pusposes:


* Get list of all supported encodings, which can by set as following parameters:
  * **ConInputEncoding** \- Set encoding name or codepage for writing characters if **WinUse=0**\.
  * **ConOutputEncoding** \- Set encoding name or codepage for printing characters if **WinUse=0**\.
  * **FileReadEncoding** \- Encoding used in file reading\.
  * **FileWriteEncoding** \- Encoding used in file writing\.
  * **ServerEncoding** \- Character encoding used to send data via network if **ServerPort>0**\.
  * **TerminalEncoding** \- Character encoding used to aquire characters from Telnet service\.
* Keyboard input test through displaying key name and character\.

Afer running **TextPaint**, you have press various keys\. After every pressing key, there will be displayed one item of encoding list and name/character of pressed key\. If you press the same key 5 times, application will be quit\.

# Encoding list

The \.NET Framework supports about 100 character encodings, the number varies depending on operating system and \.NET/Mono version\. If you press key first fime, there will be displayed word **Items** and actual number of supported encoding\. Then, if you press keys one more times, there will be displayed items\. After last item, there will be displayed blank line and the encoding list will be repeated\.

Each encoding list item consists of codepage number followed by colon and optiona encoding names separated by comma\. Every encoding name does not contain spaces\. In **Config\.txt** file or command line parameter you can use either code page number or encoding name\.

# Keyboard input test

After pressing any key, there will be displayed informations about pressed key:


* Key name in quotes\. In some cases, the name of the same key can differ depending on **WinUse** parameter\. In the application some key names are replaced to other name, whis is uset to implement key action\. In such cases, there is displayed both names: The original name followed by the replacement name, both names in quotes\.
* Character number in decimal format\.
* Character number in hexadecimal format\.
* Character in apostrophes when the character is printable\.
* Key modifiers, displayed when key is pressed with **Shift** or **Ctrl** or **Alt**\.

The key names and character numbers are \.NET standard names and can be used in developing other \.NET applications\.

# Key name replacement

There is the table, which describes key name changes to uniformity key event handling regardless **WinUse** parameter\.

| Original name | Replacement name |
| --- | --- |
| Return | Enter |
| Up | UpArrow |
| Down | DownArrow |
| Left | LeftArrow |
| Right | RightArrow |
| Prior | PageUp |
| Next | PageDown |
| Back | Backspace |
| Spacebar | Space |




