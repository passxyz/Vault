using System;
using System.Collections.Generic;
using System.Text;

using KeePassLib;
using KeePassLib.Interfaces;

namespace PassXYZLib
{
    public class PxGroup: PwGroup
    {
        public PxGroup(bool bCreateNewUuid, bool bSetTimes) : base(bCreateNewUuid, bSetTimes) { }
        public PxGroup() : base() { }
    }

    public static class PwGroupEx 
    {
        public static List<Item> GetItems(this PwGroup group)
        {
            List<Item> itemList = new List<Item>();

            foreach (PwEntry entry in group.Entries)
            {
                entry.SetIcon();
                itemList.Add((Item)entry);
            }

            foreach (PwGroup gp in group.Groups)
            {
                gp.SetIcon();
                itemList.Add((Item)gp);
            }
            return itemList;
        }

		/// <summary>
		/// Find a group by Id.
		/// </summary>
		/// <param name="id">ID identifying the group the caller is looking for.</param>
		/// <param name="bSearchRecursive">If <c>true</c>, the search is recursive.</param>
		/// <returns>Returns reference to found group, otherwise <c>null</c>.</returns>
		public static PwGroup FindGroup(this PwGroup group, string id, bool bSearchRecursive)
		{
			if (group.Id == id) return group;

			if (bSearchRecursive)
			{
				PwGroup pgRec;
				foreach (PwGroup pg in group.Groups)
				{
					pgRec = pg.FindGroup(id, true);
					if (pgRec != null) return pgRec;
				}
			}
			else // Not recursive
			{
				foreach (PwGroup pg in group.Groups)
				{
					if (pg.Id == id)
						return pg;
				}
			}

			return null;
		}
	}
}
