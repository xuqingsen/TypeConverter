using System;
using System.Collections.Generic;
using System.Reflection;
using System.Data;
using System.Collections.Specialized;

namespace XqsLibrary
{
    public class TypeConverter
    {
        static Type _nullableType;

        static TypeConverter()
        {
            _nullableType = typeof(Nullable<>);
        }

        static void SetValue<T>(object value, PropertyInfo property, T Model)
        {
            if (!property.CanWrite)
                return;
            bool isNullable = property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == _nullableType;
            Type targetType = property.PropertyType;
            if (isNullable)
                targetType = property.PropertyType.GetGenericArguments()[0];            
            if (targetType.IsValueType || targetType.Name == "String")//避免自定义Class等非基础类型抛出异常
            {
                if (targetType.IsEnum)
                    property.SetValue(Model, Enum.Parse(targetType, value.ToString()), null);
                else
                    property.SetValue(Model, Convert.ChangeType(value, targetType), null);
            }
        }

        static void SetValue<T>(DataRow dr, PropertyInfo property, string colName, T Model)
        {
            if (colName == null)
                colName = property.Name;
            if (dr.Table.Columns.Contains(colName) && dr[colName].GetType().Name != "DBNull")
            {
                SetValue(dr[property.Name], property, Model);
            }
        }

        /// <summary>
        /// 对象转换为bool型
        /// </summary>
        /// <param name="expression">要转换的对象</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的bool类型结果</returns>
        public static bool ToBoolean<T>(T expression, bool defValue)
        {
            if (expression != null)
            {
                bool result;
                if (Boolean.TryParse(expression.ToString(), out result))
                    return result;
                else
                    return defValue;
            }
            return defValue;
        }

        /// <summary>
        /// 将对象转换为Int32类型
        /// </summary>
        /// <param name="expression">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static int ToInteger<T>(T expression, int defValue)
        {
            if (expression != null)
            {
                int rv;
                if (Int32.TryParse(expression.ToString(), out rv))
                    return rv;
                else
                    return defValue;
            }
            return defValue;
        }

        /// <summary>
        /// 对象转换为float型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static float ToFloat<T>(T strValue, float defValue)
        {
            if (strValue == null)
                return defValue;
            float intValue = 0;
            if (float.TryParse(strValue.ToString(), out intValue))
            {
                return intValue;
            }
            else
                return defValue;
        }

        #region 日期转换
        /// <summary>
        ///  Object 转换为dateTime(转换失败则返回最小日期)
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(object strValue)
        {
            if (strValue == null)
            {
                return DateTime.MinValue;
            }
            DateTime dt;
            if (DateTime.TryParse(strValue.ToString(), out dt))
            {
                return dt;
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// string 转换为dateTime
        /// </summary>
        /// <param name="strValue">待转换字符串</param>
        /// <param name="defValue">默认值</param>
        /// <returns></returns>
        public static DateTime ToDateTime(string strValue, DateTime defValue)
        {
            DateTime dt = ToDateTime(strValue);
            if (dt != null)
            {
                return dt;
            }
            else
            {
                return defValue;
            }
        }

        /// <summary>        
        /// Unix时间戳转换为DateTime
        /// </summary>        
        /// <param name="timeStamp"></param>        
        /// <returns></returns>        
        public static DateTime UnixStampToDate(string timeStamp, bool isMillSecond)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            if (isMillSecond)
                timeStamp = timeStamp + "0000";
            else
                timeStamp = timeStamp + "0000000";
            long lTime = long.Parse(timeStamp);
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);

        }

        /// <summary>
        /// Unix时间戳转换为DateTime
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="isMillSecond"></param>
        /// <returns></returns>
        public static DateTime UnixStampToDate2(double timeStamp, bool isMillSecond)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            if (isMillSecond)
                return origin.AddMilliseconds(timeStamp).ToLocalTime();
            else
                return origin.AddSeconds(timeStamp).ToLocalTime();
        }

        /// <summary>
        /// DateTime转换为Unix时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <param name="isMillSecond">是否精确到毫秒</param>
        /// <returns></returns>
        public static long DateToUnixStamp(DateTime time, bool isMillSecond)
        {
            long tmp = isMillSecond ? 10000 : 10000000;
            return (time.ToUniversalTime().Ticks - 621355968000000000) / tmp;
        }

        /// <summary>
        /// DateTime转换为Unix时间戳
        /// </summary>
        /// <param name="date"></param>
        /// <param name="isMillSecond"></param>
        /// <returns></returns>
        public static double DateToUnixStamp2(DateTime date, bool isMillSecond)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date.ToUniversalTime() - origin;
            if (!isMillSecond)
                return Math.Floor(diff.TotalSeconds);
            else
                return Math.Floor(diff.TotalMilliseconds);
        }
        #endregion

        /// <summary>
        /// 对象转换
        /// </summary>
        /// <typeparam name="T">转换目标类型</typeparam>
        /// <param name="obj">转换对象</param>
        /// <param name="parseValue">转换值(转换失败后保持原值)</param>
        /// <returns>bool对象，表示转换是否成功</returns>
        public static bool ConvertTo<T>(object obj, ref T parseValue)
        {
            try
            {
                parseValue = (T)Convert.ChangeType(obj, typeof(T));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 对象转换
        /// </summary>
        /// <typeparam name="T">转换目标类型</typeparam>
        /// <param name="obj">转换对象</param>
        /// <param name="defaultVal">缺省值</param>        
        public static T ConvertTo<T>(object obj, T defaultVal) where T : struct
        {
            try
            {
                return (T)Convert.ChangeType(obj, typeof(T));
            }
            catch
            {
                return defaultVal;
            }
        }

        /// <summary>
        /// 将指定对象(T)转换成目标对象(U)
        /// </summary>
        /// <typeparam name="T">源对象数据类型</typeparam>
        /// <typeparam name="U">目标对象数据类型</typeparam>
        /// <param name="sourceObj"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static U ObjectConvert<T, U>(T sourceObj, out bool result)
            where T : class
            where U : class,new()
        {
            if (sourceObj == null)
            {
                result = true;
                return null;
            }
            U destObj = new U();
            Type t = typeof(T);
            Type u = typeof(U);
            PropertyInfo[] tps = t.GetProperties();
            PropertyInfo[] ups = u.GetProperties();
            Dictionary<string, PropertyInfo> upDics = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo up in ups)
            {
                upDics[up.Name] = up;
            }
            object value;
            foreach (PropertyInfo tp in tps)
            {
                if (upDics.ContainsKey(tp.Name))
                {
                    value = tp.GetValue(sourceObj, null);
                    upDics[tp.Name].SetValue(destObj, value, null);
                }
            }
            result = true;
            return destObj;
        }

        #region 将DataRow对象转换成实体类

        /// <summary>
        /// 将DataRow对象转换成实体类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static T ToModel<T>(DataRow dr) where T : class,new()
        {
            bool result = false;
            T t = ToModel<T>(dr, null, out result);
            return result ? t : null;
        }

        /// <summary>
        /// 将DataRow对象转换成实体类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static T ToModel<T>(DataRow dr, Dictionary<string, string> maps, out bool result) where T : class,new()
        {
            result = true;
            T Model = null;
            if (dr != null)
            {
                Model = new T();
                Type modelType = Model.GetType();
                PropertyInfo[] properties = modelType.GetProperties();
                string colName;
                try
                {
                    foreach (PropertyInfo property in properties)
                    {
                        if (maps != null && maps.ContainsKey(property.Name))
                            colName = maps[property.Name];
                        else
                        {
                            colName = property.Name;
                        }
                        SetValue(dr, property, colName, Model);
                    }
                }
                catch
                {
                    Model = null; result = false;
                }
            }
            else
            {
                result = false;
            }
            return Model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="result"></param>
        /// <param name="ignores">转换时忽略的字段名</param>
        /// <returns></returns>
        public static T ToModel<T>(DataRow dr, out bool result, params string[] ignores) where T : class,new()
        {
            if (dr == null)
            {
                result = false;
                return null;
            }
            result = true;
            T model = new T();
            Type modelType = typeof(T);
            PropertyInfo[] properties = modelType.GetProperties();
            List<string> ignoreNames = null;
            bool hasIgnoreNames = ignores != null && ignores.Length > 0, shouldIgnore;
            if (hasIgnoreNames)
                ignoreNames = new List<string>(ignores);
            try
            {
                foreach (PropertyInfo property in properties)
                {
                    shouldIgnore = hasIgnoreNames && ignoreNames.FindIndex(p => { return string.Compare(property.Name, p, true) == 0; }) > -1;
                    if (!shouldIgnore)
                        SetValue(dr, property, null, model);
                }
            }
            catch
            {
                model = null; result = false;
            }
            return model;
        }

        /// <summary>
        /// 将DataTable对象转换成实体类集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static List<T> ToModelList<T>(DataTable table, out bool result) where T : class,new()
        {
            result = true;
            List<T> list = new List<T>();
            if (table != null)
            {
                T Model = new T();
                Type modelType = Model.GetType();
                PropertyInfo[] properties = modelType.GetProperties();
                try
                {
                    foreach (DataRow dr in table.Rows)
                    {
                        Model = new T();
                        foreach (PropertyInfo property in properties)
                        {
                            SetValue(dr, property, null, Model);
                        }
                        list.Add(Model);
                    }
                }
                catch
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }
            return list;
        }
        #endregion

        #region 将NameValueCollection对象转换成Model

        /// <summary>
        /// 将NameValueCollection对象转换成Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static T ToModel<T>(NameValueCollection collection) where T : class, new()
        {
            bool result = false;
            return ToModel<T>(collection, out result);
        }

        /// <summary>
        /// 将NameValueCollection对象转换成Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="result">转换是否发生异常</param>
        /// <returns></returns>
        public static T ToModel<T>(NameValueCollection collection, out bool result) where T : class, new()
        {
            result = true;
            T Model = new T();
            if (collection != null)
            {
                try
                {
                    Type modelType = Model.GetType();
                    PropertyInfo[] properties = modelType.GetProperties();
                    bool isExits = true;
                    foreach (PropertyInfo property in properties)
                    {
                        isExits = collection[property.Name] != null;
                        if (isExits)
                        {
                            SetValue(collection[property.Name], property, Model);
                        }
                    }
                }
                catch
                {
                    Model = null; result = false;
                }
            }
            else
            {
                result = false;
                Model = null;
            }
            return Model;
        }
        #endregion

        /// <summary>
        /// 将实体类的值填充到DataRow中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Model">实体类</param>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static bool SetDrFromModel<T>(T Model, DataRow dr) where T : new()
        {
            if (dr == null) { return false; }
            bool result = true;
            try
            {
                Type modelType = Model.GetType();
                PropertyInfo[] properties = modelType.GetProperties();
                bool isExits = true;
                foreach (PropertyInfo property in properties)
                {
                    isExits = dr.Table.Columns.Contains(property.Name);
                    if (isExits)
                    {
                        dr[property.Name] = property.GetValue(Model, null);
                    }
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Model"></param>
        /// <param name="dr"></param>
        /// <param name="columnMaps">实体类与DataRow字段对应表</param>
        /// <returns></returns>
        public static bool SetDrFromModel<T>(T Model, DataRow dr, Dictionary<string, string> columnMaps) where T : new()
        {
            if (dr == null) { return false; }
            bool result = true;
            try
            {
                Type modelType = Model.GetType();
                PropertyInfo[] properties = modelType.GetProperties();
                bool isExits = true;
                foreach (PropertyInfo property in properties)
                {
                    isExits = columnMaps.ContainsKey(property.Name) && dr.Table.Columns.Contains(property.Name);
                    if (isExits)
                    {
                        dr[property.Name] = property.GetValue(Model, null);
                    }
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 将DataTable转换成Dictionary
        /// </summary>
        /// <param name="table"></param>
        /// <param name="keyF"></param>
        /// <param name="valueF"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ConvertFormTable(DataTable table, string keyF, string valueF)
        {
            Dictionary<string, string> dics = new Dictionary<string, string>();
            if (table != null && table.Rows.Count > 0)
            {
                foreach (DataRow dr in table.Rows)
                {
                    dics.Add(dr[keyF].ToString(), dr[valueF].ToString());
                }
            }
            return dics;
        }
    }
}
