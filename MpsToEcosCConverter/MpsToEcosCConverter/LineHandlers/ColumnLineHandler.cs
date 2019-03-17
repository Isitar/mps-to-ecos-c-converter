using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MpsToEcosCConverter.LineHandlers
{
    class ColumnLineHandler : ILineHandler
    {
        public void HandleLine(string line, Dictionary<Row, List<VariableInRow>> matrix, Dictionary<string, Row> rows, Dictionary<string, Variable> variables)
        {
            var splitUp = Regex.Split(line.Trim(), @"\s+");
            var variableName = splitUp[0];
            var constraintName = splitUp[1];
            var coefficient = double.Parse(splitUp[2]);

            if (!variables.ContainsKey(variableName))
            {
                variables.Add(variableName, new Variable(variableName));
            }

            matrix[rows[constraintName]].Add(new VariableInRow(variables[variableName], coefficient));

            if (splitUp.Length > 3)
            {
                constraintName = splitUp[3];
                coefficient = double.Parse(splitUp[4]);
                matrix[rows[constraintName]].Add(new VariableInRow(variables[variableName], coefficient));
            }

        }
    }
}
