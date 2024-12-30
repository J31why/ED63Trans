using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ED63Trans.ViewModels;

namespace ED63Trans.Views;

public partial class MsTransView : UserControl
{
    public MsTransView()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var vm = DataContext as MsTransViewModel;
        var data = ((Button)sender!).DataContext;
        vm.DeleteMsCommand.Execute(data);
    }
}