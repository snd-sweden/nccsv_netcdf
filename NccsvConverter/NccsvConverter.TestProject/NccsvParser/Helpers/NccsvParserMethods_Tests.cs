using NccsvConverter.NccsvParser.Models;
using System.Globalization;

namespace NccsvConverter.TestProject.NccsvParser.Helpers;

public class NccsvParserMethods_Tests
{

    [Fact]
    public void FindGlobalAttributes_ReturnsCorrectList()
    {
        //Arrange
        var separatedNccsv = Handler.NccsvFileReader(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName +
            "\\NccsvConverter.ConsoleApp\\TestData\\ryder.nccsv");
        bool result = true;

        //Act
        var globalAttributes = NccsvParserMethods.FindGlobalAttributes(separatedNccsv);

        //Assert
        // If the nccvs was handled properly, it should consist of a list of arrays of length 2:
        // each a key/value pair excellent for putting in a Dictionary.
        foreach (var sArr in globalAttributes)
        {
            if (sArr.Length != 2)
            {
                result = false;
            }
        }

        Assert.True(result);
    }


    [Theory]
    [InlineData("title")]
    public void FindTitle_FindsTitle(string title)
    {
        //Arrange
        var testList = new List<string[]>()
        {
            new []{"hi","row1"},
            new []{title, "row2"},
            new []{"bye", "row3"}

        };

        //Act
        string foundTitle = NccsvParserMethods.FindTitle(testList);

        //Assert
        Assert.Equal("row2", foundTitle);
    }


    [Theory]
    [InlineData("summary")]
    public void FindSummary_FindsSummary(string summary)
    {
        //Arrange
        var testList = new List<string[]>()
        {
            new []{"hi","row1"},
            new []{summary, "row2"},
            new []{"bye", "row3"}

        };

        //Act
        string foundSummary = NccsvParserMethods.FindSummary(testList);

        //Assert
        Assert.Equal("row2", foundSummary);
    }


    [Fact]
    public void AddGlobalAttributes_AddsAttributesProperly()
    {
        //Arrange
        var globalAttributes = new List<string[]>
        {
            new string[2] { "hej", "då" },
            new string[2] { "ses", "sen" }
        };

        var dataSet = new DataSet();

        //Act
        NccsvParserMethods.AddGlobalAttributes(dataSet, globalAttributes);

        //Assert
        Assert.Equal("då", dataSet.MetaData.GlobalAttributes["hej"]);
        Assert.Equal("sen", dataSet.MetaData.GlobalAttributes["ses"]);
    }


    [Fact]
    public void FindVariableMetaData_ReturnsListOfStringArrays()
    {
        //Arrange
        var separatedNccsv = Handler.NccsvFileReader(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName +
            "\\NccsvConverter.ConsoleApp\\TestData\\ryder.nccsv");

        //Act 
        var variableMetaData = NccsvParserMethods.FindVariableMetaData(separatedNccsv);

        //Assert
        Assert.IsType<List<string[]>>(variableMetaData);
    }


    [Fact]
    public void FindVariableMetaData_DoesNotReturnGlobalAttributes()
    {
        //Arrange
        var separatedNccsv = Handler.NccsvFileReader(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName 
          + "\\NccsvConverter.ConsoleApp\\TestData\\ryder.nccsv");

        var expected = "*GLOBAL*";

        //Act 
        var variableMetaData = NccsvParserMethods.FindVariableMetaData(separatedNccsv);

        //Assert
        Assert.NotEqual(expected, variableMetaData[0][0]);
    }


    [Theory]
    [InlineData("rainfall_avg")]
    [InlineData("ship_name")]
    public void CheckIfVariableExists_FindsVariable(string variableName)
    {
        //Arrange
        var variableList = new List<Variable>
        {
            new Variable() { DataType = "int", VariableName = "rainfall_avg" },
            new Variable() { DataType = "string",VariableName = "ship_name"}
        };

        //Act
        var result = NccsvParserMethods.CheckIfVariableExists(variableList, variableName);

        //Assert
        Assert.True(result);
    }


    [Fact]
    public void CreateVariable_CreatesVariableWithExpectedAttributes()
    {
        //TODO: complete this test
        //Arrange
        var variableMetaData = new List<string[]>
        {
            new []{"depth","*DATA_TYPE*","double"},
            new []{"depth","positive","down"},
            new []{"depth","standard_name","sea_floor_depth_below_sea_surface"},
            new []{"depth","units","m"},
            new []{"depth","_OrigionalName","Oden.MB.SeaDepth%Avg"}
        };

        //Act
        var variable = NccsvParserMethods.CreateVariable(variableMetaData);

        //Assert
        Assert.Equal("depth",variable.VariableName);
        Assert.Equal("double", variable.DataType);
        Assert.Contains(variableMetaData[1][1], variable.Attributes.Keys);
        Assert.Contains(variableMetaData[2][1], variable.Attributes.Keys);
        Assert.Contains(variableMetaData[3][1], variable.Attributes.Keys);
        Assert.Contains(variableMetaData[4][1], variable.Attributes.Keys);



    }


    [Fact]
    public void CreateVariable_CreatesScalarVariableWithCorrectType()
    {
        //Arrange
        var variableMetaData = new List<string[]>
        {
            new []{"project","*SCALAR*","Ryder 2019"}
        };

        //Act
        var variable = NccsvParserMethods.CreateVariable(variableMetaData);

        //Assert
        Assert.True(variable.Scalar);
        Assert.Equal("string", variable.DataType);
    }


    [Fact]
    public void IsolateVariableMetaData_ReturnsCorrectLines()
    {
        //Arrange
        var separatedNccsv = Handler.NccsvFileReader(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName +
            "\\NccsvConverter.ConsoleApp\\TestData\\ryder.nccsv");
        var variableMetaData = NccsvParserMethods.FindVariableMetaData(separatedNccsv);
        var variableName = "depth";

        //Act
        var depthProperty = NccsvParserMethods.IsolateVariableAttributes(variableMetaData, variableName);

        //Assert
        Assert.True(depthProperty.Count >= 2);
        foreach (var line in depthProperty)
        {
            Assert.Equal(variableName, line[0]);
        }
    }


    [Fact]
    public void SetVariableDataType_SetsCorrectDataType()
    {
        //Arrange
        var separatedNccsv = Handler.NccsvFileReader(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName +
            "\\NccsvConverter.ConsoleApp\\TestData\\ryder.nccsv");
        var variableMetaData = NccsvParserMethods.FindVariableMetaData(separatedNccsv);
        var variableName = "depth";
        var isolatedVariableAttributes = NccsvParserMethods.IsolateVariableAttributes(variableMetaData, variableName);

        var testVariable = new Variable() { VariableName = variableName };

        var expected = "double";

        //Act
        NccsvParserMethods.SetVariableDataType(testVariable, isolatedVariableAttributes);

        //Assert
        Assert.Equal(expected, testVariable.DataType);
    }


    [Fact]
    public void FindVariableMetaData_ReturnsExpectedList()
    {
        //Arrange
        var separatedNccsv = new List<string[]>
        {
            new string[] { "*GLOBAL*", "abc", "def" },
            new string[] { "abc", "def", "ghi", "j\",k\"l" },
            new string[] { "mno", "pqr", "stu", "vxy" },
        };

        var expected = new List<string[]>
        {
            new string[] { "abc", "def", "ghi", "j\",k\"l" },
            new string[] { "mno", "pqr", "stu", "vxy" }
        };

        //Act 
        var variableMetaData = NccsvParserMethods.FindVariableMetaData(separatedNccsv);

        //Assert
        Assert.Equal(expected, variableMetaData);
    }


    [Fact]
    public void FindData_ReturnsDataAsListOfStringArrays()
    {
        //Arrange
        var separatedNccsv = Handler.NccsvFileReader(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName +
            "\\NccsvConverter.ConsoleApp\\TestData\\ryder.nccsv");

        //Act
        var data = NccsvParserMethods.FindData(separatedNccsv);

        //Assert
        Assert.IsType<List<string[]>>(data);
    }


    [Fact]
    public void FindData_FindsExpectedData()
    {
        //Arrange
        var separatedNccsv = new List<string[]>
        {
            new string[]
            {
                "*GLOBAL*", "Attributes", "Here"
            },
            new string []
            {
                "Attributes", "Are", "Here", "To", "Stay"
            },
            new string[]
            {
                "*END_METADATA*"
            },
            new string[]
            {
                "header1", "header2", "header3"
            },
            new string[]
            {
                "value1", "value2", "value3"
            },
            new string[]
            {
                "*END_DATA*"
            }
        };

        var expected = new List<string[]>
        {
            new string[]
            {
                "header1", "header2", "header3"
            },
            new string[]
            {
                "value1", "value2", "value3"
            },
        };

        //Act
        var data = NccsvParserMethods.FindData(separatedNccsv);

        //Assert
        Assert.Equal(expected, data);
    }


    [Fact]
    public void FindData_FindsDataAsList()
    {
        //Arrange
        var separatedNccsv = Handler.NccsvFileReader(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName +
            "\\NccsvConverter.ConsoleApp\\TestData\\ryder.nccsv");

        //Act
        var data = NccsvParserMethods.FindData(separatedNccsv);

        //Assert
        Assert.True(data.Count >= 2);
    }


    [Fact]
    public void AddData_AddsExpectedDataToDataSet()
    {
        //Arrange
        var dataSet = new DataSet();

        dataSet.MetaData.Variables.AddRange(
            new List<Variable>
            {
                new Variable ()
                {
                    VariableName = "header1",
                    DataType = "string"
                },
                new Variable ()
                {
                    VariableName = "header2",
                    DataType = "int"
                },
                new Variable ()
                {
                    VariableName = "header3",
                    DataType = "double"
                },
                new Variable ()
                {
                    VariableName = "header4",
                    DataType = "double"
                }
            });

        var data = new List<string[]>
        {
            new string[]
            {
                "header1", "header2", "header3", "header4"
            },
            new string[]
            {
                "value1", "", "2", "NaN"
            },
        };

        var expected = new List<DataValue[]>
        {
            new DataValue[]
            {
                new DataValueAs<string>
                {
                    Variable = new Variable()
                    {
                        DataType = "string", 
                        VariableName = "header1"
                    },
                    Value = "value1"
                },
                new DataValueAs<int?>
                {
                    Variable = new Variable ()
                    {
                        VariableName = "header2",
                        DataType = "int"
                    },
                    Value = null
                },
                new DataValueAs<double>
                {
                    Variable = new Variable ()
                    {
                        VariableName = "header3",
                        DataType = "double"
                    },
                    Value = 2
                },
                new DataValueAs<double>
                {
                    Variable = new Variable ()
                    {
                        VariableName = "header4",
                        DataType = "double"
                    },
                    Value = double.NaN
                }
            }
        };

        int rowNumber = 1;

        //Act
        NccsvParserMethods.AddData(data, dataSet, rowNumber);

        //Assert
        Assert.Equivalent(expected, dataSet.Data);
    }


    [Theory]
    [InlineData("100", "byte")]
    [InlineData("-100", "byte")]
    [InlineData("230", "ubyte")]
    [InlineData("-30000", "short")]
    [InlineData("60000", "ushort")]
    [InlineData("-100000", "int")]
    [InlineData("4123456789", "uint")]
    [InlineData("12345678987654321L", "long")]
    [InlineData("9007199254740992uL", "ulong")]
    [InlineData("1,23", "float")]
    [InlineData("1.23", "float")]
    [InlineData("1,23e2", "float")]
    [InlineData("1,23e-2", "float")]
    [InlineData("1,23E-2", "float")]
    [InlineData("1,87", "double")]
    [InlineData("1.87", "double")]
    [InlineData("1,87e-12", "double")]
    [InlineData("1.87e-12", "double")]
    [InlineData("1,87E12", "double")]
    [InlineData("1,87e12", "double")]
    [InlineData("h", "char")]
    [InlineData("hello!", "string")]
    public void CreateDataValueAccordingToDataType_ReturnsDataValueAsExpected(string value, string dataType)
    {
        //Arrange
        var variable = new Variable
        {
            VariableName = "header",
            DataType = dataType
        };

        DataValue expected = null;

        if (dataType == "byte")
        {
            expected = new DataValueAs<sbyte>
            {
                Variable = variable,
                Value = sbyte.Parse(value)
            };
        }
        else if (dataType == "ubyte")
        {
            expected = new DataValueAs<byte>
            {
                Variable = variable,
                Value = byte.Parse(value)
            };
        }
        else if (dataType == "short")
        {
            expected = new DataValueAs<short>
            {
                Variable = variable,
                Value = short.Parse(value)
            };
        }
        else if (dataType == "ushort")
        {
            expected = new DataValueAs<ushort>
            {
                Variable = variable,
                Value = ushort.Parse(value)
            };
        }
        else if (dataType == "int")
        {
            expected = new DataValueAs<int>
            {
                Variable = variable,
                Value = int.Parse(value)
            };
        }
        else if (dataType == "uint")
        {
            expected = new DataValueAs<uint>
            {
                Variable = variable,
                Value = uint.Parse(value)
            };
        }
        else if (dataType == "long")
        {
            expected = new DataValueAs<long>
            {
                Variable = variable,
                Value = long.Parse(value[..^1])
            };
        }
        else if (dataType == "ulong")
        {
            expected = new DataValueAs<ulong>
            {
                Variable = variable,
                Value = ulong.Parse(value[..^2])
            };
        }
        else if (dataType == "float")
        {
            expected = new DataValueAs<float>
            {
                Variable = variable,
                Value = float.Parse(value, CultureInfo.InvariantCulture)
            };
        }
        else if (dataType == "double")
        {
            expected = new DataValueAs<double>
            {
                Variable = variable,
                Value = double.Parse(value, CultureInfo.InvariantCulture)
            };
        }
        else if (dataType == "string")
        {
            expected = new DataValueAs<string>
            {
                Variable = variable,
                Value = value
            };
        }
        else if (dataType == "char")
        {
            expected = new DataValueAs<char>
            {
                Variable = variable,
                Value = char.Parse(value)
            };
        }

        //Act 
        var result = NccsvParserMethods.CreateDataValueAccordingToDataType(value, variable);

        //Assert
        Assert.Equivalent(expected, result);
    }


    [Theory]
    [InlineData("", "byte")]
    [InlineData("", "ubyte")]
    [InlineData("", "short")]
    [InlineData("", "ushort")]
    [InlineData("", "int")]
    [InlineData("", "uint")]
    [InlineData("", "long")]
    [InlineData("", "ulong")]
    [InlineData("", "float")]
    [InlineData("", "double")]
    [InlineData("", "char")]
    [InlineData("", "string")]
    public void CreateDataValueAccordingToDataType_ReturnsMissingDataValuesAsExpected(string value, string dataType)
    {
        //Arrange
        var variable = new Variable
        {
            VariableName = "header",
            DataType = dataType
        };

        DataValue expected = null;

        if (dataType == "byte")
        {
            expected = new DataValueAs<sbyte?>
            {
                Variable = variable,
                Value = null
            };
        }
        else if (dataType == "ubyte")
        {
            expected = new DataValueAs<byte?>
            {
                Variable = variable,
                Value = null
            };
        }
        else if (dataType == "short")
        {
            expected = new DataValueAs<short?>
            {
                Variable = variable,
                Value = null
            };
        }
        else if (dataType == "ushort")
        {
            expected = new DataValueAs<ushort?> 
            { 
                Variable = variable, 
                Value = null 
            };
        }
        else if (dataType == "int")
        {
            expected = new DataValueAs<int?>
            {
                Variable = variable,
                Value = null
            };
        }
        else if (dataType == "uint")
        {
            expected = new DataValueAs<uint?>
            {
                Variable = variable,
                Value = null
            };
        }
        else if (dataType == "long")
        {
            expected = new DataValueAs<long?>
            {
                Variable = variable,
                Value = null
            };
        }
        else if (dataType == "ulong")
        {
            expected = new DataValueAs<ulong?>
            {
                Variable = variable,
                Value = null
            };
        }
        else if (dataType == "float")
        {
            expected = new DataValueAs<float>
            {
                Variable = variable,
                Value = float.NaN
            };
        }
        else if (dataType == "double")
        {
            expected = new DataValueAs<double>
            {
                Variable = variable,
                Value = double.NaN
            };
        }
        else if (dataType == "string")
        {
            expected = new DataValueAs<string>
            {
                Variable = variable,
                Value = ""
            };
        }

        //Act 
        var result = NccsvParserMethods.CreateDataValueAccordingToDataType(value, variable);

        //Assert
        Assert.Equivalent(expected, result);
    }


    [Fact]
    public void CreateDataValueAccordingToDataType_ReturnsNullIfParseFailed()
    {
        //Arrange
        var value = "1474.5319";

        var variable = new Variable
        {
            VariableName = "header",
            DataType = "int"
        };

        //Act 
        var result = NccsvParserMethods.CreateDataValueAccordingToDataType(value, variable);

        //Assert
        Assert.Equivalent(null, result);
    }


    [Fact]
    public void Separate_ReturnsSeparatedValuesAsList()
    {
        //Arrange
        var row = 1;

        var line = "abc,def,ghi,jkl";

        List<string> expected = new List<string>
        {
            "abc",
            "def",
            "ghi",
            "jkl"
        };

        //Act 
        var separatedLine = NccsvParserMethods.Separate(line, row);

        //Assert
        Assert.Equal(expected, separatedLine);
    }


    [Fact]
    public void Separate_ReturnsSeparatedValues_WhenQuotes()
    {
        //Arrange
        var row = 1;

        var line = "abc,def,ghi,\"jkl,mno\"";

        List<string> expected = new List<string>
        {
            "abc",
            "def",
            "ghi",
            "jkl,mno"
        };

        //Act 
        var separatedLine = NccsvParserMethods.Separate(line, row);

        //Assert
        Assert.Equal(expected, separatedLine);
    }


    [Fact]
    public void Separate_ReturnsSeparatedValues_WhenQuotesInQuotes()
    {
        //Arrange
        var row = 1;

        var line = "abc,def,ghi,\"jkl,\"\"m,\"\"no\"";

        List<string> expected = new List<string>
        {
            "abc",
            "def",
            "ghi",
            "jkl,\"\"m,\"\"no"
        };

        //Act 
        var separatedLine = NccsvParserMethods.Separate(line, row);

        //Assert
        Assert.Equal(expected, separatedLine);
    }


    [Fact]
    public void AddAttributes_AddsPropertiesAsExpected()
    {
        //Arrange
        var variable = new Variable();
        var variableAttributes = new List<string[]>
        {
            new [] {"abc", "def", "ghi", "j\",k\"l" }
        };

        var expected = new Dictionary<string, List<string>>()
        {
            { "def", new List<string> { "ghi", "j\",k\"l" }}
        };

        //Act 
        NccsvParserMethods.AddAttributes(variable, variableAttributes);

        //Assert
        Assert.Equal(expected, variable.Attributes);
    }

    [Theory]
    [InlineData("100b", DataType.Byte)]
    [InlineData("-100b", DataType.Byte)]
    [InlineData("230ub", DataType.Ubyte)]
    [InlineData("-30000s", DataType.Short)]
    [InlineData("60000us", DataType.Ushort)]
    [InlineData("-100000i", DataType.Int)]
    [InlineData("4123456789ui", DataType.Uint)]
    [InlineData("12345678987654321L", DataType.Long)]
    [InlineData("9007199254740992uL", DataType.Ulong)]
    [InlineData("1,23f", DataType.Float)]
    [InlineData("1.23f", DataType.Float)]
    [InlineData("1,23e2f", DataType.Float)]
    [InlineData("1,23e-2f", DataType.Float)]
    [InlineData("1,23E-2f", DataType.Float)]
    [InlineData("1,87d", DataType.Double)]
    [InlineData("1.87d", DataType.Double)]
    [InlineData("1,87e-12d", DataType.Double)]
    [InlineData("1.87e-12d", DataType.Double)]
    [InlineData("1,87E12d", DataType.Double)]
    [InlineData("1,87e12d", DataType.Double)]
    [InlineData("'h'", DataType.Char)]
    [InlineData("hello!", DataType.String)]
    public void GetTypeOf_GetsCorrectType(string value, DataType expected)
    {
        //Arrange 

        //Act
        var result = NccsvParserMethods.GetTypeOf(value);
        //Assert
        Assert.Equal(result, expected);
    }
};