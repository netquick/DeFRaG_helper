﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeFRaG_Helper
{
    public static class MessagingService
    {
        public static event Action<string> MessageReceived;

        public static void SendMessage(string message)
        {
            MessageReceived?.Invoke(message);
        }

        public static void Subscribe(Action<string> messageHandler)
        {
            MessageReceived += messageHandler;
        }

        public static void Unsubscribe(Action<string> messageHandler)
        {
            MessageReceived -= messageHandler;
        }
    }
}
