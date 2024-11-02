using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

// Custom converter for handling 2D arrays
public class GameTileArrayConverter : JsonConverter<GameTile[,]>
{
    public override GameTile[,] ReadJson(JsonReader reader, Type objectType, GameTile[,] existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var jArray = JArray.Load(reader);
        int rows = jArray.Count;
        int cols = ((JArray)jArray[0]).Count;
        var result = new GameTile[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = jArray[i][j].ToObject<GameTile>(serializer);
            }
        }

        return result;
    }

    public override void WriteJson(JsonWriter writer, GameTile[,] value, JsonSerializer serializer)
    {
        writer.WriteStartArray();

        for (int i = 0; i < value.GetLength(0); i++)
        {
            writer.WriteStartArray();
            for (int j = 0; j < value.GetLength(1); j++)
            {
                serializer.Serialize(writer, value[i, j]);
            }
            writer.WriteEndArray();
        }

        writer.WriteEndArray();
    }
}