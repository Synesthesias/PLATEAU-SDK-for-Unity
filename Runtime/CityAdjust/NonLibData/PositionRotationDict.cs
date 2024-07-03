using PLATEAU.Util;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.CityAdjust.NonLibData
{
    
    public class PositionRotationDict : INonLibData
    {
        private Dictionary<NonLibKeyName, PositionRotation> data = new();
        
        public void ComposeFrom(UniqueParentTransformList src)
        {
            src.BfsExec(trans =>
            {
                data.Add(new NonLibKeyName(trans), new PositionRotation(trans));
                return NextSearchFlow.Continue;
            });
        }

        public void RestoreTo(UniqueParentTransformList target)
        {
            target.BfsExec(trans =>
            {
                var key = new NonLibKeyName(trans);
                if (data.TryGetValue(key, out var posRot))
                {
                    posRot.Apply(trans);
                }
                return NextSearchFlow.Continue;
            });
        }
    }

    public struct PositionRotation
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