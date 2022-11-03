using System.Runtime.CompilerServices;

// このアセンブリの設定です。

// internal を Editor から利用できるようにします。
[assembly: InternalsVisibleTo("PLATEAU.Editor")]

// internal をテストから利用できるようにします。
[assembly: InternalsVisibleTo("PLATEAU.EditModeTests")]
[assembly: InternalsVisibleTo("PLATEAU.PlayModeTests")]
[assembly: InternalsVisibleTo("PLATEAU.TestUtils")]