using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Data.Entity;
using System.Security.Cryptography;
using System.Text;

public static partial class ExtensionMethod {
    /// <summary>
    /// 判斷變數是否可用(Object=非NULL, 文字=非空字串及空白, 集合=項目數目大於0)
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool HasValue(this object obj) {
        var result = true;

        object nullableObj = DBNull.Value.Equals(obj) == true ? null : obj;

        if (nullableObj == null) {
            result = false;
        }
        //List
        else if (nullableObj.GetType().GetProperty("Count") != null) {
            if (nullableObj.GetType().GetProperty("Count").GetValue(nullableObj, null).ToInt32() == 0) {
                result = false;
            }
        }
        //Array
        else if (nullableObj.GetType().GetProperty("Length") != null) {
            if (nullableObj.GetType().GetProperty("Length").GetValue(nullableObj, null).ToInt32() == 0) {
                result = false;
            }
        }
        //XmlElement
        else if (nullableObj.GetType().Module.Name == "System.Xml.dll") {
            XmlElement castedObj = (XmlElement)nullableObj;

            if (castedObj.InnerXml.HasValue() == false) {
                if (Array.TrueForAll<XmlAttribute>(castedObj.Attributes.Cast<XmlAttribute>().ToArray(), a => a.Value.HasValue() == false) == true) {
                    return false;
                }
                else {
                    return true;
                }
            }
            else {
                return true;
            }
        }
        else {
            //Linq Iterator
            var iterator = nullableObj.GetType().GetInterface("IEnumerable"); ;
            if (iterator != null) {
                dynamic castedObj = Convert.ChangeType(nullableObj, nullableObj.GetType());

                result = false;
                foreach (var castedObjItem in castedObj) {
                    result = true;

                    break;
                }
            }
            else {
                string text = nullableObj.ToString();
                if (string.IsNullOrWhiteSpace(text) == true) {
                    result = false;
                }
            }
        }

        return result;
    }

    public static bool HasNoValue(this object obj) {
        return obj.HasValue() ? false : true;
    }

    public static string FormatWithArgs(this string str, params object[] args) {
        return string.Format(str, args);
    }

    public static bool ToBoolean(this object obj) {
        return Convert.ToBoolean(obj);
    }

    public static bool? ToNullableBoolean(this object obj) {
        return obj.HasValue() ? (bool?)obj.ToBoolean() : null;
    }

    public static int ToInt32(this object obj) {
        return Convert.ToInt32(obj);
    }

    public static int? ToNullableInt32(this object obj) {
        return obj.HasValue() ? (int?)obj.ToInt32() : null;
    }

    public static DateTime ToDateTime(this object obj) {
        return Convert.ToDateTime(obj);
    }

    public static DateTime? ToNullableDateTime(this object obj) {
        return obj.HasValue() ? (DateTime?)obj.ToDateTime() : null;
    }

    public static float ToFloat(this object obj) {
        return Convert.ToSingle(obj);
    }

    public static float? ToNullableFloat(this object obj) {
        return obj.HasValue() ? (float?)obj.ToFloat() : null;
    }

    public static Double ToDouble(this object obj) {
        return Convert.ToDouble(obj);
    }

    public static double? ToNullableDouble(this object obj) {
        return obj.HasValue() ? (Double?)obj.ToDouble() : null;
    }

    public static decimal ToDecimal(this object obj) {
        return Convert.ToDecimal(obj);
    }

    public static decimal? ToNullableDecimal(this object obj) {
        return obj.HasValue() ? (decimal?)obj.ToDecimal() : null;
    }

    public static char ToChar(this object obj) {
        return Convert.ToChar(obj);
    }

    public static char? ToNullableChar(this object obj) {
        return obj.HasValue() ? (char?)obj.ToChar() : null;
    }

    /// <summary>
    /// 新增或更新Dictionary內容
    /// </summary>
    /// <param name="dic"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value) {
        if (dic.ContainsKey(key)) {
            dic[key] = value;
        }
        else {
            dic.Add(key, value);
        }
    }

    /// <summary>
    /// 文字分割成純陣列(去除僅空白字元及無內容項目)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static string[] ToCleanArray(this string text, char separator) {
        return Array.ConvertAll(text.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries), array => array.Trim());
    }

    /// <summary>
    /// 判斷文字是否為數字
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static bool IsNumberic(this string text) {
        bool result = false;

        double temp;
        result = double.TryParse(text, out temp);

        return result;
    }

    /// <summary>
    /// 將變數的值轉換為字串, 如果是null則輸出預設字串
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="format"></param>
    /// <param name="provider"></param>
    /// <returns></returns>
    public static string ToStringOrDefault(this object obj, string defaultString, string format = null, IFormatProvider provider = null) {
        dynamic castedObj = obj;

        if (format.HasValue()) {
            return obj.HasValue() ? castedObj.ToString(format) : defaultString;
        }

        if (provider.HasValue()) {
            return obj.HasValue() ? castedObj.ToString(provider) : defaultString;
        }

        if (format.HasValue() && provider.HasValue()) {
            return obj.HasValue() ? castedObj.ToString(format, provider) : defaultString;
        }

        return obj.HasValue() ? castedObj.ToString() : defaultString;
    }

    /// <summary>
    /// 傳回從字串算來左邊的個數的字串
    /// </summary>
    /// <param name="str"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string Left(this string str, int length) {
        if (str.Length > length) {
            return str.Substring(0, length);
        }
        else {
            return str;
        }
    }

    /// <summary>
    /// 傳回從字串算來右邊的個數的字串
    /// </summary>
    /// <param name="str"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string Right(this string str, int length) {
        if (str.Length > length) {
            return str.Substring(str.Length - length, length);
        }
        else {
            return str;
        }
    }

    /// <summary>
    /// 傳回兩個字串中間的字串(有多個字串則取最大範圍)
    /// </summary>
    /// <param name="str"></param>
    /// <param name="beginStr"></param>
    /// <param name="EndStr"></param>
    /// <returns></returns>
    public static string Between(this string str, string beginStr, string EndStr) {
        var beginStrIndex = str.IndexOf(beginStr);
        var endStrIndex = str.LastIndexOf(EndStr);

        if (beginStrIndex != -1 && endStrIndex != -1) {
            return str.Substring(beginStrIndex + 1, endStrIndex - beginStrIndex - 1);
        }
        else {
            return str;
        }
    }

    /// <summary>
    /// 取得Function名稱
    /// </summary>
    /// <returns></returns>
    public static string GetFunctionName(this object obj) {
        var currentMethod = new StackTrace(true).GetFrame(1).GetMethod();
        var typeName = currentMethod.DeclaringType.Name;
        var methodName = currentMethod.Name;

        return typeName + "." + methodName;
    }

    /// <summary>
    /// 取得Function參數, 參數數量及順序必須與叫用方法相同, 否則無法取得內容
    /// </summary>
    /// <returns></returns>
    /// https://stackoverflow.com/questions/135782/generic-logging-of-function-parameters-in-exception-handling
    public static string GetFunctionParameters(this object obj, params object[] values) {
        var result = "";

        var currentMethod = new StackTrace(true).GetFrame(1).GetMethod();

        ParameterInfo[] parameters = currentMethod.GetParameters();
        if (values.Length == parameters.Length) {
            for (int currentParamIndex = 0; currentParamIndex < parameters.Length; currentParamIndex++) {
                if (values[currentParamIndex].GetType().Name == "UserToken") {
                    continue;
                }

                result += (currentParamIndex == 0 ? "" : ",") + "[" + parameters[currentParamIndex].Name + "]" + "=" + "{" + values[currentParamIndex].ToStringOrDefault("null") + "}";
            }
        }

        return result;
    }

    /// <summary>
    /// 取得Client端Ip Address(單元測試則改取本機Ip Address)
    /// </summary>
    /// <returns></returns>
    public static string GetIpAddress(this object obj) {
        return Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(item => item.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).SingleOrDefault().ToString();
    }

    /// <summary>
    /// 轉成附加索引的IEnumerable物件供foreach時使用, 用法:var (item, index) in items.WithIndex()
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns></returns>
    /// https://stackoverflow.com/questions/43021/how-do-you-get-the-index-of-the-current-iteration-of-a-foreach-loop/39997157#39997157
    public static IEnumerable<(T Item, int Index)> WithIndex<T>(this IEnumerable<T> self) {
        return self.Select((item, index) => (item, index));
    }

    /// <summary>
    /// 取得EntityFramework新增/修改/刪除的紀錄(需在SaveChanges方法前執行才可取得)
    /// </summary>
    /// <param name="database"></param>
    /// <returns></returns>
    public static string GetChangeHistory(this DbContext database) {
        #region 新增紀錄
        var insertedMessages = "";
        database.ChangeTracker.Entries().Where(record => record.State == EntityState.Added).ToList()
        .ForEach(record => {
            var tableName = record.Entity.GetType().Name;
            var columns = record.Entity.GetType().GetProperties();

            foreach (var column in columns) {
                var columnName = column.Name;
                var value = column.GetValue(record.Entity, null).ToStringOrDefault("null");

                insertedMessages += Environment.NewLine + "." + "[" + columnName + "]" + "=" + "{" + value + "}";
            }

            insertedMessages = "[" + tableName + "]" + insertedMessages;
        });
        insertedMessages = "inserted records: " + Environment.NewLine + insertedMessages;
        #endregion

        #region 修改紀錄
        var updatedMessages = "";
        database.ChangeTracker.Entries().Where(record => record.State == EntityState.Modified).ToList()
           .ForEach(record => {
               var tableName = record.Entity.GetType().Name;

               foreach (var columnName in record.OriginalValues.PropertyNames) {
                   var originalValue = record.OriginalValues[columnName].ToStringOrDefault("null");
                   var currentValue = record.CurrentValues[columnName].ToStringOrDefault("null");

                   if (currentValue != originalValue) {
                       updatedMessages += Environment.NewLine + "." + "[" + columnName + "]" + "=" + "{" + originalValue + "→" + currentValue + "}";
                   }
               }

               updatedMessages = "[" + tableName + "]" + updatedMessages;
           });
        updatedMessages = "updated records: " + Environment.NewLine + updatedMessages;
        #endregion

        #region 刪除紀錄
        var deletedMessages = "";
        database.ChangeTracker.Entries().Where(record => record.State == EntityState.Deleted).ToList()
            .ForEach(record => {
                var tableName = record.Entity.GetType().Name;
                var columns = record.Entity.GetType().GetProperties();

                foreach (var column in columns) {
                    var columnName = column.Name;
                    var value = column.GetValue(record.Entity, null).ToStringOrDefault("null");
                    deletedMessages += Environment.NewLine + "." + "[" + columnName + "]" + "=" + "{" + value + "}";
                }

                deletedMessages = "[" + tableName + "]" + deletedMessages;

            });
        deletedMessages = "deleted records: " + Environment.NewLine + deletedMessages;
        #endregion

        var GetChangeHistory = "";
        GetChangeHistory = insertedMessages + Environment.NewLine + updatedMessages + Environment.NewLine + deletedMessages;

        return GetChangeHistory;
    }

    /// <summary>
    /// 取得加密字串
    /// </summary>
    /// <param name="originalString"></param>
    /// <returns></returns>
    public static string GetEncryptionString(this string originalString) {
        SHA256 crypto = new SHA256CryptoServiceProvider();

        return Convert.ToBase64String(crypto.ComputeHash(Encoding.Default.GetBytes(originalString)));
    }
}