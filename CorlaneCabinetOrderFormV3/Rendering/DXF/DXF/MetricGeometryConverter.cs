using System.Collections.Generic;
using System.Linq;
using CorlaneCabinetOrderFormV3.Rendering.DXF.Core;

namespace CorlaneCabinetOrderFormV3.Rendering.DXF.DXF
{
    // Changed to internal to match PartGeometry accessibility
    internal static class MetricGeometryConverter
    {
        private const double InchesToMillimeters = 25.4;

        // Changed to internal
        internal static List<PartGeometry> ConvertToMetric(List<PartGeometry> imperialGeometries)
        {
            if (imperialGeometries == null) return new List<PartGeometry>();

            return imperialGeometries.Select(part => part with
            {
                OutlineVertices = part.OutlineVertices?.Select(v => new Vector2(v.X * InchesToMillimeters, v.Y * InchesToMillimeters)).ToList()!,
                
                TenonThinningPockets = part.TenonThinningPockets?.Select(p => 
                    (p.x1 * InchesToMillimeters, p.x2 * InchesToMillimeters, p.y1 * InchesToMillimeters, p.y2 * InchesToMillimeters)).ToList()!,
                
                MortisePockets = part.MortisePockets?.Select(p => 
                    (p.x1 * InchesToMillimeters, p.x2 * InchesToMillimeters, p.y1 * InchesToMillimeters, p.y2 * InchesToMillimeters)).ToList()!,

                MortisePocketsThru = part.MortisePocketsThru?.Select(p =>
                    (p.x1 * InchesToMillimeters, p.x2 * InchesToMillimeters, p.y1 * InchesToMillimeters, p.y2 * InchesToMillimeters)).ToList()!,

                Holes = part.Holes?.Select(h => 
                    (h.x * InchesToMillimeters, h.y * InchesToMillimeters, h.radius * InchesToMillimeters)).ToList()!,
                
                HolesThru = part.HolesThru?.Select(h => 
                    (h.x * InchesToMillimeters, h.y * InchesToMillimeters, h.radius * InchesToMillimeters)).ToList()!
            }).ToList();
        }
    }
}
