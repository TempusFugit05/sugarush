using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Helpers
{
    static public partial class HP
    {
        private static HelperNode HelperInstance;
        private static uint MaxDecals = 48;
        private static Godot.Collections.Array<BulletHole> DecalPool = new();
        private static float AirResistance = 2.0f;
        private static float GroundResistance = 1.0f;

        /// <summary>
        ///     Get an array of rids contained by nodes in a specific group.
        /// </summary>
        /// <param name="GroupName">The name of the group which Rids should be ignored</param>
        /// <returns></returns>
        static private Godot.Collections.Array<Rid> GetGroupRids(string GroupName)
        {
            if (HelperInstance is not null)
            {
                Godot.Collections.Array<Node> GroupNodes = HelperInstance.GetTree().GetNodesInGroup(GroupName);
                if (GroupNodes.Count != 0)
                {
                    Godot.Collections.Array<Rid> GroupRids = new();
                    GroupRids.Resize(GroupNodes.Count);
                    int i = 0;
                    foreach(CollisionObject3D node in GroupNodes.Cast<CollisionObject3D>())
                    {
                        GroupRids[i] = node.GetRid();
                        i++;
                    }
                    return GroupRids;
                }
            }
            return new Godot.Collections.Array<Rid>();
        }

		/// <summary>
        /// 	Request an array of rids that should be excluded by weapons by default
        /// </summary>
        /// <returns></returns>
		static public Godot.Collections.Array<Rid> GetDefaultExclusionList()
		{
            return GetGroupRids("GIgnoreWeapons");
        }

		/// <summary>
        /// 	Request an array of rids that should be excluded by weapons heald by enemies
        /// </summary>
        /// <returns></returns>
		static public Godot.Collections.Array<Rid> GetEnemyExclusions()
		{
            return GetGroupRids("GEnemies");
		}

        static public float GetGroundResistance()
        {
            return GroundResistance;
        }

        static public float GetAirResistance()
        {
            return AirResistance;
        }

        static public void RemoveFromDecalPool(BulletHole decal)
        {
            int Index = DecalPool.IndexOf(decal);
            if (Index != -1)
            {
                DecalPool.RemoveAt(Index);
            }
            else
            {
                GD.PushError("Decal was not found in the pool");
            }
        }

        static public int AddToDecalPool(BulletHole decal)
        {
            if (DecalPool.Count > MaxDecals)
            {
                
                DecalPool.First().QueueFree();
            }
            DecalPool.Add(decal);
            return DecalPool.Count - 1;
        }

		static public void Init(HelperNode helperNode)
		{
            HelperInstance = helperNode;
        }

        static public void CreateDamageIndicator(float damage, Vector3 damagePosition)
        {
            DamageIndicator Indicator = new(damage); // Create a damage indicator 
            HelperInstance.GetTree().Root.AddChild(Indicator); // Add it to the scene
            Indicator.GlobalPosition = damagePosition; // Set position of indicator to a specific position on body (ie bullethole) or object position for non specific damage soruce (ie fall damage)
        }

        /// <summary>
        ///     This is an internal class used to access some engine specific things.
        ///     Initialized by the GameManager node.
        /// </summary>
        public partial class HelperNode : Node
        {
            public override void _Ready()
            {
                Init(this);
            }
        }

    }

    /// <summary>
    ///     Resource Helper.
    ///     Contains helper functions for resource management as well as access functions for global resources.
    /// </summary>
    static public partial class HR
    {
        private static Node3D PlayerNode;
        
        static public void SetPlayerNode(Node3D node)
        {
            PlayerNode = node;
        }

        static public Node3D GetPlayerNode()
        {
            return PlayerNode;
        }

        static public bool RecHasChildOfType<[MustBeVariant] T>(Node parentNode)
        {
            foreach (Node child in parentNode.GetChildren())
            {
                if (child is T)
                {
                    return true;
                }
                return RecHasChildOfType<T>(child);
            }
            return false;
        }

        /// <summary>
        ///     Check if a node has at least one child of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of node</typeparam>
        /// <param name="parentNode">Node to check children for</param>
        /// <param name="recursive">Recursively check children's children</param>
        /// <returns>True if it contains child of type, false if not</returns>
        static public bool HasChildOfType<[MustBeVariant] T>(Node parentNode, bool recursive = false)
        {
            if (recursive)
            {
                return RecHasChildOfType<T>(parentNode);
            }
            foreach (Node child in parentNode.GetChildren())
            {
                if (child is T)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Recursively get all children of a specific type in this node.
        /// </summary>
        /// <typeparam name="T">Type of node</typeparam>
        /// <param name="parentNode">Parent node to get chilren from</param>
        /// <param name="childArr">Array to which to append the children</param>
        static private void GetChildrenOfTypeRec<[MustBeVariant] T>(Node parentNode, Godot.Collections.Array<T> childArr)
        {
            foreach (Node child in parentNode.GetChildren())
            {
                if (child is T tChild)
                {
                    childArr.Add(tChild);
                }
                GetChildrenOfTypeRec(child, childArr);
            }
        }

        /// <summary>
        ///     Get all children of a specific type in this node.
        /// </summary>
        /// <typeparam name="T">Type of node</typeparam>
        /// <param name="parentNode">Parent node to get chilren from</param>
        /// <param name="recursive">If true get all children of the children</param>
        /// <returns></returns>
        static public Godot.Collections.Array<T> GetChildrenOfType<[MustBeVariant] T>(Node parentNode, bool recursive = false)
        {
            Godot.Collections.Array<T> children = new();

            if (recursive)
            {
                GetChildrenOfTypeRec(parentNode, children);
            }
            else
            {
                foreach (Node child in parentNode.GetChildren())
                {
                    if (child is T tChild)
                    {
                        children.Add(tChild);
                    }
                }

            }
            return children;
        }

        /// <summary>
        ///     Remove all collision exceptions in a PhysicsBody3D.
        /// </summary>
        /// <param name="body">PhysicsBody3D for which to clear the exceptions</param>
        static public void ClearCollisionExceptions(PhysicsBody3D body)
        {
            foreach (Node collider in body.GetCollisionExceptions())
            {
                body.RemoveCollisionExceptionWith(collider);
            }
        }

    }

    /// <summary>
    ///     Math Helper.
    ///     Contains helper functions for math calculations.
    /// </summary>
    static public partial class HM
    {
        public const float Epsilon = 1e-6f;

        /// <summary>
        ///     Rotate a quaternion towards a target quaternion at a constant angle.
        ///     Useful for rotating a quaternion by a constant angular speed.
        /// </summary>
        /// <param name="from">Starting rotation.</param>
        /// <param name="to">Target rotation.</param>
        /// <param name="angle">Angle by which to rotate the from quaternion.</param>
        /// <returns>Rotated quaternion.</returns>
        public static Quaternion RotateTowards(Quaternion from, Quaternion to, float angle)
        {
            float differenceAngle = from.AngleTo(to);
            if (angle < differenceAngle)
            {
                return from.Slerp(to,  angle / differenceAngle);
            }
            return to;
        }

        public static (Quaternion Swing, Quaternion Twist) SwingTwistDecomp(Quaternion quat, Vector3 refAxis)
        {
            // Credit to: https://www.gamedev.net/forums/topic/696882-swing-twist-interpolation-sterp-an-alternative-to-slerp/

            Vector3 quatAxis = new(quat.X, quat.Y, quat.Z); // Extract the axis of rotation from the given quaternion
            
            Quaternion swing;
            Quaternion twist;

            // If the angle between the referance axis and the quaternion axis is 180 degrees, we have a singularity.
            // This is because there is an infinite number of swing rotations we can perform to get from one axis to another.
            // We can check for such a rotation when the magnitude of the vector part of a quaternion is near-zero.
            if (quatAxis.LengthSquared() <= 10e-4)
            {
                // Magic???????
                Vector3 rotatedRef = quat * refAxis;
                Vector3 trialSwing = refAxis.Cross(rotatedRef);
                GD.Print("AH OH! " + refAxis + " " + quatAxis.LengthSquared());
                
                if (trialSwing.LengthSquared() > 10e-4)
                {
                    float swingAngle = refAxis.AngleTo(rotatedRef);
                    swing = new(refAxis, swingAngle);
                }

                else
                {
                    swing = Quaternion.Identity;
                }
            
                twist = new(refAxis, Mathf.Tau / 2);
            }

            else
            {
                Vector3 projection = refAxis.Project(quatAxis).Normalized();
                swing = new Quaternion(projection.X, projection.Y, projection.Z, quat.W).Normalized();
                twist = quat * swing.Inverse(); // The remainder of the quaternion minus the swing is the twist component
            }

            return (swing, twist);
        }

        /// <summary>
        ///     Extract the component of rotation around a specific axis from a quaternion.
        ///     Original quaternion and projection vector must both be normalized.
        /// </summary>
        /// <param name="original">The quaternion from which the component will be extracted.</param>
        /// <param name="projection">The axis around which the rotation will be projected.</param>
        /// <returns>The component of rotation around the given axis.</returns>
        public static Quaternion ProjectQuaternion(Quaternion original, Vector3 projection)
        {
            Vector3 Projection = original.GetAxis().Project(projection); // Project the quaternion axis onto the new axis
            Quaternion Out = new Quaternion(Projection.X, Projection.Y, Projection.Z, original.W).Normalized(); // Construct modified quaternion
            
            if (Out.IsFinite())
            {
                return Out;
            }
            return original;
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="lookingNode"></param>
        /// <param name="lookedAtNode"></param>
        /// <param name="Axis"></param>
        /// <returns></returns>
        public static Quaternion LookingAtAxis(Node3D lookingNode, Node3D lookedAtNode, Vector3 Axis)
        {
            Quaternion targetRotation;
            targetRotation = lookingNode.GlobalTransform.LookingAt(lookedAtNode.GlobalPosition, Vector3.Down, true).Basis.GetRotationQuaternion(); // Get target rotation
            targetRotation = ProjectQuaternion(targetRotation, Axis); // Extract rotation around the given axis
            return targetRotation;
        }

        /// <summary>
        ///     Checks that the quaternion is not infinite and returns a blank normalized quaternion if it is.
        /// </summary>
        /// <param name="quatToCheck">Quaternion to validate.</param>
        /// <returns>Original quaternion or a new normalized one.</returns>
        public static Quaternion ValidateQuaternion(Quaternion quatToCheck)
        {
            if (quatToCheck.IsFinite())
            {
                return quatToCheck;
            }
            return new Quaternion(0, 0, 0, 1); // Return new normalized quaternion
        }
    
        public static Vector3 projectOntoPlane(Vector3 planeVec1, Vector3 planeVec2, Vector3 toProject)
        {
            Vector3 planeNormal = planeVec1.Cross(planeVec2); // Normal created by the two plane vectors
            Vector3 projection = toProject.Project(planeNormal);
            return toProject - projection;
        }
    }

    public class GenericCache
    {
        private class FunctionCache
        {
            public string FuncName;
            public bool WasRan = false;
            public object CachedValue = null;

            public FunctionCache(string funcName)
            {
                FuncName = funcName;
            }
        }

        private List <FunctionCache> Cache = new();
        private ulong IterationNum = 0;

        private bool GetUpdateRan(string funcName)
        {
            for (int i = 0; i < Cache.Count; i++)
            {
                var item = Cache[i];

                if (item.FuncName == funcName)
                {
                    bool ret = item.WasRan;
                    item.WasRan = true;
                    return ret;
                }
            }
            Cache.Add(new(funcName));

            return false;
        }

        public void UpdateCache(string funcName, object toCache)
        {
            for (int i = 0; i < Cache.Count; i++)
            {
                var item = Cache[i];
                if (item.FuncName == funcName)
                {
                    item.CachedValue = toCache;
                }
            }
        }

        public object GetCachedItem(string funcName)
        {
            for (int i = 0; i < Cache.Count; i++)
            {
                var item = Cache[i];
                if (item.FuncName == funcName)
                {
                    return item.CachedValue;
                }
            }
            return null;
        }

        /// <summary>
        ///     Check if an iteration has passed since last call to this function.
        ///     Used for caching purposes.
        /// </summary>
        /// <returns>true if this is the same iteration; false if not</returns>
        public bool IsSameIteration(string funcName)
        {
            ulong currentFrame = Engine.GetPhysicsFrames(); // Get current physics iteration

            if (IterationNum != currentFrame)
            {
                IterationNum = currentFrame;
                // GD.Print("NEW");
                for (int i = 0; i < Cache.Count; i++)
                {
                    var item = Cache[i];
                    item.WasRan = false;
                }

                return false;
            }
            // GD.Print("OLD");
            return GetUpdateRan(funcName);
        }
    }

}