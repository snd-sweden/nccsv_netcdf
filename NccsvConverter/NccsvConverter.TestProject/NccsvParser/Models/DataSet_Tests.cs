﻿using NccsvConverter.NccsvParser.Models;

namespace NccsvConverter.TestProject.NccsvParser.Models;

public class DataSet_Tests
{
    [Fact]
    public void FromStream_ReturnsDataSetWithCorrectDataAndMetaData()
    {
        //Arrange
        var filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName +
            "\\NccsvConverter.ConsoleApp\\TestData\\justenough.nccsv";

        var expectedData = new List<DataValue[]>
        {
            new DataValue[]
            {
                new DataValueAs<string>
                {
                    Variable = new Variable
                    {
                        VariableName = "variable",
                        DataType = "string",
                        Scalar = false,
                        ScalarValue = null,
                        Attributes = new Dictionary<string, List<string>>()
                    },
                    Value = "datavalue"
                }
            }
        };

        var expectedMetaData = new MetaData
        {
            Title =  null,
            Summary = null,
            GlobalAttributes = new Dictionary<string, string>
            {
                { "Conventions","COARDS, CF-1.6, ACDD-1.3, NCCSV-1.1" }
            },
            Variables = new List<Variable>
            {
                new Variable
                {
                    VariableName = "variable",
                    DataType = "string",
                    Scalar = false,
                    ScalarValue = null,
                    Attributes = new Dictionary<string, List<string>>()
                }
            }
        };

        //Act
        FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        DataSet dataSet = DataSet.FromStream(stream, true);

        //Assert
        Assert.Equivalent(expectedData, dataSet.Data);
        Assert.Equivalent(expectedMetaData, dataSet.MetaData);
    }


    //[Fact]
    //public void MetaDataHandler()
    //{
    //    //Arrange
    //    var dataSet = new DataSet();

    //    var metaDataList = new List<string[]>
    //    {
    //        new string[] 
    //        {
    //            "*GLOBAL*","Conventions","COARDS, CF-1.6, ACDD-1.3, NCCSV-1.1"
    //        },
    //        new string[] 
    //        {
    //            "*GLOBAL*","title","title"
    //        },
    //        new string[] 
    //        { 
    //            "*GLOBAL*","summary","summary"
    //        },
    //        new string[]
    //        {
    //            "variable","*DATA_TYPE*", "String"
    //        },
    //        new string[]
    //        {
    //            "variable","attribute","value1","value2"
    //        }
    //    };

    //    var expectedMetaData = new MetaData
    //    {
    //        Title = "title",
    //        Summary = "summary",
    //        GlobalAttributes = new Dictionary<string, string>
    //        {
    //            { "Conventions","COARDS, CF-1.6, ACDD-1.3, NCCSV-1.1" },
    //            { "title", "title"},
    //            { "summary", "summary" }
    //        },
    //        Variables = new List<Variable>
    //        {
    //            new Variable
    //            {
    //                VariableName = "variable",
    //                DataType = "string",
    //                Scalar = false,
    //                ScalarValue = null,
    //                Attributes = new Dictionary<string, List<string>>
    //                {
    //                    { 
    //                        "attribute", new List<string>
    //                        {
    //                            "value1", "value2"
    //                        } 
    //                    }
    //                }
    //            }
    //        }
    //    };

    //    //Act
    //    dataSet.MetaDataHandler(metaDataList);

    //    //Assert
    //    Assert.Equivalent(expectedMetaData, dataSet.MetaData);
    //}


    //[Fact]
    //public void DataRowHandler()
    //{
    //    //Assign
    //    var dataSet = new DataSet()
    //    {
    //        MetaData = new MetaData
    //        {
    //            Variables = new List<Variable>
    //            {
    //                new Variable
    //                {
    //                    VariableName = "strings",
    //                    DataType = "string",
    //                    Scalar = false,
    //                    ScalarValue = null,
    //                    Attributes = new Dictionary<string, List<string>>
    //                    {
    //                        {
    //                            "attribute", new List<string>
    //                            {
    //                                "value1", "value2"
    //                            }
    //                        }
    //                    }
    //                },
    //                new Variable
    //                {
    //                    VariableName = "ints",
    //                    DataType = "int",
    //                    Scalar = false,
    //                    ScalarValue = null,
    //                    Attributes = new Dictionary<string, List<string>>
    //                    {
    //                        {
    //                            "attribute", new List<string>
    //                            {
    //                                "value1", "value2"
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    };

    //    var headers = new string[]
    //    {
    //        "strings", "ints"
    //    };

    //    var dataRow = new string[] 
    //    {
    //        "text", "1"
    //    };

    //    var expectedData = new List<DataValue[]>
    //    {
    //        new DataValue[]
    //        {
    //            new DataValueAs<string>
    //            {
    //                DataType = "string",
    //                Value = "text"
    //            },
    //            new DataValueAs<int>
    //            {
    //                DataType = "int",
    //                Value = 1
    //            }
    //        }
    //    };

    //    //Act
    //    dataSet.DataRowHandler(dataRow, headers, true);

    //    //Assert
    //    Assert.Equivalent(expectedData, dataSet.Data);
    //}
}

