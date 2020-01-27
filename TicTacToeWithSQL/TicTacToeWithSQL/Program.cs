using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace TicTacToe_Application
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlConnection connection = new SqlConnection(@"Data Source = localhost\SQLEXPRESS; Initial Catalog = dbTestDatabase; Integrated Security = True");
            connection.Open();
            //game functions on a grid like so
            // 1  2  3
            // 4  5  6
            // 7  8  9

            //each array in validWins represents a combination of numbers that would function as a win
            //condition in tic tac toe, so an array with 1, 5, and 9 as values represents winning diagonally like this:
            // X  2  3
            // 4  X  6
            // 7  8  X

            int[][] validWins = new int[8][];

            //diagonal wins
            validWins[0] = new int[] { 1, 5, 9 };
            validWins[1] = new int[] { 3, 5, 7 };

            //horizontal wins
            validWins[2] = new int[] { 1, 2, 3 };
            validWins[3] = new int[] { 4, 5, 6 };
            validWins[4] = new int[] { 7, 8, 9 };

            //vertical wins
            validWins[5] = new int[] { 1, 4, 7 };
            validWins[6] = new int[] { 2, 5, 8 };
            validWins[7] = new int[] { 3, 6, 9 };

            //this while loop is to allow the players to play again after the first game
            while (true)
            {
                List<int> player1Selections = new List<int>();
                List<int> player2Selections = new List<int>();
                bool player1Turn = true;

                //this is necessary to know what the next value we use as a gameID should be
                //better design would be to have a game table with an auto number primary key used as a reference by a turn table
                string selectMaxQuery = "Select MAX(GameID) FROM tblTicTacToe";
                SqlCommand command = new SqlCommand(selectMaxQuery, connection);
                int max = Convert.ToInt32(command.ExecuteScalar()) + 1;


                Console.WriteLine("Player 1, Enter your name, or enter Computer for a bot:");
                var player1Name = Console.ReadLine();

                Console.WriteLine("Player 2, Enter your name, or enter Computer for a bot:");
                var player2Name = Console.ReadLine();

                //this while loops keeps going until a player wins or the game ends in a draw
                //each iteration represents a turn, the player1Turn bool switches at the end of
                //each turn which allows us to cycle between the two players
                while (true)
                {
                    //Player 1 takes a turn
                    if (player1Turn)
                    {
                        DisplayBoard(player1Selections, player2Selections);
                        string selection = "";
                        //this loop is here to force the player to make a valid selection before moving to the next turn
                        while (true)
                        {
                            Console.WriteLine(player1Name + ", make your selection:");
                            if (player1Name == "computer" || player1Name == "Computer")
                            {
                                selection = MakeSelection(player2Selections, player1Selections, validWins).ToString();
                                player1Selections.Add(int.Parse(selection));
                                break;
                            }
                            selection = Console.ReadLine();

                            //test that the user entered a valid selection
                            if (IsValidSelection(player1Selections, player2Selections, selection))
                            {
                                player1Selections.Add(int.Parse(selection));
                                break;
                            }
                        }

                        //now that we have a valid selection we add it to our SQL table
                        AddTurnToSQLTable(max, player1Name, selection, 'X', connection);

                        //once they have made their selection check if they have won
                        if (CheckIfPlayerHasWon(validWins, player1Selections))
                        {
                            DisplayBoard(player1Selections, player2Selections);
                            Console.WriteLine("Congratulations " + player1Name + ", you won the game!");
                            break;
                        }

                        player1Turn = !player1Turn;
                    }

                    //Player 2 takes a turn
                    else
                    {
                        DisplayBoard(player1Selections, player2Selections);
                        string selection = "";
                        //this loop is here to force the player to make a valid selection before moving to the next turn
                        while (true)
                        {
                            
                            Console.WriteLine(player2Name + ", make your selection:");
                            if (player2Name == "computer" || player2Name == "Computer")
                            {
                                selection = MakeSelection(player2Selections, player1Selections, validWins).ToString();
                                player2Selections.Add(int.Parse(selection));
                                break;
                            }
                            selection = Console.ReadLine();
                            //test that the user entered an int
                            if (IsValidSelection(player1Selections, player2Selections, selection))
                            {
                                player2Selections.Add(int.Parse(selection));
                                break;
                            }
                        }

                        //now that we have a valid selection we add it to our SQL table
                        AddTurnToSQLTable(max, player2Name, selection, 'O', connection);

                        //once they have made their selection check if they have won
                        if (CheckIfPlayerHasWon(validWins, player2Selections))
                        {
                            DisplayBoard(player1Selections, player2Selections);
                            Console.WriteLine("Congratulations " + player2Name + ", you won the game!");
                            break;
                        }

                        player1Turn = !player1Turn;
                    }

                    //if player 1 has made 5 selections and no winner has been found that means the game is a draw so
                    //we alert the players and break out of this while loop to move on to the next game
                    if (player1Selections.Count == 5)
                    {
                        Console.WriteLine("Draw!");
                        DisplayBoard(player1Selections, player2Selections);
                        break;
                    }
                }

                //After game has concluded ask if they wish to play again
                Console.WriteLine("Play again? y/n");
                string response = Console.ReadLine();

                if (response != "yes" && response != "y" && response != "Yes")
                    break;
            }

            connection.Close();
        }

        public static bool CheckIfPlayerHasWon(int[][] validWins, List<int> playerSelections)
        {

            //iterate through every valid win condition in our array
            for (int i = 0; i < 8; i++)
            {
                int count = 0;
                //iterate through the three values required for a specific win
                for (int j = 0; j < 3; j++)
                {
                    //check if the player has the necessary value selected
                    if (playerSelections.Contains(validWins[i][j]))
                    {
                        count++;
                    }
                }
                //gives the player a win only if they have all three of the required values in a specific win
                if (count == 3)
                    return true;

            }

            //return false if no wins are found
            return false;
        }

        public static void DisplayBoard(List<int> player1Selections, List<int> player2Selections)
        {
            string[] board = new string[9];
            //fills the starting board with numbers that indicate the selection to make to "fill" a certain square
            for (int k = 0; k < board.Length; k++)
            {
                board[k] = (k + 1).ToString();
            }
            //fill the board with Xs where player 1 has selected
            foreach (int i in player1Selections)
            {
                board[i - 1] = "X";
            }
            //fill the board with Os where player 2 has selected
            foreach (int j in player2Selections)
            {
                board[j - 1] = "O";
            }

            Console.WriteLine("");
            //Display the first row
            Console.WriteLine(board[0] + " | " + board[1] + " | " + board[2]);

            //Decoration
            Console.WriteLine("---------");

            //Display the second row
            Console.WriteLine(board[3] + " | " + board[4] + " | " + board[5]);

            //Decoration
            Console.WriteLine("---------");

            //Display the third row
            Console.WriteLine(board[6] + " | " + board[7] + " | " + board[8]);

        }

        public static bool IsValidSelection(List<int> player1Selections, List<int> player2Selections, string text)
        {
            int value;

            int.TryParse(text, out value);

            //for a selection to be valid it needs to be A) parsable as an int B) a number between 1 and 9
            //so it corresponds to a spot on the board and C) not already selected by either player
            bool valid = (!player1Selections.Contains(value) && !player2Selections.Contains(value) && value > 0 && value < 10);
            //warn the user if they made an invalid selection
            if (!valid)
            {
                Console.WriteLine("Invalid Selection, pick a number between 1 and 9 that has not already been selected");
            }

            return valid;
        }

        //this procedure is how a computer decides which spot on the board to select during it's turn
        public static int MakeSelection(List<int> playerSelections, List<int> opponentSelections, int[][] validWins)
        {
            int[][] priorities = new int[3][];
            //first priority is center space
            priorities[0] = new int[] { 5 };
            //second priority is corners
            priorities[1] = new int[] { 1, 3, 7, 9 };
            //rest of spaces are least priority
            priorities[2] = new int[] { 2, 4, 6, 8 };

            //test if its possible for computer to win
            int selectToWin = PossibleToWin(playerSelections, opponentSelections, validWins);
            //if selectToWin is not -1 that means picking this value will win the game so computer picks it
            if (selectToWin != -1)
            {
                return selectToWin;
            }

            //now test if its possible for opponent to win
            int selectToStopWin = PossibleToWin(opponentSelections, playerSelections, validWins);
            //if selectToStopWin is not -1 that means it's possible for opponent to win and computer needs to block it
            if (selectToStopWin != -1)
            {
                return selectToStopWin;
            }

            //if we made it this far we know neither player can win so we select based on the predetermined priorities

            //move through each "level" of priority
            for (int i = 0; i < 3; i++)
            {
                //move through each value in that level of priority
                for (int j = 0; j < priorities[i].Length; j++)
                {
                    //check that neither player has selected the current value, if niether has than we can select it
                    if (!playerSelections.Contains(priorities[i][j]) && !opponentSelections.Contains(priorities[i][j]))
                        return priorities[i][j];

                }
            }

            //we should never reach this point, it would mean that all the spots have been filled in which case we shouldn't have called this
            return -1;
        }

        //this checks whether it's possible for a given player to win, this can be used to search for a win or for checking if the opponent
        //has a possible win we need to block
        public static int PossibleToWin(List<int> playerSelections, List<int> opponentSelections, int[][] validWins)
        {
            //iterate through every valid win condition in our array
            for (int i = 0; i < 8; i++)
            {
                int count = 0;
                //this value represents the "missing" value in a win condition the computer would need to pick
                //to complete a win condition(or block an opponents) that they have 2 out of 3 values for already

                int missingValue = 0;

                //tracks whether an opponent is "blocking" a win condition
                bool opponentBlocking = false;

                for (int j = 0; j < 3; j++)
                {
                    if (opponentSelections.Contains(validWins[i][j]))
                    {
                        //if the opponent has selected one of the spaces in a win condition we know it isn't achievable
                        //so we break out of this iteration and flag it as impossible
                        opponentBlocking = true;
                        break;
                    }

                    //check if the player has the necessary values selected
                    if (playerSelections.Contains(validWins[i][j]))
                    {
                        count++;
                    }
                    else
                    {
                        //if not add value as the "missing value" that needs to be selected
                        missingValue = validWins[i][j];
                    }
                }
                //if two values in a win condition have been filled(and it isn't being blocked by opponent
                //that means the computer can win by picking it, or we need to pick it to block an opponents win
                if (count == 2 && !opponentBlocking)
                    return missingValue;

            }
            //if we make it here then the player has no possibility to win so we return -1
            return -1;

        }

        public static void AddTurnToSQLTable(int gameID, string playerName, string selection, char value, SqlConnection connection)
        {
            //create insert query to add this turn to our sql table
            var insertCommandQuery = "Insert Into tblTicTacToe(GameID, PlayerName, Position, Value) Values(@GameID, @PlayerName, @Position, @Value)";
            SqlCommand insertCommand = new SqlCommand(insertCommandQuery, connection);
            insertCommand.Parameters.AddWithValue("@GameID", gameID);
            insertCommand.Parameters.AddWithValue("@PlayerName", playerName);
            insertCommand.Parameters.AddWithValue("@Position", int.Parse(selection));
            insertCommand.Parameters.AddWithValue("@Value", value);

            //execute our completed sql query
            insertCommand.ExecuteNonQuery();
        }
    }
}

