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

            //Handle ties
            if (self.PowerLevel == opponent.PowerLevel)
            {
                if (self.Age > opponent.Age)
                {
                    opponent.IsAlive = false;
                    message = $"{self.Name} wins by age advantage against {opponent.Name} and absorbs {opponent.PowerLevel} power!";
                    Console.WriteLine(message);
                    Logger.Log(message);
                    self.PowerLevel += opponent.PowerLevel;
                }
                else if (self.Age < opponent.Age)
                {
                    self.IsAlive = false;
                    message = $"{opponent.Name} wins by age advantage against {self.Name} and absorbs {self.PowerLevel} power!";
                    Console.WriteLine(message);
                    Logger.Log(message);
                    opponent.PowerLevel += self.PowerLevel;
                }
                else
                {
                    //Pick winner randomly if they are both the same power level and age
                    Random rand = new Random();
                    if (rand.Next(2) == 0)
                    {
                        opponent.IsAlive = false;
                        message = $"{self.Name} wins by random decision!";
                        Console.WriteLine(message);
                        Logger.Log(message);
                        self.PowerLevel += opponent.PowerLevel;
                    }
                    else
                    {
                        self.IsAlive = true;
                        message = $"{opponent.Name} wins by random decision!";
                        Console.WriteLine(message);
                        Logger.Log(message);
                        opponent.PowerLevel += self.PowerLevel;
                    }
                }
            }
            else
            {
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
}
