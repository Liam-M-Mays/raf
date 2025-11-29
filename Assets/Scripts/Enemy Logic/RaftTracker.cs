using UnityEngine;
using System.Collections.Generic;

public static class RaftTracker
{
    public static List<IBehavior> Braft = new List<IBehavior>();
     public static bool addSelf(IBehavior B)
    {
        if (Braft.Contains(B)) return true;
        else if (Braft.Count < 3)
        {
            Braft.Add(B);
            return true;
        }
        return false;

    }

    public static void removeSelf(IBehavior B)
    {
        Braft.Remove(B);
    }
}