using System;
using System.Collections.Generic;
using System.Text;

namespace MpsToEcosCConverter
{
    class VariableInRow
    {
        public Variable Variable { get; set; }

        public double Coefficient { get; set; }

        public VariableInRow()
        {
            
        }

        public VariableInRow(Variable variable, double coefficient)
        {
            this.Variable = variable;
            Coefficient = coefficient;
        }

        public override string ToString()
        {
            return $"{Coefficient} {Variable.Name}";
        }
    }
}
