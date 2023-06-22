extends Node2D

@onready var 日志: RichTextLabel = %"日志"
@onready var 境界榜: VBoxContainer = %"境界榜"
@onready var 杀戮榜: VBoxContainer = %"杀戮榜"
@onready var 地图: CanvasGroup = %"地图"

@onready var TEST = preload("res://scenes/test.tscn")
@onready var PAR = preload("res://scenes/particles.tscn")
@onready var TIP = preload("res://scenes/tip.tscn")
@onready var BALL = preload("res://scenes/ball.tscn")
@onready var 年月: Label = $"CanvasLayer/年月"
@onready var 红出生点: RigidBody2D = %"红出生点"
@onready var 黄出生点: RigidBody2D = %"黄出生点"
@onready var 蓝出生点: RigidBody2D = %"蓝出生点"
@onready var 绿出生点: RigidBody2D = %"绿出生点"

var 月: int = 1
var 年: int = 0
var 限时: int = 100 * 10

func _ready() -> void:
	GlData.Log.connect(log)
	红出生点.modulate = GlData.阵营.红
	黄出生点.modulate = GlData.阵营.黄
	蓝出生点.modulate = GlData.阵营.蓝
	绿出生点.modulate = GlData.阵营.绿
	$CanvasLayer2/ColorRect/Label.text = "限时{0}年，修为最高者所在的阵营获胜".format([限时])
	var tween = create_tween()
	tween.tween_interval(2)
	tween.tween_property($CanvasLayer2/ColorRect/Label, "modulate:a", 0, 0.5)
	tween.tween_callback(func():
		$CanvasLayer2/ColorRect.visible = false
		红出生点.freeze = false
		黄出生点.freeze = false
		蓝出生点.freeze = false
		绿出生点.freeze = false

		红出生点.tick.connect(func():
			出生("红", 红出生点.global_position)
		)
		黄出生点.tick.connect(func():
			出生("黄", 黄出生点.global_position)
		)
		蓝出生点.tick.connect(func():
			出生("蓝", 蓝出生点.global_position)
		)
		绿出生点.tick.connect(func():
			出生("绿", 绿出生点.global_position)
		)

#		红出生点.area_2d.body_entered.connect(func(body):
#			if body is Ball and body.身份 != "红":
#				body.apply_central_force(-(红出生点.global_position - body.global_position) * 25 * body.mass)
#		)
#		黄出生点.area_2d.body_entered.connect(func(body):
#			if body is Ball and body.身份 != "黄":
#				body.apply_central_force(-(黄出生点.global_position - body.global_position) * 25 * body.mass)
#		)
#		蓝出生点.area_2d.body_entered.connect(func(body):
#			if body is Ball and body.身份 != "蓝":
#				body.apply_central_force(-(蓝出生点.global_position - body.global_position) * 25 * body.mass)
#		)
#		绿出生点.area_2d.body_entered.connect(func(body):
#			if body is Ball and body.身份 != "绿":
#				body.apply_central_force(-(绿出生点.global_position - body.global_position) * 25 * body.mass)
#		)

		红出生点.get_node("AnimationPlayer").play("闪烁")
		黄出生点.get_node("AnimationPlayer").play("闪烁")
		蓝出生点.get_node("AnimationPlayer").play("闪烁")
		绿出生点.get_node("AnimationPlayer").play("闪烁")


		$Timer.start()
	)
	pass

func 出生(身份, pos):
	var ball = BALL.instantiate() as Ball
	ball.身份 = 身份
	ball.global_position = pos
	ball.linear_velocity = (Vector2(randf_range(-10, 10), randf_range(-10, 10)).normalized() * 25 * 1)
	ball.死亡事件.connect(func(b, v):
		var par = PAR.instantiate()
		par.emitting = true
		par.modulate = b.body.modulate
		par.global_position = b.global_position
		地图.add_child(par)
		GlData.log("[color=%s][font_size=25]%s[/font_size][/color]%s" % [GlData.阵营[b.身份].to_html(), "%s(%s)" % [b.名字, b.境界名], " {0}：享年{1}岁\n".format([v, b.年龄])])
	)
	ball.突破事件.connect(func(b, v):
		GlData.log("[color=%s][font_size=25]%s[/font_size][/color]%s" % [GlData.阵营[b.身份].to_html(), b.名字, " 突破了 {0}：寿命 {1}  资质 {2}  生命 {3}  修为{4}\n".format(["[color=%s][font_size=25]%s[/font_size][/color]" % [GlData.阵营[b.身份].to_html(), v], "{0}/{1}".format([b.年龄, b.寿命]), b.资质, "{0}/{1}".format([b.生命, b.最大生命]), "{0}/{1}".format([b.修为, b.修为上限])])])
	)
	地图.add_child(ball)
	ball.境界 = GlData.境界体系.武徒
	ball.年龄 = 0

func _on_timer_timeout() -> void:
	月 += 1
	get_tree().call_group("武者", "过月")
	if 月 > 12:
		月 = 1
		年 += 1
		get_tree().call_group("武者", "过年")
		if 年 > 限时:
			红出生点.get_node("AnimationPlayer").stop()
			黄出生点.get_node("AnimationPlayer").stop()
			蓝出生点.get_node("AnimationPlayer").stop()
			绿出生点.get_node("AnimationPlayer").stop()
			$Timer.stop()
			var arr = get_tree().get_nodes_in_group("武者").filter(func(b): return !b.已死)
			arr.sort_custom(func(a, b):
				return true if a.境界 > b.境界 else (\
					(true if a.修为 >= b.修为 else false)\
					if a.境界 == b.境界 else false)
			)
			var one = arr[0]
			for b in arr:
				b = b as Ball
				if b.身份 != one.身份:
					b.死()
				else:
					b.sleeping = true
					b.freeze = true
			await get_tree().create_timer(1.5).timeout
			结束(one)
			if OS.has_feature("movie"):
				await get_tree().create_timer(6).timeout
				get_tree().quit()
			return

	杀戮榜.更新()
	境界榜.更新()
	年月.text = "{0}年{1}月".format([年, 月])

	pass

func log(text: String) -> void:
	日志.append_text(text)
	日志.append_text("\n")

func 结束(b: Ball):
	$CanvasLayer2/ColorRect.visible = true
	var label = $CanvasLayer2/Label
	label.add_theme_color_override("font_color", b.body.modulate)
	label.text = "时辰到，{0}色胜利！\n名字：{1}\n境界：{2}\n寿命：{3}\n资质：{4}\n生命：{5}\n修为：{6}".format([
		b.身份,
		b.名字,
		GlData.境界体系.find_key(b.境界),
		"{0}/{1}".format([b.年龄, b.寿命]),
		b.资质,
		"{0}/{1}".format([b.生命, b.最大生命]),
		"{0}/{1}".format([b.修为, b.修为上限])
	])
	pass
