using PLATEAU.CityAdjust.AlignLand;
using PLATEAU.CityInfo;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab.AdjustGuiParts;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System.Linq;

namespace PLATEAU.Editor.Window.Main.Tab.AlignLandGui
{
    public class AlignLandGui : ITabContent
    {
        private ElementGroup guis;
        private ObjectFieldElement<PLATEAUInstancedCityModel> TargetField => guis.Get<ObjectFieldElement<PLATEAUInstancedCityModel>>("targetField");
        private ButtonElement SearchButton => guis.Get<ButtonElement>("searchButton");
        private MultiPackageSelectElement PackageSelect => guis.Get<MultiPackageSelectElement>("packageSelect");
        private ALTargetSearcher searcher;

        public AlignLandGui()
        {
            // ここに必要なGUIを列挙します。
            guis =
                new ElementGroup("root",
                    new HeaderElementGroup("", "平面モデルの高さを地形に合わせます", HeaderType.Subtitle),
                    new HeaderElementGroup("", "対象選択", HeaderType.Header,
                        new ObjectFieldElement<PLATEAUInstancedCityModel>("targetField", "対象の都市モデル",
                            OnTargetChanged),
                        new ButtonElement("searchButton", "検索", OnSearchButtonPushed)
                    ),
                    new ElementGroup("afterSearch",
                        new MultiPackageSelectElement("packageSelect", "高さ合わせする対象を選択してください", true),
                        new HeaderElementGroup("", "一般設定", HeaderType.Header,
                            new DestroyOrPreserveSrcGui()
                        ),
                        new HeaderElementGroup("", "実行", HeaderType.Header,
                            new ButtonElement("execButton", "実行", OnExecButtonPushed
                            )
                        )
                    )
                );
            SwitchVisible();
        }
        
        public void Draw()
        {
            guis.Draw();   
        }

        /// <summary> 条件に応じて表示/非表示を切り替えたいGUIを記述します。 </summary>
        private void SwitchVisible()
        {
            SearchButton.IsVisible = TargetField.SelectedObject != null;
            guis.Get("afterSearch").IsVisible = PackageSelect.Choices.Count > 0;
        }

        private void OnSearchButtonPushed()
        {
            var targetModel = TargetField.SelectedObject;
            searcher = new ALTargetSearcher(targetModel);
            if (!searcher.IsValid())
            {
                Dialogue.Display("高さ合わせに適した対象が見つかりませんでした。", "OK");
                return;
            }
            PackageSelect.SetChoices(searcher.TargetPackages);
            SwitchVisible();
        }
        
        private void OnTargetChanged(PLATEAUInstancedCityModel nextTarget)
        {
            SwitchVisible();
        }

        private void OnExecButtonPushed()
        {
            var conf = searcher.ToConfig();
            conf.TargetPackages = PackageSelect.SelectedPackages;
            new AlignLandExecutor().ExecAsync(conf).ContinueWithErrorCatch();   
        }

        public void OnTabUnselect()
        {
        }
        
        public void Dispose()
        {
        }
    }
}