#include <iostream>
#include <vector>
#include <string>
#include "GameState.hpp"

using namespace std;

int main()
{
   uint8_t** rowHints;
   uint8_t** columnHints;

   uint8_t rows = 5;
   uint8_t columns = 5;

   GameState game(rowHints, columnHints, rows, columns);

   cout << game(0,0) << endl;

   game(0,0) = Block;

   cout << game(0,0) << endl;
}