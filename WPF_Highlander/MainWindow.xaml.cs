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

namespace WPF_Highlander
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HighlanderGameService gameService;
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
            int age = int.Parse(highlanderAgeTextBox.Text);
            int powerLevel = int.Parse(highlanderPowerLevelTextBox.Text);
            bool isGood = (bool)goodRadioButton.IsChecked;

            gameService.AddHighlander(name, age, powerLevel, isGood);
        }

        private void StartGame()
        {
            bool option1 = (bool)option1RadioButton.IsChecked;
            bool option2 = (bool)option2RadioButton.IsChecked;

            gameService.StartGame(option1, option2);
        }

        //Add Highlander button
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            AddHighlander();
            MessageBox.Show("Highlander added!");
        }

        //Start Game button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }
    }
}
