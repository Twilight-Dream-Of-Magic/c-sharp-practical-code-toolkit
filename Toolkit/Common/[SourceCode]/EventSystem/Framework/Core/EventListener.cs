// You are currently viewing the code for the Event System module
// Project Name: C# Practical code toolkit by Twilight-Dream
// Copyright 2021@Twilight-Dream. All rights reserved.
// Gmail: yujiang1187791459@gmail.com
// Github: https://github.com/Twilight-Dream-Of-Magic
// Git Repository: https://github.com/Twilight-Dream-Of-Magic/c-sharp-practical-code-toolkit
// China Tencent QQ Mail: 1187791459@qq.com
// China Bilibili Video Space: https://space.bilibili.com/21974189
// China Bilibili Live Space: https://live.bilibili.com/1210760

using System;
using System.Collections.Generic;

namespace Twilight_Dream.EventSystem.Framework.Core
{
	public class EventListener
	{
		/// <summary>
		/// Delegate of the event handler (Event processor)
		/// </summary>
		/// <param name="eventArgs"></param>
		public delegate void EventHandler(MyEventArgs eventArgs);

		/// <summary>
		/// Collection of event handlers (Event processors)
		/// </summary>
		public EventHandler eventHandler;

		public void Invoke(MyEventArgs eventArgs)
		{
			if (eventHandler != null)
			{
				eventHandler.Invoke(eventArgs);
			}
		}

		/// <summary>
		/// Clear delegates for all events
		/// </summary>
		public void ClearAll()
		{
			eventHandler = null;
		}
	}
}
