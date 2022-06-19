using System.Runtime.CompilerServices;

// このアセンブリの設定です。

// internal を Editor から利用できるようにします。
[assembly: InternalsVisibleTo("PlateauUnitySDK.Editor")]

// internal をテストから利用できるようにします。
[assembly: InternalsVisibleTo("PlateauUnitySDK.EditModeTests")]
[assembly: InternalsVisibleTo("PlateauUnitySDK.TestUtils")]