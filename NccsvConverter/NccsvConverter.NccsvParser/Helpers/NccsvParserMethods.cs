﻿
using System.Linq.Expressions;
using System.Text.Json.Serialization.Metadata;
using NccsvConverter.NccsvParser.Models;

namespace NccsvConverter.NccsvParser.Helpers
{
    public class NccsvParserMethods
    {
        // read file = new dataset

        public static string FindTitle(List<string[]> globalProperties)
        {
            foreach (var line in globalProperties)   
            {
                if (line[0] == "title")
                {
                    return line[1];
                }
            }

            return null;
        }

        public static string FindSummary(List<string[]> globalProperties)
        {
            foreach (var line in globalProperties)
            {
                if (line[0] == "summary")
                {
                    return line[1];
                }
            }

            return null;
        }

        //finds global attributes and stores them without *GLOBAL*-tag in a string array
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

        //takes the list generated by FindGlobalAttributes, and adds them to it's
        //GlobalAttributes attribute.
        public static void AddGlobalAttributes(DataSet dataSet, List<string[]> globalAttributes)
        {
            foreach (var keyValuePair in globalAttributes)
            {
                dataSet.GlobalAttributes.Add(keyValuePair[0], keyValuePair[1]);
            }

        }


        /*Constructs a list of properties where each property is represented as a string array where [0]
        is the variable name, [1] is the attribute name and [2] to [n] is the values. Does not include
        global properties as they are collected in FindGlobalAttributes.*/
        public static List<string[]> FindVariableMetaData(List<string[]> separatedNccsv)
        {
            var variableMetaData = new List<string[]>();

            foreach (var line in separatedNccsv)
            {
                // disgregard global properties as they are collected in FindGlobalAttributes
                if (line[0].Contains("*GLOBAL*"))
                {
                    continue;
                }

                // end collection of properties at metadata end tag (needs to exist)
                if (line[0].Contains("*END_METADATA*"))
                {
                    break;
                }

                // add to properties
                variableMetaData.Add(line);

            }

            return variableMetaData;
        }

        //returns true if variable name exists, otherwise false.
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

        //extracts a named property and all it's associated metadata from a FindVariableMetaData list
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

        //when given a IsolateVariableAttributes list, creates a Variable out of that data.
        public static Variable CreateVariable(List<string[]> variableMetaData)
        {
            var newVariable = new Variable();

            //this loop finds the name of the variable, even if there would be
            //empty lines before the actual property data
            foreach (var line in variableMetaData)
            {
                if (line[0].Length > 0)
                {
                    newVariable.VariableName = line[0];
                    break;
                }
            }

            SetVariableDataType(newVariable, variableMetaData);
            AddAttributes(newVariable, variableMetaData);

            return newVariable;
        }

        // Takes a variable property list, and extracts the name of the data type.
        // Used by: CreateVariable
        public static Variable SetVariableDataType(
            Variable variable, List<string[]> variableMetaData)
        {
            string variableDataType = "";

            foreach (var line in variableMetaData)
            {
                if (line[1] == "*DATA_TYPE*")
                {
                    variableDataType = line[2];
                }
            }

            variable.DataType = variableDataType;

            return variable;
        }


        // Adds string array of properties to variable as a dictionary where
        // [1] is the attribute name and [2] to [n] is the attribute values
        // Used by: CreateVariable
        public static void AddAttributes(Variable variable, List<string[]> variableMetaData )
        {
            foreach (var attribute in variableMetaData)
            {
                // disregard data type row as datatype is set in SetVariableDataType
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

                // add to Attributes as <[1], List<[2]-[n]>>
                variable.Attributes.Add(attributeName, values);

            }
        }

       //Extracts the data section of the nccsv-file.
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
                
                if (line[0] =="*END_METADATA*")
                {
                    dataSectionReached = true;
                }

            }

            return data;
        }

        public static void AddData(List<string[]> data, DataSet dataSet)
        {
            dataSet.Data = data;
        }


        // Splits line at "," but not if it's within a string
        public static List<string> Separate(string line)
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
                    // Only regards commas outside of quotes
                    if (!inQuotes)
                    {
                        separatedLine.Add(tempString);
                        tempString = string.Empty;
                        continue;
                    }
                }

                tempString += line[i];
            }

            separatedLine.Add(tempString);

            return separatedLine;
        }



    }
}
