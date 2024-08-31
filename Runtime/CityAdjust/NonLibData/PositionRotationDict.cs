using PLATEAU.Util;
using System.Linq;
using UnityEngine;

namespace PLATEAU.CityAdjust.NonLibData
{
    
    public class PositionRotationDict : INonLibData
    {
        private NonLibDictionary<PositionRotation> data = new();
        
        public void ComposeFrom(UniqueParentTransformList src)
        {
            src.BfsExec(trans =>
            {
                data.Add(trans, src.Get.ToArray(), new PositionRotation(trans));
                return NextSearchFlow.Continue;
            });
        }

        public void RestoreTo(UniqueParentTransformList target)
        {
            target.BfsExec(trans =>
            {
                var posRot = data.GetNonRestoredAndMarkRestored(trans, target.Get.ToArray());
                if (posRot != null)
                {
                    posRot.Apply(trans);
                }
                return NextSearchFlow.Continue;
            });
        }
    }

    public class PositionRotation
    {
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;

        public PositionRotation(Vector3 localPosition, Quaternion localRotation)
        {
            LocalPosition = localPosition;
            LocalRotation = localRotation;
        }
        public PositionRotation(Transform trans) : this(trans.localPosition, trans.localRotation){}

        public void Apply(Transform trans)
        {
            trans.SetLocalPositionAndRotation(LocalPosition, LocalRotation);
        }
    }
}