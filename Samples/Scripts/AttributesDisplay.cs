using UnityEngine;
using UnityEngine.UI;

namespace PLATEAU.Samples.Scripts
{
    public class AttributesDisplay : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text attributesText;

        public string TitleText
        {
            set => this.titleText.text = value;
        }

        public string AttributesText
        {
            set => this.attributesText.text = value;
        }

        private void Awake()
        {
            if (this.titleText == null)
            {
                Debug.LogError($"{nameof(this.titleText)} が null です。インスペクタから設定してください。");
            }

            if (this.attributesText == null)
            {
                Debug.LogError($"{nameof(this.attributesText)} が null です。インスペクタから設定してください。");
            }
        }
    }
}
