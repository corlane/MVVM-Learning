using CorlaneCabinetOrderFormV3.Converters;
using CorlaneCabinetOrderFormV3.Models;
using System.Reflection;

namespace CorlaneCabinetOrderFormV3.ViewModels;

/// <summary>
/// Reflection-based model → ViewModel property mapper shared by all cabinet ViewModels.
/// </summary>
internal static class ViewModelMappingHelper
{
    private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

    /// <summary>
    /// Copies all matching public properties from <paramref name="model"/> to <paramref name="vm"/>,
    /// applying dimension formatting to properties whose names appear in <paramref name="dimensionProperties"/>.
    /// </summary>
    internal static void MapModelToViewModel(
        object vm,
        CabinetModel model,
        string dimFormat,
        HashSet<string> dimensionProperties)
    {
        var vmType = vm.GetType();
        var modelType = model.GetType();

        foreach (var modelProp in modelType.GetProperties(PublicInstance))
        {
            var vmProp = vmType.GetProperty(modelProp.Name, PublicInstance);
            if (vmProp is null || !vmProp.CanWrite) continue;

            var modelValue = modelProp.GetValue(model);
            if (modelValue is null)
            {
                vmProp.SetValue(vm, null);
                continue;
            }

            if (vmProp.PropertyType == typeof(string))
            {
                var raw = modelValue.ToString() ?? "";

                if (dimensionProperties.Contains(modelProp.Name))
                {
                    double numeric = ConvertDimension.FractionToDouble(raw);
                    vmProp.SetValue(vm, string.Equals(dimFormat, "Fraction", StringComparison.OrdinalIgnoreCase)
                        ? ConvertDimension.DoubleToFraction(numeric)
                        : numeric.ToString());
                }
                else
                {
                    vmProp.SetValue(vm, raw);
                }
            }
            else if (vmProp.PropertyType == typeof(int))
            {
                if (modelValue is int i) vmProp.SetValue(vm, i);
                else if (int.TryParse(modelValue.ToString(), out var v)) vmProp.SetValue(vm, v);
            }
            else if (vmProp.PropertyType == typeof(bool))
            {
                if (modelValue is bool b) vmProp.SetValue(vm, b);
                else if (bool.TryParse(modelValue.ToString(), out var vb)) vmProp.SetValue(vm, vb);
            }
            else
            {
                vmProp.SetValue(vm, modelValue);
            }
        }
    }
}