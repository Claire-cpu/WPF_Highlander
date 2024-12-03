using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassComponents_Highlander
{
    public class Fight : BehaviorStrategy
    {
        public void Execute(ConsoleApp app, Highlander self, Highlander opponent)
        {
            string message;

            message = $"{self.Name} (Power: {self.PowerLevel}) is fighting {opponent.Name} (Power: {opponent.PowerLevel})";
            Console.WriteLine(message);
            Logger.Log(message);

            //Find stronger highlander
            Random rand = new Random();
            int totalPower = self.PowerLevel + opponent.PowerLevel;
            int chance = rand.Next(1, totalPower + 1);

            if (chance <= self.PowerLevel)
            {
                message = $"{self.Name} wins against {opponent.Name} and absorbs {opponent.PowerLevel} power!";
                Console.WriteLine(message);
                Logger.Log(message);
                self.PowerLevel += opponent.PowerLevel;
                opponent.IsAlive = false;
            }
            else
            {
                message = $"{opponent.Name} wins against {self.Name} and absorbs {self.PowerLevel} power!";
                Console.WriteLine(message);
                Logger.Log(message);
                opponent.PowerLevel += self.PowerLevel;
                self.IsAlive = false;
            }
        }
    }
}
