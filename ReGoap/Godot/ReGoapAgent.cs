using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ReGoap.Core;
using ReGoap.Utilities;

namespace ReGoap.Godot
{
    public class ReGoapAgent<T, W> : Node, IReGoapAgent<T, W>
    {
        private static class Time { public static float time => OS.GetTicksMsec() / 1000; }

        [Export]
        public float CalculationDelay { get; set; } = 0.5f;

        [Export]
        public bool BlackListGoalOnFailure { get; set; }

        [Export]
        public bool CalculateNewGoalOnStart = true;

        [Export]
        public NodePath MemoryNode { get; set; }

        protected List<IReGoapGoal<T, W>> goals = new List<IReGoapGoal<T, W>>();

        public List<IReGoapGoal<T, W>> GoalsSet
        {
            get
            {
                if (possibleGoalsDirty)
                    UpdatePossibleGoals();
                return possibleGoals;
            }

            protected set => this.goals = value;
        }

        public IReGoapGoal<T, W> CurrentGoal { get; protected set; }

        public List<IReGoapAction<T, W>> ActionsSet { get; protected set; }

        public bool Active { get => IsProcessing(); }
        public List<ReGoapActionState<T, W>> StartingPlan { get; protected set; }

        protected float lastCalculationTime;

        public IReGoapMemory<T, W> Memory { get; protected set; }

        protected ReGoapActionState<T, W> currentActionState;

        protected Dictionary<IReGoapGoal<T, W>, float> goalBlacklist;
        protected List<IReGoapGoal<T, W>> possibleGoals;
        protected bool possibleGoalsDirty;
        protected Dictionary<T, W> planValues;
        protected bool interruptOnNextTransition;

        protected bool startedPlanning;
        protected ReGoapPlanWork<T, W> currentReGoapPlanWorker;
        public bool IsPlanning { get => startedPlanning && currentReGoapPlanWorker.NewGoal == null; }

        #region GodotFunctions

        public ReGoapAgent()
        {
            lastCalculationTime = -100;
            goalBlacklist = new Dictionary<IReGoapGoal<T, W>, float>();
        }

        public override void _Ready()
        {
            base._Ready();

            RefreshMemory();
            RefreshGoalsSet();
            RefreshActionsSet();

            if (CalculateNewGoalOnStart)
                CalculateNewGoal(true);
        }

        public override void _Notification(int what)
        {
            base._Notification(what);

            if (what == NotificationProcess && !IsProcessing() && currentActionState != null)
            {
                currentActionState.Action.Exit(null);
                currentActionState = null;
                CurrentGoal = null;
            }
        }

        #endregion

        protected virtual void UpdatePossibleGoals()
        {
            possibleGoalsDirty = false;
            if (goalBlacklist.Count > 0)
            {
                possibleGoals = new List<IReGoapGoal<T, W>>(goals.Count);
                foreach (var goal in goals)
                {
                    if (!goalBlacklist.ContainsKey(goal))
                    {
                        possibleGoals.Add(goal);
                    }
                    else if (goalBlacklist[goal] < Time.time)
                    {
                        goalBlacklist.Remove(goal);
                        possibleGoals.Add(goal);
                    }
                }
            }
            else
            {
                possibleGoals = goals;
            }
        }


        protected virtual void TryWarnActionFailure(IReGoapAction<T, W> action)
        {
            if (action.Interruptable)
                WarnActionFailure(action);
            else
                action.InterruptWhenPossible = true;
        }

        public virtual bool CalculateNewGoal(bool forceStart = false)
        {
            if (IsPlanning)
                return false;
            if (!forceStart && (Time.time - lastCalculationTime <= CalculationDelay))
                return false;
            lastCalculationTime = Time.time;

            interruptOnNextTransition = false;
            UpdatePossibleGoals();
            startedPlanning = true;

            currentReGoapPlanWorker = GetNode<ReGoapPlannerManager<T, W>>("/root/ReGoapPlannerManager").Plan(this, BlackListGoalOnFailure ? CurrentGoal : null,
                CurrentGoal != null ? CurrentGoal.Plan : null, OnDonePlanning);

            return true;
        }

        protected virtual void OnDonePlanning(IReGoapGoal<T, W> newGoal)
        {
            startedPlanning = false;
            currentReGoapPlanWorker = default(ReGoapPlanWork<T, W>);
            if (newGoal == null)
            {
                if (CurrentGoal == null)
                    ReGoapLogger.LogWarning($"GoapAgent {this} could not find a plan.");
                return;
            }

            if (currentActionState != null)
                currentActionState.Action.Exit(null);
            currentActionState = null;
            CurrentGoal = newGoal;
            if (StartingPlan != null)
            {
                foreach (var i in StartingPlan.Select((_, i) => i))
                {
                    StartingPlan[i].Action.PlanExit(i > 0 ? StartingPlan[i - 1].Action : null, i + 1 < StartingPlan.Count ? StartingPlan[i + 1].Action : null, StartingPlan[i].Settings, CurrentGoal.GoalState);
                }
            }
            StartingPlan = CurrentGoal.Plan.ToList();
            ClearPlanValues();
            foreach (var i in StartingPlan.Select((_, i) => i))
            {
                StartingPlan[i].Action.PlanEnter(i > 0 ? StartingPlan[i - 1].Action : null, i + 1 < StartingPlan.Count ? StartingPlan[i + 1].Action : null, StartingPlan[i].Settings, CurrentGoal.GoalState);
            }
            CurrentGoal.Run(WarnGoalEnd);
            PushAction();
        }

        public virtual void WarnActionEnd(IReGoapAction<T, W> thisAction)
        {
            if (thisAction != currentActionState.Action)
                return;
            PushAction();
        }

        protected virtual void PushAction()
        {
            if (interruptOnNextTransition)
            {
                CalculateNewGoal();
                return;
            }
            var plan = CurrentGoal.Plan;
            if (plan.Count == 0)
            {
                if (currentActionState != null)
                {
                    currentActionState.Action.Exit(currentActionState.Action);
                    currentActionState = null;
                }
                CalculateNewGoal();
            }
            else
            {
                var previous = currentActionState;
                currentActionState = plan.Dequeue();
                IReGoapAction<T, W> next = null;
                if (plan.Count > 0)
                    next = plan.Peek().Action;
                if (previous != null)
                    previous.Action.Exit(currentActionState.Action);
                currentActionState.Action.Run(previous != null ? previous.Action : null, next, currentActionState.Settings, CurrentGoal.GoalState, WarnActionEnd, WarnActionFailure);
            }
        }

        public virtual void WarnActionFailure(IReGoapAction<T, W> thisAction)
        {
            if (currentActionState != null && thisAction != currentActionState.Action)
            {
                ReGoapLogger.LogWarning($"[GoapAgent] Action {thisAction} warned for failure but is not current action.");
                return;
            }
            if (BlackListGoalOnFailure)
                goalBlacklist[CurrentGoal] = Time.time + CurrentGoal.ErrorDelay;
            CalculateNewGoal(true);
        }

        public virtual void WarnGoalEnd(IReGoapGoal<T, W> goal)
        {
            if (goal != CurrentGoal)
            {
                ReGoapLogger.LogWarning($"[GoapAgent] Goal {goal} warned for end but is not current goal.");
                return;
            }
            CalculateNewGoal();
        }

        public void WarnPossibleGoal(IReGoapGoal<T, W> goal)
        {
            if ((CurrentGoal != null) && (goal.Priority <= CurrentGoal.Priority))
                return;
            if (currentActionState != null && !currentActionState.Action.Interruptable)
            {
                interruptOnNextTransition = true;
                currentActionState.Action.InterruptWhenPossible = true;
            }
            else
                CalculateNewGoal();
        }

        protected virtual void ClearPlanValues()
        {
            if (planValues == null)
                planValues = new Dictionary<T, W>();
            else
            {
                planValues.Clear();
            }
        }

        public bool HasPlanValue(T key) => planValues.ContainsKey(key);

        public W GetPlanValue(T key) => planValues[key];

        public void SetPlanValue(T key, W value) => planValues[key] = value;

        public virtual void RefreshMemory() => Memory = GetNode<IReGoapMemory<T, W>>(MemoryNode);

        public virtual void RefreshGoalsSet()
        {
            goals = new List<IReGoapGoal<T, W>>();

            Action<Node> addGoalsRecursive = null;
            addGoalsRecursive = node =>
            {
                if (node is IReGoapGoal<T, W> goal)
                    goals.Add(goal);

                foreach (var child in node.GetChildren()) addGoalsRecursive((Node)child);
            };
            addGoalsRecursive(this);
        }

        public virtual void RefreshActionsSet()
        {
            ActionsSet = new List<IReGoapAction<T, W>>();

            Action<Node> addActionsRecursive = null;
            addActionsRecursive = node =>
            {
                if (node is IReGoapAction<T, W> action)
                    ActionsSet.Add(action);

                foreach (var child in node.GetChildren()) addActionsRecursive((Node)child);
            };
            addActionsRecursive(this);
        }
    }

    public class ReGoapAgent : ReGoapAgent<object, object>
    {
        [Signal]
        public delegate void PlanningCompleted();

        [Signal]
        public delegate void GoalChanged(ReGoapGoal newGoal);

        [Signal]
        public delegate void PlanChanged(List<ReGoapAction> newPlan);

        protected override void OnDonePlanning(IReGoapGoal<object, object> newGoal)
        {
            List<ReGoapAction> newPlan = null;

            if (newGoal != null)
                newPlan = newGoal.Plan.Select(action => (ReGoapAction)action.Action).ToList<ReGoapAction>();

            var prevGoalHash = GD.Hash(null);
            var prevPlanHash = GD.Hash(null);

            if (CurrentGoal != null)
            {
                prevGoalHash = GD.Hash(CurrentGoal);

                if (CurrentGoal.Plan != null)
                    prevPlanHash = CurrentGoal.Plan.GetHashCode();
            }

            base.OnDonePlanning(newGoal);

            var curGoalHash = GD.Hash(null);
            var curPlanHash = GD.Hash(null);

            if (CurrentGoal != null)
            {
                curGoalHash = GD.Hash(CurrentGoal);

                if (CurrentGoal.Plan != null)
                    curPlanHash = CurrentGoal.Plan.GetHashCode();
            }

            if (prevGoalHash != curGoalHash)
            {
                if (CurrentGoal != null)
                    EmitSignal(nameof(GoalChanged), CurrentGoal);
                else
                    EmitSignal(nameof(GoalChanged), null);
            }

            if (prevPlanHash != curPlanHash)
            {
                if (CurrentGoal != null && CurrentGoal.Plan != null)
                    EmitSignal(nameof(PlanChanged), newPlan);
                else
                    EmitSignal(nameof(PlanChanged), null);
            }

            EmitSignal(nameof(PlanningCompleted));
        }
    }
}
