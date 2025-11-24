using System;
using System.Collections.Generic;
using System.Text;

namespace MVVM_Learning.Models;

public static class ComboLists
{
    public static List<string> ListCabSpecies { get; } =
    [
        "Prefinished Ply",
        "Maple Ply",
        "Red Oak Ply",
        "White Oak Ply",
        "Cherry Ply",
        "Alder Ply",
        "Mahogany Ply",
        "Walnut Ply",
        "Hickory Ply",
        "MDF",
        "Melamine",
        "Custom"
    ];

    public static List<string> ListEBSpecies { get; }  =
    [
        "None",
        "PVC White",
        "PVC Black",
        "PVC Hardrock Maple",
        "PVC Paint Grade",
        "Wood Prefinished Maple",
        "Wood Maple",
        "Wood Red Oak",
        "Wood White Oak",
        "Wood Walnut",
        "Wood Cherry",
        "Wood Alder",
        "Wood Hickory",
        "Wood Mahogany",
        "Custom"
    ];

}
