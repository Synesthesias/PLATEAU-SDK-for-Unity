using System;
using System.Text;

namespace PLATEAU.Util
{
    /// <summary>
    /// インデント機能付きの StringBuilder です。
    /// </summary>
    public class StringBuilderWithIndent
    {
        private readonly StringBuilder sb;
        private int indent;

        public StringBuilderWithIndent()
        {
            this.sb = new StringBuilder();
        }

        public void AppendLine(string str)
        {
            IndentStr();
            this.sb.AppendLine(str);
        }

        public void IncrementIndent()
        {
            this.indent++;
        }

        public void DecrementIndent()
        {
            this.indent = Math.Max(0, this.indent - 1);
        }

        public override string ToString() => this.sb.ToString();

        private void IndentStr()
        {
            for (int i = 0; i < this.indent; i++)
            {
                this.sb.Append("    ");
            }
        }
    }
}
