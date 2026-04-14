using CorlaneCabinetOrderFormV3.Models;
using CorlaneCabinetOrderFormV3.Services;
using System.Windows.Media.Media3D;

namespace CorlaneCabinetOrderFormV3.Rendering;

internal static partial class BaseCabinetBuilder
{
    /// <summary>
    /// Builds and positions rollout shelves and/or the trash drawer box,
    /// adding them to <paramref name="cabinet"/> and optionally recording
    /// them via <paramref name="addDrawerBoxRow"/>.
    /// </summary>
    private static void BuildRolloutsAndTrash(
        Model3DGroup cabinet,
        BaseCabinetModel baseCab,
        BaseCabinetDimensions dim,
        Action<BaseCabinetModel, string, double, double, double> addDrawerBoxRow,
        CabinetBuildResult? result)
    {
        if (!baseCab.IncRollouts && !baseCab.IncRolloutsInList && !baseCab.IncTrashDrwBox) return;

        double MaterialThickness34 = MaterialDefaults.Thickness34;
        double tandemSideSpacing = .4;
        double accurideSideSpacing = 1;
        const double rolloutMountBracketSpacing = 1;
        double rolloutHeight = 4;

        string style1 = CabinetStyles.Base.Standard;

        double height = dim.Height;
        double tk_Height = dim.TKHeight;
        double interiorWidth = dim.InteriorWidth;
        double interiorDepth = dim.InteriorDepth;
        double backThickness = dim.BackThickness;
        double opening1Height = dim.Opening1Height;
        double dbxDepth = dim.DrawerBoxDepth;

        double dbxWidth = interiorWidth;
        double dbxHeight = rolloutHeight;

        if (baseCab.RolloutStyle is not null)
        {
            if (baseCab.RolloutStyle.Contains("Blum"))
            {
                dbxWidth = interiorWidth - tandemSideSpacing;
            }
            else if (baseCab.RolloutStyle.Contains("Accuride"))
            {
                dbxWidth = interiorWidth - accurideSideSpacing;
            }
        }

        if (baseCab.RolloutCount > 0)
        {
            dbxWidth -= rolloutMountBracketSpacing * baseCab.DoorCount;
        }

        // ── Capture rollout dims — these are the actual values used for geometry ──
        if (result is not null)
        {
            result.RolloutWidth = dbxWidth;
            result.RolloutHeight = dbxHeight;
            result.RolloutDepth = dbxDepth;
        }

        if (baseCab.RolloutCount >= 1 || (baseCab.IncTrashDrwBox && baseCab.TrashDrawer))
        {
            if (baseCab.IncTrashDrwBox && baseCab.TrashDrawer)
            {
                dbxHeight = 12;
            }

            if (baseCab.IncRollouts)
            {
                // Calculate rollout opening for even vertical spacing
                double rolloutOpeningBottom = MaterialThickness34 + tk_Height;
                double rolloutOpeningTop = (baseCab.Style == style1 && baseCab.DrwCount == 1)
                    ? height - ((baseCab.DrwCount + 1) * MaterialThickness34) - opening1Height
                    : height - MaterialThickness34;

                double rolloutOpeningHeight = rolloutOpeningTop - rolloutOpeningBottom;
                double bottomOffset = 0.8406;
                int rolloutCount = baseCab.RolloutCount;
                double totalRolloutHeight = rolloutCount * dbxHeight;
                double rolloutGap = rolloutCount > 0
                    ? (rolloutOpeningHeight - bottomOffset - totalRolloutHeight) / rolloutCount
                    : 0;

                for (int r = 0; r < rolloutCount; r++)
                {
                    if (baseCab.IncRolloutsInList)
                    {
                        addDrawerBoxRow(baseCab, "Rollout", dbxHeight, dbxWidth, dbxDepth);
                    }

                    var rotateGroup =  BuildDrawerBoxRotateGroup(dbxWidth, dbxHeight, dbxDepth, MaterialThickness34, baseCab, "", false);
                    var placement = new Model3DGroup();
                    placement.Children.Add(rotateGroup);

                    double rolloutY = rolloutOpeningBottom + bottomOffset + (r * (dbxHeight + rolloutGap));

                    ModelTransforms.ApplyTransform(placement, (dbxWidth / 2) - MaterialThickness34, rolloutY, interiorDepth + backThickness - .25, 0, 0, 0);
                    cabinet.Children.Add(placement);
                }
            }

            // Reset dbxWidth for trash drawer (uses DrwStyle, not RolloutStyle)
            if (baseCab.DrwStyle is not null)
            {
                if (baseCab.DrwStyle.Contains("Blum"))
                {
                    dbxWidth = interiorWidth - tandemSideSpacing;
                }
                else if (baseCab.DrwStyle.Contains("Accuride"))
                {
                    dbxWidth = interiorWidth - accurideSideSpacing;
                }
            }

            if (baseCab.IncTrashDrwBox && baseCab.TrashDrawer)
            {
                if (baseCab.IncDrwBoxesInList)
                {
                    addDrawerBoxRow(baseCab, "Trash Drawer", dbxHeight, dbxWidth, dbxDepth);
                }

                if (baseCab.IncDrwBoxes)
                {
                    var rotateGroup =  BuildDrawerBoxRotateGroup(dbxWidth, dbxHeight, dbxDepth, MaterialThickness34, baseCab, "", false);
                    var trashDrawer = new Model3DGroup();
                    trashDrawer.Children.Add(rotateGroup);
                    ModelTransforms.ApplyTransform(trashDrawer, (dbxWidth / 2) - MaterialThickness34, MaterialThickness34 + tk_Height + 0.5906, interiorDepth + backThickness, 0, 0, 0);
                    cabinet.Children.Add(trashDrawer);
                }
            }
        }
    }

}
