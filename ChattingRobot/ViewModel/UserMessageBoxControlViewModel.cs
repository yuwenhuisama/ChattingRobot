using ChattingRobot.Util;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChattingRobot.ViewModel
{
    public class UserMessageBoxControlViewModel : ViewModelBase
    {
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


        public Messenger UniqueMessenger { get; set; }

        public void InitMessenger()
        {
            UniqueMessenger.Register<ChatterType>(this, UserMessageBoxMessages.ChatterTypeChangedFromView, (type) =>
            {
                ChatterType = type;
            });

            UniqueMessenger.Register<string>(this, UserMessageBoxMessages.MessageTextChangedFromView, (msg) =>
            {
                MessageText = msg;
            });

            UniqueMessenger.Register<string>(this, UserMessageBoxMessages.TimeStampChangedFromView, (time) =>
            {
                TimeStamp = time;
            });
        }

        public UserMessageBoxControlViewModel()
        {
        }

    }
}
