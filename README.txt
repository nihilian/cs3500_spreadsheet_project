Authors: Ben Allred and Lance Atkinson

This editor allows basic spreadsheet functionality, as well as some sick features.
First feature: After loading a spreadsheet or saving a new one for the first time, the Save feature
will remember the filepath, allowing any future saves to work without popping up a dialog box. To specificy a new filepath
use the Save As option. Second feature: Cell values will be updated constantly. No need to even select the contents text
box! Use the arrow keys to quickly navigate the spreadsheet. Strings beginning with a "=" will be treated as a formula. A
red exclamtion mark will dispay while the formula cannot be compiled. Third feature: Undo/Redo functionality. Use the edit tool
menu to select one of these options.

External Resources:
sheet_icon.ico is the icon for the GUI.

Design:
-  Any time the editable text box is changed, the selected spreadsheet cell will change and the
value of this cell and any dependents will change in the panel.
-  Any time a different cell is selected, the information about undoing will be updated.
-  Any time the user tries to close the window or open a new file, if changes are unsaved, then the user will be prompted to save.
-  Opening a file simply replaces the spreadsheet object in the GUI and updates the panel accordingly.

Additional Features:
-  Anytime the undo button is pressed, the last change to a cell will be reverted
-  Anytime the redo button is pressed, if there was a change undone before the user manually changing any cells, that undo will be undone.
-  Pressing the arrow keys will select one of the adjacent keys

Squashed bugs:
-  There was a bug that prevented undo changes from being added, but we since squashed it.
-  Invalid cell names are no longer allowed and cell contents will not be changed.
-  Changing a formula solely to a different letter case no longer creates a change that can be undone.
-  Cell contents of an invalid formula now revert to the most recent form of that formula that was valid instead of staying as in invalid
formula. For example, if cell A1 is empty and the user types "A123" into it and clicks a new cell, A1's contents will then be "A12"
since the cell will be updated for every single change to the contents. This is intentional and not a bug.
