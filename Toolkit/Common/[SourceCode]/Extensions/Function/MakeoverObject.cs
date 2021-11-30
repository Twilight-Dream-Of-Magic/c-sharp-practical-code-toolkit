// You are currently viewing the code for the Object Pooling module
// Project Name: C# Practical code toolkit by Twilight-Dream
// Copyright 2021@Twilight-Dream. All rights reserved.
// Gmail: yujiang1187791459@gmail.com
// Github: https://github.com/Twilight-Dream-Of-Magic
// Git Repository: https://github.com/Twilight-Dream-Of-Magic/c-sharp-practical-code-toolkit
// China Tencent QQ Mail: 1187791459@qq.com
// China Bilibili Video Space: https://space.bilibili.com/21974189
// China Bilibili Live Space: https://live.bilibili.com/1210760

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Twilight_Dream.Extensions.Function
{
	public static class MakeoverObject
	{
        public static T CloneObjectByReflection<T>(this T source) where T : class
		{
            Type sourceType = source.GetType();
            T target = Activator.CreateInstance<T>();
            Type targetType = target.GetType();

            //We get the array of fields for the new type instance.
            FieldInfo[] targetFieldInfos = targetType.GetFields();

            int index = 0;

            foreach (FieldInfo sourceFieldInfo in sourceType.GetFields())
            {
                Type sourceFieldInfoType = sourceFieldInfo.FieldType;
                //We query if the fiels support the ICloneable interface.
                Type ICloneType = sourceFieldInfoType.GetInterface("ICloneable", true);

                if (ICloneType != null)
                {
                    //Getting the ICloneable interface from the object.
                    ICloneable IClone = (ICloneable)sourceFieldInfo.GetValue(source);

                    //We use the clone method to set the new value to the field.
                    targetFieldInfos[index].SetValue(target, IClone.Clone());
                }
                else
                {
                    // If the field doesn't support the ICloneable 
                    // interface then just set it.
                    targetFieldInfos[index].SetValue(target, sourceFieldInfo.GetValue(source));
                }

                //Now we check if the object support the 
                //IEnumerable interface, so if it does
                //we need to enumerate all its items and check if 
                //they support the ICloneable interface.
                Type IEnumerableType = sourceFieldInfoType.GetInterface("IEnumerable", true);
                if (IEnumerableType != null)
                {
                    //Get the IEnumerable interface from the field.
                    IEnumerable<T> IEnum = (IEnumerable<T>)sourceFieldInfo.GetValue(source);

                    //This version support the IList and the 
                    //IDictionary interfaces to iterate on collections.
                    Type IListType = targetFieldInfos[index].FieldType.GetInterface("IList", true);
                    Type IDictionaryType = targetFieldInfos[index].FieldType.GetInterface("IDictionary", true);

                    int index2 = 0;
                    if (IListType != null)
                    {
                        //Getting the IList interface.
                        IList<T> list = (IList<T>)targetFieldInfos[index].GetValue(target);

                        foreach (var item in IEnum)
                        {
                            //Checking to see if the current item 
                            //support the ICloneable interface.
                            object obj = item as object;
                            Type type2 = obj.GetType(); 
                            ICloneType = type2.GetInterface("ICloneable", true);

                            if (ICloneType != null)
                            {
                                //If it does support the ICloneable interface, 
                                //we use it to set the clone of
                                //the object in the list.
                                ICloneable clone = (ICloneable)obj;

                                list[index2] = (T)clone.Clone();
                            }

                            //NOTE: If the item in the list is not 
                            //support the ICloneable interface then in the 
                            //cloned list this item will be the same 
                            //item as in the original list
                            //(as long as this type is a reference type).

                            index2++;
                        }
                    }
                    else if (IDictionaryType != null)
                    {
                        //Getting the dictionary interface.
                        IDictionary dictionary = (IDictionary)targetFieldInfos[index].GetValue(target);
                        index2 = 0;

                        foreach (var item in IEnum)
                        {
                            //Checking to see if the item 
                            //support the ICloneable interface.
                            object obj = item as object;
                            DictionaryEntry de = (DictionaryEntry)obj;
                            Type type3 = de.Value.GetType();
                            ICloneType = type3.GetInterface("ICloneable", true);

                            if (ICloneType != null)
                            {
                                ICloneable clone = (ICloneable)de.Value;

                                dictionary[de.Key] = clone.Clone();
                            }
                            index2++;
                        }
                    }
                }
                index++;
            }
            return target;
		}

        public static T CloneObjectByBinaryData<T>(this T source) where T : class
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, source);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(memoryStream);
            }
        }

        public static T CloneObjectByNewtonSoftJSON<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
            var serializeSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source, serializeSettings), deserializeSettings);
        }
    }
}
