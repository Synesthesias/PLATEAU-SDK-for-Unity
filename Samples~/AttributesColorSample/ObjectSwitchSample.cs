using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace PLATEAU
{
    /// <summary>
    /// 表示するゲームオブジェクトを切り替えるシンプルなスクリプトです。
    /// </summary>
    public class ObjectSwitchSample : MonoBehaviour
    {
        [SerializeField] private GameObject[] urbanPlanningObjects;
        [SerializeField] private GameObject[] floodObjects;

        private void Start()
        {
            DisplayLandUse();
        }

        public void DisplayLandUse()
        {
            Switch(true);
        }

        public void DisplayFlood()
        {
            Switch(false);
        }

        private void Switch(bool mode)
        {
            foreach (var land in urbanPlanningObjects)
            {
                land.SetActive(mode);
            }

            foreach (var flood in floodObjects)
            {
                flood.SetActive(!mode);
            }
        }
    }
}
