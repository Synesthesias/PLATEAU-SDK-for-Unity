using System;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Drawer
{
    /// <summary>
    /// 基本の描画オプション
    /// </summary>
    [Serializable]
    public class DrawOption
    {
        public bool visible = true;
        public Color color = Color.white;

        public DrawOption() { }

        public DrawOption(bool visible, Color color)
        {
            this.visible = visible;
            this.color = color;
        }
    }
}