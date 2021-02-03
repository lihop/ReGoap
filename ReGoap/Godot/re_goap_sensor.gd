class_name ReGoapSensor, "icon_sensor.png"
extends Node


const ReGoapMemory = preload("ReGoapMemory.cs")

export(NodePath) var memory_node

onready var _memory: ReGoapMemory = get_node(memory_node)


func set_state(key, value) -> void:
	_memory.SetState(key, value)
