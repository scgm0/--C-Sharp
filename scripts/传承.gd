extends Area2D

var duration: float = 30
var 修为: float
var 资质: float
var 计时: float = 1.0:
	set(value):
		计时 = value
		$MeshInstance2D2.material.set_shader_parameter('fill_ratio', 计时)

var 身份: StringName:
	set(value):
		身份 = value
		modulate = GlData.阵营[身份]

func _ready() -> void:
	var tween = create_tween()
	tween.tween_property(self, "计时", 0.0, duration)
	tween.tween_callback(func():
		queue_free()
		$CollisionShape2D.set_deferred("disabled", true)
	)


func _on_body_entered(body: Node2D) -> void:
	if body is Ball and body.身份 == 身份:
		body = body as Ball
		body.资质事件.emit(body, 资质)
		body.修为事件.emit(body, 修为 * 0.25)
		body.生命事件.emit(body, ceil(body.最大生命 * 0.1))
		queue_free()
		$CollisionShape2D.set_deferred("disabled", true)
	pass # Replace with function body.
