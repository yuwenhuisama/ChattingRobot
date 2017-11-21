using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChattingRobot.Util
{
    public class MessageObject : ObservableObject
    {
        private string _messageText;

        public string MessageText
        {
            get { return _messageText; }
            set
            {
                if (value == _messageText)
                {
                    return;
                }
                _messageText = value;
                RaisePropertyChanged(() => MessageText);
            }
        }

        private ChatterType _chatterType;

        public ChatterType ChatterType
        {
            get { return _chatterType; }
            set
            {
                if (value == _chatterType)
                {
                    return;
                }
                _chatterType = value;
                RaisePropertyChanged(() => ChatterType);
            }
        }

        private string _timeStamp;

        public string TimeStamp
        {
            get { return _timeStamp; }
            set
            {
                if (value == _timeStamp)
                {
                    return;
                }
                _timeStamp = value;
                RaisePropertyChanged(() => TimeStamp);
            }
        }


    }
}
