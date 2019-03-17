using System;
using System.Collections.Generic;
using System.Text;

namespace MpsToEcosCConverter
{
    class Row
    {
        public RowTypes RowType { get; set; }

        public enum RowTypes
        {
            N,
            E,
            L,
            U
        };

        public string Name { get; set; }

        public override string ToString()
        {
            return $"{RowType} {Name}";
        }

        public double B { get; set; }
    }
}
