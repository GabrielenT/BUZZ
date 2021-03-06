﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BUZZ.Core.Models;
using BUZZ.Core.Thumbnails;

namespace BUZZ.Core.Multiboxing
{
    /// <summary>
    /// Interaction logic for PullerView.xaml
    /// </summary>
    public partial class PullerView : UserControl
    {
        public readonly PullerViewModel CurrentViewModel;
        private readonly Window ParentWindow;

        public PullerView(BuzzCharacter buzzCharacter, Window parentWindow)
        {
            InitializeComponent();
            CurrentViewModel = new PullerViewModel(buzzCharacter);
            this.DataContext = CurrentViewModel;
            buzzCharacter.SystemInformationUpdated += BuzzCharacterSystemInformationUpdated;

            ParentWindow = parentWindow;

            SizeChanged += (s, e) => { CurrentViewModel.ClientSizeChanged(GetRenderArea()); };

            this.Loaded += (sender, args) =>
            {
                var renderArea = GetRenderArea();
                CurrentViewModel.InitializeThumbnailInfo(new WindowInteropHelper(Window.GetWindow(this)).Handle,
                    renderArea);
            };
        }

        private void BuzzCharacterSystemInformationUpdated(object sender, Models.Events.SystemUpdatedEventArgs e)
        {
            if (e.OldSystemName == e.NewSystemName) return;

            AnimateBackground(BackgroundGrid, Colors.LightGreen,Colors.Transparent, TimeSpan.FromSeconds(30),1);
        }

        private void BringCharacterToForeground(object sender, MouseButtonEventArgs e)
        {
            try
            {
                CurrentViewModel.MakePullerActiveWindow();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        public DwmClass.Rect GetRenderArea()
        {
            var currentControlOffset = this.TranslatePoint(new Point(0, 0), ParentWindow);

            return new DwmClass.Rect()
            {
                Left = (int)currentControlOffset.X + (int)ViewGrid.Margin.Left,
                Top = (int)currentControlOffset.Y + (int)ViewGrid.Margin.Top + (int)(this.ActualHeight * (1.0 / 3.0)),
                Right = (int)currentControlOffset.X + (int)this.ActualWidth - (int)ViewGrid.Margin.Right,
                Bottom = (int)currentControlOffset.Y + (int)this.ActualHeight - (int)ViewGrid.Margin.Bottom
            };
        }

        public void AnimateBackground(Panel targetPanel, Color fromColor, Color toColor, TimeSpan duration, double startingOpacity)
        {
            var colorAnimation = new ColorAnimation(toColor, duration);
            targetPanel.Opacity = startingOpacity;
            targetPanel.Background = new SolidColorBrush(fromColor);
            targetPanel.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void CharacterNameLabel_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CurrentViewModel.OptimizeRouteFromClipboard();
        }
    }
}
