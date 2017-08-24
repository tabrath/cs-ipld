using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using IPLD.BlockFormat;
using IPLD.ContentIdentifier;
using IPLD.Format;
using Multiformats.Base;
using Multiformats.Codec;
using Multiformats.Hash;
using Multiformats.Hash.Algorithms;

namespace IPLD.Git
{
    public class IPLDGit
    {
        public static INode DecodeBlock(IBlock block)
        {
            var prefix = block.Cid().Prefix;

            if (prefix.Codec != MulticodecCode.Git || prefix.MultihashType != HashType.SHA1)
                return null;

            return ParseObjectFromBuffer(block.RawData());
        }

        public static INode ParseObjectFromBuffer(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
                return ParseObject(stream);
        }

        public static INode ParseCompressedObject(Stream stream)
        {
            using (var decompressor = new DeflateStream(stream, CompressionMode.Decompress))
                return ParseObject(decompressor);
        }

        private static INode ParseObject(Stream stream)
        {
            var type = stream.ReadString(' ');
            if (string.IsNullOrEmpty(type))
                return null;
            type = type.Substring(0, type.Length - 1);

            switch (type)
            {
                case "tree":
                    return ReadTree(stream);
                case "commit":
                    return ReadCommit(stream);
                case "blob":
                    return ReadBlob(stream);
                case "tag":
                    return ReadTag(stream);
                default:
                    return null;
            }
        }

        private static INode ReadTag(Stream stream)
        {
            var size = stream.ReadString(0);
            if (string.IsNullOrEmpty(size))
                return null;

            var output = new Tag
            {
                dataSize = size.Substring(0, size.Length - 1)
            };

            while (true)
            {
                var (line, eof) = stream.ReadLine();
                if (eof)
                    break;

                if (string.IsNullOrEmpty(line))
                {
                    output.Message = stream.ReadAll();
                }
                else if (line.StartsWith("object "))
                {
                    output.Object = Multibase.Base16.Decode(line.Substring(7)).AsShaToCid();
                }
                else if (line.StartsWith("tag "))
                {
                    output.Name = line.Substring(4);
                }
                else if (line.StartsWith("tagger "))
                {
                    var c = ParsePersonInfo(line);
                    if (c == null)
                        return null;

                    output.Tagger = c;
                }
                else if (line.StartsWith("type "))
                {
                    output.Type = line.Substring(5);
                }
            }

            output.cid = HashObject(output.RawData());

            return output;
        }

        public static INode ReadBlob(Stream stream)
        {
            var size = stream.ReadString(0);
            if (string.IsNullOrEmpty(size))
                return null;

            if (!int.TryParse(size.Substring(0, size.Length - 1), out int sizen))
                return null;

            using (var buffer = new MemoryStream())
            {
                buffer.Write($"blob {sizen}\x00");

                var n = stream.Copy(buffer);
                if (n != sizen)
                    return null;

                return new Blob(buffer.ToArray());
            }
        }

        public static INode ReadCommit(Stream stream)
        {
            var size = stream.ReadString(0);
            if (size.Length == 0)
                return null;

            var output = new Commit
            {
                DataSize = size.Substring(0, size.Length - 1)
            };

            while (true)
            {
                var (line, eof) = stream.ReadLine();
                if (eof)
                    break;

                if (!ParseCommitLine(output, line, stream))
                    return null;
            }

            output.cid = HashObject(output.RawData());

            return output;
        }

        private static bool ParseCommitLine(Commit output, string line, Stream stream)
        {
            if (string.IsNullOrEmpty(line))
            {
                output.Message = stream.ReadAll();
            }
            else if (line.StartsWith("tree "))
            {
                var sha = Multibase.Base16.Decode(line.Substring(5));
                output.GitTree = sha.AsShaToCid();
            }
            else if (line.StartsWith("parent "))
            {
                var psha = Multibase.Base16.Decode(line.Substring(7));
                output.Parents = output.Parents.Append(psha.AsShaToCid());
            }
            else if (line.StartsWith("author "))
            {
                output.Author = ParsePersonInfo(line);
            }
            else if (line.StartsWith("committer "))
            {
                output.Committer = ParsePersonInfo(line);
            }
            else if (line.StartsWith("encoding "))
            {
                output.Encoding = line.Substring(9);
            }
            else if (line.StartsWith("mergetag object"))
            {
                var sha = Multibase.Base16.Decode(line.Substring(16));

                var (mt, rest) = ReadMergeTag(sha, stream);

                output.MergeTag = output.MergeTag.Append(mt);

                if (!string.IsNullOrEmpty(rest))
                {
                    if (!ParseCommitLine(output, rest, stream))
                        return false;
                }
            }
            else if (line.StartsWith("gpgsig "))
            {
                output.Signature = ReadPgpSignature(stream);
            }
            else
            {
                output.Other = output.Other.Append(line);
            }

            return true;
        }

        private static GpgSignature ReadPgpSignature(Stream stream)
        {
            var (line, eof) = stream.ReadLine();
            if (eof)
                return null;

            var output = new GpgSignature();

            if (line != " ")
            {
                if (line.StartsWith(" Version: ") || line.StartsWith(" Comment: "))
                {
                    output.Text += line + '\n';
                }
                else
                {
                    return null;
                }
            }
            else
            {
                output.Text += " \n";
            }

            while (true)
            {
                (line, eof) = stream.ReadLine();
                if (eof)
                    return null;

                if (line == " -----END PGP SIGNATURE-----")
                    break;

                output.Text += line + "\n";
            }

            return output;
        }

        private static (MergeTag, string) ReadMergeTag(byte[] hash, Stream stream)
        {
            var output = new MergeTag
            {
                Object = hash.AsShaToCid()
            };

            while (true)
            {
                var (line, eof) = stream.ReadLine();
                if (eof)
                    break;

                if (line == " ")
                {
                    while (true)
                    {
                        var (line2, eof2) = stream.ReadLine();
                        if (eof2)
                            break;

                        if (line2[0] != ' ')
                            return (output, line);

                        output.Text += line2 + '\n';
                    }
                }
                else if (line.StartsWith(" type "))
                {
                    output.Type = line.Substring(6);
                }
                else if (line.StartsWith(" tag "))
                {
                    output.Tag = line.Substring(5);
                }
                else if (line.StartsWith(" tagger "))
                {
                    var tagger = ParsePersonInfo(line.Substring(1));
                    if (tagger == null)
                        return (null, string.Empty);

                    output.Tagger = tagger;
                }
            }

            return (output, string.Empty);
        }

        private static PersonInfo ParsePersonInfo(string line)
        {
            var parts = line.Split(' ');
            if (parts.Length < 3)
                return null;

            var at = 1;
            var pi = new PersonInfo();
            var name = string.Empty;

            while (true)
            {
                if (at == parts.Length)
                    return null;

                var part = parts[at];
                if (part.Length > 0)
                {
                    if (part[0] == '<')
                        break;

                    name += part + " ";
                }
                else if (name.Length > 0)
                {
                    name += " ";
                }
                at++;
            }

            if (name.Length > 0)
                pi.Name = name.Substring(0, name.Length - 1);

            var email = string.Empty;

            while (true)
            {
                if (at == parts.Length)
                    return null;

                var part = parts[at];
                if (part[0] == '<')
                    part = part.Substring(1);

                at++;
                if (part[part.Length - 1] == '>')
                {
                    email += part.Substring(0, part.Length - 1);
                    break;
                }
                email += part + " ";
            }
            pi.Email = email;

            if (at == parts.Length)
                return pi;

            pi.Date = parts[at];

            at++;
            if (at == parts.Length)
                return pi;

            pi.Timezone = parts[at];

            return pi;
        }

        private static Cid HashObject(byte[] data) => new Prefix(1, MulticodecCode.Git, HashType.SHA1, -1).Sum(data);

        private static INode ReadTree(Stream stream)
        {
            var lstr = stream.ReadString(0);
            if (string.IsNullOrEmpty(lstr))
                return null;
            lstr = lstr.Substring(0, lstr.Length - 1);

            if (!int.TryParse(lstr, out int n))
                return null;

            var tree = new Tree
            {
                entries = new Dictionary<string, TreeEntry>(),
                size = n
            };

            var order = new List<string>();
            while (true)
            {
                var (entry, eof) = TreeEntry.Read(stream);
                if (eof)
                    break;
                if (entry == null)
                    return null;

                order.Add(entry.name);
                tree.entries[entry.name] = entry;
            }

            tree.order = order.ToArray();
            tree.cid = HashObject(tree.RawData());

            return tree;
        }
    }
}