using System.Collections.Generic;

namespace CrudeServer.Providers.Utilities
{
    public static class FormFieldUtility
    {
        public static void ProcessRegularField(Dictionary<string, object> fields, string fieldName, string value)
        {
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

                    while (list.Count < index + 1)
                    {
                        list.Add(null); // Add nulls or a default value for missing indices
                    }

                    if (isLastPart && value != null)
                    {
                        list[index] = value;
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
    }
}
