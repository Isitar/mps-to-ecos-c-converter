using System;
using System.Collections.Generic;
using System.Text;

namespace MpsToEcosCConverter
{
    class Variable
    {
        public string Name { get; set; }

        public Variable(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
