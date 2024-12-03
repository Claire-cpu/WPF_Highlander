using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassComponents_Highlander
{
    public class GoodHighlander : Highlander
    {
        public GoodHighlander(string name, int age, int powerLevel, int[] position)
        : base(name, age, powerLevel, position)
        {
            _isGood = true; // Set default value of the characteristic for GoodHighlander
        }
        private bool _isGood;
        public override bool IsGood { get { return true; } set { _isGood = value; } }
    }
}
