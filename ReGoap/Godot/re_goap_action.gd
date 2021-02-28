class_name ReGoapAction, "icon_action.png"
extends Node


const ReGoapActionCS = preload("res://addons/ReGoap/Godot/ReGoapAction.cs")


var wrapped := ReGoapActionCS.new()

func _ready():
	print(wrapped)
