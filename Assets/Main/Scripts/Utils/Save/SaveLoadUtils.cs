using System.Collections.Generic;
using System.IO;
using Main.Scripts.Player.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WebSocketSharp;

namespace Main.Scripts.Utils.Save
{
    public static class SaveLoadUtils
    {
        private const string DIRECTORY_PATH = "/Saves/";
        private const string POSTFIX = "_save.json";
        private const string FILE_PATH_FORMAT = DIRECTORY_PATH + "{0}" + POSTFIX;

        public static List<string> GetSavesList()
        {
            var list = new List<string>();
            foreach (var file in GetAllFiles())
            {
                list.Add(file.Name.Substring(0, file.Name.Length - POSTFIX.Length));
            }

            return list;
        }

        public static void Save(string userId, PlayerData playerData)
        {
            var jObject = playerData.toJSON();
            var filePath = GetFilePath(userId);
            if (filePath.IsNullOrEmpty())
            {
                return;
            }

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

        public static void DeleteSave(string userId)
        {
            var filePath = GetFilePath(userId);
            if (filePath.IsNullOrEmpty())
            {
                return;
            }

            var file = new FileInfo(filePath);
            if (file.Exists)
            {
                file.Delete();
            }
        }

        public static void DeleteAllSaves()
        {
            foreach (var file in GetAllFiles())
            {
                file.Delete();
            }
        }

        private static IEnumerable<FileInfo> GetAllFiles()
        {
            var path = Application.persistentDataPath + DIRECTORY_PATH;
            var dir = new DirectoryInfo(path);
            return dir.GetFiles("*" + POSTFIX);
        }

        private static string GetFilePath(string playerName)
        {
            return Application.persistentDataPath + string.Format(FILE_PATH_FORMAT, playerName);
        }
    }
}