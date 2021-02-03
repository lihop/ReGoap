using ReGoap.Core;
using Godot;

namespace ReGoap.Godot
{
	public class ReGoapSensor<T, W> : Node, IReGoapSensor<T, W>
	{
		[Export]
		public NodePath MemoryNode { get; set; }

		public IReGoapMemory<T, W> Memory { get; protected set; }

		#region GodotFunctions

		public override void _Ready() => Init(GetNodeOrNull<IReGoapMemory<T, W>>(MemoryNode));

		#endregion

		public virtual void Init(IReGoapMemory<T, W> memory)
		{
			Memory = memory;
		}

		public virtual void UpdateSensor()
		{
		}
	}

	public class ReGoapSensor : ReGoapSensor<object, object>
	{
	}
}
