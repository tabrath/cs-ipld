using System;
using System.Text;

namespace IPLD.Git
{
    public class PersonInfo
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Date { get; set; } // @todo: DateTime?
        public string Timezone { get; set; } // @todo: TimeZoneInfo?

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append($"{Name} <{Email}>");
            if (!string.IsNullOrEmpty(Date))
                sb.Append($" {Date}");
            if (!string.IsNullOrEmpty(Timezone))
                sb.Append($" {Timezone}");

            return sb.ToString();
        }

        internal string[] GetTree(string name, int depth)
        {
            return depth == 1 ? new[] {name} : new[] {$"{name}/name", $"{name}/email", $"{name}/date"};
        }

        internal (object, string[]) Resolve(string[] path)
        {
            switch (path[0])
            {
                case "name":
                    return (Name, path.Skip(1));
                case "email":
                    return (Email, path.Skip(1));
                case "date":
                    return ($"{Date} {Timezone}", path.Skip(1));
                default:
                    return (null, Array.Empty<string>());
            }
        }
    }
}