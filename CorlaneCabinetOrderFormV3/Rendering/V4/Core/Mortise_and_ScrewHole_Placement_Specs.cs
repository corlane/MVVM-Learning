using System;
using System.Collections.Generic;
using System.Text;

namespace CorlaneCabinetOrderFormV3.Rendering.V4.Core;

internal record MortisePlacementSpec
(
    MortiseEdge Edge,
    double EdgeLength,
    double PartWidth,
    double PartHeight,
    double OffsetFromEdge,
    double OffsetAlongEdge,
    bool ForceTwoTenons,
    double BlindStartOverride,
    double BlindStopOverride,
    bool FullThicknessTenon,
    double MaterialThickness34
);

internal record ScrewHolePlacementSpec
(
    ScrewHoleEdge Edge,
    double EdgeLength,
    double PartWidth,
    double PartHeight,
    double OffsetFromEdge,
    double OffsetAlongEdge,
    bool ForceTwoTenons,
    double BlindStartOverride,
    double BlindStopOverride,
    double MaterialThickness34,
    bool IncludeEndHoles = true
);
