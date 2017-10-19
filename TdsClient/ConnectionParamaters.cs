using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TdsClient
{
    public class ConnectionParamaters
    {
        static ConnectionParamaters()
        {
            ParameterDescriptors = new List<ParameterDescriptor>
            {
                new ParameterDescriptor((p, v) => p.ServerAddress = v, new[] { "Server", "Address", "Data Source","ServerHostname", "Hostname" }, null, null),
                new ParameterDescriptor((p, v) => p.ServerPort = int.Parse(v), new[] { "Port" }, p => p.ServerPort == 0, p => p.ServerPort = 5000),
                new ParameterDescriptor((p, v) => p.ClientHostname = v, new[] { "ClientHostname"  }, p => p.ClientHostname == null, p => p.ClientHostname = Environment.MachineName),
                new ParameterDescriptor((p, v) => p.Username = v, new[] { "Username", "usr", "uid", "user" }, null, null),
                new ParameterDescriptor((p, v) => p.Password = v, new[] { "Password", "pwd", "pass" }, null, null),
                new ParameterDescriptor((p, v) => p.DatabaseName = v, new[] { "Database", "db", "Initial Catalog", "Catalog" }, null, null),
                new ParameterDescriptor((p, v) => p.ProcessId = v, new[] { "ProcessId", "pid" }, p => p.ProcessId == null, p => p.ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id.ToString()),
                new ParameterDescriptor((p, v) => p.AppliactionName = v, new[] { "AppliactionName", "Appliaction", "app" }, p => p.AppliactionName == null, p => p.AppliactionName = System.Diagnostics.Process.GetCurrentProcess().ProcessName),
                new ParameterDescriptor((p, v) => p.ServerName = v, new[] { "ServerName" }, p => p.ServerName == null, p => p.ServerName = p.ServerAddress),
                new ParameterDescriptor((p, v) => p.Language = v, new[] { "Language", "lang" }, p => p.Language == null, p => p.Language = ""),
                new ParameterDescriptor((p, v) => p.Charset = v, new[] { "Charset" }, p => p.Charset == null, p => p.Charset = "iso_1"),
            };
        }

        public ConnectionParamaters(string connectionString)
        {
            var parsed = Parse(connectionString);

            foreach (var d in ParameterDescriptors)
            {
                foreach (var synonym in d.Synonymes)
                {
                    if (parsed.ContainsKey(synonym))
                    {
                        d.Set(this, parsed[synonym]);
                    }
                }
            }

            foreach (var d in ParameterDescriptors)
            {
                if (d.FallBack != null && d.IsEmpty != null && d.IsEmpty(this))
                {
                    d.FallBack(this);
                }
            }
        }

        private static List<ParameterDescriptor> ParameterDescriptors { get; set; }

        public string ServerAddress { get; private set; }
        public int ServerPort { get; private set; }
        public string ClientHostname { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string DatabaseName { get; private set; }
        public string ProcessId { get; private set; }
        public string AppliactionName { get; private set; }
        public string ServerName { get; private set; }
        public string Language { get; private set; }
        public string Charset { get; private set; }

        public static IDictionary<string, string> Parse(string connectionString)
        {
            var splitParam = connectionString.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            var splitValue = splitParam.Select(p => p.Split(new[] { "=" }, 2, StringSplitOptions.None)).ToList();

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in splitValue)
            {
                if (p.Any())
                {
                    var key = p.First().Trim();
                    if (!string.IsNullOrEmpty(key))
                    {
                        var value = p.Skip(1).Select(v => v.TrimStart()).FirstOrDefault();
                        result[key] = value;
                    }
                }
            }

            return result;
        }

        private class ParameterDescriptor
        {
            public ParameterDescriptor(
                Action<ConnectionParamaters, string> set,
                IEnumerable<string> synonymes,
                Func<ConnectionParamaters, bool> isEmpty,
                Action<ConnectionParamaters> fallBack)
            {
                Set = set;
                Synonymes = synonymes;
                IsEmpty = isEmpty;
                FallBack = fallBack;
            }

            public Action<ConnectionParamaters, string> Set { get; private set; }
            public IEnumerable<string> Synonymes { get; private set; }
            public Func<ConnectionParamaters, bool> IsEmpty { get; private set; }
            public Action<ConnectionParamaters> FallBack { get; private set; }
        }
    }
}
