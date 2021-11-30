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

namespace Twilight_Dream.Conversion.DataFormat
{
    public static class HexUnsafeHelper
    {
        [System.Diagnostics.Contracts.Pure]
        public static unsafe string ToHex(this byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            const string alphabet = @"0123456789ABCDEF";

            string result = new string(' ', checked(value.Length * 2));
            fixed (char* alphabetPointer = alphabet)
            fixed (char* resultPointer = result)
            {
                char* pointer = resultPointer;
                unchecked
                {
                    for (int index = 0; index < value.Length; index++)
                    {
                        *pointer++ = *(alphabetPointer + (value[index] >> 4));
                        *pointer++ = *(alphabetPointer + (value[index] & 0xF));
                    }
                }
            }
            return result;
        }

        [System.Diagnostics.Contracts.Pure]
        public static unsafe byte[] FromHex(this string textValue)
        {
            if (textValue == null)
                throw new ArgumentNullException("value");
            if (textValue.Length % 2 != 0)
                throw new ArgumentException("Hexadecimal value length must be even.", "value");

            unchecked
            {
                byte[] result = new byte[textValue.Length / 2];
                fixed (char* valuePointer = textValue)
                {
                    char* integerValuePointer = valuePointer;
                    for (int index = 0; index < result.Length; index++)
                    {
                        // 0(48) - 9(57) -> 0 - 9
                        // A(65) - F(70) -> 10 - 15
                        int bit = *integerValuePointer++; // High 4 bits.
                        int integerValue = ((bit - '0') + ((('9' - bit) >> 31) & -7)) << 4;
                        bit = *integerValuePointer++; // Low 4 bits.
                        integerValue += (bit - '0') + ((('9' - bit) >> 31) & -7);
                        result[index] = checked((byte)integerValue);
                    }
                }
                return result;
            }
        }
    }
}
