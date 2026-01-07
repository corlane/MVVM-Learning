# Contributing

## Overview
This repository contains a WPF MVVM application.

## Guidelines
- Keep changes small and focused.
- Prefer MVVM patterns (ViewModels contain behavior/state; Views contain layout and bindings).
- Use consistent naming for Views/ViewModels.

## Standards

### Process Order “Exception Tabs” Pattern
For tabs in `REALLYProcessOrderView.xaml` that show job-wide exceptions (like `Toekick` and `Edgebanding`), implement new tabs using the same pattern for consistent UX:

#### Required structure
- Create a dedicated View:
  - `PO<Feature>View.xaml`
  - `PO<Feature>View.xaml.cs`
- Create a dedicated ViewModel:
  - `PO<Feature>ViewModel`

#### ViewModel requirements
- Expose a baseline/default comparison property (e.g., `DefaultTkHeight`, `DefaultEbSpecies`).
- Expose an `ObservableCollection<...Row>` of exception rows.
- Each row includes an `IsDone` boolean (drives “Done” toggle in the grid).
- Expose `Brush TabHeaderBrush`:
  - Green when there are no exceptions OR all rows are marked done.
  - Red when there is at least one exception not marked done.
- Implement `Refresh()` and keep it in sync by subscribing to `ICabinetService.Cabinets.CollectionChanged`.

#### View requirements
- Provide UI to edit the baseline/default comparison value.
- Display a `DataGrid` bound to the exception rows.
- Include a `Done` toggle column bound to `IsDone`.
- Optionally show a footer summary (e.g., `TotalCabsNeedingChange`).

#### Wiring requirements
- Register the ViewModel with DI in `App.xaml.cs`.
- Add the ViewModel to `REALLYProcessOrderViewModel` via constructor injection and an exposed property (e.g., `PO<Feature>Vm`).
- In `REALLYProcessOrderView.xaml`:
  - Use a custom `TabItem.Header` with a `TextBlock` whose `Foreground` is bound to `PO<Feature>Vm.TabHeaderBrush`.
  - Host the tab content with `<views:PO<Feature>View DataContext="{Binding PO<Feature>Vm}"/>`.