﻿using NccsvConverter.NccsvParser.FileHandling;
using NccsvConverter.NccsvParser.Models;
using NccsvConverter.NccsvParser.Repositories;
using System.Globalization;

namespace NccsvConverter.NccsvParser.Helpers;

public class NccsvParserMethods
{
    // Finds global attributes snd stores them without *GLOBAL*-tag in a string array.
    public static List<string[]> FindGlobalAttributes(List<string[]> separatedNccsv)
    {
        var globalAttributes = new List<string[]>();

        foreach (var stringArray in separatedNccsv)
        {
            if (stringArray[0] == "*GLOBAL*")
            {
                if (stringArray.Length < 3)
                {
                    string attributeValue = "";
                    for (int i = 1; i < stringArray.Length; i++)
                    {
                        attributeValue += stringArray[i + 1];
                    }

                    globalAttributes.Add(new[] { stringArray[1], attributeValue });
                }

                else
                {
                    globalAttributes.Add(new[] { stringArray[1], stringArray[2] });
                }
            }
        }

        return globalAttributes;
    }


    // If able to find one, FindTitle returns the global attribute known as "title",
    // otherwise returns null.
    public static string? FindTitle(List<string[]> globalAttributes)
    {
        foreach (var line in globalAttributes)
        {
            if (line[0] == "title")
            {
                return line[1];
            }
        }

        return null;
    }


    // If able to find one, FindSummary returns the global attribute known as "summary",
    // otherwise returns null.
    public static string? FindSummary(List<string[]> globalAttributes)
    {
        foreach (var line in globalAttributes)
        {
            if (line[0] == "summary")
            {
                return line[1];
            }
        }

        return null;
    }


    // Takes the list generated by FindGlobalAttributes, and adds them to a dataset
    // as a dictionary .
    public static void AddGlobalAttributes(DataSet dataSet, List<string[]> globalAttributes)
    {
        foreach (var keyValuePair in globalAttributes)
        {
            dataSet.GlobalAttributes.Add(keyValuePair[0], keyValuePair[1]);
        }
    }


    // Returns a list of variable metadata points where each metadata point is represented
    // as a string array where [0] is the variable name, [1] is the attribute name and
    // [2] to [n] is the values.
    // Note: Does not include global metadata.

    public static List<string[]> FindVariableMetaData(List<string[]> separatedNccsv)
    {
        var variableMetaData = new List<string[]>();

        foreach (var line in separatedNccsv)
        {

            // Disgregard global attributes as they are collected in FindGlobalAttributes
            if (line[0].Contains("*GLOBAL*"))
            {
                continue;
            }

            // End collection of metadata at metadata end tag (needs to exist)
            if (line[0].Contains("*END_METADATA*"))
            {
                break;
            }

            variableMetaData.Add(line);
        }


        return variableMetaData;
    }


    // Given a list of variables and a variable name,
    // returns true if variable name exists in the dataset variables, otherwise false.
    public static bool CheckIfVariableExists(List<Variable> variables, string variableName)
    {
        foreach (var v in variables)
        {
            if (v.VariableName == variableName)
            {
                return true;
            }
        }

        return false;
    }


    // Given a list of arrays of variable metadata, returns extracted variable metadata
    // associated to a given variable name.
    public static List<string[]> IsolateVariableAttributes(List<string[]> variableMetaData, string variableName)
    {
        var isolatedVariableAttributes = new List<string[]>();

        foreach (var line in variableMetaData)
        {
            if (line[0] == variableName)
            {
                isolatedVariableAttributes.Add(line);
            }
        }

        return isolatedVariableAttributes;
    }


    // When given a IsolateVariableAttributes list, creates a Variable out of that data.
    public static Variable CreateVariable(List<string[]> variableMetaData)
    {
        var newVariable = new Variable();

        // This loop finds the name of the variable, even if there would be
        // empty lines before it.
        foreach (var line in variableMetaData)
        {
            if (line[0].Length > 0)
            {
                newVariable.VariableName = line[0];

                //Also, adding a Scalar property to the Variable model, also inserts the value of the scalar.
                if (line[1] == "*SCALAR*")
                {
                    newVariable.Scalar = true;
                    newVariable.ScalarValue = line[2];
                }
                break;
            }
        }

        SetVariableDataType(newVariable, variableMetaData);
        AddAttributes(newVariable, variableMetaData);

        return newVariable;
    }


    // Takes a variable metadata list, extracts the name of the data type
    // and sets a given variable data type to that name.
    // Used by: CreateVariable
    public static void SetVariableDataType(
        Variable variable, List<string[]> variableMetaData)
    {
        string variableDataType = "";

        foreach (var line in variableMetaData)
        {
            if (line[1] == "*DATA_TYPE*")
            {
                foreach (var typeName in Enum.GetNames(typeof(DataType)))
                {
                    if (typeName.ToLower() == line[2].ToLower())
                    {
                        variableDataType = typeName.ToLower();
                    }
                }
            }

            if (line[1] == "*SCALAR*")
            {
                variableDataType = GetTypeOf(line[2]).ToString().ToLower();
            }
        }

        variable.DataType = variableDataType;

    }


    //when testing is done, this should maybe be made private?
    public static DataType GetTypeOf(string value)
    {
        switch (value[^1])
        {
            case 'b':
                if (value[^2] == 'u')
                {
                    if (Int32.TryParse(value[..^3], out _))
                    {
                        return DataType.Ubyte;
                    }

                }
                else if (Int32.TryParse(value[..^2], out _))
                {
                    return DataType.Byte;
                }

                return DataType.String;

            case 's':
                if (value[^2] == 'u')
                {
                    if (Int32.TryParse(value[..^3], out _))
                    {
                        return DataType.Ushort;
                    }
                }

                else if (Int32.TryParse(value[..^2], out _))
                {
                    return DataType.Short;
                }

                return DataType.String;

            case 'i':
                if (value[^2] == 'u')
                {
                    if (uint.TryParse(value[..^3], out _))
                    {
                        return DataType.Uint;
                    }
                }

                else if (int.TryParse(value[..^2], out _))
                {
                    return DataType.Int;
                }

                return DataType.String;

            case 'L':
                if (value[^2] == 'u')
                {
                    if (ulong.TryParse(value[..^3], out _))
                    {
                        return DataType.Ulong;
                    }
                }

                else if (long.TryParse(value[..^2], out _))
                {
                    return DataType.Long;
                }

                return DataType.String;


            case 'f':
                value = value.Replace('.', ',').ToLower();
                if (float.TryParse(value[..^1], out _))
                {
                    return DataType.Float;
                }

                return DataType.String;


            case 'd':
                value = value.Replace('.', ',').ToLower();
                if (double.TryParse(value[..^2], out _))
                {
                    return DataType.Double;
                }

                return DataType.String;

            case '\'':
                if (value[0] == '\'')
                {
                    return DataType.Char;
                }

                return DataType.String;

            default:
                return DataType.String;
        }

    }


    // Adds attributes to a given Variable as a dictionary where
    // [1] is the attribute name and [2] to [n] is the attribute values
    // Used by: CreateVariable
    public static void AddAttributes(Variable variable, List<string[]> variableMetaData)
    {
        foreach (var attribute in variableMetaData)
        {
            // Disregard data type row as datatype is set in SetVariableDataType
            if (attribute[1] == "*DATA_TYPE*")
            {
                continue;
            }

            var attributeName = attribute[1];
            List<string> values = new List<string>();

            for (int i = 2; i < attribute.Length; i++)
            {
                values.Add(attribute[i]);
            }

            // Add to Attributes as <[1], List<[2]-[n]>>
            variable.Attributes.Add(attributeName, values);
        }
    }


    // Extracts and returns the data section from the nccsv-file.
    public static List<string[]> FindData(List<string[]> separatedNccsv)
    {
        var data = new List<string[]>();
        var dataSectionReached = false;

        foreach (var line in separatedNccsv)
        {
            if (line[0] == "*END_DATA*")
            {
                break;
            }

            if (dataSectionReached)
            {
                data.Add(line);
            }

            if (line[0] == "*END_METADATA*")
            {
                dataSectionReached = true;
            }
        }

        return data;
    }


    // Adds data to a given DataSet
    public static void AddData(List<string[]> data, DataSet dataSet)
    {
        for (int i = 1; i < data.Count; i++)
        {
            List<DataValue> dataRow = new();

            for (int j = 0; j < data[i].Length; j++)
            {
                var variable = dataSet.Variables
                    .FirstOrDefault(v => v.VariableName
                        .Equals(data[0][j]));

                if (variable != null)
                {
                    var dataValue = CreateDataValueAccordingToDataType(data[i][j], variable);

                    if (dataValue != null)
                        dataRow.Add(dataValue);
                    else
                    {
                        MessageRepository.Messages.Add(
                            new Message($"Data value: {data[i][j]} could not be parsed to variable datatype: {variable.DataType}.", Severity.NonCritical));
                    }
                }
                else
                {
                    MessageRepository.Messages.Add(
                        new Message($"Header: {data[0][j]} did not match any variables.", Severity.NonCritical));
                }
            }

            dataSet.Data.Add(dataRow.ToArray());
        }
    }


    // Creates and returns a DataValueAs<T> from a given value and variable,
    // where T is the DataType of the variable that acts as column header.
    // If unsuccessfull, returns a null value.
    // TODO: Proper test
    public static DataValue? CreateDataValueAccordingToDataType(string value, Variable variable)
    {
        bool result;

        switch (variable.DataType)
        {
            case "byte":
                // byte -> c# sbyte
                result = sbyte.TryParse(value, out sbyte byteValue);

                if (result)
                    return new DataValueAs<sbyte>()
                    {
                        DataType = "byte",
                        Value = byteValue
                        // Variable = variable
                    };
                else
                    return null;

            case "ubyte":
                // unsigned byte -> c# byte
                result = byte.TryParse(value, out byte ubyteValue);

                if (result)
                    return new DataValueAs<byte>()
                    {
                        DataType = "ubyte",
                        Value = ubyteValue
                        // Variable = variable
                    };
                else
                    return null;

            case "short":
                result = short.TryParse(value, out short shortValue);

                if (result)
                    return new DataValueAs<short>()
                    {
                        DataType = "short",
                        Value = shortValue
                        // Variable = variable
                    };
                else
                    return null;

            case "ushort":
                result = ushort.TryParse(value, out ushort ushortValue);

                if (result)
                    return new DataValueAs<ushort>()
                    {
                        DataType = "ushort",
                        Value = ushortValue
                        // Variable = variable
                    };
                else
                    return null;

            case "int":
                result = int.TryParse(value, out int intValue);

                if (result)
                    return new DataValueAs<int>()
                    {
                        DataType = "int",
                        Value = intValue
                        // Variable = variable
                    };
                else
                    return null;

            case "uint":
                result = uint.TryParse(value, out uint uintValue);

                if (result)
                    return new DataValueAs<uint>()
                    {
                        DataType = "uint",
                        Value = uintValue
                        // Variable = variable
                    };
                else
                    return null;

            case "long":
                // TODO: check L
                result = long.TryParse(value[..^1], out long longValue);

                if (result)
                    return new DataValueAs<long>()
                    {
                        DataType = "long",
                        Value = longValue
                        // Variable = variable
                    };
                else
                    return null;

            case "ulong":
                // TODO: check uL
                result = ulong.TryParse(value[..^2], out ulong ulongValue);

                if (result)
                    return new DataValueAs<ulong>()
                    {
                        DataType = "ulong",
                        Value = ulongValue
                        // Variable = variable
                    };
                else
                    return null;

            case "float":
                result = float.TryParse(value, CultureInfo.InvariantCulture, out float floatValue);

                if (result)
                    return new DataValueAs<float>()
                    {
                        DataType = "float",
                        Value = floatValue
                        // Variable = variable
                    };
                else
                    return null;

            case "double":
                result = double.TryParse(value, CultureInfo.InvariantCulture, out double doubleValue);

                if (result)
                    return new DataValueAs<double>()
                    {
                        DataType = "double",
                        Value = doubleValue
                        // Variable = variable
                    };
                else
                    return null;

            case "string":
                return new DataValueAs<string>
                {
                    DataType = "string",
                    Value = value,
                    // Variable = variable
                };

            case "char":
                // TODO: handle special char cases
                result = char.TryParse(value, out char charValue);

                if (result)
                    return new DataValueAs<char>()
                    {
                        DataType = "char",
                        Value = charValue
                        // Variable = variable
                    };
                else
                    return null;

            default:
                return null;
        }
    }


    // Splits a given line at "," but not if it's within a string.
    // Returns the split line as a list of strings.
    public static List<string> Separate(string line, int row)
    {
        List<string> separatedLine = new List<string>();
        string tempString = string.Empty;
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '\"')
            {
                inQuotes = !inQuotes;
            }

            else if (line[i] == ',')
            {
                // Ignore commas within a string
                if (!inQuotes)
                {
                    Verifier.VerifyValue(tempString, row);
                    separatedLine.Add(tempString.Trim().Trim('"'));
                    tempString = string.Empty;
                    continue;
                }
            }

            tempString += line[i];
        }

        Verifier.VerifyValue(tempString, row);
        separatedLine.Add(tempString.Trim().Trim('"'));

        return separatedLine;
    }
}

