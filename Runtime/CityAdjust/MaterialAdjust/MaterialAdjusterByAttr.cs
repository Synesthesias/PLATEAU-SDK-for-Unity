using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.CityInfo;
using UnityEngine;

namespace PLATEAU.CityAdjust.MaterialAdjust
{
    internal class MaterialAdjusterByAttr : MaterialAdjusterBase
    {
        public override void InitBySearch(SearchArg searchArg)
        {
            var searchArgByAttr = (SearchArgByArr)searchArg;
            this.targetObjs = searchArgByAttr.TargetObjs;
            var targetTransforms = this.targetObjs.Select(obj => obj.transform).ToArray();
            var foundAttrs = new CityAttrSearcher().Search(targetTransforms, searchArgByAttr.AttrKey);
            MaterialAdjustConf = new MaterialAdjustConf<string>(foundAttrs);
        }

        public override Task Exec()
        {
            throw new System.NotImplementedException();
        }
    }
}