# Code a text based minesweeper game

Start with this sample project and expand so that it accepts input
in the form of coordinates: "x y". For every coordinate input the
program shall follow the rules of the minesweeper game and print the
new state of the game.

Initially all coordinates are unvisited, i.e. covered. This shall be illustrated
with a "?". The first program output will be:

```
  01234
4|?????
3|?????
2|?????
1|?????
0|?????
```

If the user then inputs "4 0" the new state will be:

```
  01234
4|?????
3|?????
2|?????
1|??111
0|??1
```

The program shall uncover all coordinates adjacent to the visited one
until a bomb is uncovered

When the state is printed the number of bombs that can be reached from
uncovered coordinates shall be output for those coordinates or empty
for coordinated with no adjacent bombs.

The program shall ask for coordinates until a bomb is encountered or
all bombs have been uncovered.

Feel free to make your own assumptions and change the code as you like.

Test cases that you feel are relevant can be added to the test project.
MinesweeperTest uses MSTest but feel free to use another test framework
(e.g. nunit, xunit.net etc) or with no framework

Good luck!

