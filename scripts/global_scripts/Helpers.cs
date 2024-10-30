using System.Linq;
using Godot;

namespace Helpers
{
    static public partial class HP
    {
        private static _HelperNode HelperInstance;

        /// <summary>
        ///     Get an array of rids contained by nodes in a specific group.
        /// </summary>
        /// <param name="GroupName">The name of the group which Rids should be ignored</param>
        /// <returns></returns>
        static private Godot.Collections.Array<Rid> GetGroupRids(string GroupName)
        {
            Godot.Collections.Array<Node> GroupNodes = HelperInstance.GetTree().GetNodesInGroup(GroupName);
            Godot.Collections.Array<Rid> GroupRids = new();

			foreach(CollisionObject3D node in GroupNodes.Cast<CollisionObject3D>())
			{
                GroupRids.Add(node.GetRid());
			}

            return GroupRids;
        }

		/// <summary>
        /// 	Request an array of rids that should be excluded by weapons by default
        /// </summary>
        /// <returns></returns>
		static public Godot.Collections.Array<Rid> GetDefaultExclusionList()
		{
            return GetGroupRids("GPickups" );
        }


		/// <summary>
        /// 	Request an array of rids that should be excluded by weapons heald by enemies
        /// </summary>
        /// <returns></returns>
		static public Godot.Collections.Array<Rid> GetEnemyExclusions()
		{
            return GetGroupRids("GEnemies");
		}


		static public void Init(_HelperNode helperNode)
		{
            HelperInstance = helperNode;
        }

        /// <summary>
        ///     This is an internal class used to access some engine specific things.
        ///     Initialized by the GameManager node.
        /// </summary>
        public partial class _HelperNode : Node
        {
            private void OnTreeEntered()
            {
                
            }

            public override void _Ready()
            {
                TreeEntered += OnTreeEntered;
                Init(this);
            }
        }

    }
}