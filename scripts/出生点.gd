extends RigidBody2D

signal tick

@onready var area_2d: Area2D = $Area2D

func _tick():
	tick.emit()
	pass

