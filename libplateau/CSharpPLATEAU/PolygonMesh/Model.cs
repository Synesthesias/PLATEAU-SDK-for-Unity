using System;
using PLATEAU.Interop;

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
    
    public class Model : IDisposable
    {
        private readonly IntPtr handle;
        private bool isDisposed;

        public Model()
        {
            var result = NativeMethods.plateau_create_model(out IntPtr outModelPtr);
            DLLUtil.CheckDllError(result);
            this.handle = outModelPtr;
        }
        
        public Model(IntPtr handle)
        {
            this.handle = handle;
        }

        /// <summary>
        /// <see cref="Model"/> の持つ ルート<see cref="Node"/> の数を返します。
        /// </summary>
        public int RootNodesCount
        {
            get
            {
                int childCount = DLLUtil.GetNativeValue<int>(this.handle,
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
                this.handle, index,
                NativeMethods.plateau_model_get_root_node_at_index);
            return new Node(nodePtr);
        }

        public void Dispose()
        {
            if (this.isDisposed) return;
            NativeMethods.plateau_delete_model(this.handle);
            GC.SuppressFinalize(this);
            this.isDisposed = true;
        }

        ~Model()
        {
            Dispose();
        }

        public IntPtr Handle => this.handle;
    }
}
