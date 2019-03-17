using System;
using System.Collections.Generic;
using System.Text;

namespace MpsToEcosCConverter
{
    class Variable
    {
        public string Name { get; set; }

        public enum VariableTypes
        {
            Continuous,
            Boolean,
            Integer
        }

        public VariableTypes VariableType { get; set; }

        public Variable(string name, VariableTypes variableType = VariableTypes.Continuous)
        {
            Name = name;
            VariableType = variableType;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
