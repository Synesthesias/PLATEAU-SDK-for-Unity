using System.Collections.Generic;

namespace PLATEAU.Interop
{
    public interface INativeVector<out T> : IEnumerable<T>
    {
        T At(int index);
        int Length { get; }
    }
}
