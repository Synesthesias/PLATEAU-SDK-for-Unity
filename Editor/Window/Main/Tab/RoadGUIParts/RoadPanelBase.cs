using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// 道路調整タブ <see cref="RoadAdjustGui"/> の子タブである「生成」「編集」「追加」に共通する基底クラスです。
    /// 共有インターフェイス、共通処理はここに記載すします。
    /// </summary>
    public abstract class RoadAdjustGuiPartBase
    {
        // 対応するVisualElementのキー
        private readonly string rootKey;
        protected readonly VisualElement rootVisualElement;
        protected VisualElement self { get; private set; }

        protected RoadAdjustGuiPartBase(string name, VisualElement rootVisualElement)
        {
            rootKey = name;
            this.rootVisualElement = rootVisualElement;
            self = GetRoot(rootVisualElement);
            if (self == null)
            {
                Debug.LogError($"{this} : failed to find root visual element.");
            }
        }

        /// <summary>
        /// 「道路調整」タブ内の各子タブ「生成」「編集」「追加」が選択された時に呼ばれます
        /// </summary>
        public void OnRoadChildTabSelected(VisualElement root)
        {
            if (self == null)
            {
                Debug.LogError($"{this} : self is null.");
            }
            else
            {
                self.style.display = DisplayStyle.Flex;
            }
            OnTabSelected(self);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        public void InitUXMLState(VisualElement root)
        {
            var s = GetRoot(root);
            if (s == null)
                return;
            s.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// タブが選択された時の初期化処理をサブクラスで記述できるようにします。
        /// 終了処理がされていない状態での呼び出しも考慮する
        /// </summary>
        protected virtual void OnTabSelected(VisualElement root)
        {
        }

        /// <summary>
        /// 道路調整タブの各子タブ「生成」「編集」「追加」の選択が解除された時に呼ばれます。
        /// 終了処理を行います。
        /// 初期化されていない状態での呼び出しも考慮します。
        /// </summary>
        public void OnRoadChildTabUnselected(VisualElement root)
        {
            if (root.Contains(self) == false)
                return;

            self.style.display = DisplayStyle.None;

            OnTabUnselected();
        }
        
        /// <summary>
        /// タブの選択が解除されたときの終了処理をサブクラスで記述できるようにします。
        /// </summary>
        protected virtual void OnTabUnselected()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        private VisualElement GetRoot(VisualElement root)
        {
            return root.Q<VisualElement>(rootKey);
        }

        /// <summary>
        /// 値を取得する
        /// </summary>
        /// <typeparam name="_Type"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        protected _Type Get<_Type>(string key)
                    where _Type : VisualElement
        {
            var v = self.Q<_Type>(key);
            if (v == null)
            {
                Debug.LogError($"Can't find {key}");
            }
            return v;
        }

        protected IntegerField GetI(string key)
        {
            return Get<IntegerField>(key);
        }

        protected FloatField GetF(string key)
        {
            return Get<FloatField>(key);
        }

        protected Toggle GetT(string key)
        {
            return Get<Toggle>(key);
        }

    }
}