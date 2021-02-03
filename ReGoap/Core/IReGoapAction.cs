using System;
using System.Collections.Generic;

namespace ReGoap.Core
{
	public struct GoapActionStackData<T, W>
	{
		public ReGoapState<T, W> currentState;
		public ReGoapState<T, W> goalState;
		public IReGoapAgent<T, W> agent;
		public IReGoapAction<T, W> next;
		public ReGoapState<T, W> settings;
	}

	public interface IReGoapAction<T, W>
	{
#pragma warning disable 618 // Supress "Obsolete" warning.

		string Name { get => GetName(); }
		float Cost { get; }
		ReGoapState<T, W> Preconditions { get; }
		ReGoapState<T, W> Effects { get; }
		ReGoapState<T, W> Settings { get; }
		IReGoapAgent<T, W> Agent { get; set; }
		bool InterruptWhenPossible { get; set; }
		bool Interruptable { get => IsInterruptable(); }
		bool Active { get => IsActive(); }

#pragma warning disable 618 // Restore "Obsolete" warning.

		// this should return current's action calculated parameter, will be added to the run method
		// userful for dynamic actions, for example a GoTo action can save some informations (wanted position)
		// while being chosen from the planner, we save this information and give it back when we run the method
		// most of actions would return a single item list, but more complex could return many items
		List<ReGoapState<T, W>> GetSettings(GoapActionStackData<T, W> stackData) => new List<ReGoapState<T, W>> { Settings };
		void Run(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction, ReGoapState<T, W> settings, ReGoapState<T, W> goalState, Action<IReGoapAction<T, W>> done, Action<IReGoapAction<T, W>> fail);
		// Called when the action has been added inside a running Plan
		void PlanEnter(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction, ReGoapState<T, W> settings, ReGoapState<T, W> goalState) { }
		// Called when the plan, which had this action, has either failed or completed
		void PlanExit(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction, ReGoapState<T, W> settings, ReGoapState<T, W> goalState) { }
		void Exit(IReGoapAction<T, W> nextAction);

		// MUST BE IMPLEMENTED AS THREAD SAFE
		ReGoapState<T, W> GetPreconditions(GoapActionStackData<T, W> stackData) => Preconditions;
		ReGoapState<T, W> GetEffects(GoapActionStackData<T, W> stackData) => Effects;
		bool CheckProceduralCondition(GoapActionStackData<T, W> stackData) => true;
		float GetCost(GoapActionStackData<T, W> stackData) => Cost;
		// DO NOT CHANGE RUNTIME ACTION VARIABLES, precalculation can be runned many times even while an action is running
		void Precalculations(GoapActionStackData<T, W> stackData) => Agent = stackData.agent;

		string ToString(GoapActionStackData<T, W> stackData)
		{
			string result = $"GoapAction('{Name}')";
			if (stackData.settings != null && stackData.settings.Count > 0)
			{
				result += " - ";
				foreach (var pair in stackData.settings.GetValues())
				{
					result += $"{pair.Key}='{pair.Value}' ; ";
				}
			}
			return result;
		}

		[Obsolete("GetName is deprecated. Use the Name property instead.")]
		string GetName() => Name;

		[Obsolete("IsActive is deprecated. Use the Active property instead.")]
		bool IsActive() => Active;

		[Obsolete("IsInterruptable is deprecated. Use the Interruptable property instead.")]
		bool IsInterruptable() => Interruptable;

		[Obsolete("AskForInterruption is deprecated. Use the InterruptWhenPossible property instead.")]
		void AskForInterruption() => InterruptWhenPossible = true;
	}
}
