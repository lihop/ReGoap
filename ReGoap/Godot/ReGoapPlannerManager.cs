using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Godot;
using ReGoap.Core;
using ReGoap.Planner;
using ReGoap.Utilities;

namespace ReGoap.Godot
{
	public class ReGoapPlannerManager<T, W> : Node
	{
		[Export]
		public ReGoapLogger.DebugLevel LogLevel { get; set; } = ReGoapLogger.DebugLevel.Full;

		[Export]
		public int NodeWarmupCount { get; set; } = 1000;

		[Export]
		public int StatesWarmupCount { get; set; } = 10000;

		public ReGoapPlannerSettings PlannerSettings { get; } = new ReGoapPlannerSettings();

		private Node threadPool;
		private Dictionary<ulong, PlannerTask> tasks = new Dictionary<ulong, PlannerTask>();

		#region GodotFunctions

		public override void _Ready()
		{
			ReGoapLogger.Level = LogLevel;

			// Ensure that there is only one instance of ReGoapPlannerManager.
			var planner = GetNodeOrNull<ReGoapPlannerManager<T, W>>("/root/ReGoapPlannerManager");
			if (planner != this && planner != null)
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

			// Add ThreadPool from GODOThreadPool plugin.
			var ThreadPool = (GDScript)GD.Load("res://addons/thread_pool/thread_pool.gd");
			threadPool = (Node)ThreadPool.New();
			threadPool.Name = "ThreadPool";
			threadPool.Set("discard_finished_tasks", true);
			threadPool.Connect("task_discarded", this, "OnTaskCompleted");
			AddChild(threadPool);

			ReGoapLogger.Log($"[GoapPlannerManager] Running in multi-thread mode ({OS.GetProcessorCount()} threads)");


			ReGoapNode<T, W>.Warmup(NodeWarmupCount);
			ReGoapState<T, W>.Warmup(StatesWarmupCount);
		}

		public void OnTaskCompleted(object task)
		{
			if (task is Reference t && t.Get("target_instance") is global::Godot.Reference ti)
			{
				var instanceId = ti.GetInstanceId();
				var plannerTask = tasks[instanceId];
				var result = plannerTask.Result;
				var work = result.Work;

				work.NewGoal = result.NewGoal;

				if (work.NewGoal != null && ReGoapLogger.Level == ReGoapLogger.DebugLevel.Full)
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
				finally
				{
					tasks.Remove(instanceId);
				}
			}
		}

		#endregion

		public ReGoapPlanWork<T, W> Plan(IReGoapAgent<T, W> agent, IReGoapGoal<T, W> blacklistGoal, Queue<ReGoapActionState<T, W>> currentPlan, Action<IReGoapGoal<T, W>> callback)
		{
			var work = new ReGoapPlanWork<T, W>(agent, blacklistGoal, currentPlan, callback);
			PlannerTask task = new PlannerTask(PlannerSettings, work);

			var funcRef = new FuncRef();
			funcRef.SetInstance(task);
			funcRef.Function = "Run";

			tasks.Add(funcRef.GetInstanceId(), task);

			threadPool.Call("submit_task_unparameterized", funcRef, "call_func");

			return work;
		}

		public class PlannerTask : Reference
		{
			public TaskResult Result { get; private set; }

			private ReGoapPlanner<T, W> planner;
			private ReGoapPlanWork<T, W> _work;
			private readonly AutoResetEvent signal = new AutoResetEvent(false);

			public PlannerTask(ReGoapPlannerSettings plannerSettings, ReGoapPlanWork<T, W> work)
			{
				planner = new ReGoapPlanner<T, W>(plannerSettings);
				_work = work;
			}

			public void Run()
			{
				try
				{
					planner.Plan(_work.Agent, _work.BlacklistGoal, _work.Actions, (newGoal) => OnDone(this, _work, newGoal));
					signal.WaitOne();
				}
				catch (ObjectDisposedException)
				{
					ReGoapLogger.LogWarning("Objects disposed before planning completed.");
				}
			}

			public void OnDone(PlannerTask task, ReGoapPlanWork<T, W> work, IReGoapGoal<T, W> newGoal)
			{
				Result = new TaskResult(task, work, newGoal);
				signal.Set();
			}

			public struct TaskResult
			{
				public readonly PlannerTask Task;
				public readonly ReGoapPlanWork<T, W> Work;
				public readonly IReGoapGoal<T, W> NewGoal;

				public TaskResult(PlannerTask task, ReGoapPlanWork<T, W> work, IReGoapGoal<T, W> newGoal)
				{
					Task = task;
					Work = work;
					NewGoal = newGoal;
				}
			}
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
			base._Ready();
			PlannerSettings.DebugPlan = DebugPlan;
		}
	}
}





