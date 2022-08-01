using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.SqlServer.Server;
using NUnit.Framework.Constraints;
using PlasticPipe.PlasticProtocol.Messages;
using Debug = UnityEngine.Debug;

namespace PLATEAU.Editor.Diagnostics
{
    public class TimeDiagnosticsTable
    {
        // 処理 / データ　を軸とする二次元テーブル
        private Stopwatch[,] table = new Stopwatch[maxNumProcess, maxNumData];
        private Dictionary<string, int> processNameToId = new Dictionary<string, int>();
        private Dictionary<string, int> dataNameToId = new Dictionary<string, int>();
        private const int maxNumProcess = 30;
        private const int maxNumData = 99;
        private Stopwatch currentStopwatch = null;
        private bool outOfRangeFlag;

        public TimeDiagnosticsTable()
        {
            Clear();
        }

        public void Start(string processName, string dataName)
        {
            this.currentStopwatch?.Stop();
            var stopwatch = GetStopwatch(processName, dataName);
            stopwatch.Start();
            this.currentStopwatch = stopwatch;
        }

        public void Stop(string processName, string dataName)
        {
            GetStopwatch(processName, dataName).Stop();
            this.currentStopwatch = null;
        }

        public string SummaryByProcess()
        {
            if (this.outOfRangeFlag) return "Error. Exceeds max num.";
            var sb = new StringBuilder();
            int numProcess = this.processNameToId.Count;
            float[] times = new float[numProcess];
            for (int i = 0; i < numProcess; i++)
            {
                float t = 0f;
                for (int j = 0; j < this.table.GetLength(1); j++)
                {
                    t += this.table[i, j].ElapsedMilliseconds / 1000f;
                }

                times[i] = t;
            }

            for (int i = 0; i < numProcess; i++)
            {
                string processName = this.processNameToId.First(pair => pair.Value == i).Key;
                sb.Append($"{processName} : {times[i]}sec\n");
            }

            return sb.ToString();
        }

        public string SummaryByData()
        {
            if (this.outOfRangeFlag) return "Error. Exceeds max num.";
            var sb = new StringBuilder();
            int numData = this.dataNameToId.Count;
            float[] times = new float[numData];
            for (int j = 0; j < numData; j++)
            {
                float t = 0f;
                for (int i = 0; i < this.table.GetLength(0); i++)
                {
                    t += this.table[i, j].ElapsedMilliseconds / 1000f;
                }

                times[j] = t;
            }

            for (int j = 0; j < numData; j++)
            {
                string dataName = this.dataNameToId.First(pair => pair.Value == j).Key;
                sb.Append($"{dataName} : {times[j]}sec\n");
            }

            return sb.ToString();
        }


        private Stopwatch GetStopwatch(string processName, string dataName)
        {
            return this.table[ProcessId(processName), DataId(dataName)];
        }

        private int ProcessId(string processName)
        {
            return NameToId(this.processNameToId, processName, maxNumProcess);
        }

        private int DataId(string dataName)
        {
            return NameToId(this.dataNameToId, dataName, maxNumProcess);
        }

        private int NameToId(IDictionary<string, int> dict, string name, int maxNum)
        {
            if (dict.TryGetValue(name, out int id))
            {
                return id;
            }

            int newId = dict.Count;
            if (newId >= maxNum)
            {
                this.outOfRangeFlag = true;
                return maxNum - 1;
            }
            dict[name] = newId;
            return newId;
        }


        private void Clear()
        {
            for (int i = 0; i < this.table.GetLength(0); i++)
            {
                for (int j = 0; j < this.table.GetLength(1); j++)
                {
                    this.table[i, j] = new Stopwatch();
                }
            }
            this.processNameToId.Clear();
            this.dataNameToId.Clear();
        }
    }
}