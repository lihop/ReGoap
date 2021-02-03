using System;

namespace ReGoap.Core
{
    public interface IReGoapMemory<T, W>
    {
#pragma warning disable 618 // Supress "Obsolete" warning.
        ReGoapState<T, W> WorldState { get => GetWorldState(); }
#pragma warning disable 618 // Restore "Obsolete" warning.
        [Obsolete("GetWorldState is deprecated. Use the WorldState property instead.")]
        ReGoapState<T, W> GetWorldState() => WorldState;
    }
}