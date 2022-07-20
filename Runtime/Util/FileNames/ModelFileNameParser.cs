using System;
using System.IO;

namespace PLATEAU.Util.FileNames
{
    internal static class ModelFileNameParser
    {
        public static int GetLod(string modelFilePath)
        {
            string fileName = Path.GetFileName(modelFilePath);
            if (!fileName.StartsWith("LOD"))
            {
                throw new ArgumentException($"modelFileName should starts with 'LOD', but actual is {fileName}.");
            }

            int underScoreIndex = fileName.IndexOf('_');
            int lodLen = "LOD".Length;
            string lodStr = fileName.Substring(lodLen, underScoreIndex - lodLen);
            return Convert.ToInt32(lodStr);
        }
    }
}