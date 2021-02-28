using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ReGoap.Core;
using ReGoap.Planner;
using ReGoap.Utilities;

namespace ReGoap.Godot
{
	public class ReGoapPlannerManager<T, W> : Node
	{
		[Export]
		public ReGoapLogger.DebugLevel LogLevel { get; set; } = ReGoapLogger.DebugLevel.Info;

		[Export]
		public int NodeWarmupCount { get; set; } = 1000;

		[Export]
		public int StatesWarmupCount { get; set; } = 10000;

		public ReGoapPlannerSettings PlannerSettings { get; } = new ReGoapPlannerSettings();

		private ReGoapPlanner<T, W> planner;
		private Queue<ReGoapPlanWork<T, W>> works = new Queue<ReGoapPlanWork<T, W>>();


		#region GodotFunctions

		public override void _Ready()
		{
			ReGoapLogger.Level = LogLevel;

			// Ensure that there is only one instance of ReGoapPlannerManager.
			var plannerManager = GetNodeOrNull<ReGoapPlannerManager<T, W>>("/root/ReGoapPlannerManager");
			if (plannerManager != this && plannerManager != null)
			{
				GD.PushError("Found an existing ReGoapPlannerManager in the scene tree");
				QueueFree();
				return;
			}

			// Ensure that we are an AutoLoad named 'ReGoapPlannerManager'.
			if (GetPath() != "/root/ReGoapPlannerManager" || this == GetTree().CurrentScene)
			{
				GD.PushError("ReGoapPlannerManager must be an AutoLoad named 'ReGoapPlannerManager'");
				QueueFree();
				return;
			}

			ReGoapLogger.Log($"[GoapPlannerManager] Running in single-thread mode.");

			ReGoapNode<T, W>.Warmup(NodeWarmupCount);
			ReGoapState<T, W>.Warmup(StatesWarmupCount);

			planner = new ReGoapPlanner<T, W>(PlannerSettings);
		}

		public override void _Process(float _delta)
		{
			ReGoapPlanWork<T, W> work;
			if (works.TryDequeue(out work))
			{
				planner.Plan(work.Agent, work.BlacklistGoal, work.Actions, newGoal =>
				{
					work.NewGoal = newGoal;

					if (work.NewGoal != null && ReGoapLogger.Level == ReGoapLogger.DebugLevel.Info)
					{
						ReGoapLogger.Log("[GoapPlannerManager] Done calculating plan, actions list:");
						foreach (var item in work.NewGoal.Plan.Select((action, i) => new { i, action }))
						{
							ReGoapLogger.Log($"{item.i}: {item.action.Action}");
						}
					}

					try
					{
						work.Callback(work.NewGoal);
					}
					catch (Exception ex)
					{
						GD.PushError($"Work callback failed: {ex.Message}");
					}
				});
			}
		}

		#endregion

		public ReGoapPlanWork<T, W> Plan(IReGoapAgent<T, W> agent, IReGoapGoal<T, W> blacklistGoal, Queue<ReGoapActionState<T, W>> currentPlan, Action<IReGoapGoal<T, W>> callback)
		{
			var work = new ReGoapPlanWork<T, W>(agent, blacklistGoal, currentPlan, callback);

			lock (works)
				works.Enqueue(work);

			return work;
		}
	}

	public struct ReGoapPlanWork<T, W>
	{
		public readonly IReGoapAgent<T, W> Agent;
		public readonly IReGoapGoal<T, W> BlacklistGoal;
		public readonly Queue<ReGoapActionState<T, W>> Actions;
		public readonly Action<IReGoapGoal<T, W>> Callback;

		public IReGoapGoal<T, W> NewGoal;

		public ReGoapPlanWork(IReGoapAgent<T, W> agent, IReGoapGoal<T, W> blacklistGoal, Queue<ReGoapActionState<T, W>> actions, Action<IReGoapGoal<T, W>> callback) : this()
		{
			Agent = agent;
			BlacklistGoal = blacklistGoal;
			Actions = actions;
			Callback = callback;
		}
	}

	public class ReGoapPlannerManager : ReGoapPlannerManager<object, object>
	{
		[Export]
		public bool DebugPlan { get; set; } = false;

		public override void _Ready()
		{
			PlannerSettings.DebugPlan = DebugPlan;
			base._Ready();
		}
	}
}





