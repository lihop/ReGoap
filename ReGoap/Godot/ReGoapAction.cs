using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using ReGoap.Core;

namespace ReGoap.Godot
{
	public class ReGoapAction<T, W> : Node, IReGoapAction<T, W>
	{
		[Export]
		public float Cost { get; protected set; } = 1;

		[Export]
		public Dictionary Effect { get; set; } = new Dictionary();

		[Export]
		public Dictionary Required { get; set; } = new Dictionary();

		public ReGoapState<T, W> Preconditions { get; protected set; } = ReGoapState<T, W>.Instantiate();
		public ReGoapState<T, W> Effects { get; protected set; } = ReGoapState<T, W>.Instantiate();
		public ReGoapState<T, W> Settings { get; protected set; } = ReGoapState<T, W>.Instantiate();

		public IReGoapAgent<T, W> Agent { get; set; }
		public bool Interruptable { get; } = true;
		public bool InterruptWhenPossible { get; set; }

		public bool Active { get => IsProcessing() || IsPhysicsProcessing(); }

		protected Action<IReGoapAction<T, W>> doneCallback;
		protected Action<IReGoapAction<T, W>> failCallback;
		protected IReGoapAction<T, W> previousAction;
		protected IReGoapAction<T, W> nextAction;

		protected ReGoapState<T, W> settings = null;

		#region GodotFunctions

		public override void _Ready()
		{
			SetProcess(false);
			SetPhysicsProcess(false);

			settings = ReGoapState<T, W>.Instantiate();
		}

		#endregion

		public void Exit(IReGoapAction<T, W> nextAction)
		{
			if (!IsInsideTree())
			{
				SetProcess(false);
				SetPhysicsProcess(false);
			}
		}

		public virtual List<ReGoapState<T, W>> GetSettings(GoapActionStackData<T, W> stackData) => new List<ReGoapState<T, W>> { settings };

		public virtual float GetCost(GoapActionStackData<T, W> stackData) => Cost;

		public virtual void Run(IReGoapAction<T, W> previous, IReGoapAction<T, W> next, ReGoapState<T, W> settings,
			ReGoapState<T, W> goalState, Action<IReGoapAction<T, W>> done, Action<IReGoapAction<T, W>> fail)
		{
			InterruptWhenPossible = false;
			this.settings = settings;

			doneCallback = done;
			failCallback = fail;

			previousAction = previous;
			nextAction = next;

			SetProcess(true);
			SetPhysicsProcess(true);
		}

		public override string ToString() => $"GoapAction('{Name}')";
	}

	public class ReGoapAction : ReGoapAction<object, object>
	{
		public override void _Ready()
		{
			base._Ready();

			foreach (var key in Effect.Keys)
			{
				Effects.Set(key, Effect[key]);
			}

			foreach (var key in Required.Keys)
			{
				Preconditions.Set(key, Required[key]);
			}
		}
	}

}
