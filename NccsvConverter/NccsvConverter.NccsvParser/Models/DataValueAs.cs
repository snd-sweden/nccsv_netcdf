﻿namespace NccsvConverter.NccsvParser.Models;

public class DataValueAs<T> : DataValue
{
    public T? Value { get; internal protected set; }
}
