using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_HighLander
{
    internal class BadHighlander : Highlander
    {
        private bool _isGood;

        public BadHighlander(string name, int age, int powerLevel, int[] position)
        : base(name, age, powerLevel, position)
        {
            _isGood = false; // Set default value of the characteristic for BadHighlander
        }
        public override bool IsGood { get { return false; } set { _isGood = value; } }

    }
}
