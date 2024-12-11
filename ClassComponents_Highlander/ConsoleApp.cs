using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace ConsoleApp_HighLander
{
    public class ConsoleApp
    {
        private List<Highlander> _highlanderList = new List<Highlander>();
        private int _gridRowDimension;
        private int _gridColumnDimension;
        private int[,] _grid;
        private int _currentRound = 1;

        private SqlConnection conn = new SqlConnection("Server=(local);" +
                "Database=Highlander2024;" +
                "User=Cort2024;Password=12345");

        public ConsoleApp(int gridRowDimension, int gridColumnDimension)
        {
            _gridRowDimension = gridRowDimension;
            _gridColumnDimension = gridColumnDimension;
            _grid = new int[_gridRowDimension, _gridColumnDimension]; // Play grid dimensions
        }

        public List<Highlander> HighlanderList
        {
            get { return _highlanderList; }
            set { _highlanderList = value; }
        }

        public int GridRowDimension
        {
            get { return _gridRowDimension; }
            set { _gridRowDimension = value; }
        }

        public int GridColumnDimension
        {
            get { return _gridColumnDimension; }
            set { _gridColumnDimension = value; }
        }

        public void AddHighlander(Highlander highlander)
        {
            if (_highlanderList.Any(h => h.Name.Equals(highlander.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("A Highlander with the same name already exists.");
            }
            _highlanderList.Add(highlander);
        }

        public void RemoveHighlander(Highlander highlander)
        {
            _highlanderList.RemoveAll(h => h.Id == highlander.Id);
        }

        public void PlayGame(bool option1, bool option2)
        {
            //ClearDatabase();

            if (option1)
            {
                if (!_highlanderList.Any(h => !h.IsGood)) //if all highlanders initially input by user are good highlanders, no winner for the game
                {
                    Console.WriteLine("The game has no winner, as all highlanders are good highlanders.");
                }
                else
                {
                    while (_highlanderList.Count(h => h.IsAlive) > 1)
                    {
                        ExecuteRound();

                        //When more than 1 highlander is alive and none of them is bad, break the loop
                        if (!_highlanderList.Any(h => h.IsAlive && !h.IsGood))
                        {
                            break;
                        }
                    }
                    //Console.WriteLine("The game has ended. Winner is {0}!", _highlanderList[0].Name);
                }
                //ClearDatabase();
                Console.WriteLine("Simulation complete.");
            }

            if (option2)
            {
                Console.WriteLine("Input how many rounds of simulation you want to run:");
                int rounds = Convert.ToInt32(Console.ReadLine());

                for (int round = 1; round <= rounds; round++)
                {
                    Console.WriteLine($"Round {round} begins.");
                    ExecuteRound();
                    Console.WriteLine($"Round {round} ends. Remaining Highlanders: {_highlanderList.Count(h => h.IsAlive)}");
                }
                //ClearDatabase() ;
                Console.WriteLine("Simulation complete.");
            }
        }

        private void ExecuteRound()
        {
            Console.WriteLine($"Round {_currentRound} begins.");

            // Create a snapshot of currently alive Highlanders at the start of the round
            var liveHighlanders = _highlanderList.Where(h => h.IsAlive).ToList();

            // Process each live Highlander for this round
            foreach (Highlander highlander in liveHighlanders)
            {
                // Skip if this Highlander somehow died before processing (safety check)
                if (!highlander.IsAlive) continue;

                // Find opponents in the same cell
                var opponentsInCell = _highlanderList
                    .Where(h => h.IsAlive && h != highlander && h.Row == highlander.Row && h.Column == highlander.Column)
                    .ToList();

                // If there are opponents, handle fights
                if (opponentsInCell.Count > 0)
                {
                    foreach (var opponent in opponentsInCell)
                    {
                        if (!highlander.IsAlive || !opponent.IsAlive) continue;

                        // Engage in fight
                        highlander.Behavior = new Fight();
                        highlander.ExecuteBehavior(this, opponent);

                        // Log the interaction
                        LogInteraction(_currentRound, highlander, opponent);

                        // Immediately remove any defeated Highlanders
                        if (!opponent.IsAlive)
                        {
                            Console.WriteLine($"{opponent.Name} has been defeated and removed.");
                            _highlanderList.Remove(opponent);
                        }

                        if (!highlander.IsAlive)
                        {
                            Console.WriteLine($"{highlander.Name} has been defeated and removed.");
                            _highlanderList.Remove(highlander);
                            break; // No further actions for this highlander
                        }

                        // After a single fight, break to avoid multiple consecutive fights by the same Highlander in one round
                        break;
                    }
                }
                else
                {
                    // No opponents, move randomly
                    if (highlander.IsAlive)
                    {
                        highlander.Behavior = new RandomMove();
                        highlander.ExecuteBehavior(this, highlander);
                    }
                }

                // After handling fights and moves for this Highlander, check if we have a single winner
                if (_highlanderList.Count(h => h.IsAlive) == 1)
                {
                    var winner = _highlanderList.First(h => h.IsAlive);
                    Console.WriteLine($"The game has ended. Winner is {winner.Name}!");
                    // Since a winner is found, break out of the round processing entirely
                    break;
                }
            }

            // Final cleanup: remove all dead Highlanders (redundant but safe)
            _highlanderList.RemoveAll(h => !h.IsAlive);

            Console.WriteLine($"Round {_currentRound} ends. {_highlanderList.Count(h => h.IsAlive)} Highlanders remain.");

            // Increment round counter only after a full round completes
            _currentRound++;
        }







        private void LogInteraction(int roundInfo, Highlander highlander1, Highlander highlander2)
        {
            string query = @"
                INSERT INTO GameRounds 
                (Round, Name, OpponentName, IdIsAlive, PowerAbsorb) 
                VALUES (@Round, @Highlander1Name, @Highlander2Name, @Highlander1IsAlive, @PowerAbsorbed)";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Round", roundInfo);
                cmd.Parameters.AddWithValue("@Highlander1Name", highlander1.Name);
                cmd.Parameters.AddWithValue("@Highlander2Name", highlander2.Name);
                cmd.Parameters.AddWithValue("@Highlander1IsAlive", highlander1.IsAlive);
                cmd.Parameters.AddWithValue("@PowerAbsorbed", highlander1.IsAlive ? highlander2.PowerLevel : highlander1.PowerLevel);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error logging interaction: {ex.Message}");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void ClearDatabase()
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("DELETE FROM GameRounds; DELETE FROM Highlanders;", conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing database: {ex.Message}");
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
