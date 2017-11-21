using ChattingRobot.Util;
using ChattingRobot.ViewModel;
using GalaSoft.MvvmLight.Messaging;
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

namespace ChattingRobot.AppUserControl
{
    /// <summary>
    /// UserMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class UserMessageBox : UserControl
    {

        public ChatterType ChatterType
        {
            get { return (ChatterType)GetValue(ChatterTypeProperty); }
            set
            {
                if (value == (ChatterType)GetValue(ChatterTypeProperty))
                {
                    return;
                }
                SetValue(ChatterTypeProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for ChatterType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChatterTypeProperty =
            DependencyProperty.Register("ChatterType", typeof(ChatterType), typeof(UserMessageBox), new PropertyMetadata(OnChatterTypeChanged));

        private static void OnChatterTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var con = d as UserMessageBox;
            con.UniqueMessenger.Send<ChatterType>((ChatterType)e.NewValue, UserMessageBoxMessages.ChatterTypeChangedFromView);
        }


        public string MessageText
        {
            get { return (string)GetValue(MessageTextProperty); }
            set
            {
                if (value == (string)GetValue(MessageTextProperty))
                {
                    return;
                }
                SetValue(MessageTextProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for MessageText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageTextProperty =
            DependencyProperty.Register("MessageText", typeof(string), typeof(UserMessageBox), new PropertyMetadata(OnMessageTextChanged));

        private static void OnMessageTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var con = d as UserMessageBox;
            con.UniqueMessenger.Send<string>(e.NewValue as string, UserMessageBoxMessages.MessageTextChangedFromView);
        }


        public string TimeStamp
        {
            get { return (string)GetValue(TimeStampProperty); }
            set
            {
                if (value == (string)GetValue(TimeStampProperty))
                {
                    return;
                }
                SetValue(TimeStampProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for TimeStamp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeStampProperty =
            DependencyProperty.Register("TimeStamp", typeof(string), typeof(UserMessageBox), new PropertyMetadata(OnTimeStampChanged));

        private static void OnTimeStampChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var con = d as UserMessageBox;
            con.UniqueMessenger.Send<string>(e.NewValue as string, UserMessageBoxMessages.TimeStampChangedFromView);
        }

        private Messenger UniqueMessenger { get; set; }

        public UserMessageBox()
        {
            InitializeComponent();

            UniqueMessenger = this.Resources["UniqueMessenger"] as Messenger;
            var viewModel = this.Resources["UniqueViewModel"] as UserMessageBoxControlViewModel;
            viewModel.UniqueMessenger = UniqueMessenger;
            viewModel.InitMessenger();

        }
    }
}
