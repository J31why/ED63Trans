using Avalonia.Controls;

namespace ED63Trans.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    private void TextBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        var tb = (TextBox)sender!;
        tb.CaretIndex = int.MaxValue;
    }
}