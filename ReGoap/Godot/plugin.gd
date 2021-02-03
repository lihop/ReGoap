tool
extends EditorPlugin


func _enter_tree():
	add_custom_type("ReGoapPlannerManager", "Node", preload("ReGoapPlannerManager.cs"), preload("icon_planner_manager.png"))
	add_custom_type("ReGoapAgent", "Node", preload("ReGoapAgent.cs"), preload("icon_agent.png"))
	add_custom_type("ReGoapMemory", "Node", preload("ReGoapMemory.cs"), preload("icon_memory.png"))
	add_custom_type("ReGoapGoal", "Node", preload("ReGoapGoal.cs"), preload("icon_goal.png"))
	add_custom_type("ReGoapAction", "Node", preload("ReGoapAction.cs"), preload("icon_action.png"))
	add_custom_type("ReGoapSensor", "Node", preload("ReGoapSensor.cs"), preload("icon_sensor.png"))


func _exit_tree():
	remove_custom_type("ReGoapPlannerManager")
	remove_custom_type("ReGoapAgent")
	remove_custom_type("ReGoapMemory")
	remove_custom_type("ReGoapGoal")
	remove_custom_type("ReGoapAction")
	remove_custom_type("ReGoapSensor")
