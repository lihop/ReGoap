using System;
using System.Collections.Generic;
using System.Linq;
using ReGoap.Planner;

namespace ReGoap.Core
{
	public interface IReGoapGoal<T, W>
	{
		void Run(Action<IReGoapGoal<T, W>> callback);

#pragma warning disable 618 // Supress "Obsolete" warning.

		// THREAD SAFE METHODS/PROPERTIES (cannot use any unity library!)
		string Name { get => GetName(); }
		Queue<ReGoapActionState<T, W>> Plan { get => GetPlan(); set => SetPlan(value); }
		ReGoapState<T, W> GoalState { get => GetGoalState(); }
		float Priority { get => GetPriority(); }
		float ErrorDelay { get => GetErrorDelay(); }

#pragma warning restore 618 // Restore "Obsolete" warning.

		void Precalculations(IGoapPlanner<T, W> goapPlanner);
		bool IsGoalPossible();

		static string PlanToString(IEnumerable<IReGoapAction<T, W>> plan)
		{
			var result = "GoapPlan(";
			var reGoapActions = plan as IReGoapAction<T, W>[] ?? plan.ToArray();
			var end = reGoapActions.Length;
			for (var index = 0; index < end; index++)
			{
				var action = reGoapActions[index];
				result += $"'{action}'{(index + 1 < end ? ", " : "")}";
			}
			result += ")";
			return result;
		}

		[Obsolete("GetPlan is deprecated. Use the Plan property instead.")]
		Queue<ReGoapActionState<T, W>> GetPlan() => Plan;

		[Obsolete("GetName is deprecated. Use the Name property instead.")]
		string GetName() => Name;

		[Obsolete("GetGoalState is deprecated. Use the GoalState property instead.")]
		ReGoapState<T, W> GetGoalState() => GoalState;

		[Obsolete("GetPriority is deprecated. Use the Priority property instead.")]
		float GetPriority() => Priority;

		[Obsolete("SetPlan is deprecated. Use the Plan property instead.")]
		void SetPlan(Queue<ReGoapActionState<T, W>> path) => Plan = path;

		[Obsolete("GetErrorDelay is deprecated. Use the ErrorDelay property instead.")]
		float GetErrorDelay() => ErrorDelay;
	}
}
