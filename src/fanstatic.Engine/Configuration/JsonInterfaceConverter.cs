using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Components.DictionaryAdapter;
using Castle.DynamicProxy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fanstatic.Engine.Configuration
{
    public class JsonInterfaceConverter : JsonConverter
    {
        private static readonly DictionaryAdapterFactory DictionaryAdapterFactory = new DictionaryAdapterFactory();

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsInterface;
        }

        public override object ReadJson(JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var dict = ToDictionary(jObject);
            var target = DictionaryAdapterFactory.GetAdapter<object>(objectType, dict);

            return target;
        }

        public override void WriteJson(JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private Dictionary<string, object> ToDictionary(JObject jObject)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var jProp in jObject.Properties())
            {
                dict[jProp.Name] = ObjectFromToken(jProp.Value);
            }
            return dict;
        }

        private object ObjectFromToken(JToken jToken)
        {
            var jObject = jToken as JObject;
            if (jObject != null)
            {
                return ToDictionary(jObject);
            }
            var jArray = jToken as JArray;
            if (jArray != null)
            {
                return ToArray(jArray);
            }
            var jValue = jToken as JValue;
            if (jValue != null)
            {
                return ToValue(jValue);
            }
            throw new InvalidOperationException();
        }

        private object ToArray(JArray jArray)
        {
            var objectArray = jArray.Select(ObjectFromToken).ToArray();
            if (objectArray.All(o => o is string))
            {
                return objectArray.Cast<string>().ToArray();
            }
            return objectArray;
        }

        private object ToValue(JValue jValue)
        {
            return jValue.Value.ToString();
        }
    }
}