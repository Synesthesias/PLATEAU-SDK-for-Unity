using PLATEAU.CityInfo;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PLATEAU.CityAdjust.NonLibData
{
    /// <summary>
    /// モデル調整において、共通ライブラリを介してやり取りしないコンポーネント（NonLibコンポーネントと呼びます）を覚えておき、
    /// 変換後に同じコンポーネントを復元できるようにするクラスです。
    /// </summary>
    public class NonLibComponentsDict : INonLibData
    {
        private NonLibDictionary<Component[]> data = new();

        /// <summary>
        /// ゲームオブジェクトとその子から、NonLibコンポーネントの辞書を構築します。
        /// </summary>
        public void ComposeFrom(UniqueParentTransformList src)
        {
            src.BfsExec(trans =>
            {
                List<Component> nonLibComps = new();
                foreach (var comp in trans.GetComponents<Component>())
                {
                    if (IsNonLibComponent(comp))
                    {
                        nonLibComps.Add(comp);
                    }
                }

                data.Add(trans, src.Get.ToArray(), nonLibComps.ToArray());
                return NextSearchFlow.Continue;
            });
        }

        public void RestoreTo(UniqueParentTransformList target)
        {
            target.BfsExec(dst =>
            {
                var comps = data.GetNonRestoredAndMarkRestored(dst, target.Get.ToArray());
                if (comps == null) return NextSearchFlow.Continue;
                foreach (var comp in comps)
                {
                    if (comp == null) continue;
                    CopyComponent(comp, dst.gameObject);
                }

                return NextSearchFlow.Continue;
            });
        }

        /// <summary>
        /// NonLibコンポーネントであればtrueを返します。
        /// </summary>
        private bool IsNonLibComponent(Component c)
        {
            if (c == null) return false;
            if (c is Transform or Renderer or PLATEAUCityObjectGroup or MeshCollider or MeshFilter) return false;
            return true;
        }

        /// <summary>
        /// コンポーネントをコピーします。
        /// </summary>
        private void CopyComponent(Component srcComp, GameObject dstObj)
        {
            Type type = srcComp.GetType();
            Component dstComp = dstObj.GetComponent(type);
            try
            {
                if (dstComp == null)
                {
                    dstComp = dstObj.AddComponent(type);
                }
            }
            catch (Exception)
            {
                Debug.LogWarning($"Could not copy component {type.Name} to {dstObj.name}. Skipping.");
                return;
            }

            // フィールドをコピーします。
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                try
                {
                    field.SetValue(dstComp, field.GetValue(srcComp));
                }
                catch (Exception)
                {
                    Debug.LogWarning($"Failed to write field {field.Name} of {type.Name}. Skipping.");
                }
                
            }
            
            // プロパティをコピーします。
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead && property.CanWrite)
                {
                    try
                    {
                        property.SetValue(dstComp, property.GetValue(srcComp));
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning($"Failed to write property {property.Name} of {type.Name}. Skipping.");
                    }
                    
                }
            }
        }
    }
}