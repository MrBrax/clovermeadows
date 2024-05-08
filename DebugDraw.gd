# DebugDraw.gd (autoloaded)
extends MeshInstance3D

var mat: StandardMaterial3D = StandardMaterial3D.new()

class PersistentLine:
	var start : Vector3
	var end : Vector3
	var color : Color
	var duration : float = 0.0

	func _init(_start, _end, _color, _duration):
		start = _start
		end = _end
		color = _color
		duration = _duration

class PersistentSphere:
	var center : Vector3
	var radius : float
	var color : Color
	var duration : float = 0.0

	func _init(_center, _radius, _color, _duration):
		center = _center
		radius = _radius
		color = _color
		duration = _duration

class PersistentCircle:
	var center : Vector3
	var radius : float
	var color : Color
	var duration : float = 0.0

	func _init(_center, _radius, _color, _duration):
		center = _center
		radius = _radius
		color = _color
		duration = _duration

var lines = []
var spheres = []
var circles = []


func add_line(begin_pos: Vector3, end_pos: Vector3, color: Color = Color.RED, duration: float = 1.0):
	lines.push_back(PersistentLine.new(begin_pos, end_pos, color, duration))

func add_sphere(center: Vector3, radius : float, color: Color = Color.RED, duration: float = 1.0):
	spheres.push_back(PersistentSphere.new(center, radius, color, duration))

func add_circle(center: Vector3, radius : float, color: Color = Color.RED, duration: float = 1.0):
	circles.push_back(PersistentCircle.new(center, radius, color, duration))

func line(begin_pos: Vector3, end_pos: Vector3, color: Color = Color.RED) -> void:
	mesh.surface_begin(Mesh.PRIMITIVE_LINES)
	mesh.surface_set_color(color)
	mesh.surface_add_vertex(begin_pos)
	mesh.surface_add_vertex(end_pos)
	mesh.surface_end()

func sphere(center: Vector3, radius: float = 1.0, color: Color = Color.RED) -> void:
	var step: int = 15
	var sppi: float = 2 * PI / step
	var axes = [
		[Vector3.UP, Vector3.RIGHT],
		[Vector3.RIGHT, Vector3.FORWARD],
		[Vector3.FORWARD, Vector3.UP]
	]
	mesh.surface_begin(Mesh.PRIMITIVE_LINE_STRIP)
	mesh.surface_set_color(color)
	for axis in axes:
		for i in range(step + 1):
			mesh.surface_add_vertex(center + (axis[0] * radius)
				.rotated(axis[1], sppi * (i % step)))
	mesh.surface_end()

func circle(center: Vector3, radius: float = 1.0, color: Color = Color.RED) -> void:
	var step: int = 15
	var sppi: float = 2 * PI / step
	var axes = [
		#[Vector3.UP, Vector3.RIGHT],
		[Vector3.RIGHT, Vector3.UP]
		#[Vector3.FORWARD, Vector3.UP]
	]
	mesh.surface_begin(Mesh.PRIMITIVE_LINE_STRIP)
	mesh.surface_set_color(color)
	for axis in axes:
		for i in range(step + 1):
			mesh.surface_add_vertex(center + (axis[0] * radius)
				.rotated(axis[1], sppi * (i % step)))
	mesh.surface_end()

func _ready():
	mesh = ImmediateMesh.new()
	mat.no_depth_test = true
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.vertex_color_use_as_albedo = true
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	set_material_override(mat)

func _process(_delta):
	
	mesh.clear_surfaces()
	
	for i in range(lines.size() - 1, -1, -1):
		line(lines[i].start, lines[i].end, lines[i].color)
		lines[i].duration -= _delta
		if lines[i].duration <= 0.0:
			lines.remove_at(i)
	
	for i in range(spheres.size() - 1, -1, -1):
		sphere(spheres[i].center, spheres[i].radius, spheres[i].color)
		spheres[i].duration -= _delta
		if spheres[i].duration <= 0.0:
			spheres.remove_at(i)
			
	for i in range(circles.size() - 1, -1, -1):
		circle(circles[i].center, circles[i].radius, circles[i].color)
		circles[i].duration -= _delta
		if circles[i].duration <= 0.0:
			circles.remove_at(i)
