using ConsoleApp_HighLander;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassComponents_Highlander
{
    public class HighlanderGameService
    {
        public ConsoleApp HighlanderApp { get; private set; }

        public HighlanderGameService(int rows, int columns)
        {
            HighlanderApp = new ConsoleApp(rows, columns);
        }

        public void AddHighlander(string name, int age, int powerLevel, bool isGoodHighlander)
        {
            if (HighlanderApp.HighlanderList.Count >= HighlanderApp.GridColumnDimension * HighlanderApp.GridRowDimension)
            {
                MessageBox.Show("Grid is full. No more highlanders can be added.", "Grid Full", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int[] randomPosition = GetRandomPosition();
            Highlander highlander;

            if (isGoodHighlander)
            {
                highlander = new GoodHighlander(name, age, powerLevel, randomPosition);
            }
            else
            {
                highlander = new BadHighlander(name, age, powerLevel, randomPosition);
            }

            HighlanderApp.AddHighlander(highlander);
        }

        public void StartGame(bool option1, bool option2)
        {
            HighlanderApp.PlayGame(option1, option2);
        }

        public int[] GetRandomPosition()
        {
            Random rand = new Random();
            var existingPositions = HighlanderApp.HighlanderList
                .Select(h => (h.Row, h.Column))
                .ToHashSet();

            while (true)
            {
                int row = rand.Next(0, HighlanderApp.GridRowDimension);
                int column = rand.Next(0, HighlanderApp.GridColumnDimension);

                if (!existingPositions.Contains((row, column)))
                {
                    return new[] { row, column };
                }
            }
        }
    }
}
