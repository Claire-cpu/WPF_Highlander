using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_HighLander
{
    public class Escape : BehaviorStrategy
    {
        public void Execute(ConsoleApp app, Highlander self, Highlander opponent = null)
        {
            string message;

            // here Checking the type of the opponent

            self.Behavior = new RandomMove();
            self.Behavior.Execute(app, self);

            message = opponent != null
               ? $"{self.Name} escaped from {opponent.Name} to position ({self.Position[0]}, {self.Position[1]})."
               : $"{self.Name} moved to a new position ({self.Position[0]}, {self.Position[1]}) to avoid confrontation.";
            Console.WriteLine(message);
            Logger.Log(message);
        }
    }
}
