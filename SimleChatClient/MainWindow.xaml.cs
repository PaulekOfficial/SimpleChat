﻿using System.Windows;
using System.Windows.Input;
using SimpleChatClient;

namespace ChatClientGUI;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Client _client = ClientFactory.GetClientInstance();

    public MainWindow()
    {
        InitializeComponent();
    }

    private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;

        DragMove();
    }

    private void ButtonMinimalize_OnClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void ButtonMaximalize_OnClick(object sender, RoutedEventArgs e)
    {
        if (WindowState.Maximized == WindowState)
        {
            WindowState = WindowState.Normal;
            return;
        }

        WindowState = WindowState.Maximized;
    }

    private void ButtonExit_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}