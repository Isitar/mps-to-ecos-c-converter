using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MpsToEcosCConverter.LineHandlers
{
    class RowLineHandler : ILineHandler
    {
        public void HandleLine(string line, Dictionary<Row, List<VariableInRow>> matrix, Dictionary<string, Row> rows, Dictionary<string, Variable> variables)
        {
            var splitUp = Regex.Split(line.Trim(), @"\s+");
            var type = splitUp[0];
            var row = new Row();
            double? coefficient = null;
            switch (type)
            {
                case "E":
                    row.RowType = Row.RowTypes.E;
                    break;
                case "L":
                    row.RowType = Row.RowTypes.L;
                    coefficient = 1.0;
                    break;
                case "G":
                    row.RowType = Row.RowTypes.U;
                    coefficient = -1.0;
                    break;
                case "N":
                    row.RowType = Row.RowTypes.N;
                    break;
                default: throw new ArgumentException($"Unknown type {type}", nameof(line));
            }


            row.Name = splitUp[1];
            matrix.Add(row, new List<VariableInRow>());
            rows.Add(row.Name, row);
            if (coefficient.HasValue)
            {
                var variable = new Variable(row.Name);
                variables.Add(variable.Name, variable);
                matrix[row].Add(new VariableInRow(variable, coefficient.Value));
            }
        }
    }
}
