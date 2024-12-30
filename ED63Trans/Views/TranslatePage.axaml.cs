#region

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System.Windows.Input;

#endregion

namespace ED63Trans.Views;

public partial class TranslatePage : UserControl
{
    public static readonly StyledProperty<ICommand?> DropClmCommandProperty =
        AvaloniaProperty.Register<TranslatePage, ICommand?>(
            nameof(DropClmCommand));

    public TranslatePage()
    {
        InitializeComponent();
        AddHandler(DragDrop.DragEnterEvent, DragDrop_DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragDrop_DragLeave);
        AddHandler(DragDrop.DropEvent, DragDrop_DragDrop);
        HotKeyManager.SetHotKey(transCurrentButton, new KeyGesture(Key.R, KeyModifiers.Control));
    }

    public ICommand? DropClmCommand
    {
        get => GetValue(DropClmCommandProperty);
        set => SetValue(DropClmCommandProperty, value);
    }

    private void DragDrop_DragDrop(object? sender, DragEventArgs e)
    {
        dragRect.Classes.Remove("drag");
        DropClmCommand?.Execute(e);
    }

    private void DragDrop_DragLeave(object? sender, DragEventArgs e)
    {
        dragRect.Classes.Remove("drag");
    }

    private void DragDrop_DragEnter(object? sender, DragEventArgs e)
    {
        dragRect.Classes.Add("drag");
    }

    private void TextBox_OnTextChanging(object? sender, TextChangingEventArgs e)
    {
        var textBox = (TextBox)sender!;
        
    }
}