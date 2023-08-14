using System;
using System.Collections.Generic;
using System.Text.Json;

namespace IdentityServer.IntegrationTests.Common
{
    public static class JsonExtensions
    {
        public static Dictionary<string, object> ToDictionary(this JsonElement element)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var property in element.EnumerateObject())
            {
                object value = property.Value.ValueKind switch
                {
                    JsonValueKind.Object => ToDictionary(property.Value),
                    JsonValueKind.Array => ParseJsonArray(property.Value.EnumerateArray()),
                    JsonValueKind.String => property.Value.GetString(),
                    JsonValueKind.Number => property.Value.TryGetInt64(out var intValue)
                        ? intValue
                        : (object) property.Value.GetDecimal(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => throw new ArgumentOutOfRangeException()
                };

                dictionary.Add(property.Name, value);
            }

            return dictionary;
        }

        static List<object> ParseJsonArray(JsonElement.ArrayEnumerator arrayEnumerator)
        {
            var list = new List<object>();

            foreach (var item in arrayEnumerator)
            {
                switch (item.ValueKind)
                {
                    case JsonValueKind.Object:
                        list.Add(ToDictionary(item));
                        break;
                    case JsonValueKind.Array:
                        list.Add(ParseJsonArray(item.EnumerateArray()));
                        break;
                    case JsonValueKind.String:
                        list.Add(item.GetString());
                        break;
                    case JsonValueKind.Number:
                        list.Add(item.TryGetInt32(out int intValue) ? intValue : (object) item.GetDecimal());
                        break;
                    case JsonValueKind.True:
                        list.Add(true);
                        break;
                    case JsonValueKind.False:
                        list.Add(false);
                        break;
                    case JsonValueKind.Null:
                        list.Add(null);
                        break;
                    case JsonValueKind.Undefined:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return list;
        }
    }
}