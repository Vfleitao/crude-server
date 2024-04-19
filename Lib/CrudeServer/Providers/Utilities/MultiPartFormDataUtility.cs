using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using CrudeServer.Models.Contracts;

namespace CrudeServer.Providers.Utilities
{
    public static class MultiPartFormDataUtility
    {
        public static (Dictionary<string, object> fields, List<HttpFile> files) ParseFormData(byte[] data, string contentType)
        {
            Dictionary<string, object> fields = new Dictionary<string, object>();
            List<HttpFile> files = new List<HttpFile>();

            string boundary = GetBoundary(contentType).Replace("\"", "");
            byte[] boundaryBytes = Encoding.UTF8.GetBytes("--" + boundary);
            byte[] endBoundaryBytes = Encoding.UTF8.GetBytes("--" + boundary + "--");

            int position = 0;
            int length = data.Length;

            while (position < length)
            {
                int boundaryIndex = IndexOf(data, boundaryBytes, position);
                if (boundaryIndex < 0)
                {
                    break;
                }

                position = boundaryIndex + boundaryBytes.Length;

                int endIndex = IndexOf(data, boundaryBytes, position);
                if (endIndex < 0)
                {
                    endIndex = IndexOf(data, endBoundaryBytes, position);
                    if (endIndex < 0)
                    {
                        break;
                    }
                }

                int contentLength = endIndex - position;
                if (contentLength > 0)
                {
                    ProcessPart(data, position, contentLength, fields, files);
                }

                position = endIndex;
                if (Array.Equals(data.Skip(position).Take(endBoundaryBytes.Length).ToArray(), endBoundaryBytes))
                {
                    break;
                }
            }

            return (fields, files);
        }

        private static void ProcessPart(byte[] data, int start, int length, Dictionary<string, object> fields, List<HttpFile> files)
        {
            var separator = Encoding.UTF8.GetBytes("\r\n\r\n");
            var headerEndIndex = IndexOf(data, separator, start);
            if (headerEndIndex < 0)
            {
                return;
            }

            var headerSection = Encoding.UTF8.GetString(data, start, headerEndIndex - start);
            var contentStartIndex = headerEndIndex + separator.Length;
            var endIndex = IndexOf(data, Encoding.UTF8.GetBytes("\r\n--"), contentStartIndex);
            var contentLength = (endIndex > 0 ? endIndex : start + length) - contentStartIndex;
            var content = new byte[contentLength];
            Array.Copy(data, contentStartIndex, content, 0, contentLength);

            Match filenameMatch = Regex.Match(headerSection, @"filename=([^""]*)");
            if (filenameMatch.Success)
            {
                HandleFileContent(files, content, filenameMatch);

                return;
            }

            Match contentType = Regex.Match(headerSection, @"Content-Type:\s+(.*?)\s*$", RegexOptions.Multiline);
            if (contentType.Success && contentType.Groups[1].Value.Trim().StartsWith("application/json"))
            {
                HandleJsonContent(fields, content);

                return;
            }

            HandleRegularField(fields, headerSection, content);
        }

        private static void HandleRegularField(Dictionary<string, object> fields, string headerSection, byte[] content)
        {
            Match nameMatch = Regex.Match(headerSection, @"name=""([^""]*)"""); // case for quotes (ie: arrays)
            if (!nameMatch.Success) // case for single fields
            {
                nameMatch = Regex.Match(headerSection, @"name=([^""]*)");
            }
            string fieldName = nameMatch.Groups[1].Value.Trim();
            string value = Encoding.UTF8.GetString(content).Trim('\r', '\n');

            if (!fieldName.Contains(".") && !fieldName.Contains("[")) //then its not a complex object and not an array
            {
                fields.Add(fieldName, value);
                return;
            }

            Dictionary<string, object> currentField = fields;
            string[] nameParts = fieldName.Split(".");

            for (int i = 0; i < nameParts.Length; i++)
            {
                bool isArrayField = false;
                bool isLastPart = i == nameParts.Length - 1;

                string part = nameParts[i];
                if (part.Contains("["))
                {
                    isArrayField = true;
                    int startIndex = part.IndexOf("[");
                    int lastIndex = part.IndexOf("]");
                    int length = lastIndex - startIndex - 1;

                    string arrayFieldName = part.Substring(0, startIndex);
                    if (string.IsNullOrEmpty(arrayFieldName)) // for fields which are pure arrays
                    {
                        arrayFieldName = "__array__";
                    }

                    string number = part.Substring(startIndex + 1, length);
                    int index = int.Parse(number);

                    List<object> list;
                    if (!currentField.ContainsKey(arrayFieldName))
                    {
                        list = new List<object>();
                        currentField.Add(arrayFieldName, list);
                    }
                    else
                    {
                        list = (List<object>)currentField[arrayFieldName];
                    }

                    while (list.Count <= index)
                    {
                        list.Add(null); // Add nulls or a default value for missing indices
                    }

                    if (isLastPart)
                    {
                        list.Insert(index, value);
                    }
                    else
                    {
                        if (list[index] == null)
                        {
                            list[index] = new Dictionary<string, object>(); // Initialize a new dictionary if necessary
                        }

                        currentField = (Dictionary<string, object>)list[index];
                    }
                }
                else if (!isLastPart)
                {
                    if (!currentField.ContainsKey(part))
                    {
                        currentField.Add(part, new Dictionary<string, object>());
                    }

                    currentField = (Dictionary<string, object>)currentField[part];
                }
                else if (isLastPart && !isArrayField)
                {
                    currentField.Add(part, value);
                }
            }
        }

        private static void HandleJsonContent(Dictionary<string, object> fields, byte[] content)
        {
            string jsonString = Encoding.UTF8.GetString(content);
            Dictionary<string, object> jsonData = JsonUtilities.DictionaryFromString(jsonString);

            foreach (KeyValuePair<string, object> item in jsonData)
            {
                fields.TryAdd(item.Key, item.Value);
            }
        }

        private static void HandleFileContent(List<HttpFile> files, byte[] content, Match filenameMatch)
        {
            string filename = filenameMatch.Groups[1].Value;
            string[] filenamesChunks = filename.Split(";");

            if (filenamesChunks.Length > 1)
            {
                filename = filenamesChunks[0];
            }

            filename = filename.Trim();

            files.Add(new HttpFile()
            {
                Name = filename,
                Content = content,
            });
        }

        private static int IndexOf(byte[] buffer, byte[] value, int start)
        {
            int end = buffer.Length - value.Length;
            for (int i = start; i <= end; i++)
            {
                var match = true;
                for (int j = 0; j < value.Length; j++)
                {
                    if (buffer[i + j] != value[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return i;
            }
            return -1;
        }

        private static string GetBoundary(string contentType)
        {
            int boundaryIndex = contentType.IndexOf("boundary=");

            if (boundaryIndex < 0)
            {
                throw new ArgumentException("Boundary not found in content type");
            }

            return contentType.Substring(boundaryIndex + 9);
        }
    }
}
