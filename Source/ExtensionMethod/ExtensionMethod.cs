using System;
using System.Collections.Generic;
using System.Data.Common;


public static partial class ExtensionMethod {
    /// <summary>
    /// 判斷變數是否可用(Object=非NULL, 文字=非空字串及空白, 集合=項目數目大於0)
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsValid(this object obj) {
        bool result = true;
        object nullableObj = obj.GetValueOrNull();

        if (nullableObj == null) {
            result = false;
        }
        //List
        else if (nullableObj.GetType().GetProperty("Count") != null) {
            dynamic castedObj = Convert.ChangeType(nullableObj, nullableObj.GetType());
            if (castedObj.Count == 0) {
                result = false;
            }
        }
        //Array
        else if (nullableObj.GetType().GetProperty("Length") != null) {
            dynamic castedObj = Convert.ChangeType(nullableObj, nullableObj.GetType());
            if (castedObj.Length == 0) {
                result = false;
            }
        }
        else {
            //Linq Iterator
            var isIterator = nullableObj.GetType().Name.Contains("Where") && nullableObj.GetType().Name.Contains("Iterator");
            if (isIterator == true) {
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

    public static bool IsNotValid(this object obj) {
        return obj.IsValid() ? false : true;
    }

    public static string FormatWithArgs(this string str, params object[] args) {
        return string.Format(str, args);
    }

    public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action) {
        foreach (T item in enumeration) {
            action(item);
        }
    }

    public static bool ToBoolean(this object obj) {
        return Convert.ToBoolean(obj);
    }

    public static Nullable<bool> ToNullableBoolean(this object obj) {
        return obj.GetValueOrNull() != null ? (bool?)obj.ToBoolean() : null;
    }

    public static int ToInt32(this object obj) {
        return Convert.ToInt32(obj);
    }

    public static Nullable<int> ToNullableInt32(this object obj) {
        return obj.GetValueOrNull() != null ? (int?)obj.ToInt32() : null;
    }

    public static DateTime ToDateTime(this object obj) {
        return Convert.ToDateTime(obj);
    }

    public static Nullable<DateTime> ToNullableDateTime(this object obj) {
        return obj.GetValueOrNull() != null ? (DateTime?)obj.ToDateTime() : null;
    }

    public static float ToFloat(this object obj) {
        return Convert.ToSingle(obj);
    }

    public static Nullable<float> ToNullableFloat(this object obj) {
        return obj.GetValueOrNull() != null ? (float?)obj.ToFloat() : null;
    }

    public static Double ToDouble(this object obj) {
        return Convert.ToDouble(obj.GetValueOrNull());
    }

    public static Nullable<Double> ToNullableDouble(this object obj) {
        return obj.GetValueOrNull() != null ? (Double?)obj.ToDouble() : null;
    }

    public static decimal ToDecimal(this object obj) {
        return Convert.ToDecimal(obj.GetValueOrNull());
    }

    public static Nullable<decimal> ToNullableDecimal(this object obj) {
        return obj.GetValueOrNull() != null ? (decimal?)obj.ToDecimal() : null;
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
    public static bool IsNumber(this string text) {
        bool result = false;

        double temp;
        result = double.TryParse(text, out temp);

        return result;
    }

    /// <summary>
    /// 取得值或返回C#的null
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static object GetValueOrNull(this object obj) {
        if (DBNull.Value.Equals(obj) == false) {
            return obj;
        }
        else {
            return null;
        }
    }

    /// <summary>
    /// 取得值或返回資料庫的NULL
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static object GetValueOrDBNull(this object obj) {
        if (obj != null) {
            return obj;
        }
        else {
            return DBNull.Value;
        }
    }

    public static void SetCommandWithParameters(this DbCommand command, string sqlStatement, params DbParameter[] args) {
        command.CommandText = sqlStatement;

        foreach (var arg in args) {
            arg.Value = arg.Value.GetValueOrDBNull();

            command.Parameters.Add(arg);
        }
    }
}