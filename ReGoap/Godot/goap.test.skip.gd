extends "res://addons/gut/test.gd"


const ReGoapMemory = preload("res://addons/ReGoap/Godot/ReGoapMemory.cs")
const ReGoapAgent = preload("res://addons/ReGoap/Godot/ReGoapAgent.cs")


class TestOrphans:
	extends "res://addons/gut/test.gd"
	
	
	func test_goap_memory_does_not_leave_orphan():
		var memory = add_child_autofree(ReGoapMemory.new())
		print_stray_nodes()
		assert_true(true)


	func test_agent_does_not_leave_orphan():
		var memory = add_child_autofree(ReGoapMemory.new())
		var agent = ReGoapAgent.new()
		agent.CalculateNewGoalOnStart = false
		agent.MemoryNode = memory.get_path()
		agent = add_child_autofree(agent)
		print_stray_nodes()
		assert_true(true)
