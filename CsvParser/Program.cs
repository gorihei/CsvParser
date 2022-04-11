using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Threading;

namespace CsvParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 入力
            var csvFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Book2.csv");
            var csv = new CsvParser();
            foreach (var record in csv.ReadAllRecords<Test>(csvFile))
            {
                Console.WriteLine(record);
            }


            // 出力
            var header = new string[] { "string", "int", "long", "float", "double", "enum", "date" };
            var records = new Test[1000];
            for (var i = 0; i < records.Length; i++)
            {
                records[i] = new Test
                {
                    文字列 = $"ごりへい{i}",
                    整数_int = i,
                    整数_long = i * 10000000000,
                    浮動小数点数_float = (float)(i * 1.123),
                    浮動小数点数_double = (double)(i * 0.00000123),
                    列挙値 = null,
                    日付 = DateTime.Now + TimeSpan.FromDays(i),
                };
            }
            csvFile = Path.Combine(Path.GetDirectoryName(csvFile), "Book2.csv");
            csv.SaveRecords(csvFile, header, records);

            Console.ReadKey();
        }

        public class Test
        {
            [CsvField(1)]
            public string 文字列 { get; set; }
            [CsvField(2)]
            public int? 整数_int { get; set; }
            [CsvField(3)]
            public long? 整数_long { get; set; }
            [CsvField(4)]
            public float? 浮動小数点数_float { get; set; }
            [CsvField(5)]
            public double? 浮動小数点数_double { get; set; }
            [CsvField(6)]
            public Environment.SpecialFolder? 列挙値 { get; set; }
            [CsvField(7)]
            public DateTime? 日付 { get; set; }

            public override string ToString()
            {
                var str = "";
                foreach (var pi in this.GetType().GetProperties())
                {
                    str += $"{pi.Name}:{pi.GetValue(this)}\r\n";
                }
                return str;
            }
        }
    }
}
