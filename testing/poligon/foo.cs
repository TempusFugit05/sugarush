using Godot;
using System;

[Tool]
public partial class foo : MeshInstance3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        if (Engine.IsEditorHint())
        {
            Vector3[] vertex =
            {
            new (0, 0, 0),
            new (1, 0, 0),
            new (1, 1, 0)
            };
            ArrayMesh meshArr = new();
            Godot.Collections.Array arrays = new();
            arrays.Resize((int)Mesh.ArrayType.Max);
            arrays[(int)Mesh.ArrayType.Vertex] = vertex;
            meshArr.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
            Mesh = meshArr;
        }
    }

}
