using Newtonsoft.Json;
using System;

public class MultiDimensionalArrayConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsArray && objectType.GetArrayRank() == 2;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var array = (GameTile[,])value;
        writer.WriteStartArray();
        for (int i = 0; i < array.GetLength(0); i++)
        {
            writer.WriteStartArray();
            for (int j = 0; j < array.GetLength(1); j++)
            {
                serializer.Serialize(writer, array[i, j]);
            }
            writer.WriteEndArray();
        }
        writer.WriteEndArray();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jaggedArray = serializer.Deserialize<GameTile[][]>(reader);
        int width = jaggedArray.Length;
        int height = jaggedArray[0].Length;
        var array = new GameTile[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                array[i, j] = jaggedArray[i][j];
            }
        }
        return array;
    }
}
