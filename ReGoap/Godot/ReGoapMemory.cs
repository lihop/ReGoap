using Godot;
using ReGoap.Core;

namespace ReGoap.Godot
{
	public class ReGoapMemory<T, W> : Node, IReGoapMemory<T, W>
	{
		public ReGoapState<T, W> WorldState { get; protected set; }

		#region GodotFunctions

		public ReGoapMemory() => WorldState = ReGoapState<T, W>.Instantiate();

		~ReGoapMemory() => WorldState.Recycle();

		#endregion
	}

	public class ReGoapMemory : ReGoapMemory<object, object>
	{
		public void SetState(object key, object val) => WorldState.Set(key, val);
	}
}
