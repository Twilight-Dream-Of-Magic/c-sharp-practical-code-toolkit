// You are currently viewing the code for the Object Pooling module
// Project Name: C# Practical code toolkit by Twilight-Dream
// Copyright 2050@Twilight-Dream. All rights reserved.
// Github: https://github.com/Twilight-Dream-Of-Magic
// Git Repository: https://github.com/Twilight-Dream-Of-Magic/c-sharp-practical-code-toolkit
// China Bilibili Video Space: https://space.bilibili.com/21974189
// China Bilibili Live Space: https://live.bilibili.com/1210760

using System;
using System.Collections.Generic;

namespace Twilight_Dream.EventSystem.Framework.Core
{
    public class MyEventArgs
    {
        /// <summary>
        /// The name of the event
        /// </summary>
        public readonly string _eventName;

        /// <summary>
        /// The argument of the event
        /// </summary>
        public readonly object[] _eventArgs;

        public MyEventArgs(string eventName)
        {
            this._eventName = eventName;
        }

        public MyEventArgs(string eventName, params object[] eventArgs)
        {
            this._eventName = eventName;
            this._eventArgs = eventArgs;
        }
    }
}
