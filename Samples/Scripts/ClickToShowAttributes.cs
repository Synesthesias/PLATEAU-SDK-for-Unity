using System.IO;
using System.Threading.Tasks;
using PlasticPipe.PlasticProtocol.Messages;
using PLATEAU.CityInfo;
using PLATEAU.Util.Async;
using UnityEngine;

namespace PLATEAU.Samples.Scripts
{
    /// <summary>
    /// クリック位置にレイキャストして、当たった都市オブジェクトの属性を Canvas に表示します。
    ///
    /// 前提:
    /// ・ゲームオブジェクトの名前が都市オブジェクトのIDになっていること。
    ///     ・インポート時、メッシュ結合単位が "最小地物単位" または "主要地物単位" であればこの条件を満たします。
    ///     ・インポート時、メッシュ結合単位が "地域単位" であればこの条件を満たしません。
    /// ・都市の各ゲームオブジェクトに MeshCollider がアタッチされていること。
    ///     ・インポート時に "MeshColliderをアタッチする" のオプションがあります。
    ///     ・そのオプションをオフにしていたとしても、あとから手動で MeshCollider をアタッチすればOKです。
    /// ・ゲームオブジェクトの階層構造をインポート時から変えていないこと
    /// </summary>
    public class ClickToShowAttributes : MonoBehaviour
    {
        [SerializeField] private AttributesDisplay display;
        private Camera mainCamera;

        private static readonly string cityDataPath =
            Path.GetFullPath("Packages/com.synesthesias.plateau-unity-sdk/Samples/");

        private void Start()
        {
            this.mainCamera = Camera.main;
            if (this.mainCamera == null)
            {
                Debug.LogError("メインカメラがありません。");
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) // 左クリックで---
            {
                var ray = this.mainCamera.ScreenPointToRay(Input.mousePosition); // クリック位置に---
                if (Physics.Raycast(ray, out var hit)) // レイキャストして当たったとき---
                {
                    var task = GetAttributesAndShowAsync(hit.transform); // その属性を取得して表示します。 
                    task.ContinueWith(t =>
                        {
                            if (!t.Result) NotifyLoadFailure(); // 失敗時は "読込失敗" と表示します。
                        }, TaskScheduler.FromCurrentSynchronizationContext())
                        .ContinueWithErrorCatch();
                }
            }
        }

        /// <summary>
        /// 都市オブジェクトの属性を取得して表示します。
        /// インポート時に生成されるゲームオブジェクト名と階層構造は変えていないことが前提です。
        /// </summary>
        /// <returns>成否をboolで返します。</returns>
        private async Task<bool> GetAttributesAndShowAsync(Transform trans)
        {
            // インポートすると、ゲームオブジェクトの階層構造は次のようになっているはずです。(矢印の先が親)
            // ルートフォルダ名 <- GMLファイル名  <- LOD番号 <- 都市オブジェクトID
            
            // ここで名前が (都市オブジェクトID) である Transform が引数に渡されるので、
            // GMLファイル名をその 2つ親 から取得し、ルートフォルダ名を 3つ親 から取得します。
            
            if (trans == null || trans.parent == null || trans.parent.parent == null || trans.parent.parent.parent == null){return false;}
            string cityObjID = trans.name; // 例 : "BLD_1532c5ce-c78d-44d3-8d73-9f7b6b81cfcc"
            var gmlTrans = trans.parent.parent; // 例 : "53391540_bldg_6697_op.gml"
            string rootDirName = gmlTrans.parent.name; // 例 : "SampleDataMinatoMirai~"

            this.display.AttributesText = "読込中...";
            
            // GMLファイルをパースします。
            // パースした結果は CityModel 型で返されます。
            // パースは重い処理ですが、結果はキャッシュに入るので2回目以降は速いです。
            // 3つ目の引数を省略すると、ローカルインポート時に自動でコピーされるパスになります。今回はサンプル用のデータを読みたいので指定します。
            var cityModel = await PLATEAUCityGmlProxy.LoadAsync(gmlTrans.gameObject, rootDirName, cityDataPath);
            
            if (cityModel == null) return false;
            
            // CityModel の中では、 CityObject が階層構造になっています。
            // クリックされた CityObject を ID をキーとして取得します。
            var cityObj = cityModel.GetCityObjectById(cityObjID);

            if (cityObj == null) return false;
            this.display.TitleText = cityObjID;
            
            // CityObject の中に属性情報が含まれます。
            // AttributesMap は属性の辞書であり、キーと値の集合です。
            // 属性は入れ子構造、すなわち AttributesMap の中に AttributesMap がある場合があります。
            // AttributesMap.ToString() を実行すると、入れ子構造の子まで含めて再帰的に属性の内容を文字列にします。
            this.display.AttributesText = cityObj.AttributesMap.ToString();
            return true;
        }

        private void NotifyLoadFailure()
        {
            this.display.TitleText = "";
            this.display.AttributesText = "読込失敗";
        }

        private void Awake()
        {
            if (this.display == null)
            {
                Debug.LogError($"{nameof(AttributesDisplay)} が null です。インスペクタから設定してください。");
            }
        }
    }
}

