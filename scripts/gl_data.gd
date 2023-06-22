extends Node

signal Log(text: String)

var 阵营: Dictionary = {
	红 = Color("ff4527"),
	黄 = Color("ffd700"),
	蓝 = Color("00bfff"),
	绿 = Color("6bde32")
}

enum 境界体系 {
	武徒,

	炼精化气,
	炼气化神,
	炼神还虚,

	铜皮铁骨,
	毫发不爽,
	心领神会,

	滴血重生,
	合道同归,
	独步乾坤,

	武神

}

var 境界属性: Dictionary = {
	境界体系.武徒 : {
		寿命 = 1.0,
		生命 = 1.0,
		修为上限 = 1.0
	},
	境界体系.炼精化气 : {
		寿命 = 1.5,
		生命 = 1.5,
		修为上限 = 2.0
	},
	境界体系.炼气化神 : {
		寿命 = 1.5,
		生命 = 1.5,
		修为上限 = 2.0
	},
	境界体系.炼神还虚 : {
		寿命 = 1.5,
		生命 = 1.5,
		修为上限 = 2.0
	},
	境界体系.铜皮铁骨 : {
		寿命 = 2.0,
		生命 = 2.0,
		修为上限 = 3.0
	},
	境界体系.毫发不爽 : {
		寿命 = 2.0,
		生命 = 2.0,
		修为上限 = 3.0
	},
	境界体系.心领神会 : {
		寿命 = 2.0,
		生命 = 2.0,
		修为上限 = 3.0
	},
	境界体系.滴血重生 : {
		寿命 = 2.5,
		生命 = 2.5,
		修为上限 = 4.0
	},
	境界体系.合道同归 : {
		寿命 = 2.5,
		生命 = 2.5,
		修为上限 = 4.0
	},
	境界体系.独步乾坤 : {
		寿命 = 2.5,
		生命 = 2.5,
		修为上限 = 4.0
	},
	境界体系.武神 : {
		寿命 = 3.0,
		生命 = 3.0,
		修为上限 = 5.0
	},
}

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS

func _input(_event: InputEvent) -> void:
	if Input.is_action_pressed("ui_accept"):
		get_tree().paused = !get_tree().paused

func log(text: String) -> void:
	Log.emit(text)

func getAgeGroup(年龄, 寿命) -> String:
	var x : String = "幼年"
	var i = float(年龄) / float(寿命)
	if i < 5.0/100.0:
		x = "幼年"
	elif i < 15.0/100.0:
		x = "少年"
	elif i < 40.0/100.0:
		x = "青年"
	elif i < 60.0/100.0:
		x = "中年"
	elif i < 80.0/100.0:
		x = "老年"
	else:
		x = "晚年"
	return x

func getRandomChineseCharacter() -> String:
	return String.chr(randi_range(0x4E00, 0x9FA5))
