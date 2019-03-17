using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MpsToEcosCConverter.LineHandlers
{
    class RHSLineHandler : ILineHandler
    {
        public void HandleLine(string line, Dictionary<Row, List<VariableInRow>> matrix, Dictionary<string, Row> rows, Dictionary<string, Variable> variables)
        {
            var splitUp = Regex.Split(line.Trim(), @"\s+");
            var constraintName = splitUp[1];
            var bVal = double.Parse(splitUp[2]);
            rows[constraintName].B = bVal;
            if (splitUp.Length > 3)
            {
                constraintName = splitUp[3];
                bVal = double.Parse(splitUp[4]);
                rows[constraintName].B = bVal;
            }
        }
    }
}
