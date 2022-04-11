using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvParser
{
    /// <summary>
    /// CSVフィールドインデックス属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CsvFieldAttribute : Attribute
    {
        public int Index { get; }

        public CsvFieldAttribute(int index)
        {
            if (index < 1) throw new ArgumentOutOfRangeException(nameof(index), $"You must specify a value of 1 or greater for index.");
            Index = index;
        }
    }
}
