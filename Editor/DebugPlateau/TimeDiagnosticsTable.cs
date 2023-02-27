using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PLATEAU.Editor.DebugPlateau
{
    /// <summary>
    /// 処理時間を計測するためのクラスです。
    /// 異なるデータに対して複数の同じ処理を適用するときの処理時間を計測・集計します。
    /// 時間の記録は 二次元テーブル上で行われ、縦軸に処理名、横軸にデータ名をとる表の各マスにストップウォッチが並んでいるイメージです。
    /// データの集計は 処理ごと、データごと の2種類の方法で可能です。
    /// </summary>
    internal class TimeDiagnosticsTable
    {
        
        
        
        
        // 今は使ってないのでコメントアウトしておきますが、今後の開発で使うことがあるかもしれないので残しておきます。

        
        
        
        
        // /// <summary>処理 と データ　を軸とする二次元テーブルです。 </summary>
        // private readonly Stopwatch[,] table = new Stopwatch[maxNumProcess, maxNumData];
        //
        // /// <summary> 処理名とテーブルの縦軸インデックスを紐付けます。 </summary>
        // private readonly Dictionary<string, int> processNameToId = new Dictionary<string, int>();
        //
        // /// <summary> データ名とテーブルの横軸インデックスを紐付けます。 </summary>
        // private readonly Dictionary<string, int> dataNameToId = new Dictionary<string, int>();
        //
        // private const int maxNumProcess = 30;
        // private const int maxNumData = 99;
        // private Stopwatch currentStopwatch;
        // private bool outOfRangeFlag;
        //
        // public TimeDiagnosticsTable()
        // {
        //     Clear();
        // }
        //
        //
        // /// <summary>
        // /// 名前によって表の1マスを指定し、そこのストップウォッチを開始させます。
        // /// 前のストップウォッチを停止させます。
        // /// 引数で　処理名（縦軸）とデータ名（横軸）によって表のマスを指定します。
        // /// 引数で未知の名前が与えられたらその名前に新しい行（列）を割り当て、既知の名前が与えられたらその行（列）を指定した扱いになります。
        // /// </summary>
        // public void Start(string processName, string dataName)
        // {
        //     this.currentStopwatch?.Stop();
        //     var stopwatch = GetStopwatch(processName, dataName);
        //     stopwatch.Start();
        //     this.currentStopwatch = stopwatch;
        // }
        //
        // public void Stop(string processName, string dataName)
        // {
        //     GetStopwatch(processName, dataName).Stop();
        //     this.currentStopwatch = null;
        // }
        //
        // /// <summary>
        // /// 処理ごとに時間を集計します。
        // /// </summary>
        // public string SummaryByProcess()
        // {
        //     if (this.outOfRangeFlag) return "Error. Exceeds max num.";
        //     var sb = new StringBuilder();
        //     int numProcess = this.processNameToId.Count;
        //     float[] times = new float[numProcess];
        //     for (int i = 0; i < numProcess; i++)
        //     {
        //         float t = 0f;
        //         for (int j = 0; j < this.table.GetLength(1); j++)
        //         {
        //             t += this.table[i, j].ElapsedMilliseconds / 1000f;
        //         }
        //
        //         times[i] = t;
        //     }
        //
        //     for (int i = 0; i < numProcess; i++)
        //     {
        //         string processName = this.processNameToId.First(pair => pair.Value == i).Key;
        //         sb.Append($"{processName} : {times[i]}sec\n");
        //     }
        //
        //     return sb.ToString();
        // }
        //
        // /// <summary>
        // /// データごとに時間を集計します。
        // /// </summary>
        // public string SummaryByData()
        // {
        //     if (this.outOfRangeFlag) return "Error. Exceeds max num.";
        //     var sb = new StringBuilder();
        //     int numData = this.dataNameToId.Count;
        //     float[] times = new float[numData];
        //     for (int j = 0; j < numData; j++)
        //     {
        //         float t = 0f;
        //         for (int i = 0; i < this.table.GetLength(0); i++)
        //         {
        //             t += this.table[i, j].ElapsedMilliseconds / 1000f;
        //         }
        //
        //         times[j] = t;
        //     }
        //
        //     for (int j = 0; j < numData; j++)
        //     {
        //         string dataName = this.dataNameToId.First(pair => pair.Value == j).Key;
        //         sb.Append($"{dataName} : {times[j]}sec\n");
        //     }
        //
        //     return sb.ToString();
        // }
        //
        //
        // private Stopwatch GetStopwatch(string processName, string dataName)
        // {
        //     return this.table[ProcessId(processName), DataId(dataName)];
        // }
        //
        // private int ProcessId(string processName)
        // {
        //     return NameToId(this.processNameToId, processName, maxNumProcess);
        // }
        //
        // private int DataId(string dataName)
        // {
        //     return NameToId(this.dataNameToId, dataName, maxNumProcess);
        // }
        //
        // private int NameToId(IDictionary<string, int> dict, string name, int maxNum)
        // {
        //     if (dict.TryGetValue(name, out int id))
        //     {
        //         return id;
        //     }
        //
        //     int newId = dict.Count;
        //     if (newId >= maxNum)
        //     {
        //         this.outOfRangeFlag = true;
        //         return maxNum - 1;
        //     }
        //     dict[name] = newId;
        //     return newId;
        // }
        //
        //
        // private void Clear()
        // {
        //     for (int i = 0; i < this.table.GetLength(0); i++)
        //     {
        //         for (int j = 0; j < this.table.GetLength(1); j++)
        //         {
        //             this.table[i, j] = new Stopwatch();
        //         }
        //     }
        //     this.processNameToId.Clear();
        //     this.dataNameToId.Clear();
        // }
    }
}