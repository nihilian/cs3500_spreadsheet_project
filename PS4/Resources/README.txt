Benjamin Allred
u1090524



10/6/17

Reading from XML files makes use of C#'s LINQ to XML feature. This allows my program to quickly filter out all relevant data
from an XML file.


10/4/17

Tests from PS4 will be updated to accomodate the access type of SetCellContents.


10/2/17

To accomdoate the new functionality that is expected for PS5 I will modify the original constructor to call the new "default" constructor
which takes two delegates for IsValid and Normalize and a string to represent the version number. This default constructor will call
the parent class's (AbstractSpreadsheet) constructor, in addition to setting up the usedCells dictionary and dependency graph. Another
constructor will take a filepath to load a previously saved spreadsheet.

Methods will be implemented to load and save spreadsheets in the form of XML documents. This will allow users to save their work.

I will add another data member to the internal Cell class called value. This will store the value part of a cell. Methods will be
implemented to retrieve this value.

Some methods will be tweaked to meet the expectations of PS5. Cell names no longer use underscore. They can only be any number of letters
followed by any number of digits. The Normalizer and IsValid delegates from Formula.cs will be passed into this new version of
Spreadsheet.cs and will be used.

The original methods for setting cell contents are now protected methods that help a single public cell content setter. This setter
will only accept contents in the form of strings, and it is then up to the original cell setters to determine what kind of data to store.
Strings beginning with a "=" will be parsed into Formulas.

The tests written for PS4 will remain unchanged to ensure that these modifications don't "break" the original functionality. New tests
will be added to test the new methods.


9/28/17

Spreadsheet.cs

This class requires two data structures. A dictionary to store all cells that actually have contents, and a dependency graph
which will keep track of which cells are dependent on which other cells. A cell class will also be implemented to keep track
of its contents. Cell values will not be used in this Spreadsheet class. The only relevant information will be the cell name
and its contents.

The contents of any cell with a valid name can be accessed. Any cell that is not saved as a used cell will return an empty string
upon a call to retreive its contents.

Setting the contents of a cell will be divided into 3 separate cases:
	1. If the contents are a numerical value (double) that cell's contents will simply be set to that value.
	2. If the contents are a string that cell's contents will be set to that string. Setting a cell to an empty string will
	   effectively remove it from being classified as a "used cell". Its placeholder in dictionary may remain, but its value
	   will no longer be returned upon a call to get contents.
	3. If the contents are a formula that cell's dependencies will need to be updated. In the case where the formula links a cell
	   to a circular dependency the old dependcies will need to be restored as if no change was made to that cell. I will make use
	   of the included helper methods in AbstractSpreadsheet, as well GetDirectDependencies which I will implement, to gather
	   information about a cell's dependencies. These helper methods will also detect circular dependencies.
After setting the contents of a cell this method will return a set of all cells (including the one being modified) which are dependent
on it. This will be returned to some other class which is using Spreadsheet.

Null and invalid cell names will throw an exception (as well as null strings and formulas).

This class makes use of two libraries from previous assignments. Note: a bug was found in the Formula class which threw an exception
when checking equality for a formula with a null value. This bug was squished and the library was updated. DependencyGraph class
is the same version as it was when submitted.