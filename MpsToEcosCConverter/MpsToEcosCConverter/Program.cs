using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using MpsToEcosCConverter.LineHandlers;

namespace MpsToEcosCConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (null == args || args.Length == 0)
            {
                args = new[] { "demo.mps" };
            }
            var filename = args[0];
            var lines = File.ReadAllLines(filename);

            var n = 0;

            var matrix = new Dictionary<Row, List<VariableInRow>>();
            var constraints = new Dictionary<string, Row>();
            var variables = new Dictionary<string, Variable>();
            var problemName = "Generic Problem";
            ILineHandler curreHandler = null;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                switch (line.Split(" ")[0])
                {
                    case "NAME":
                        curreHandler = null;
                        problemName = new String(line.Replace("NAME", "").SkipWhile(c => c == ' ').ToArray());
                        continue;
                    case "ROWS":
                        curreHandler = new RowLineHandler();
                        continue;
                    case "COLUMNS":
                        curreHandler = new ColumnLineHandler();
                        break;
                    case "RHS":
                        curreHandler = new RHSLineHandler();
                        break;
                    case "ENDATA":
                        curreHandler = null;
                        break;
                    default:
                        curreHandler.HandleLine(line, matrix, constraints, variables);
                        break;
                }
            }

            var sb = new StringBuilder();
            n = variables.Count;
            var p = constraints.Values.Count(c => c.RowType != Row.RowTypes.N);

            sb.AppendLine($"int lp_{problemName.Replace(" ", "_")}()");
            sb.AppendLine("{");
            
            sb.AppendLine($"idxint n = {n};");
            sb.AppendLine($"idxint m = {n};");
            sb.AppendLine($"idxint p = {p};");
            sb.AppendLine($"idxint l = {n};");
            sb.AppendLine("idxint nCones = 0;");
            sb.AppendLine();
            sb.AppendLine("// cost function");
            var costFunction = matrix[constraints.First(c => c.Value.RowType == Row.RowTypes.N).Value];

            sb.AppendLine($@"pfloat c[{n}] = {{{
            string.Join(", ",
                variables.Values.Select(variable =>
                {
                    return costFunction.FirstOrDefault(v => v.Variable.Equals(variable))?.Coefficient ?? 0.0;
                })
            ) }}};");

            sb.AppendLine();
            sb.AppendLine("//cone");
            sb.AppendLine($"idxint Gjc[{n + 1}] = {{{string.Join(", ", Enumerable.Range(0, n + 1))}}};");
            sb.AppendLine($"idxint Gir[{n}] = {{{string.Join(", ", Enumerable.Range(0, n))}}};");
            sb.AppendLine($"pfloat Gpr[{n}] = {{{string.Join(", ", Enumerable.Range(0, n).Select(_ => "-1.0"))}}};");
            sb.AppendLine($"pfloat h[{n}] = {{{string.Join(", ", Enumerable.Range(0, n).Select(_ => "0.0"))}}};");
            sb.AppendLine("idxint *q = NULL;");
            sb.AppendLine();
            sb.AppendLine("//lp matrix");

            var ajc = new List<int>();
            var air = new List<int>();
            var apr = new List<double>();

            foreach (var variable in variables.Values)
            {
                ajc.Add(apr.Count);
                int i = 0;
                foreach (var constr in constraints.Values.Where(c => c.RowType != Row.RowTypes.N))
                {

                    var varCoefficient = matrix[constr].FirstOrDefault(varInRow => varInRow.Variable.Equals(variable))?.Coefficient;
                    if (varCoefficient == null)
                    {
                        i++;
                        continue;
                    }

                    apr.Add(varCoefficient.Value);
                    air.Add(i);
                    i++;
                }
            }
            ajc.Add(apr.Count);

            sb.AppendLine($@"idxint Ajc[{n + 1}] = {{{string.Join(", ", ajc)}}};");
            sb.AppendLine($@"idxint Air[{apr.Count}] = {{{string.Join(", ", air)}}};");
            sb.AppendLine($@"pfloat Apr[{apr.Count}] = {{{string.Join(", ", apr)}}};");
            sb.AppendLine($"pfloat b[{p}] = {{{string.Join(", ", constraints.Values.Where(c => c.RowType != Row.RowTypes.N).Select(c => c.B))}}};");

            var workName = problemName.Split(" ")[0];
            sb.AppendLine($"pwork *{workName};");
            sb.AppendLine("idxint exitFlag;");
            sb.AppendLine($"{workName} = ECOS_setup(n, m, p, l, nCones, q, 0, Gpr, Gjc, Gir, Apr, Ajc, Air, c, h, b);");
            sb.AppendLine($"exitFlag = ECOS_solve({workName});");
            sb.AppendLine($"for ( int i = 0; i < n; i++) {{ PRINTTEXT(\"X % d: % f\\n\", i, {workName}->x[i]); }}");
            sb.AppendLine($"PRINTTEXT(\"Objective: %f\\n\", {workName}->best_info->pcost);");
            sb.AppendLine($"ECOS_cleanup({workName}, 0);");
            sb.AppendLine("return exitFlag;");
            sb.AppendLine("}");
            File.WriteAllText(problemName, sb.ToString());


        }
    }
}
