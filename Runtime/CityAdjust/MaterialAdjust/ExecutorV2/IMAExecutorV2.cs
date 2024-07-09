using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.Util;
using System.Threading.Tasks;

namespace PLATEAU.CityAdjust.MaterialAdjust.ExecutorV2
{
    internal interface IMAExecutorV2
    {
        public Task<UniqueParentTransformList> ExecAsync(MAExecutorConf conf);
    }
}