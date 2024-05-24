using PLATEAU.CityInfo;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGUI.Parts
{
    /// <summary>
    /// 属性情報のキーについて、指定オブジェクトで存在するものを列挙し、
    /// その中から1つ選択するGUIです。
    /// </summary>
    internal class AttrKeySelectGui : IEditorDrawable
    {
        private List<string> keys;
        private int selectedIndex;
        private EditorWindow mainWindow;
        private EditorWindow attrWindow;
        private IAttrKeySelectResultReceiver resultReceiver;
        private EnterAndCancelButtonElement enterCancelButton;
        
        public AttrKeySelectGui(UniqueParentTransformList searchTarget, IAttrKeySelectResultReceiver resultReceiver, EditorWindow mainWindow, EditorWindow attrWindow)
        {
           keys = FindAllKeys(searchTarget).ToList();
           keys.Sort();
           this.resultReceiver = resultReceiver;
           this.mainWindow = mainWindow;
           this.attrWindow = attrWindow;
           
           enterCancelButton = new EnterAndCancelButtonElement("",
               () =>
               {
                   this.resultReceiver.ReceiveAttrKeySelectResult(keys[selectedIndex]);
                   this.mainWindow.Repaint();
                   this.attrWindow.Close();
               },
               () =>
               {
                   this.attrWindow.Close();
               });
        }
        
        public void Draw()
        {
            EditorGUILayout.LabelField("属性情報のキーを1つ選択してください");
            for (int i = 0; i < keys.Count; i++)
            {
                if (EditorGUILayout.ToggleLeft(keys[i], selectedIndex == i))
                {
                    selectedIndex = i;
                }
            }
            
            enterCancelButton.Draw();
        }

        private HashSet<string> FindAllKeys(UniqueParentTransformList searchTargets)
        {
            var retKeys = new HashSet<string>(); 
            foreach (var target in searchTargets.Get)
            {
                var cogs = target.GetComponentsInChildren<PLATEAUCityObjectGroup>();
            
                foreach (var cog in cogs)
                {
                    foreach (var co in cog.GetAllCityObjects())
                    {
                        foreach (var k in FindAllKeys(co))
                        {
                            retKeys.Add(k);
                        }
                    }
                }
            }

            return retKeys;
        }

        private HashSet<string> FindAllKeys(CityObjectList.CityObject cityObj)
        {
            var retKeys = new HashSet<string>();
            FindAllKeysRecursive(cityObj.AttributesMap, retKeys, "");
            return retKeys;
        }
        
        private void FindAllKeysRecursive(CityObjectList.Attributes attrs, HashSet<string> outKeys, string parentKey)
        {
            foreach (var (key, value) in attrs)
            {
                if (!string.IsNullOrEmpty(value.StringValue))
                {
                    string slashedKey = parentKey == "" ? key : $"{parentKey}/{key}";
                    outKeys.Add(slashedKey);
                }
                else
                {
                    var childAttr = value.AttributesMapValue;
                    if (childAttr != null)
                    {
                        FindAllKeysRecursive(childAttr, outKeys, key);
                    }
                }
            }
        }

        public void Dispose()
        {
            
        }
    }
}