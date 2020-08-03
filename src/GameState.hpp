/**
 * @file GameState.hpp
 * @author Ben Lewis (me@lewisbj.com)
 * @brief GameState and CellState header file
 * @version 0.1
 * @date 03-08-2020
 * 
 * @copyright Copyright (c) 2020
 * 
 */
#pragma once
#include<cstdint>

/**
 * @brief Cell state enumerator
 * 
 *  Enumerator contains three cell states:
 *      - Empty (no markings)
 *      - Space (false marking)
 *      - Block (true marking)
 */
enum CellState 
{ 
    Empty, 
    Space, 
    Block 
}; 

/**
 * @brief Game state class
 * 
 *  Class to keep track of the state of the game.
 */
class GameState
{
private:
    CellState** cellStates;
    uint8_t** rowHints;
    uint8_t** columnHints;
    uint8_t rows;
    uint8_t columns;

public:
    /**
     * @brief Construct a new GameState object
     * 
     * @param rowHints Jagged array of row hints starting from the top
     * @param columnHints Jagged array of column hints starting from the left
     * @param rows Number of rows in the game board
     * @param columns Number of columns in the game board
     */
    GameState(uint8_t** rowHints, uint8_t** columnHints, uint8_t rows, uint8_t columns){
        this->rowHints = rowHints;
        this->columnHints = columnHints;
        this->rows = rows;
        this->columns = columns;

        cellStates = new CellState*[rows];
        for(int i = 0; i < rows; ++i)
        {
            cellStates[i] = new CellState[columns];
        }
    }

    /**
     * @brief Destroy the GameState object
     * 
     */
    ~GameState() {
        for (int i = 0; i < rows; i++) {
            delete[] cellStates[i];
        }
        delete[] cellStates;
    }

    /**
     * @brief Bracket operator for getting cell state value
     * 
     * @param row Row index of cell state to be received
     * @param column Column index of cell state to be received
     * @return CellState Received cell state
     */
    CellState operator()(uint8_t row, uint8_t column) const
    {
        return cellStates[row][column];
    }

    /**
     * @brief Bracket operator for setting cell state value
     * 
     * @param row Row index of cell state to be set
     * @param column Column index of cell state to be received
     * @return CellState Reference of cell state to be set
     */
    CellState& operator()(uint8_t row, uint8_t column)
    {
        return cellStates[row][column];
    }
};