using ClassComponents_Highlander;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using System.Data;
using System.Collections;

namespace WPF_Highlander
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HighlanderGameService gameService;
        private SqlConnection conn = new SqlConnection();
        private SqlCommand cmd;
        private string conString = "Server=(local);" +
                "Database=Week10Fall2024;" +
                "User=CaraFall2024;Password=12345";
        private bool option1, option2;
        public MainWindow()
        {
            InitializeComponent();
            ClearDatabase();
     
        }

        private void InitializeGameService()
        {
            int rows = int.Parse(rowsTextBox.Text);
            int columns = int.Parse(columnsTextBox.Text);
            gameService = new HighlanderGameService(rows, columns);
        }

        private void AddHighlander()
        {
            string name = highlanderNameTextBox.Text;
            int age;
            int powerLevel;
            bool isGood = (bool)goodRadioButton.IsChecked;

            if (string.IsNullOrEmpty(name) ||
                !int.TryParse(highlanderAgeTextBox.Text, out age) ||
                !int.TryParse(highlanderPowerLevelTextBox.Text, out powerLevel))
            {
                MessageBox.Show("Please provide valid inputs for age and power level.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            gameService.AddHighlander(name, age, powerLevel, isGood);

            conn.ConnectionString = conString;
            cmd = conn.CreateCommand();

            try
            {
                string query = "INSERT INTO Highlanders (Name, Age, PowerLevel , IsGood, IsAlive) VALUES (@Name, @Age, @PowerLevel, @IsGood, @IsAlive)";

                //cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.NVarChar) { Value = gameService.HighlanderApp });
                cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar) { Value = name });
                cmd.Parameters.Add(new SqlParameter("@Age", SqlDbType.Int) { Value = age });
                cmd.Parameters.Add(new SqlParameter("@PowerLevel", SqlDbType.Int) { Value = powerLevel });
                cmd.Parameters.Add(new SqlParameter("@IsGood", SqlDbType.Bit) { Value = isGood });
                cmd.Parameters.Add(new SqlParameter("@IsAlive", SqlDbType.Bit) { Value = 1 });
                cmd.CommandText = query;

                conn.Open();
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding highlander: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                cmd.Dispose();
                conn.Close();
            }
            UpdateGameGrid();
        }

        private void StartGame()
        {
            option1 = (bool)option1RadioButton.IsChecked;
            option2 = (bool)option2RadioButton.IsChecked;
            string inputText = numRoundsTextBox.Text;
            int playRounds;

            if (int.TryParse(inputText, out playRounds) && !option1)
            {
                // Conversion succeeded
                Console.WriteLine($"Converted value: {playRounds}");
            }
            else if(option2)
            {
                // Conversion failed
                MessageBox.Show("Please enter a valid number of rounds.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }


            gameService.StartGame(option1, option2, playRounds);
        }

        private void UpdateGameGrid()
        {
            gameGrid.Children.Clear();
            for (int row = 0; row < gameService.HighlanderApp.GridRowDimension; row++)
            {
                for (int col = 0; col < gameService.HighlanderApp.GridColumnDimension; col++)
                {
                    Button cell = new Button();
                    var occupant = gameService.HighlanderApp.HighlanderList.FirstOrDefault(h => h.Row == row && h.Column == col);

                    if (occupant != null)
                    {
                        cell.Background = occupant.IsGood ? Brushes.LightGreen : Brushes.Red;
                    }
                    else
                    {
                        cell.Background = Brushes.White;
                    }

                    gameGrid.Children.Add(cell);
                }
            }
        }
        //Add Highlander button
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (gameService == null)
            {
                InitializeGameService();
            }

            int maxHighlanders = gameService.HighlanderApp.GridColumnDimension * gameService.HighlanderApp.GridRowDimension;
            int currentHighlanders = gameService.HighlanderApp.HighlanderList.Count;

            AddHighlander();
            UpdateGameGrid();
            numOfHighlandersTextBox.Text = (maxHighlanders - currentHighlanders).ToString();
            MessageBox.Show("Highlander added!");
        }

        //display game result
        private void DisplayResultOption1()
        {
            try
            {
                conn.ConnectionString = conString;

                // Query to get the latest winner's name
                string winnerQuery1 = "SELECT TOP 1 FighterName FROM GameRounds WHERE FighterIsAlive = 1 ORDER BY Round DESC;";
                string winnerQuery1res = GetSingleValueFromDatabase(winnerQuery1, "No winner found");
                string winnerQuery2 = "SELECT TOP 1 OpponentName FROM GameRounds WHERE FighterIsAlive = 0 ORDER BY Round DESC;";
                string winnerQuery2res = GetSingleValueFromDatabase(winnerQuery2, "No winner found");
                string winnerName;
                if (winnerQuery1res != "No winner found") {
                    winnerName = winnerQuery1res;
                }
                else if(winnerQuery2res != "No winner found")
                {
                    winnerName = winnerQuery2res;
                }
                else
                {
                    winnerName = "No winner found";
                }

                if (winnerName == "No winner found")
                {
                    gameResult.Text = "No game results found.";
                    return;
                }

                // Get victims, rounds, and power absorbed where the winner is the killer
                string victimQuery = "SELECT OpponentName FROM GameRounds WHERE FighterName = @WinnerName AND FighterIsAlive = 1;";
                List<string> victims = GetMultipleValuesFromDatabase<string>(victimQuery, "@WinnerName", winnerName);

                string roundQuery = "SELECT Round FROM GameRounds WHERE FighterName = @WinnerName AND FighterIsAlive = 1;";
                List<int> rounds = GetMultipleValuesFromDatabase<int>(roundQuery, "@WinnerName", winnerName);

                string powerQuery = "SELECT PowerAbsorb FROM GameRounds WHERE FighterName = @WinnerName AND FighterIsAlive = 1;";
                List<int> power = GetMultipleValuesFromDatabase<int>(powerQuery, "@WinnerName", winnerName);

                // Get victims, rounds, and power absorbed where the winner was the opponent
                string reverseVictimQuery = "SELECT FighterName FROM GameRounds WHERE OpponentName = @WinnerName AND FighterIsAlive = 0;";
                List<string> reverseVictims = GetMultipleValuesFromDatabase<string>(reverseVictimQuery, "@WinnerName", winnerName);

                string reverseRoundQuery = "SELECT Round FROM GameRounds WHERE OpponentName = @WinnerName AND FighterIsAlive = 0;";
                List<int> reverseRounds = GetMultipleValuesFromDatabase<int>(reverseRoundQuery, "@WinnerName", winnerName);

                string reversePowerQuery = "SELECT PowerAbsorb FROM GameRounds WHERE OpponentName = @WinnerName AND FighterIsAlive = 0;";
                List<int> reversePower = GetMultipleValuesFromDatabase<int>(reversePowerQuery, "@WinnerName", winnerName);

                // Construct the result message
                StringBuilder resultMessage = new StringBuilder($"Option 1 Result \n Winner: {winnerName}\n");

                if (victims.Count > 0)
                {
                    string[] victimList = victims.ToArray();
                    int[] roundList = rounds.ToArray();
                    int[] powerList = power.ToArray();
                    for(int i = 0; i < victimList.Length; i++)
                    {
                        resultMessage.AppendLine($"Victims: {victimList[i]} \t Killed in Round: {roundList[i]} \t Power Absorbed: {powerList[i]} \n");
                    }
                    
                }

                if (reverseVictims.Count > 0)
                {
                    string[] reverseVictimList = reverseVictims.ToArray();
                    int[] reverseRoundList = reverseRounds.ToArray();
                    int[] reversePowerList = reversePower.ToArray();
                    for (int i = 0; i < reverseVictimList.Length; i++)
                    {
                        resultMessage.AppendLine($"Victims: {reverseVictimList[i]} \t Killed in Round: {reverseRoundList[i]} \t Power Absorbed: {reversePowerList[i]} \n");
                    }
                    
                }

                gameResult.Text = resultMessage.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayResultOption2()
        {
            try
            {
                conn.ConnectionString = conString; 
                string query = "SELECT * FROM GameRounds";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                Console.WriteLine("Database connection successful.");
                SqlDataReader reader = cmd.ExecuteReader();

                // Create a StringBuilder for the result message
                StringBuilder resultMessage = new StringBuilder($"Option 2 Result:\n");

                // Check if there are rows in the result set
                if (reader.HasRows)
                {
                    // Read each row and process the data
                    while (reader.Read())
                    {
                        // Example: Retrieve columns by index or column name
                        string fighterName = reader["FighterName"].ToString();
                        string opponentName = reader["OpponentName"].ToString();
                        bool isAlive = (bool)reader["FighterIsAlive"];
                        int round = (int)reader["Round"];

                        // Append the data to the result message
                        resultMessage.AppendLine($"Round {round}: {fighterName} vs {opponentName} - {(isAlive ? "Alive" : "Defeated")}");
                    }
                }
                else
                {
                    // No rows found
                    resultMessage.AppendLine("No fight ever initiated. All highlanders are alive");
                }

                // Close the reader
                reader.Close();

                // Display the result in the WPF control
                gameResult.Text = resultMessage.ToString();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                cmd.Dispose();
                conn.Close();
            }
        }

        private string GetSingleValueFromDatabase(string query, string defaultValue)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : defaultValue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing query: {ex.Message}");
                return defaultValue;
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        private List<T> GetMultipleValuesFromDatabase<T>(string query, string parameterName, string parameterValue)
        {
            var results = new List<T>();

            try
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue(parameterName, parameterValue);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                results.Add((T)reader.GetValue(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing query: {ex.Message}");
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return results;
        }



        //Start Game button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (gameService == null)
            {
                InitializeGameService();
            }

            StartGame();
            UpdateGameGrid();
            if (option1)
            {
                DisplayResultOption1();
            }

            if (option2)
            {
                DisplayResultOption2();
            }
        }

        private void ClearDatabase()
        {
            try
            {
                conn.ConnectionString = conString;
                cmd = conn.CreateCommand();

                // Combine the delete queries
                string query = "DELETE FROM GameRounds; DELETE FROM Highlanders;";
                cmd.CommandText = query;

                conn.Open();
                cmd.ExecuteNonQuery();
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
