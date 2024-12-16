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

        public void PlayGame(bool option1, bool option2, int playRounds)
        {
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
                        ExecuteRoundOption1();

                        //When more than 1 highlander is alive and none of them is bad, break the loop
                        if (!_highlanderList.Any(h => h.IsAlive && !h.IsGood))
                        {
                            break;
                        }
                    }
                    
                    var winner = _highlanderList.FirstOrDefault(h => h.IsAlive);
                    if (winner != null)
                    {
                        string message = $"The game has ended. Winner is {winner.Name}!";
                        Console.WriteLine(message);
                        Logger.Log(message);
                        UpdateWinnerAndTotalPower(winner.Name, winner.PowerLevel);
                    }
                }
            }

            if (option2)
            {

                for (int round = 1; round <= playRounds; round++)
                {
                    Console.WriteLine($"Round {round} begins.");
                    Logger.Log($"Round {round} begins.");
                    ExecuteRoundOption2(playRounds);
                    Console.WriteLine($"Round {round} ends. Remaining Highlanders: {_highlanderList.Count(h => h.IsAlive)}");
                }

                /*var aliveHighlanders = _highlanderList.Where(h => h.IsAlive).ToList();
                if (aliveHighlanders.Count == 1)
                {
                    //One winner for option2
                    var winner = aliveHighlanders.First();
                    string message = $"The game has ended. Winner is {winner.Name}!";
                    Console.WriteLine(message);
                    Logger.Log(message);
                    UpdateWinnerAndTotalPower(winner.Name, winner.PowerLevel);
                }
                else
                {
                    //No single winner just show final good/bad count
                    string message = "No single winner emerged at the end of option2.";
                    Console.WriteLine(message);
                    Logger.Log(message);
                }*/

                UpdateGoodAndBadCount();
                Console.WriteLine("Simulation complete.");
            }
        }

        private void ExecuteRoundOption1()
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
                        LogInteraction(_currentRound, highlander, oppo);
                        if (!highlander.IsAlive) 
                        {
                            UpdateLifeStatus(highlander, false);
                            break;
                        }
                        else
                        {
                            UpdateLifeStatus(oppo, false);
                        }
                    }
                    if (highlander.IsAlive)
                    {
                        highlander.Behavior = new Escape();
                       
                    }
                }
                else if (!highlander.IsGood && highlander.IsAlive && opponentsInCell.Count > 0)
                {
                    foreach (Highlander oppo in opponentsInCell)
                    {
                        highlander.Behavior = new Fight();
                        highlander.ExecuteBehavior(this, oppo);
                        _currentRound++;
                        LogInteraction(_currentRound, highlander, oppo);
                        if (!highlander.IsAlive)
                        {
                            UpdateLifeStatus(highlander, false);
                            break;
                        }
                        else
                        {
                            UpdateLifeStatus(oppo, false);
                        }
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

        public void ExecuteRoundOption2(int playRounds)
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
                            _currentRound++;
                            if (_currentRound == playRounds) break;

                            oppo.Behavior = new Fight();
                            oppo.ExecuteBehavior(this, highlander);
                            
                            LogInteraction(_currentRound, highlander, oppo);

                            if (!highlander.IsAlive)
                            {
                                UpdateLifeStatus(highlander, false);
                                break;
                            }
                            else
                            {
                                UpdateLifeStatus(oppo, false);
                            }
                        }
                        if (highlander.IsAlive)
                        {
                            highlander.Behavior = new Escape();

                        }
                    }
                    else if (!highlander.IsGood && highlander.IsAlive && opponentsInCell.Count > 0)
                    {
                        foreach (Highlander oppo in opponentsInCell)
                        {
                            _currentRound++;
                            if (_currentRound == playRounds) break;
                            highlander.Behavior = new Fight();
                            highlander.ExecuteBehavior(this, oppo);
                            
                            LogInteraction(_currentRound, highlander, oppo);
                            if (!highlander.IsAlive)
                            {
                                UpdateLifeStatus(highlander, false);
                                break;
                            }
                            else
                            {
                                UpdateLifeStatus(oppo, false);
                            }
                        }
                        if (highlander.IsAlive)
                        {
                            highlander.Behavior = new RandomMove();
                            highlander.ExecuteBehavior(this, highlander);
                        }
                    }
                    else
                    {
                        _currentRound++;
                        if (_currentRound == playRounds) break;
                        highlander.Behavior = new RandomMove();
                        highlander.ExecuteBehavior(this, highlander);
                    }
                }
                _highlanderList.RemoveAll(h => !h.IsAlive);
            
        }
        private void LogInteraction(int roundInfo, Highlander highlander1, Highlander highlander2)
        {
            string query = @"
                INSERT INTO GameRounds 
                (Round, FighterName, OpponentName, FighterIsAlive, PowerAbsorb) 
                VALUES (@Round, @Highlander1Name, @Highlander2Name, @Highlander1IsAlive, @PowerAbsorb)";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Round", roundInfo);
                cmd.Parameters.AddWithValue("@Highlander1Name", highlander1.Name);
                cmd.Parameters.AddWithValue("@Highlander2Name", highlander2.Name);
                cmd.Parameters.AddWithValue("@Highlander1IsAlive", highlander1.IsAlive);
                cmd.Parameters.AddWithValue("@PowerAbsorb", highlander1.IsAlive ? highlander2.PowerLevel : highlander1.PowerLevel);

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

        private void UpdateLifeStatus(Highlander highlander, bool isAlive)
        {
            string query = @"
               UPDATE Highlanders
               SET IsAlive = @IsAlive
               WHERE Name = @HighlanderName";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@HighlanderName", highlander.Name);
                cmd.Parameters.AddWithValue("@IsAlive", Convert.ToInt32(isAlive));

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

        public void IncrementVictimCount(string winnerName)
        {
            string query = @"
                UPDATE Highlanders
                SET VictimNumber = COALESCE(VictimNumber, 0) +1
                WHERE Name = @Name";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Name", winnerName);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error incrementing VictimNumber: {ex.Message}");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void UpdateWinnerAndTotalPower(string winnerName, int winnerPower)
        {
            string query = @"
                UPDATE Highlanders
                SET Winner = 1, TotalPowerLevel = @TotalPower
                WHERE Name = @Name";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Name", winnerName);
                cmd.Parameters.AddWithValue("@TotalPower", winnerPower);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating winner and total power: {ex.Message}");
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void UpdateGoodAndBadCount()
        {
            string query = @"
                UPDATE Highlanders
                SET GoodHighlanders = (SELECT COUNT(*) FROM Highlanders WHERE IsAlive = 1 AND IsGood = 1),
                BadHighlanders = (SELECT COUNT(*) FROM Highlanders WHERE IsAlive = 1 AND IsGood = 0);";


            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating good and bad counts: {ex.Message}");
                }
                finally
                {
                    conn.Close();
                }
            }

            Console.WriteLine($"Updated database with final count of good and bad Highlanders remaining.");
        }
    }
}
