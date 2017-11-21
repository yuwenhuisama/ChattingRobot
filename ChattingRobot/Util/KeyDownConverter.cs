using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChattingRobot.Util
{

    public class KeyDownConverter : IEventArgsConverter
    {
        public object Convert(object value, object parameter)
        {
            //var source = parameter as System.Windows.Controls.TextBox;
            var args = value as KeyEventArgs;

            return args.Key == Key.Enter;
        }
    }

}
