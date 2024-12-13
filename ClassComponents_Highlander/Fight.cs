using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Schema;

namespace ConsoleApp_HighLander
{
    public class Fight : BehaviorStrategy
    {
        public void Execute(ConsoleApp app, Highlander self, Highlander opponent)
        {
            string message;

            message = $"{self.Name} (Power: {self.PowerLevel}) is fighting {opponent.Name} (Power: {opponent.PowerLevel})";
            Console.WriteLine(message);
            Logger.Log(message);

            //Handle ties
            if (self.PowerLevel == opponent.PowerLevel)
            {
                HandleTie(self, opponent, app);
            }
            else
            {
                ResolveFight(self, opponent, app);
            }
        }

        private void HandleTie(Highlander self, Highlander opponent, ConsoleApp app)
        {
            string message;
            
            if (self.Age > opponent.Age)
            {
                opponent.IsAlive = false;
                self.PowerLevel += opponent.PowerLevel;
                message = $"{self.Name} wins by age advantage against {opponent.Name} and absorbs {opponent.PowerLevel} power!";
                app.IncrementVictimCount(self.Name);
            }
            else if (self.Age < opponent.Age)
            {
                self.IsAlive = false;
                opponent.PowerLevel += self.PowerLevel;
                message = $"{opponent.Name} wins by age advantage against {self.Name} and absorbs {self.PowerLevel} power!";
                app.IncrementVictimCount(opponent.Name);
            }
            else
            {
                //Pick winner randomly if they are both the same power level and age
                Random rand = new Random();
                if (rand.Next(2) == 0)
                {
                    opponent.IsAlive = false;
                    self.PowerLevel += opponent.PowerLevel;
                    message = $"{self.Name} wins by random decision!";
                    app.IncrementVictimCount(self.Name);
                }
                else
                {
                    self.IsAlive = false;
                    opponent.PowerLevel += self.PowerLevel;
                    message = $"{opponent.Name} wins by random decision!";
                    app.IncrementVictimCount(opponent.Name);
                }
            }
            Console.Write(message);
            Logger.Log(message);
        }

        private void ResolveFight(Highlander self, Highlander opponent, ConsoleApp app)
        {
            string message;
            //Find stronger highlander
            Random rand = new Random();
            int totalPower = self.PowerLevel + opponent.PowerLevel;
            int chance = rand.Next(1, totalPower + 1);

            if (chance <= self.PowerLevel)
            {
                self.PowerLevel += opponent.PowerLevel;
                opponent.IsAlive = false;
                message = $"{self.Name} wins against {opponent.Name} and absorbs {opponent.PowerLevel} power!";
                app.IncrementVictimCount(self.Name);
            }
            else
            {
                opponent.PowerLevel += self.PowerLevel;
                self.IsAlive = false;
                message = $"{opponent.Name} wins against {self.Name} and absorbs {self.PowerLevel} power!";
                app.IncrementVictimCount(opponent.Name);
            }

            Console.WriteLine(message);
            Logger.Log(message);
        }
    }
}