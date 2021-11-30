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
    public static class HexHelper
    {
        [System.Diagnostics.Contracts.Pure]
        public static string ToHex(this byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            const string hexAlphabet = @"0123456789ABCDEF";

            var chars = new char[checked(value.Length * 2)];
            unchecked
            {
                for (int index = 0; index < value.Length; index++)
                {
                    chars[index * 2] = hexAlphabet[value[index] >> 4];
                    chars[index * 2 + 1] = hexAlphabet[value[index] & 0xF];
                }
            }
            return new string(chars);
        }

        [System.Diagnostics.Contracts.Pure]
        public static byte[] FromHex(this string textValue)
        {
            if (textValue == null)
                throw new ArgumentNullException("value");
            if (textValue.Length % 2 != 0)
                throw new ArgumentException("Hexadecimal value length must be even.", "value");

            unchecked
            {
                byte[] result = new byte[textValue.Length / 2];
                for (int index = 0; index < result.Length; index++)
                {
                    // 0(48) - 9(57) -> 0 - 9
                    // A(65) - F(70) -> 10 - 15
                    int bit = textValue[index * 2]; // High 4 bits.
                    int integerValue = ((bit - '0') + ((('9' - bit) >> 31) & -7)) << 4;
                    bit = textValue[index * 2 + 1]; // Low 4 bits.
                    integerValue += (bit - '0') + ((('9' - bit) >> 31) & -7);
                    result[index] = checked((byte)integerValue);
                }
                return result;
            }
        }
    }
}
