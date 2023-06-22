extends RigidBody2D
class_name Ball

signal 死亡事件(body, value)
signal 突破事件(body, value)
signal 生命事件(body, value)
signal 修为事件(body, value)
signal 资质事件(body, value)

@onready var 传承 = preload("res://scenes/传承.tscn")
@onready var TIP = preload("res://scenes/tip.tscn")
@onready var body: MeshInstance2D = $Body

var 已死: bool = false
var 身份: StringName
var 名字: StringName = ''
var 境界: GlData.境界体系 = GlData.境界体系.武徒 :
	set(value):
		if is_visible_in_tree():
			境界 = value
			mass = 境界 + 1
			%境界.text = GlData.境界体系.find_key(境界)
			%境界.add_theme_font_size_override("font_size", 16 + 境界*2)
			%境界.add_theme_color_override("font_outline_color", Color(境界/10.0, 境界/10.0, 境界/10.0))

var 境界名: StringName :
	get:
		return GlData.境界体系.find_key(境界)

var 击杀数: int = 0:
	set(value):
		击杀数 = value
		%名字.add_theme_color_override("font_shadow_color", Color("000000", 击杀数/10.0))
		if 击杀数 % 10 == 0:
			修为上限 *= 1.05

var 累计修为: int = 0
var 修为上限: int = 120
var 资质: float = 0.2
var tip_index_0: int = 0
var tip_index_1: int = 0

@export var 寿命: int = 50 :
	set(value):
		if is_node_ready():
			寿命 = value
			%年龄.text = GlData.getAgeGroup(年龄, 寿命)

@export var 年龄: int = 0 :
	set(value):
		if is_node_ready():
			年龄 = min(value, 寿命 + 1)
			%年龄.text = GlData.getAgeGroup(年龄, 寿命)

@export var 最大生命: int = 100 :
	set(value):
		if is_node_ready():
			最大生命 = max(value, 1)
			%生命.material.set_shader_parameter('fill_ratio', float(生命) / float(最大生命) * 0.5)

@export var 生命: int = 最大生命 :
	set(value):
		if is_node_ready():
			生命 = min(value, 最大生命)
			%生命.material.set_shader_parameter('fill_ratio', float(生命) / float(最大生命) * 0.5)

@export var 修为: float = 0 :
	set(value):
		if is_visible_in_tree():
			修为 = snapped(value, 0.1)
			%修为.material.set_shader_parameter('fill_ratio', 修为 / 修为上限 * 0.5)
			if 修为 > 修为上限:
				突破()

func _ready() -> void:
	名字  = GlData.getRandomChineseCharacter()
	%名字.text = 名字
	add_to_group("武者")
	add_to_group(身份)
	body.modulate = GlData.阵营[身份]
	GlData.log("[color=%s][font_size=25]%s[/font_size][/color]%s" % [GlData.阵营[身份].to_html(), 名字, " 出生了：境界 {0}  寿命 {1}  资质 {2}  生命 {3}  修为 {4}\n".format([境界名, "{0}/{1}".format([年龄, 寿命]), 资质, "{0}/{1}".format([生命, 最大生命]), "{0}/{1}".format([修为, 修为上限])])])
	生命事件.connect(func(_b, v):
		addTip("生命 " + (str(v) if v < 0 else "+" + str(v)), v)
		生命 += v
	)
	修为事件.connect(func(_b, v):
		addTip("修为 " + (str(v) if v < 0 else "+" + str(v)), v)
		修为 += v
	)
	资质事件.connect(func(_b, v):
		addTip("资质 " + (str(v) if v < 0 else "+" + str(v)), v)
		资质 += v
	)
	pass # Replace with function body.

func 攻击(敌人: Ball):
	var 伤害 = 10 * ((1 + 境界) * (1.0 + (境界 - 敌人.境界) * 0.1))
	var 收获 = snapped(敌人.修为上限 * 0.05, 0.1)
	敌人.生命事件.emit(敌人, -伤害)
	修为事件.emit(self, 收获)
	if 敌人.生命 <= 0:
		敌人.死("被 [color={0}][font_size=25]{1}({2})[/font_size][/color] 杀死了".format([GlData.阵营[身份].to_html(), 名字, GlData.境界体系.find_key(境界)]))
		击杀数 += 1
		var x = 0
		if 境界 <= 敌人.境界:
			x += 敌人.累计修为 * 0.05
			x += 敌人.修为上限 * 0.1
			x += (1 + (敌人.境界 - 境界) * 0.1) * randf_range(敌人.修为 * 0.5, 敌人.修为 * 0.8)
			资质事件.emit(self, 0.1)
		else:
			x += 敌人.累计修为 * 0.05 * (1 / float(境界 - 敌人.境界))
			x += 敌人.修为上限 * 0.1
			x += (1 / float(境界 - 敌人.境界)) * randf_range(敌人.修为*0.5, 敌人.修为*0.8)
		x = snapped(x, 0.1)
		修为事件.emit(self, x)

func 过月():
	修为 += 资质


func 过年():
	年龄 += 1
	if 年龄 > 寿命:
		死("寿尽而亡")
		var cc = 传承.instantiate()
		cc.身份 = 身份
		cc.修为 = 累计修为 + 修为
		cc.资质 = 资质
		cc.duration = 寿命 * 1.2 * (1.0 + 境界 * 0.5) * 2
		cc.global_position = global_position
		add_sibling(cc)


func 突破():
	if !已死:
		累计修为 += 修为上限
		var _修为上限 = 修为上限
		境界 += 1
		var 境界属性 = GlData.境界属性[self.境界]
		修为上限 *= 境界属性.修为上限
		最大生命 *= 境界属性.生命
		生命 *= 境界属性.生命
		生命事件.emit(self, ceil(0.1 * 最大生命))
		寿命 *= 境界属性.寿命
		修为 -= _修为上限
		突破事件.emit(self, 境界名)

func 死(死因: String = ""):
	remove_from_group("武者")
	remove_from_group(身份)
	已死 = true
	死亡事件.emit(self, 死因)
#	process_mode = Node.PROCESS_MODE_DISABLED
#	set_process(false)
#	set_physics_process(false)
	set_deferred("sleeping", true)
	set_deferred("freeze", true)
	$Body.visible = false
	$CollisionShape2D.set_deferred("disabled", true)
	await get_tree().create_timer(2).timeout
	queue_free()

#func 累计修为() -> int :
#	if 境界 == 0:
#		return 0
#	var _修为 = 120
#	var 修为 = 0
#	for i in 境界:
#		_修为 *= GlData.境界属性[i].修为上限
#		修为 += _修为
#	return 修为

func addTip(v, y):
	var tip = TIP.instantiate() as Label
	tip.text = v
	tip.add_theme_color_override("font_color", body.modulate)
	if y >= 0:
		tip.y = -1
		tip.position.y -= 20
		get_tree().create_timer(0.4).timeout.connect(func():
			tip_index_0 -= 1
		)
		tip_index_0 += 1
		await get_tree().create_timer((tip_index_0 - 1) * 0.4).timeout
	else:
		tip.position.y += 30
		get_tree().create_timer(0.4).timeout.connect(func():
			tip_index_1 -= 1
		)
		tip_index_1 += 1
		await get_tree().create_timer((tip_index_1 - 1) * 0.4).timeout
	add_child(tip)

#
func _on_body_entered(body: Node) -> void:
	if body is Ball and body.身份 != 身份:
		攻击(body)
	pass # Replace with function body.
