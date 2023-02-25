using System.IO;
using Main.Scripts.Player;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Main.Scripts.Utils.Save
{
    public static class SaveLoadUtils
    {
        private const string FILE_PATH = "/game_save.json";

        public static void Save(PlayerData playerData)
        {
            var jObject = playerData.toJSON();
            using var streamWriter = File.CreateText(GetFilePath());
            using var jsonWriter = new JsonTextWriter(streamWriter);
            jObject.WriteTo(jsonWriter);
        }

        public static PlayerData Load()
        {
            var filePath = GetFilePath();
            if (File.Exists(filePath))
            {
                using var streamReader = File.OpenText(filePath);
                using var jsonReader = new JsonTextReader(streamReader);
                return PlayerData.parseJSON((JObject) JToken.ReadFrom(jsonReader));
            }

            var initialPlayerData = PlayerData.GetInitialPlayerData();
            Save(initialPlayerData);
            return initialPlayerData;
        }

        private static string GetFilePath()
        {
            return Application.persistentDataPath + FILE_PATH;
        }
    }
}