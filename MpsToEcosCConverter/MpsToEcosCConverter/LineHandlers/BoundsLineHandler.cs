using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MpsToEcosCConverter.LineHandlers
{
    class BoundsLineHandler : ILineHandler
    {
        public void HandleLine(string line, Dictionary<Row, List<VariableInRow>> matrix, Dictionary<string, Row> rows, Dictionary<string, Variable> variables)
        {
            var splitUp = Regex.Split(line.Trim(), @"\s+");
            var type = splitUp[0];
            var boundName = splitUp[1];
            var varName = splitUp[2];
            var bnd = double.Parse(splitUp[3]);

            var variable = variables[varName];

            // if var is integer and upper bound is set to 1, var becomes boolean
            if (variable.VariableType == Variable.VariableTypes.Integer && type.Equals("UP") && bnd == 1.0)
            {
                variable.VariableType = Variable.VariableTypes.Boolean;
                return;
            }

            // otherwise introduce a new constraint
            var row = new Row()
            {
                B = bnd,
                Name = $"{type}_BND_{varName}",
                RowType = type.Equals("UP") ? Row.RowTypes.L : Row.RowTypes.U
            };
            var slackVariable = new Variable(row.Name);
            variables.Add(slackVariable.Name, slackVariable);
            rows.Add(row.Name, row);
            matrix.Add(row, new List<VariableInRow> {new VariableInRow(variable,1), new VariableInRow(slackVariable, row.RowType == Row.RowTypes.U ? -1 : 1) });
        }
    }
}
