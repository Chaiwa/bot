﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixelLab.Common;

namespace PixelLab.SL.Demo {
  public partial class MainPage : UserControl {
    public MainPage() {
      InitializeComponent();

      var pageSize = new Size(200, 400);

      m_flip.SetChild(m_contentButton = new Button() { Background = Colors.Blue.ToCachedBrush(), FontSize = 40 });
      m_flip.Size = pageSize;
      m_flip.FlipStarting += (sender, args) => {
        var oldValue = args.OldValue as int?;
        var newValue = args.NewValue as int?;

        if (oldValue != null && newValue != null && oldValue.Value != newValue.Value) {
          args.FlipDirection = (newValue.Value > oldValue.Value) ? FlipDirection.Next : FlipDirection.Previous;
        }
      };

      m_contentButton.WatchDataContextChanged((oldValue, newValue) => {
        m_contentButton.Content = newValue;
      });

      m_contentButton.Click += (sender, args) => {
        DataContext = ++value;
      };

      DataContext = value;
    }

    private int value = 0;
    private readonly Button m_contentButton;
  }
}
