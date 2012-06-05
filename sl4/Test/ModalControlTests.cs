﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelLab.Common;
using PixelLab.SL;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Test.SL
{
    [TestClass]
    public class ModalControlTests : SilverlightTest
    {
        private readonly ModalControl _modalControl = new ModalControl();
        private readonly Control _baseContent = new Button
        {
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            Content = "Hello!",
            FontSize = 50,
            Background = Colors.Blue.ToCachedBrush(),
            Padding = new Thickness(20)
        };

        private Grid _contentHolder;

        private IModalToken _token1, _token2;

        [ClassInitialize]
        public void TestInit()
        {
            TestGrid.Children.Add(_baseContent);
            _modalControl.SetTarget(_baseContent);
            TestGrid.Children.Add(_modalControl);
        }

        [TestMethod, Asynchronous, Timeout(10 * 1000)]
        public void TestModal()
        {
            Assert.IsTrue(_modalControl.IsTargetSet);
            Assert.IsFalse(_modalControl.IsOpen);

            var content1 = GetContent("Content 1");

            _token1 = _modalControl.Open(content1, ModalPosition.Center, null);

            Assert.IsTrue(_modalControl.IsOpen);

            timerDelay(() => t1_afterOpen(content1));
        }

        private void t1_afterOpen(FrameworkElement content1)
        {
            var wrapperChild = (Grid)content1.Parent;
            var wrapper = (FrameworkElement)wrapperChild.Parent;
            _contentHolder = (Grid)wrapper.Parent;

            Assert.AreEqual("ContentElement", _contentHolder.Name);

            Assert.AreEqual(1, _contentHolder.Children.Count);
            Assert.IsTrue(_baseContent.Opacity == 1);

            timerDelay(t2_doClose);
        }

        private void t2_doClose()
        {
            _modalControl.Close(_token1);

            timerDelay(t3_checkClose);
        }

        private void t3_checkClose()
        {
            Assert.IsFalse(_modalControl.IsOpen);
            Assert.AreEqual(0, _contentHolder.Children.Count);

            timerDelay(t4_reopen);
        }

        private void t4_reopen()
        {
            var content1 = GetContent("Content 1");
            _token1 = _modalControl.Open(content1, ModalPosition.Center, null);

            Assert.IsTrue(_modalControl.IsOpen);

            timerDelay(t5_afterOpen);
        }

        private void t5_afterOpen()
        {
            Assert.IsTrue(_modalControl.IsOpen);

            var content2 = GetContent("Content 2");

            _token2 = _modalControl.Open(content2, ModalPosition.Center, null);

            timerDelay(t6_afterOpen2);
        }

        private void t6_afterOpen2()
        {
            _modalControl.Close(_token2);

            timerDelay(t7_after2ndClosed);
        }

        private void t7_after2ndClosed()
        {
            Assert.AreEqual(1, _contentHolder.Children.Count);
            Assert.IsTrue(_modalControl.IsOpen);

            _modalControl.Close(_token1);
            timerDelay(t8_after1stClosed);
        }

        private void t8_after1stClosed()
        {
            Assert.AreEqual(0, _contentHolder.Children.Count);
            Assert.IsFalse(_modalControl.IsOpen);
            var content1 = GetContent("Content 1");
            _token1 = _modalControl.Open(content1, ModalPosition.Center, null);

            Assert.IsTrue(_modalControl.IsOpen);

            timerDelay(t9_afterOpen);
        }

        private void t9_afterOpen()
        {
            Assert.IsTrue(_modalControl.IsOpen);

            var content2 = GetContent("Content 2");

            _token2 = _modalControl.Open(content2, ModalPosition.Center, null, true);

            timerDelay(t10_afterOpen2);
        }

        private void t10_afterOpen2()
        {
            Assert.AreEqual(2, _contentHolder.Children.Count);

            _modalControl.Close(_token1);

            timerDelay(t11_afterClose);
        }

        private void t11_afterClose()
        {
            EnqueueTestComplete();
            Assert.AreEqual(0, _contentHolder.Children.Count);
        }

        private Grid TestGrid
        {
            get { return (Grid)TestPanel; }
        }

        private static void timerDelay(Action action, double seconds = 0.5)
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(seconds) };
            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                action();
            };
            timer.Start();
        }

        private static FrameworkElement GetContent(string text)
        {
            Contract.Requires(!text.IsNullOrWhiteSpace());
            return new Border
            {
                Child = new TextBlock
                {
                    Text = text,
                    FontSize = 20
                },
                Padding = new Thickness(10),
                Background = Colors.LightGray.ToCachedBrush(),
                BorderBrush = Colors.DarkGray.ToCachedBrush(),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(5),
                Opacity = 0.9
            };
        }
    }
}
