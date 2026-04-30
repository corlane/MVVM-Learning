namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class PartOutlineBuilder
{
    internal static List<(double X1, double X2, double Y1, double Y2)> ComputeTenonThinningPockets(
        double length, double depth, LockDadoSettings s,
        EdgeDesignators tenonEdges = EdgeDesignators.None, bool forceTwoTenons = false)
    {
        var pockets = new List<(double, double, double, double)>();
        double dd = s.DadoDepth + 0.03937;

        if (tenonEdges.HasFlag(EdgeDesignators.Left) || tenonEdges.HasFlag(EdgeDesignators.Right))
        {
            double pocketY1 = s.BlindStart - s.TenonPocketOversize;
            double pocketY2 = depth - s.BlindStop + s.TenonPocketOversize;

            if (tenonEdges.HasFlag(EdgeDesignators.Left))
                pockets.Add((-dd, 0.0, pocketY1, pocketY2));

            if (tenonEdges.HasFlag(EdgeDesignators.Right))
                pockets.Add((length, length + dd, pocketY1, pocketY2));
        }

        if (tenonEdges.HasFlag(EdgeDesignators.Top) || tenonEdges.HasFlag(EdgeDesignators.Bottom))
        {
            double pocketX1 = s.BlindStart - s.TenonPocketOversize;
            double pocketX2 = length - s.BlindStop + s.TenonPocketOversize;

            if (tenonEdges.HasFlag(EdgeDesignators.Bottom))
                pockets.Add((pocketX1, pocketX2, -dd, 0.0));

            if (tenonEdges.HasFlag(EdgeDesignators.Top))
                pockets.Add((pocketX1, pocketX2, depth, depth + dd));
        }

        return pockets;
    }
}