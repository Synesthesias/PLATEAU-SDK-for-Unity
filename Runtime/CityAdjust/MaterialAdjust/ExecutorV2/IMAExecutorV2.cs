using PLATEAU.CityAdjust.MaterialAdjust.Executor;
using PLATEAU.Util;
using System.Threading.Tasks;

namespace PLATEAU.CityAdjust.MaterialAdjust.ExecutorV2
{
    public interface IMAExecutorV2
    {
        public Task<UniqueParentTransformList> ExecAsync(MAExecutorConf conf);
    }
}