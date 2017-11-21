﻿using ChattingRobot.Util;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChattingRobot
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<object>(this, MainWindowMessages.LoadedFromViewModel, (dummy) => {
                txt_Message.Focus();
            });

            Messenger.Default.Register<object>(this, MainWindowMessages.ScrollToEndFromViewModel, (dummy) =>
            {
                scroll.ScrollToEnd();
            });

        }
    }
}