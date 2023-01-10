using System.Collections.Generic;

namespace PLATEAU.Native
{
    public interface INativeVector<out T> : IEnumerable<T>
    {
        T At(int index);
        int Length { get; }
    }
}
