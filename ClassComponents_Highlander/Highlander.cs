using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassComponents_Highlander
{
    public abstract class Highlander
    {
        public static int _idCounter = 0;
        private int _id;
        private string _name;
        private int _powerLevel;
        private int[] _position;
        private int _age;
        private bool _isAlive;
        private BehaviorStrategy _behavior;

        public Highlander(string name, int age, int powerLevel, int[] position)
        {
            _id = ++_idCounter;
            _name = name;
            _age = age;
            _powerLevel = powerLevel;
            _position = position;
            _isAlive = true;
        }
        public int Id { get { return _id; } set { _id = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        public int PowerLevel { get { return _powerLevel; } set { _powerLevel = value; } }
        public int[] Position { get { return _position; } set { _position = value; } }
        public int Age { get { return _age; } set { _age = value; } }
        public bool IsAlive { get { return _isAlive; } set { _isAlive = value; } }
        public BehaviorStrategy Behavior { get { return _behavior; } set { _behavior = value; } }
        public abstract bool IsGood { get; set; }

        //Row and Column
        public int Row { get { return _position[0]; } set { _position[0] = value; } }
        public int Column { get { return _position[1]; } set { _position[1] = value; } }
        // adding some methods here to execute the current behaviour

        public void ExecuteBehavior(ConsoleApp app, Highlander opponent)
        {
            if (_behavior != null)
            {
                _behavior.Execute(app, this, opponent);
            }
        }


    }
}
