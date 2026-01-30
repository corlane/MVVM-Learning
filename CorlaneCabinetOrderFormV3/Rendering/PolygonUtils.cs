using System.Windows;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static class PolygonUtils
{
    internal static List<Point3D> FilletPolygon(List<Point3D> polygonPoints, double radius, int segments)
    {
        if (radius <= double.Epsilon || segments < 1 || polygonPoints == null || polygonPoints.Count < 3)
            return [.. polygonPoints!];

        var result = new List<Point3D>();
        int n = polygonPoints.Count;

        for (int i = 0; i < n; i++)
        {
            Point3D prev3 = polygonPoints[(i - 1 + n) % n];
            Point3D curr3 = polygonPoints[i];
            Point3D next3 = polygonPoints[(i + 1) % n];

            var currZ = curr3.Z;
            var prev = new Vector(prev3.X - curr3.X, prev3.Y - curr3.Y);
            var next = new Vector(next3.X - curr3.X, next3.Y - curr3.Y);

            double lenPrev = prev.Length;
            double lenNext = next.Length;

            if (lenPrev < 1e-8 || lenNext < 1e-8)
            {
                result.Add(curr3);
                continue;
            }

            prev.Normalize();
            next.Normalize();

            double dot = Math.Max(-1.0, Math.Min(1.0, (prev.X * next.X + prev.Y * next.Y)));
            double angle = Math.Acos(dot);

            if (angle < 1e-4 || Math.PI - angle < 1e-4)
            {
                result.Add(curr3);
                continue;
            }

            double tangentDist = radius / Math.Tan(angle / 2.0);

            double maxAllowed = Math.Min(lenPrev, lenNext) - 1e-6;
            if (tangentDist > maxAllowed) tangentDist = Math.Max(0.0, maxAllowed);

            if (tangentDist <= 1e-6)
            {
                result.Add(curr3);
                continue;
            }

            var t1 = new Point(curr3.X + prev.X * tangentDist, curr3.Y + prev.Y * tangentDist);
            var t2 = new Point(curr3.X + next.X * tangentDist, curr3.Y + next.Y * tangentDist);

            var bis = new Vector(prev.X + next.X, prev.Y + next.Y);
            double bisLen = bis.Length;
            if (bisLen < 1e-8)
            {
                result.Add(curr3);
                continue;
            }
            bis.Normalize();

            double centerDist = radius / Math.Sin(angle / 2.0);
            var center = new Point(curr3.X + bis.X * centerDist, curr3.Y + bis.Y * centerDist);

            double startAng = Math.Atan2(t1.Y - center.Y, t1.X - center.X);
            double endAng = Math.Atan2(t2.Y - center.Y, t2.X - center.X);

            double cross = prev.X * next.Y - prev.Y * next.X;
            double sweep = endAng - startAng;

            if (cross < 0)
            {
                if (sweep > 0) sweep -= 2.0 * Math.PI;
            }
            else
            {
                if (sweep < 0) sweep += 2.0 * Math.PI;
            }

            result.Add(new Point3D(t1.X, t1.Y, currZ));

            for (int s = 1; s <= segments; s++)
            {
                double t = (double)s / (segments + 1);
                double ang = startAng + sweep * t;
                double x = center.X + radius * Math.Cos(ang);
                double y = center.Y + radius * Math.Sin(ang);
                result.Add(new Point3D(x, y, currZ));
            }

            result.Add(new Point3D(t2.X, t2.Y, currZ));
        }

        return result;
    }
}