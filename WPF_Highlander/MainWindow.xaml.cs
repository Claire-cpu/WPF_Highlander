﻿using ClassComponents_Highlander;
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
        private string conString = "Server=(local);Database=Highlander2024;User=Cort2024;Password=12345";
        public MainWindow()
        {
            InitializeComponent();
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
                string query = "INSERT INTO Highlanders (Name, Age, PowerLevel , IsGood) VALUES (@Name, @Age, @PowerLevel, @IsGood)";
                cmd.CommandText = query;

                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Age", age);
                cmd.Parameters.AddWithValue("@PowerLevel", powerLevel);
                cmd.Parameters.AddWithValue("@IsGood", isGood);

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
            bool option1 = (bool)option1RadioButton.IsChecked;
            bool option2 = (bool)option2RadioButton.IsChecked;

            gameService.StartGame(option1, option2);
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
            highlanderNumberTextBox.Text = (maxHighlanders - currentHighlanders).ToString();
            MessageBox.Show("Highlander added!");
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
        }
    }
}
