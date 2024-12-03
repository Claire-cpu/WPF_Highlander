using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassComponents_Highlander
{
    public class Escape : BehaviorStrategy
    {
        public void Execute(ConsoleApp app, Highlander self, Highlander opponent = null)
        {
            string message;

            // here Checking the type of the opponent

            int[] selfPos = self.Position;
            //int[] opponentPos = opponent.Position;
            self.Behavior = new RandomMove();

            //int[] newPos = CalculateEscapePosition(selfPos, opponentPos, app.GridRowDimension - 1, app.GridColumnDimension - 1);
            self.Behavior.Execute(app, self);
            //self.UpdatePosition(newPos);

            message = $"{self.Name} escaped from {opponent.Name} to position ({self.Position[0]}, {self.Position[1]}).";
            Console.WriteLine(message);
            Logger.Log(message);


        }

    }
}
