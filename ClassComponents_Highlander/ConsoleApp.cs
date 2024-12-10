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
                "Database=Week10Fall2024;" +
                "User=CaraFall2024;Password=12345");

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
            if (option1)
            {
                while (_highlanderList.Count(h => h.IsAlive) > 1)
                {
                    ExecuteRound();
                }
                Console.WriteLine("The game has ended. Winner is {0}!", _highlanderList[0].Name);
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

                Console.WriteLine("Simulation complete.");
            }
        }

        private void ExecuteRound()
        {
            var liveHighlanders = _highlanderList.Where(h => h.IsAlive).ToList(); // Snapshot of live Highlanders

            foreach (Highlander highlander in liveHighlanders)
            {
                // Checking for collisions
                var opponentsInCell = _highlanderList
                    .Where(h => h.IsAlive && h != highlander && h.Row == highlander.Row && h.Column == highlander.Column)
                    .ToList();
                var badHighlanders = opponentsInCell.Where(h => !h.IsGood).ToList();

                if (highlander.IsGood && badHighlanders.Count > 0)
                {
                    foreach (Highlander oppo in badHighlanders)
                    {
                        oppo.Behavior = new Fight();
                        oppo.ExecuteBehavior(this, highlander);
                        _currentRound++;
                        LogInteraction(_currentRound,highlander, oppo);
                        if (!highlander.IsAlive) break;
                    }
                    if (highlander.IsAlive)
                    {
                        highlander.Behavior = new Escape();
                        /*foreach (Highlander oppo in badHighlanders)
                        {
                            highlander.ExecuteBehavior(this, oppo);
                            LogInteraction(highlander, oppo);
                        }*/
                    }
                }
                else if (!highlander.IsGood && opponentsInCell.Count > 0)
                {
                    foreach (Highlander oppo in opponentsInCell)
                    {
                        highlander.Behavior = new Fight();
                        highlander.ExecuteBehavior(this, oppo);
                        _currentRound++;
                        LogInteraction(_currentRound,highlander, oppo);
                        if (!highlander.IsAlive) break;
                    }
                    if (highlander.IsAlive)
                    {
                        highlander.Behavior = new RandomMove();
                        highlander.ExecuteBehavior(this, highlander);
                    }
                }
                else
                {
                    highlander.Behavior = new RandomMove();
                    highlander.ExecuteBehavior(this, highlander);
                    _currentRound++;
                }
            }
            _highlanderList.RemoveAll(h => !h.IsAlive);
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


    }
}
