using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum PlayerOption
{
    NONE, //0
    X, // 1
    O // 2
}

public class TTT : MonoBehaviour
{
    public int Rows;
    public int Columns;
    bool emptyBoard = true; //Added by me
    [SerializeField] BoardView board;

    PlayerOption currentPlayer = PlayerOption.X;
    Cell[,] cells;

    // Start is called before the first frame update
    void Start()
    {
        cells = new Cell[Columns, Rows];

        board.InitializeBoard(Columns, Rows);

        for(int i = 0; i < Rows; i++)
        {
            for(int j = 0; j < Columns; j++)
            {
                cells[j, i] = new Cell();
                cells[j, i].current = PlayerOption.NONE;
            }
        }
    }

    public void MakeOptimalMove()
    { //.GetLength is a magical function that gets the length of a dimension in a multidimensional array.
        if (emptyBoard) //If the board is EMPTY, pick a corner!
        {
            Debug.Log("Empty Board: Picking corner");
            int randomCornerX = UnityEngine.Random.Range(0, 2) == 0 ? 0 : cells.GetLength(0) - 1;
            int randomCornerY = UnityEngine.Random.Range(0, 2) == 0 ? 0 : cells.GetLength(1) - 1;
            ChooseSpace(randomCornerX, randomCornerY);
            return;
        }

        PlayerOption opposingPlayer;
        int[,] corners = {{0, 0}, {0, Columns - 1}, {Rows - 1, 0}, {Rows - 1, Columns - 1}}; //This array here is going to be used for some JANKY behavior below

        if (currentPlayer == PlayerOption.X)
        {
            opposingPlayer = PlayerOption.O;
        }
        else
        {
            opposingPlayer = PlayerOption.X;
        }
        
        for (int x = 0; x < cells.GetLength(0); x++) //0 is for rows
        {
            Debug.Log(x);
            for (int y = 0; y < cells.GetLength(1); y++) //1 is for columns
            {
                Debug.Log(y);
                
                if (cells[x, y].current == PlayerOption.NONE) //If the SPACE is EMPTY
                {
                    /*foreach(string player in Enum.GetNames(typeof(PlayerOption))) //Enum.GetNames<PlayerOption> doesn't work for some reason. Something about .NET version being too old?
                    { //Also, this unfortunately turns the Enums into strings
                        if (!player.Equals(PlayerOption.NONE))
                        {
                            cells[x, y].current = Enum.Parse<PlayerOption>(player); //So this is how we turn the enums back into enums
                            if (GetWinner().ToString() == player) //This should work to check for both either X's or O's winning.
                            {
                                Debug.Log("Someone tried to win");
                                cells[x, y].current = PlayerOption.NONE;         This was an attempt to shorten the two if's below into a single If that runs twice.
                                ChooseSpace(x, y);                               It's incredibly janky, having to convert an enum to a string BACK to an enum.
                                return;                                          It's a good lesson that sometimes less code isn't better code. As this looks ugly.
                            }
                            cells[x, y].current = PlayerOption.NONE;
                        }
                    }*/
                    cells[x, y].current = currentPlayer;
                    if (GetWinner() == currentPlayer) //If YOU can win, then win
                    {
                        Debug.Log("Win");
                        cells[x, y].current = PlayerOption.NONE;
                        ChooseSpace(x, y);
                        return;
                    }
                    cells[x, y].current = PlayerOption.NONE;
                }
            }
        }
        for (int x = 0; x < cells.GetLength(0); x++) //0 is for rows //The Forbidden Third For Loop
        {
            Debug.Log(x);
            for (int y = 0; y < cells.GetLength(1); y++) //1 is for columns
            {
                Debug.Log(y);
                if (cells[x, y].current == PlayerOption.NONE) //If the SPACE is EMPTY
                {
                    cells[x, y].current = opposingPlayer;
                    if (GetWinner() == opposingPlayer) //If your OPPONENT can win, STOP THEM
                    {
                        Debug.Log("Block Win");
                        cells[x, y].current = PlayerOption.NONE;
                        ChooseSpace(x, y);
                        return;
                    }
                    cells[x, y].current = PlayerOption.NONE;
                }
            }
        }
        for (int x = 0; x < corners.GetLength(0); x++) //Second nested For loop for JUST corners. Having it inside the last For loop screwed things up.
        {
            if (cells[corners[x, 0],corners[x, 1]].current == opposingPlayer && cells[1, 1].current == PlayerOption.NONE) //If the OPPONENT controls the CORNER and the CENTER is EMPTY
            {
                Debug.Log("Opponent has Corner: Taking Center");
                ChooseSpace(1, 1); //Take the center!
                return;
            } //I wanted to make this modular with any board size, but finding the center gets too complicated.
            if (cells[corners[x, 0], corners[x, 1]].current == currentPlayer && cells[1, 1].current != currentPlayer) //If the PLAYER has a CORNER but does NOT have the CENTER
            {
                int xRandom;
                int yRandom;
                if (corners[x, 0] == 0) //If it's on the left side
                    xRandom = 1;
                else //It's on the right side
                    xRandom = -1;
                if (corners[x, 1] == 0) //If it's on top
                    yRandom = 1;
                else //It's on the bottom
                    yRandom = -1;
                if (cells[corners[x, 0], corners[x, 1] + yRandom].current == PlayerOption.NONE) //Need to check if the space is open
                {
                    Debug.Log("I have a Corner but not Center: Taking adjacent(1)");
                    ChooseSpace(corners[x, 0], corners[x, 1] + yRandom);
                    return;
                }
                else if (cells[corners[x, 0] + xRandom, corners[x, 1]].current == PlayerOption.NONE) //Need to check if the space is open
                {
                    Debug.Log("I have a Corner but not Center: Taking adjacent(2)");
                    ChooseSpace(corners[x, 0] + xRandom, corners[x, 1]);
                    return;
                }
            }
        }
        bool random = true;
        while(random) //Don't stop until a space is successfully randomly picked.
        {
            int randomX = UnityEngine.Random.Range(0, cells.GetLength(0));
            int randomY = UnityEngine.Random.Range(0, cells.GetLength(1));
            if (cells[randomX, randomY].current == PlayerOption.NONE)
            {
                ChooseSpace(randomX, randomY);
                random = false;
                return;
            }
        }
    }

    public void ChooseSpace(int column, int row)
    {
        if (emptyBoard) //Added by me for empty board logic
            emptyBoard = false;

        // can't choose space if game is over
        if (GetWinner() != PlayerOption.NONE)
            return;

        // can't choose a space that's already taken
        if (cells[column, row].current != PlayerOption.NONE)
            return;

        // set the cell to the player's mark
        cells[column, row].current = currentPlayer;

        // update the visual to display X or O
        board.UpdateCellVisual(column, row, currentPlayer);

        // if there's no winner, keep playing, otherwise end the game
        if(GetWinner() == PlayerOption.NONE)
            EndTurn();
        else
        {
            Debug.Log("GAME OVER!");
        }
    }

    public void EndTurn()
    {
        // increment player, if it goes over player 2, loop back to player 1
        currentPlayer += 1;
        if ((int)currentPlayer > 2)
            currentPlayer = PlayerOption.X;
    }

    public PlayerOption GetWinner()
    {
        // sum each row/column based on what's in each cell X = 1, O = -1, blank = 0
        // we have a winner if the sum = 3 (X) or -3 (O)
        int sum = 0;

        // check rows
        for (int i = 0; i < Rows; i++)
        {
            sum = 0;
            for (int j = 0; j < Columns; j++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;
            }

            if (sum == 3)
                return PlayerOption.X;
            else if (sum == -3)
                return PlayerOption.O;

        }

        // check columns
        for (int j = 0; j < Columns; j++)
        {
            sum = 0;
            for (int i = 0; i < Rows; i++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;
            }

            if (sum == 3)
                return PlayerOption.X;
            else if (sum == -3)
                return PlayerOption.O;

        }

        // check diagonals
        // top left to bottom right
        sum = 0;
        for(int i = 0; i < Rows; i++)
        {
            int value = 0;
            if (cells[i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }

        if (sum == 3)
            return PlayerOption.X;
        else if (sum == -3)
            return PlayerOption.O;

        // top right to bottom left
        sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            int value = 0;

            if (cells[Columns - 1 - i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[Columns - 1 - i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }

        if (sum == 3)
            return PlayerOption.X;
        else if (sum == -3)
            return PlayerOption.O;

        return PlayerOption.NONE;
    }
}
