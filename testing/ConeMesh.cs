using Godot;
using Godot.NativeInterop;
using System;

public partial class ConeMesh : PrimitiveMesh
{
    public override Godot.Collections.Array _CreateMeshArray()
    {
        Godot.Collections.Array Out = new();
        Godot.Collections.Array<Vector3> meshArr = (Godot.Collections.Array<Vector3>)Out[(int)ArrayType.Vertex];
        meshArr.Resize(3);
        meshArr[0] = new(0, 0, 0);
        meshArr[1] = new(1, 0, 0);
        meshArr[2] = new(1, 1, 0);
        return Out;
    } 
}
