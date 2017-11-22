using ChattingRobot.Util;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Json;

namespace ChattingRobot.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {

        //private string _chattingText = "";

        //public string ChattingText
        //{
        //    get { return _chattingText; }
        //    set
        //    {
        //        if (value == _chattingText)
        //        {
        //            return;
        //        }
        //        _chattingText = value;
        //        RaisePropertyChanged(() => ChattingText);
        //    }
        //}

        private string _currentMessage = "";

        public string CurrentMessage
        {
            get { return _currentMessage; }
            set
            {
                if (value == _currentMessage)
                {
                    return;
                }
                _currentMessage = value;
                RaisePropertyChanged(() => CurrentMessage);
            }
        }

        private bool _waitingMessage;

        public bool WaitingMessage
        {
            get { return _waitingMessage; }
            set
            {
                if (value == _waitingMessage)
                {
                    return;
                }
                _waitingMessage = value;
                RaisePropertyChanged(() => WaitingMessage);
            }
        }

        private bool _isRecording;

        public bool IsRecording
        {
            get { return _isRecording; }
            set
            {
                if (value == _isRecording)
                {
                    return;
                }
                _isRecording = value;
                RaisePropertyChanged(() => IsRecording);
            }
        }


        private ObservableCollection<MessageObject> _messages = new ObservableCollection<MessageObject>();

        public ObservableCollection<MessageObject> Messages
        {
            get { return _messages; }
            set
            {
                if (value == _messages)
                {
                    return;
                }
                _messages = value;
                RaisePropertyChanged(() => Messages);
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            SendMessageHandler = new RelayCommand(async () =>
            {
                WaitingMessage = true;
                await SendMessage();
                WaitingMessage = false;
            },
            ()=>
            {
                return !WaitingMessage && !IsRecording;
            });

            WindowLoadedHandler = new RelayCommand(() =>
            {
                Messenger.Default.Send<object>(null, MainWindowMessages.LoadedFromViewModel);
                AppendMessage(ChatterType.Robot, "Ruby很高兴能够与你交谈！");
            });

            InputEnterDownHandler = new RelayCommand<bool>(async (enterDown)=>
            {
                if(enterDown)
                {
                    WaitingMessage = true;
                    await SendMessage();
                    WaitingMessage = false;
                }
            },
            (enterDown)=>
            {
                return enterDown && !WaitingMessage && !IsRecording;
            });

            RecordingHandler = new RelayCommand(() =>
            {
                IsRecording = !IsRecording;
                if (IsRecording)
                {
                    try
                    {
                        ChattingHelper.StartRecording();
                    }
                    catch (Exception e)
                    {
                        AppendMessage(ChatterType.Robot, String.Format("哎呀！Ruby好像遇到了点问题？\n错误信息:\n{0}", e.ToString()));
                    }
                }
                else
                {
                    try
                    {
                        ChattingHelper.StopRecording();
                        CurrentMessage = ChattingHelper.RecordText;
                    }
                    catch (Exception e)
                    {
                        AppendMessage(ChatterType.Robot, String.Format("哎呀！Ruby好像遇到了点问题？\n错误信息:\n{0}", e.ToString()));
                    }
                }

            },
            () =>
            {
                return !WaitingMessage;
            });


            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
        }

        private async System.Threading.Tasks.Task SendMessage()
        {
            if(CurrentMessage == "" || CurrentMessage == null)
            {
                return;
            }

            var msg = CurrentMessage;
            CurrentMessage = "";

            AppendMessage(ChatterType.People, msg);

            var resultMsg = await ChattingHelper.SendMessageAsync(msg);

            if(resultMsg == "")
            {
                AppendMessage(ChatterType.Robot, "哎呀，你的网络访问好像失败了，Ruby现在无法和你进行对话哦。");
            }
            else
            {
                var jobj = JsonObject.Parse(resultMsg);
                AppendMessage(ChatterType.Robot, jobj["text"]);
            }

            Messenger.Default.Send<object>(null, MainWindowMessages.ScrollToEndFromViewModel);

        }

        private void AppendMessage(ChatterType type, string message)
        {
            switch (type)
            {
                case ChatterType.People:
                    //ChattingText += ("我：" + message + "\n");
                    Messages.Add(new MessageObject()
                    {
                        ChatterType = ChatterType.People,
                        MessageText = message,
                        TimeStamp = DateTime.Now.ToString(),
                    });
                    break;
                case ChatterType.Robot:
                    //ChattingText += ("Ruby：" + message + "\n");
                    Messages.Add(new MessageObject()
                    {
                        ChatterType = ChatterType.Robot,
                        MessageText = message,
                        TimeStamp = DateTime.Now.ToString(),
                    });
                    break;
                default:
                    break;
            }
        }

        public RelayCommand SendMessageHandler { get; set; }
        public RelayCommand<bool> InputEnterDownHandler { get; set; }
        public RelayCommand WindowLoadedHandler { get; set; }
        public RelayCommand RecordingHandler { get; set; }

    }
}