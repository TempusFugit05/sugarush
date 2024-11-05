using System.Linq;
using Godot;

namespace Helpers
{
    static public partial class HP
    {
        private static HelperNode HelperInstance;
        private static uint MaxDecals = 48;
        private static Godot.Collections.Array<BulletHole> DecalPool = new();
        private static Character PlayerNode;
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

        static public void SetPlayerNode(Character node)
        {
            PlayerNode = node;
        }

        static public Character GetPlayerNode()
        {
            return PlayerNode;
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
}