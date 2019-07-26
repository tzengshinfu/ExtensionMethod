using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

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
            if (nullableObj.GetType() == typeof(XmlDocument)) {
                var xmlDocObj = (XmlDocument)nullableObj;
                if (xmlDocObj.InnerXml.HasValue() == false) {
                    return false;
                }
                else {
                    return true;
                }
            }
            else {
                var xmlElDoc = (XmlElement)nullableObj;
                if (xmlElDoc.InnerXml.HasValue() == false) {
                    if (Array.TrueForAll<XmlAttribute>(xmlElDoc.Attributes.Cast<XmlAttribute>().ToArray(), a => a.Value.HasValue() == false) == true) {
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

    public static bool NoValue(this object obj) {
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
    public static void Merge<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value) {
        if (dic.ContainsKey(key)) {
            dic[key] = value;
        }
        else {
            dic.Add(key, value);
        }
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
    /// 取得Client端Ip Address(單元測試則改取本機Ip Address)
    /// </summary>
    /// <returns></returns>
    public static string GetIpAddress(this object obj) {
        return Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(item => item.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).SingleOrDefault().ToString();
    }

    /// <summary>
    /// 回傳附加索引子的物件集合
    /// </summary>
    /// <param name="enumerable">來源集合</param>
    /// <returns>附加索引子的物件集合(Item=物件/Index=索引/No=項次=索引+1)</returns>
    public static IEnumerable<ItemIndex<T>> AppendIndex<T>(this IEnumerable<T> enumerable) {
        return enumerable.Select((item, index) => new ItemIndex<T> { Item = item, Index = index, No = index + 1 });
    }

    public struct ItemIndex<T> {
        public T Item;
        public int Index;
        public int No;
    }

    /// <summary>
    /// 字串加密(非對稱式)
    /// </summary>
    /// <param name="Source">加密前字串</param>
    /// <param name="cryptoKey">加密金鑰</param>
    /// <returns>加密後字串</returns>
    public static string Encrypt(this string sourceString, string cryptoKey) {
        string result = "";

        try {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
            byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(cryptoKey));
            byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(cryptoKey));
            aes.Key = key;
            aes.IV = iv;

            byte[] dataByteArray = Encoding.UTF8.GetBytes(sourceString);
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write)) {
                cs.Write(dataByteArray, 0, dataByteArray.Length);
                cs.FlushFinalBlock();
                result = Convert.ToBase64String(ms.ToArray());
            }
        }
        catch (Exception e) {
            throw e;
        }

        return result;
    }

    /// <summary>
    /// 字串解密(非對稱式)
    /// </summary>
    /// <param name="Source">解密前字串</param>
    /// <param name="cryptoKey">解密金鑰</param>
    /// <returns>解密後字串</returns>
    public static string Decrypt(this string sourceString, string cryptoKey) {
        string result = "";

        try {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
            byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(cryptoKey));
            byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(cryptoKey));
            aes.Key = key;
            aes.IV = iv;

            byte[] dataByteArray = Convert.FromBase64String(sourceString);
            using (MemoryStream ms = new MemoryStream()) {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write)) {
                    cs.Write(dataByteArray, 0, dataByteArray.Length);
                    cs.FlushFinalBlock();
                    result = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
        catch (Exception e) {
            throw e;
        }

        return result;
    }
}