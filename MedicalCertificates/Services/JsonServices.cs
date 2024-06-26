﻿using Newtonsoft.Json.Linq;
using System;
using System.IO;
using MedicalCertificates.Services.Alert;
using Newtonsoft.Json;

namespace MedicalCertificates.Services
{
    public static class JsonServices
    {
        public const string PATH = "settings.json";

        public static string ReadByProperty(string propName)
        {
            var res = "";

            try
            {
                if (File.Exists(PATH))
                {
                    var json = File.ReadAllText(PATH);

                    var settings = JObject.Parse(json);
                    res = (string)settings[propName];
                }
            }
            catch (Exception e)
            {
                var alert = new Alert.Alert("Ошибка!", "Ошибка" + e.Message, AlertType.Error);
                alert.ShowDialog();
            }

            return res;
        }

        public static void Write(string propName, string value)
        {
            try
            {
                if (File.Exists(PATH))
                {
                    var json = File.ReadAllText(PATH);

                    var settings = JObject.Parse(json);
                    settings[propName] = value;

                    File.WriteAllText(PATH, JsonConvert.SerializeObject(settings, Formatting.Indented));
                }
                else
                {
                    var settings = new JObject();
                    settings[propName] = value;

                    File.WriteAllText(PATH, JsonConvert.SerializeObject(settings, Formatting.Indented));
                }
            }
            catch (Exception e)
            {
                var alert = new Alert.Alert("Ошибка!", "Ошибка " + e.Message, AlertType.Error);
                alert.ShowDialog();
            }
        }
    }
}
