using System.Collections.Generic;

namespace MpsToEcosCConverter.LineHandlers
{
    interface ILineHandler
    {
        void HandleLine(string line, Dictionary<Row, List<VariableInRow>> matrix, Dictionary<string, Row> rows, Dictionary<string, Variable> variables);
    }
}
