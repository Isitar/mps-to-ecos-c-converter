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
                args = new[] { "lpa.mps", "lpa2.mps","lpa_ilp.mps", "afiro.mps", "noswot.mps" };
            }

            foreach (var filename in args)
            {

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
                            continue;
                        case "RHS":
                            curreHandler = new RHSLineHandler();
                            continue;
                        case "ENDATA":
                            curreHandler = null;
                            continue;
                        case "BOUNDS":
                            curreHandler = new BoundsLineHandler();
                            continue;
                        default:
                            curreHandler.HandleLine(line, matrix, constraints, variables);
                            break;
                    }
                }

                var bbMode = variables.Values.Any(v => v.VariableType == Variable.VariableTypes.Boolean || v.VariableType == Variable.VariableTypes.Integer);

                var sb = new StringBuilder();
                n = variables.Count;
                var p = constraints.Values.Count(c => c.RowType != Row.RowTypes.N);

                sb.AppendLine($"int {(bbMode ? "i" : "")}lp_{problemName.Replace(" ", "_")}()");
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
                    )}}};");

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

                if (bbMode)
                {
                    {
                        // add bool
                        var num_bool = variables.Values.Count(v => v.VariableType == Variable.VariableTypes.Boolean);
                        sb.AppendLine($"idxint num_bool = {num_bool};");
                        if (num_bool == 0)
                        {
                            sb.AppendLine("idxint *bool_idx = NULL;");
                        }
                        else
                        {
                            var boolIdx = new List<int>();
                            for (int i = 0; i < variables.Values.Count; i++)
                            {
                                if (variables.Values.ElementAt(i).VariableType == Variable.VariableTypes.Boolean)
                                {
                                    boolIdx.Add(i);
                                }
                            }

                            sb.AppendLine($"idxint bool_idx[{num_bool}] = {{{string.Join(", ", boolIdx)}}};");
                        }
                    }
                    {
                        // same code for int
                        var num_int = variables.Values.Count(v => v.VariableType == Variable.VariableTypes.Integer);
                        sb.AppendLine($"idxint num_int = {num_int};");
                        if (num_int == 0)
                        {
                            sb.AppendLine("idxint *int_idx = NULL;");
                        }
                        else
                        {
                            var intIdx = new List<int>();
                            for (int i = 0; i < variables.Values.Count; i++)
                            {
                                if (variables.Values.ElementAt(i).VariableType == Variable.VariableTypes.Integer)
                                {
                                    intIdx.Add(i);
                                }
                            }

                            sb.AppendLine($"idxint int_idx[{num_int}] = {{{string.Join(", ", intIdx)}}};");
                        }
                    }
                }

                var workName = problemName.Split(" ")[0];
                if (bbMode)
                {
                    sb.Append($"ecos_bb_pwork *{workName};");
                }
                else
                {
                    sb.AppendLine($"pwork *{workName};");
                }

                sb.AppendLine("idxint exitFlag;");

                if (bbMode)
                {
                    sb.AppendLine($"{workName} = ECOS_BB_setup(n, m, p, l, nCones, q, 0, Gpr, Gjc, Gir, Apr, Ajc, Air, c, h, b, num_bool, bool_idx, num_int, int_idx, NULL);");
                    sb.AppendLine($"exitFlag = ECOS_BB_solve({workName});");
                }
                else
                {
                    sb.AppendLine($"{workName} = ECOS_setup(n, m, p, l, nCones, q, 0, Gpr, Gjc, Gir, Apr, Ajc, Air, c, h, b);");
                    sb.AppendLine($"exitFlag = ECOS_solve({workName});");
                }

                sb.AppendLine($"for ( int i = 0; i < n; i++) {{ PRINTTEXT(\"X %d: %f\\n\", i, {workName}->x[i]); }}");

                if (bbMode)
                {
                    sb.AppendLine($"PRINTTEXT(\"UB: %f\\n\", {workName}->nodes->U);");
                    sb.AppendLine($"PRINTTEXT(\"LB: %f\\n\", {workName}->nodes->L);");
                }
                else
                {
                    sb.AppendLine($"PRINTTEXT(\"Objective: %f\\n\", {workName}->best_info->pcost);");
                    sb.AppendLine($"ECOS_cleanup({workName}, 0);");
                }

                sb.AppendLine("return exitFlag;");
                sb.AppendLine("}");
                File.WriteAllText($"{problemName}.c", sb.ToString());
            }

        }
    }
}
