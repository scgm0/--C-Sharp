using System.Linq;
using Godot;
using Godot.Collections;

namespace 球武道.scripts;

public partial class 资质榜 : VBoxContainer {
	private Array<Label> _labels;

	public override void _Ready() {
		_labels = new Array<Label> {
			GetNode<Label>("Label1"),
			GetNode<Label>("Label2"),
			GetNode<Label>("Label3"),
			GetNode<Label>("Label4"),
			GetNode<Label>("Label5"),
			GetNode<Label>("Label6"),
			GetNode<Label>("Label7"),
			GetNode<Label>("Label8"),
			GetNode<Label>("Label9"),
			GetNode<Label>("Label10")
		};
	}

	public void 更新() {
		var arr = GetTree().GetNodesInGroup("武者");
		if (!arr.Any()) {
			return;
		}

		var balls = new Array<Node>(arr.OrderByDescending(a => ((Ball)a).资质).ThenByDescending(a => ((Ball)a).总修为));
		balls.Resize(10);
		for (var i = 0; i < 10; i++) {
			var label = _labels[i];
			var ball = (Ball)balls[i];
			label.Modulate = Colors.Black;
			if (ball == null) {
				continue;
			}

			label.Modulate = ball.Body.Modulate;
			label.Text = $"{ball.名字}：{ball.资质:F1}";
		}
	}
}