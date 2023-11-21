// You are currently viewing the code for the Object Pooling module
// Project Name: C# Practical code toolkit by Twilight-Dream
// Copyright 2050@Twilight-Dream. All rights reserved.
// Github: https://github.com/Twilight-Dream-Of-Magic
// Git Repository: https://github.com/Twilight-Dream-Of-Magic/c-sharp-practical-code-toolkit
// China Bilibili Video Space: https://space.bilibili.com/21974189
// China Bilibili Live Space: https://live.bilibili.com/1210760

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

using Newtonsoft.Json;

namespace Twilight_Dream.Conversion.DataFormat
{
	public static class ObjectExchangeBinary
	{
        public enum ObjectExchangeBinaryWithDataFormatMode
        {
            JSON = 1,
            BINARY = 2,
		}

        public static byte[] ObjectSerializeToByteArray<ObjectType>(ObjectType instance, ObjectExchangeBinaryWithDataFormatMode mode)
        {
            if (instance != null)
            {
                if(mode == ObjectExchangeBinaryWithDataFormatMode.JSON)
				{
                    string json = JsonConvert.SerializeObject(instance);
                    if (json != null && json.Length > 0)
                    {
                        byte[] JSON_TextBytes = Encoding.UTF8.GetBytes(json);
                        string base64String = Convert.ToBase64String(JSON_TextBytes);
                        byte[] byteArray = Encoding.UTF8.GetBytes(base64String);

                        if (byteArray.Length > 0)
                        {
                            return byteArray;
                        }
                    }
                }
                if(mode == ObjectExchangeBinaryWithDataFormatMode.BINARY)
				{
                    var bf = new BinaryFormatter();
                    using (var ms = new MemoryStream())
                    {
                        try
                        {
                            bf.Serialize(ms, instance);
                            return ms.GetBuffer();
                        }
                        catch (Exception except)
                        {
                            throw except;
                        }
                    }
                }
            }
            return null;
        }

        public static ObjectType ByteArrayDeserializeToObject<ObjectType>(byte[] byteArray, ObjectExchangeBinaryWithDataFormatMode mode) where ObjectType : class
        {
            if (byteArray != null && byteArray.Length > 0)
            {
                if(mode == ObjectExchangeBinaryWithDataFormatMode.JSON)
				{
                    ObjectType instance = null;

                    string base64String = Encoding.UTF8.GetString(byteArray);
                    byte[] JSON_TextBytes = Convert.FromBase64String(base64String);
                    string json = Encoding.UTF8.GetString(JSON_TextBytes);

                    if (json != null && json.Length > 0)
                    {
                        instance = JsonConvert.DeserializeObject<ObjectType>(json);

                        if (instance != null)
                        {
                            return instance;
                        }
                    }
                }
                if(mode == ObjectExchangeBinaryWithDataFormatMode.BINARY)
				{
                    using (var ms = new MemoryStream())
                    {
                        var bf = new BinaryFormatter();
                        ms.Write(byteArray, 0, byteArray.Length);
                        ms.Seek(0, SeekOrigin.Begin);

                        try
                        {
                            ObjectType instance = bf.Deserialize(ms) as ObjectType;

                            if (instance != null)
                            {
                                return instance;
                            }
                        }
                        catch (Exception except)
                        {
                            throw except;
                        }
                    }
                }
            }
            return default(ObjectType);
        }
    }
}
