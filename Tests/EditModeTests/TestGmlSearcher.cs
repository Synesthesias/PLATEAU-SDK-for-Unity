using System.Collections.Generic;
using NUnit.Framework;
using PLATEAU.CityMeta;
using PLATEAU.Editor.CityImport;
using PLATEAU.Tests.TestUtils;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Tests.EditModeTests
{
    [TestFixture]
    public class TestGmlSearcher
    {
        [Test]
        public void AreaIds_Returns_File_AreaIds()
        {
            var searcher = new GmlSearcherModel(DirectoryUtil.TestTokyoSrcPath);
            var areaIds = searcher.AreaIds;
            Assert.Contains(53394525, areaIds, "地域IDを検索して返す");
            Assert.Contains(533925, areaIds);
            Debug.Log(searcher);
        }

        [Test]
        public void When_AreaId_And_Type_Are_All_Set_Then_All_Types_In_TestTokyo_Are_Targeted()
        {
            var presenter = PreparePresenter(DirectoryUtil.TestTokyoSrcPath, out var conf);
            conf.SetAllAreaId(true);
            conf.SetAllTypeTarget(true);
            var gmlFiles = presenter.ListTargetGmlFiles();
            Assert.IsTrue(DoStringsContains("dem", gmlFiles), "gml検索設定ですべてにチェックを入れたとき、テストデータに含まれるタイプはすべて対象となります。");
            Assert.IsTrue(DoStringsContains("lsld", gmlFiles));
            Assert.IsTrue(DoStringsContains("luse", gmlFiles));
            Assert.IsTrue(DoStringsContains("tran", gmlFiles));
            Assert.IsTrue(DoStringsContains("urf", gmlFiles));
            Assert.IsTrue(DoStringsContains("bldg", gmlFiles));
            Assert.IsTrue(DoStringsContains("brid", gmlFiles));
            Assert.IsTrue(DoStringsContains("frn", gmlFiles));
            Assert.IsFalse(DoStringsContains("veg", gmlFiles), "vegはこのテストデータには含まれないので対象としません。");
        }

        [Test]
        public void When_Etc_Type_Is_Not_Targeted_Then_GmlFiles_Do_Not_Contain_Etc()
        {
            var presenter = PreparePresenter(DirectoryUtil.TestTokyoSrcPath, out var conf);
            // Etc タイプ以外は対象とする設定にします。
            conf.SetAllAreaId(true);
            conf.SetAllTypeTarget(true);
            conf.SetIsTypeTarget(GmlType.Etc, false);
            var gmlFiles = presenter.ListTargetGmlFiles();
            Assert.IsTrue(DoStringsContains("bldg", gmlFiles), "その他枠でないものは対象になっています。");
            Assert.IsTrue(DoStringsContains("tran", gmlFiles));
            Assert.IsTrue(DoStringsContains("frn", gmlFiles));
            Assert.IsTrue(DoStringsContains("dem", gmlFiles));
            Assert.IsFalse(DoStringsContains("lsld", gmlFiles), "その他枠のものは対象になっていません。");
            Assert.IsFalse(DoStringsContains("luse", gmlFiles));
            Assert.IsFalse(DoStringsContains("urf", gmlFiles));
            Assert.IsFalse(DoStringsContains("brid", gmlFiles));
        }

        private GmlSearcherPresenter PreparePresenter(string testDataPath, out GmlSearcherConfig conf)
        {
            conf = new GmlSearcherConfig();
            var presenter = new GmlSearcherPresenter(conf);
            presenter.OnImportSrcPathChanged(DirectoryUtil.TestTokyoSrcPath, InputFolderSelectorGUI.PathChangeMethod.Dialogue);
            return presenter;
        }

        /// <summary>
        /// 部分文字列に <paramref name="searchStr"/> を含む文字列が
        /// <paramref name="strCollection"/> の中に1つでも存在すれば true,
        /// そうでなければ false を返します。
        /// </summary>
        private bool DoStringsContains(string searchStr, ICollection<string> strCollection)
        {
            foreach (string s in strCollection)
            {
                if (s.Contains(searchStr)) return true;
            }

            return false;
        }
    }
}