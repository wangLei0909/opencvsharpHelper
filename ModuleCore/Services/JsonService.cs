using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace ModuleCore.Services
{
    public class JsonService
    {
        public static void DataTableToFile(string con_file_path, DataTable dt)
        {
            //文件流 true 追加  fasle覆盖
            using StreamWriter sw = new(con_file_path, false);
            //序列化和反序列化JSON格式的对象。Newtonsoft.Json。
            //JsonSerializer使您能够控制如何将对象编码为JSON。
            JsonSerializer serializer = new();
            //构建Json.net的写入流
            JsonWriter writer = new JsonTextWriter(sw);
            //把模型数据序列化并写入Json.net的JsonWriter流中
            serializer.Serialize(writer, dt);
            writer.Close();
            sw.Close();
        }

        /// <summary>
        /// 从文件读取并反序列化
        /// </summary>
        /// <param name="jsonfile"></param>
        /// <returns></returns>
        public static DataTable DataTableFromFile(string jsonfile)
        {
            try
            {
                using StreamReader file = File.OpenText(jsonfile);
                using JsonTextReader reader = new(file);
                var jsonObject = JToken.ReadFrom(reader);
                return JsonConvert.DeserializeObject<DataTable>(jsonObject.ToString());
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 序例化并加密到文件
        /// </summary>
        /// <param name="con_file_path"></param>
        /// <param name="dt"></param>
        public static void DataTableToEncryptFile(string con_file_path, DataTable dt)
        {
            var jsonText = JsonConvert.SerializeObject(dt);
            var jsonEncryptText = EncryptService.EncryptWithSecretKey(jsonText);
            File.Delete(con_file_path);
            File.WriteAllText(con_file_path, jsonEncryptText, Encoding.UTF8);
        }

        /// <summary>
        /// 从加密文件读取并反序列化
        /// </summary>
        /// <param name="jsonfile"></param>
        /// <returns></returns>
        public static DataTable DataTableFromEncryptFile(string jsonfile)
        {
            try
            {
                var jsonEncryptText = File.ReadAllText(jsonfile, Encoding.UTF8);
                //解密
                var jsonText = EncryptService.Decrypt(jsonEncryptText);

                //var jsonObject = JToken.Parse(jsonText);
                return JsonConvert.DeserializeObject<DataTable>(jsonText);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// IO读取本地json
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string JsonFromFile(string filePath)
        {
            using FileStream fsRead = new(filePath, FileMode.OpenOrCreate);
            //读取加转换
            int fsLen = (int)fsRead.Length;
            byte[] heByte = new byte[fsLen];
            int r = fsRead.Read(heByte, 0, heByte.Length);
            return Encoding.UTF8.GetString(heByte);
        }

        private static readonly object o = new();

        /// <summary>
        /// 对象序列化到文件
        /// </summary>
        /// <param name="js"></param>
        /// <param name="con_file_path"></param>
        public static void ObjectToFile(object js, string con_file_path)
        {
            lock (o)
            {
                //文件流 true 追加  fasle覆盖
                using StreamWriter sw = new(con_file_path, false);
                //序列化和反序列化JSON格式的对象。Newtonsoft.Json。JsonSerializer使您能够控制如何将对象编码为JSON。
                JsonSerializer serializer = new();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                //获取或设置在序列化和反序列化期间如何处理空值。默认值是Newtonsoft.Json.NullValueHandling.Include (包含)。
                serializer.NullValueHandling = NullValueHandling.Ignore; //Ignore 忽略
                                                                         //构建Json.net的写入流
                JsonWriter writer = new JsonTextWriter(sw);
                //把模型数据序列化并写入Json.net的JsonWriter流中
                serializer.Serialize(writer, js);
                writer.Close();
                sw.Close();
            }
        }

        /// <summary>
        /// 从文件反序列化出对象
        /// </summary>
        /// <param name="jsonfile"></param>
        /// <returns></returns>
        public static T JObjectFromFile<T>(string jsonfile) where T : class
        {
            try
            {
                using StreamReader file = File.OpenText(jsonfile);
                using JsonTextReader reader = new(file);
                var jsonObject = JToken.ReadFrom(reader);
                var jsonObjectStr = jsonObject.ToString();
                var jobject = JsonConvert.DeserializeObject<T>(jsonObjectStr);
                return jobject;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 将对象序列化为JSON格式
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>json字符串</returns>
        public static string SerializeObject(object o)
        {
            string json = JsonConvert.SerializeObject(o);
            return json;
        }

        /// <summary>
        /// 解析JSON字符串生成对象实体
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json字符串(eg.{"ID":"112","Name":"石子儿"})</param>
        /// <returns>对象实体</returns>
        public static T DeserializeJsonToObject<T>(string json) where T : class
        {
            JsonSerializer serializer = new();
            StringReader sr = new(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(T));
            T t = o as T;
            return t;
        }

        /// <summary>
        /// 解析JSON数组生成对象实体集合
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json数组字符串(eg.[{"ID":"112","Name":"石子儿"}])</param>
        /// <returns>对象实体集合</returns>
        public static List<T> DeserializeJsonToList<T>(string json) where T : class
        {
            JsonSerializer serializer = new();
            StringReader sr = new(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(List<T>));
            List<T> list = o as List<T>;
            return list;
        }

        /// <summary>
        /// 反序列化JSON到给定的匿名对象.
        /// </summary>
        /// <typeparam name="T">匿名对象类型</typeparam>
        /// <param name="json">json字符串</param>
        /// <param name="anonymousTypeObject">匿名对象</param>
        /// <returns>匿名对象</returns>
        public static T DeserializeAnonymousType<T>(string json, T anonymousTypeObject)
        {
            T t = JsonConvert.DeserializeAnonymousType(json, anonymousTypeObject);
            return t;
        }
    }
}