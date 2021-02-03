using System;
using System.Collections.Generic;
using Godot;
using ReGoap.Core;
using ReGoap.Planner;

namespace ReGoap.Godot
{
    public class ReGoapGoal : Reference, IReGoapGoal<object, object>
    {
        public string Name { get; set; } = "GenericGoal";
        public float Priority { get; set; } = 1;
        public float ErrorDelay { get; set; } = 0.5f;
        public bool WarnPossibleGoal { get; set; } = true;

        public ReGoapState<object, object> Goal { get; protected set; }
        public Queue<ReGoapActionState<object, object>> Plan { get; protected set; }

        protected IGoapPlanner<object, object> planner;

        public ReGoapGoal() => Goal = ReGoapState<object, object>.Instantiate();

        ~ReGoapGoal() => Goal.Recycle();

        public void Run(Action<IReGoapGoal<object, object>> callback) { }

        public void Precalculations(IGoapPlanner<object, object> goapPlanner) => planner = goapPlanner;

        public bool IsGoalPossible() => WarnPossibleGoal;
    }
}
