using System;
using System.Collections.Generic;
using System.Linq;

namespace ReGoap.Core
{
    public interface IReGoapAgent<T, W>
    {
#pragma warning disable 618 // Supress "Obsolete" warning.

        string Name { get; }
        IReGoapMemory<T, W> Memory { get => GetMemory(); }
        IReGoapGoal<T, W> CurrentGoal { get => GetCurrentGoal(); }
        bool Active { get => IsActive(); }
        List<ReGoapActionState<T, W>> StartingPlan { get => GetStartingPlan(); }

        // THREAD SAFE
        List<IReGoapGoal<T, W>> GoalsSet { get => GetGoalsSet(); }
        List<IReGoapAction<T, W>> ActionsSet { get => GetActionsSet(); }

#pragma warning restore 618 // Restore "Obsolete" warning.

        W GetPlanValue(T key);
        void SetPlanValue(T key, W value);
        bool HasPlanValue(T target);

        // called from a goal when the goal is available
        void WarnPossibleGoal(IReGoapGoal<T, W> goal);

        ReGoapState<T, W> InstantiateNewState() => ReGoapState<T, W>.Instantiate();

        static string PlanToString(IEnumerable<IReGoapAction<T, W>> plan)
        {
            var result = "GoapPlan(";
            var reGoapActions = plan as IReGoapAction<T, W>[] ?? plan.ToArray();
            for (var index = 0; index < reGoapActions.Length; index++)
            {
                var action = reGoapActions[index];
                result += $"'{action}'{(index + 1 < reGoapActions.Length ? ", " : "")}";
            }
            result += ")";
            return result;
        }

        string ToString() => $"GoapAgent('{Name}')";

        [Obsolete("GetMemory is deprecated. Use the Memory property instead.")]
        IReGoapMemory<T, W> GetMemory() => Memory;

        [Obsolete("GetCurrentGoal is deprecated. Use the CurrentGoal property instead.")]
        IReGoapGoal<T, W> GetCurrentGoal() => CurrentGoal;

        [Obsolete("IsActive is deprecated. Use the Active property instead.")]
        bool IsActive() => Active;

        [Obsolete("GetStartingPlan is deprecated. Use the StartingPlan property instead.")]
        List<ReGoapActionState<T, W>> GetStartingPlan() => StartingPlan;

        [Obsolete("GetGoalsSet is deprecated. Use the GoalsSet property instead.")]
        List<IReGoapGoal<T, W>> GetGoalsSet() => GoalsSet;

        [Obsolete("GetActionsSet is deprecated. Use the ActionsSet property instead.")]
        List<IReGoapAction<T, W>> GetActionsSet() => ActionsSet;
    }
}
