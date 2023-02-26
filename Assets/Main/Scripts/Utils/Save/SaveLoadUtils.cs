using System.IO;
using Main.Scripts.Player.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Main.Scripts.Utils.Save
{
    public static class SaveLoadUtils
    {
        private const string FILE_PATH = "/Saves/{0}_save.json";

        public static void Save(string userId, PlayerData playerData)
        {
            var jObject = playerData.toJSON();
            var filePath = GetFilePath(userId);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath).ThrowWhenNull());
            using var streamWriter = File.CreateText(filePath);
            using var jsonWriter = new JsonTextWriter(streamWriter);
            jObject.WriteTo(jsonWriter);
        }

        public static PlayerData Load(string userId)
        {
            var filePath = GetFilePath(userId);
            if (File.Exists(filePath))
            {
                using var streamReader = File.OpenText(filePath);
                using var jsonReader = new JsonTextReader(streamReader);
                return PlayerData.parseJSON((JObject)JToken.ReadFrom(jsonReader));
            }

            var initialPlayerData = PlayerData.GetInitialPlayerData();
            Save(userId, initialPlayerData);
            return initialPlayerData;
        }

        private static string GetFilePath(string playerName)
        {
            return Application.persistentDataPath + string.Format(FILE_PATH, playerName);
        }
    }
}