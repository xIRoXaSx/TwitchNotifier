using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TwitchNotifier.src.config {
    class Parser {

        /// <summary>
        /// Serialize an object to a string
        /// </summary>
        /// <param name="objectToSerialze">The object to serialze</param>
        public static void Serialize(object objectToSerialze) {
            var serializer = new SerializerBuilder()
                //.WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(objectToSerialze);
            Console.WriteLine(yaml);
        }
    }
}
