using System;
using System.Runtime.InteropServices;
using PLATEAU.Interop;
using PLATEAU.Native;
using PLATEAU.Util;

namespace PLATEAU.PolygonMesh
{
    /// <summary>
    /// DLL側で実装された Model を、C# 側の Model としてデータを受け取るためのクラスです。
    /// (<see cref="Node"/>, <see cref="Mesh"/>, <see cref="SubMesh"/> も同様です。)
    /// 
    /// Model は、GMLファイルパーサーから読み取った3Dメッシュ情報を各ゲームエンジンに渡すための中間データ構造として設計されています。
    /// そのデータにはメッシュ、テクスチャパス、ゲームオブジェクトの階層構造が含まれており、
    /// DLLの利用者である Unity や Unreal Engine がメッシュやゲームオブジェクトを生成するために必要な情報が入るよう意図されています。
    /// Model はそのデータ構造の階層のトップに位置します。
    ///
    /// Model クラスのデータを元に、実際にメッシュやゲームオブジェクトを生成するのは
    /// PLATEAU Unity SDK の責務となります。
    ///
    /// 中間データ構造の階層 :
    /// Model -> 所有(0個以上) -> Node(階層構造) -> 所有(0or1個) -> Mesh -> 所有(1個以上) -> SubMesh
    ///
    /// Model が所有する <see cref="Node"/> の階層関係は、ゲームエンジン側でのゲームオブジェクトの階層関係に対応します。
    /// Node が所有する <see cref="Mesh"/> は、そのゲームオブジェクトが保持する3Dメッシュに対応します。
    /// Mesh が所有する <see cref="SubMesh"/> は、そのメッシュのサブメッシュ（テクスチャパスを含む）に対応します。
    ///
    /// DLLから得た Model を delete するのは C# 側の責務なので、
    /// IDisposable を実装して不要時に delete しています。
    /// delete 後は、配下のノードやメッシュにもアクセスできなくなります。
    /// </summary>
    
    public class Model : PInvokeDisposable
    {
        public static Model Create()
        {
            var result = NativeMethods.plateau_create_model(out IntPtr outModelPtr);
            DLLUtil.CheckDllError(result);
            return new Model(outModelPtr);
        }

        public Model(IntPtr handle) : base(handle)
        {
            
        }

        /// <summary>
        /// <see cref="Model"/> の持つ ルート<see cref="Node"/> の数を返します。
        /// </summary>
        public int RootNodesCount
        {
            get
            {
                int childCount = DLLUtil.GetNativeValue<int>(Handle,
                    NativeMethods.plateau_model_get_root_nodes_count);
                return childCount;
            }
        }

        /// <summary>
        /// <paramref name="index"/> 番目の <see cref="Node"/> を返します。
        /// </summary>
        public Node GetRootNodeAt(int index)
        {
            var nodePtr = DLLUtil.GetNativeValue<IntPtr>(
                Handle, index,
                NativeMethods.plateau_model_get_root_node_at_index);
            return new Node(nodePtr);
        }

        /// <summary>
        /// ルートにノードを加えます。
        /// 取扱注意:
        /// C++の std::move によって Node が移動するので、
        /// 実行後は元の node は利用不可になります。
        /// </summary>
        public void AddNodeByCppMove(Node node)
        {
            var result = NativeMethods.plateau_model_add_node_by_std_move(
                Handle, node.Handle);
            DLLUtil.CheckDllError(result);
            node.MarkInvalid();
        }

        protected override void DisposeNative()
        {
            NativeMethods.plateau_delete_model(Handle);
        }

        ~Model()
        {
            Dispose();
        }

        /// <summary>
        /// <see cref="Model"/> 以下の階層構造を文字列で表現します。
        /// </summary>
        public string DebugString()
        {
            var sb = new StringBuilderWithIndent();
            sb.IncrementIndent();
            for (int i = 0; i < RootNodesCount; i++)
            {
                GetRootNodeAt(i).DebugString(sb);
            }
            sb.DecrementIndent();
            return sb.ToString();
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_create_model(
                out IntPtr outModelPtr);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_delete_model(
                [In] IntPtr modelPtr);


            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_model_get_root_nodes_count(
                [In] IntPtr handle,
                out int outCount);

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_model_get_root_node_at_index(
                [In] IntPtr handle,
                out IntPtr outNode,
                int index);
            
            /// <summary>
            /// 注意:
            /// 利用後、元の <see cref="Node"/> は利用不可になります。
            /// </summary>
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult plateau_model_add_node_by_std_move(
                [In] IntPtr modelPtr,
                [In] IntPtr nodePtr);
        }
    }
}
