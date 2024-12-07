using ConsoleApp_HighLander;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            int[] randomPosition = HighlanderApp.GetRandomPosition();
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
    }
}
