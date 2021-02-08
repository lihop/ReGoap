using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using ReGoap.Core;
using ReGoap.Planner;

namespace ReGoap.Godot
{
    public class ReGoapGoal<T, W> : Node, IReGoapGoal<T, W>
    {
        [Export]
        public float Priority { get; set; } = 1;

        [Export]
        public float ErrorDelay { get; set; } = 0.5f;

        [Export]
        public bool WarnPossibleGoal { get; set; } = true;

        public ReGoapState<T, W> GoalState { get; protected set; }
        public Queue<ReGoapActionState<T, W>> Plan { get; set; }

        protected IGoapPlanner<T, W> planner;

        public ReGoapGoal() => GoalState = ReGoapState<T, W>.Instantiate();

        ~ReGoapGoal() => GoalState.Recycle();

        public void Run(Action<IReGoapGoal<T, W>> callback) { }

        public void Precalculations(IGoapPlanner<T, W> goapPlanner) => planner = goapPlanner;

        public bool IsGoalPossible() => WarnPossibleGoal;
    }

    public class ReGoapGoal : ReGoapGoal<object, object>
    {
        [Export]
        public Dictionary Goal { get; set; } = new Dictionary();

        [Export]
        public bool Interrupts { get; set; } = false;

        [Export]
        public Dictionary OnCompletion { get; set; } = new Dictionary();

        public override void _Ready()
        {
            foreach (var key in Goal.Keys)
            {
                GoalState.Set(key, Goal[key]);
            }
        }
    }
}
