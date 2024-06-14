using PLATEAU.CityExport.ModelConvert;
using PLATEAU.CityExport.ModelConvert.SubMeshConvert;
using PLATEAU.CityImport.Import.Convert;
using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PLATEAU.RoadNetwork
{
    public static class RoadNetworkEx
    {
        public static void Replace<T>(IList<T> self, T before, T after) where T : class
        {
            for (var i = 0; i < self.Count; i++)
            {
                if (self[i] == before)
                    self[i] = after;
            }
        }

        public static void ReplaceLane(IList<RoadNetworkLane> self, RoadNetworkLane before, RoadNetworkLane after)
        {
            Replace(self, before, after);
            foreach (var lane in self)
                lane.ReplaceConnection(before, after);
        }

        public static void RemoveLane(IList<RoadNetworkLane> self, RoadNetworkLane lane)
        {
            self.Remove(lane);
            foreach (var l in self)
                l.RemoveConnection(lane);
        }

        [Serializable]
        internal class ConvertCityObjectResult
        {
            public AttributeDataHelper AttributeDataHelper { get; }
            public Model Model { get; }
            public ConvertCityObjectResult(AttributeDataHelper attributeDataHelper, Model model)
            {
                AttributeDataHelper = attributeDataHelper;
                Model = model;
            }
        }

        internal static Task<ConvertCityObjectResult> ConvertCityObjectsAsync(GranularityConvertOptionUnity conf)
        {
            if (!conf.IsValid())
            {
                return Task.FromResult<ConvertCityObjectResult>(null);
            }

            // 属性情報を覚えておきます。
            var attributes = GmlIdToSerializedCityObj.ComposeFrom(conf.SrcTransforms);

            // PLATEAUInstancedCityModel が含まれる場合、これもコピーしたいので覚えておきます。
            var instancedCityModelDict = InstancedCityModelDict.ComposeFrom(conf.SrcTransforms);

            var unityMeshToDllSubMeshConverter = new UnityMeshToDllSubMeshWithGameMaterial();

            // ゲームオブジェクトを共通ライブラリのModelに変換します。
            using var srcModel = UnityMeshToDllModelConverter.Convert(
                conf.SrcTransforms,
                unityMeshToDllSubMeshConverter,
                true, // 非表示のゲームオブジェクトも対象に含めます。なぜなら、LOD0とLOD1のうちLOD1だけがActiveになっているという状況で、変換後もToolkitsのLOD機能を使えるようにするためです。
                VertexConverterFactory.NoopConverter());

            // 共通ライブラリの機能でモデルを分割・結合します。
            var converter = new GranularityConverter();
            ConvertedGameObjData a;
            ConvertedMeshData m;
            var dstModel = converter.Convert(srcModel, conf.NativeOption);
            var getter = new SerializedCityObjectGetterFromDict(attributes, dstModel);
            var attrHelper = new AttributeDataHelper(getter, conf.NativeOption.Granularity, true);
            return Task.FromResult(new ConvertCityObjectResult(attrHelper, dstModel));
        }
    }
}