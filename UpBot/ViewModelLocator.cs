using System.Windows;

namespace UpBot;

public static class ViewModelLocator
{
    public static readonly DependencyProperty AutoWireViewModelProperty =
        DependencyProperty.RegisterAttached(
            "AutoWireViewModel",
            typeof(bool),
            typeof(ViewModelLocator),
            new PropertyMetadata(false, OnAutoWireViewModelChanged));

    public static bool GetAutoWireViewModel(DependencyObject obj) =>
        (bool)obj.GetValue(AutoWireViewModelProperty);

    public static void SetAutoWireViewModel(DependencyObject obj, bool value) =>
        obj.SetValue(AutoWireViewModelProperty, value);

    private static void OnAutoWireViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue)
        {
            var viewType = d.GetType();
            var viewName = viewType.Name;

            // 규칙: View 이름 → ViewModel 이름 (예: MainWindow → MainWindowViewModel)
            var viewModelName = $"{viewName}ViewModel";

            // ViewModel 네임스페이스: UpBot.ViewModels 및 하위 네임스페이스 모두 검사
            var assembly = viewType.Assembly;
            var viewModelType = assembly.GetTypes()
                .FirstOrDefault(t =>
                    t.Name == viewModelName &&
                    t.Namespace != null &&
                    t.Namespace.StartsWith("UpBot.ViewModels"));

            if (viewModelType != null)
            {
                var vm = App.ServiceProvider.GetService(viewModelType);
                if (vm != null && d is FrameworkElement fe)
                    fe.DataContext = vm;
            }
        }
    }
}

