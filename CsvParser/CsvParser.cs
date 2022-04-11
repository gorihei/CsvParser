using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CsvParser
{
    /// <summary>
    /// 指定の型でCSVを入力・出力する機能を実装します。
    /// </summary>
    public class CsvParser
    {
        /// <summary>
        /// CSVファイルにヘッダー行があるかどうかを取得または設定します。
        /// </summary>
        public bool HasHeader { get; set; } = true;

        /// <summary>
        /// 区切り文字を取得または設定します。
        /// </summary>
        public string Delimiter { get; set; } = ",";

        /// <summary>
        /// 要素の前後の空白をトリムするかどうかを取得または設定します。
        /// </summary>
        public bool TrimWhiteSpace { get; set; } = true;

        /// <summary>
        /// CSVファイルの文字コードを取得または設定します。
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// <see cref="CsvParser"/>クラスのインスタンスを初期化します。
        /// </summary>
        public CsvParser() { }

        /// <summary>
        /// CSVファイルから全行を読込み、指定の型にマッピングしたコレクションを返却します。
        /// </summary>
        /// <param name="filePath">CSVファイルパス。</param>
        /// <typeparam name="T">プロパティにCsvFieldAttributeを実装したマッピング対象の型。</typeparam>
        /// <returns>指定した型のコレクション。</returns>
        public IEnumerable<T> ReadAllRecords<T>(string filePath) where T : class, new()
        {
            // ファイルパスが指定されていない
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

            // CsvFieldAttributeがマッピングするクラスメンバに実装されていない
            if (!IsCsvFieldAttributeImplemented<T>()) throw new Exception("CsvFieldAttribute not implemented");

            // CSVファイル読み込み
            var lines = new List<string[]>();
            using (var sr = new StreamReader(filePath, Encoding))
            {
                // ヘッダー行有りの場合は先頭行を読み飛ばす
                if (HasHeader)
                {
                    _ = sr.ReadLine();
                }

                var delimiter = new string[] { Delimiter };
                while (!sr.EndOfStream)
                {
                    lines.Add(sr.ReadLine().Split(delimiter, StringSplitOptions.None));
                }
            }

            // 指定したクラスにマッピングする
            var properties = typeof(T).GetProperties();
            foreach (var line in lines)
            {
                T record = new T();
                for (var i = 0; i < line.Length; i++)
                {
                    var pInfo = properties.FirstOrDefault(pi => (pi.GetCustomAttributes(typeof(CsvFieldAttribute), false)[0] as CsvFieldAttribute).Index == i + 1);
                    if (pInfo != null)
                    {
                        object value = TrimWhiteSpace ? line[i].Trim() : line[i];

                        // Nullable型の場合
                        if (pInfo.PropertyType.IsGenericType && pInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            // 値が空の場合はnullを設定、nullではない場合は基となる型に変換する
                            var t = Nullable.GetUnderlyingType(pInfo.PropertyType) ?? pInfo.PropertyType;
                            value = string.IsNullOrEmpty(value.ToString()) ? null : Convert(t, value.ToString());
                        }
                        pInfo.SetValue(record, value, null);
                    }
                }

                yield return record;
            }
        }

        /// <summary>
        /// 指定した型のコレクションをCSVファイルに出力します。
        /// </summary>
        /// <typeparam name="T">プロパティにCsvFieldAttributeを実装したレコードの型。</typeparam>
        /// <param name="filePath">CSVファイルパス。</param>
        /// <param name="headers">ヘッダー文字列コレクション。</param>
        /// <param name="records">指定した型のコレクション。</param>
        /// <returns></returns>
        public bool SaveRecords<T>(string filePath, IEnumerable<string> headers, IEnumerable<T> records) where T : class, new()
        {
            // ファイルパスが指定されていない
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

            // CsvFieldAttributeがマッピングするクラスメンバに実装されていない
            if (!IsCsvFieldAttributeImplemented<T>()) throw new Exception("CsvFieldAttribute not implemented");

            using (var sw = new StreamWriter(filePath, false, Encoding))
            {
                // ヘッダーが指定されている場合
                if (headers != null && headers.Count() > 0)
                {
                    // ヘッダー行出力
                    sw.WriteLine(string.Join(Delimiter, headers));
                }

                // レコード行出力
                foreach (var record in records)
                {
                    var properties = record.GetType()
                                           .GetProperties()
                                           .OrderBy(pi => (pi.GetCustomAttribute(typeof(CsvFieldAttribute), false) as CsvFieldAttribute).Index);

                    foreach (var prop in properties)
                    {
                        var value = prop.GetValue(record, null);
                        if (value == null)
                        {
                            value = "";
                        }
                    }

                    var values = properties.Select(p => {
                        var value = p.GetValue(record, null);
                        if (value == null) value = "";
                        return value.ToString();
                    });
                    sw.WriteLine(string.Join(Delimiter, values));
                }
            }

            return true;
        }

        /// <summary>
        /// 指定した型のプロパティに<see cref="CsvFieldAttribute"/>が実装されているか判定します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private bool IsCsvFieldAttributeImplemented<T>()
        {
            var members = typeof(T).GetMembers().Where(m => m.MemberType == MemberTypes.Property);
            foreach (var member in members)
            {
                if (!member.IsDefined(typeof(CsvFieldAttribute), false))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 文字列を指定の型に変換します。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private object Convert(Type type, string value)
        {
            if (type.IsEnum)
            {
                return Enum.Parse(type, value);
            }
            return System.Convert.ChangeType(value, type);
        }
    }
}
