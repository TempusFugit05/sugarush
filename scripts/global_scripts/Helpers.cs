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
                DecalPool.First().DeInitNode();
            }
            DecalPool.Add(decal);
            return DecalPool.Count - 1;
        }

		static public void Init(HelperNode helperNode)
		{
            HelperInstance = helperNode;
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
        private static Character PlayerNode;
        
        static public void SetPlayerNode(Character node)
        {
            PlayerNode = node;
        }

        static public Character GetPlayerNode()
        {
            return PlayerNode;
        }
    }

    /// <summary>
    ///     Math Helper.
    ///     Contains helper functions for math calculations.
    /// </summary>
    static public partial class HM
    {
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

        /// <summary>
        ///     Extract the component of rotation around a specific axis from a quaternion.
        ///     Original quaternion and projection vector must both be normalized.
        /// </summary>
        /// <param name="original">The quaternion from which the component will be extracted.</param>
        /// <param name="projection">The axis around which the rotation will be projected.</param>
        /// <returns>The component of rotation around the given axis.</returns>
        public static Quaternion ProjectQuaternion(Quaternion original, Vector3 projection)
        {
            /*Normalize inputs to produce a normalized output*/
            projection = projection.Normalized();
            original = original.Normalized();

            Vector3 Projection = original.GetAxis().Project(projection); // Project the quaternion axis onto the new axis
            Quaternion Out = new(Projection.X, Projection.Y, Projection.Z, original.W); // Construct modified quaternion
            if (Out.IsFinite())
            {
                return Out;
            }
            return original;
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
    }
}