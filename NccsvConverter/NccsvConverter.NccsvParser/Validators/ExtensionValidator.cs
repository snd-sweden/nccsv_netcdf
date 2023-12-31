﻿using NccsvConverter.NccsvParser.Models;
using NccsvConverter.NccsvParser.Repositories;

namespace NccsvConverter.NccsvParser.Validators;

public class ExtensionValidator
{
    public static bool Validate(string filePath)
    {
        bool criticalResult = true;

        // Non-critical
        CheckNccsvExtension(filePath);

        return criticalResult;
    }


    // Returns true if file has .nccsv extension.
    private static bool CheckNccsvExtension(string filePath)
    {
        if (filePath.EndsWith(".nccsv"))
            return true;
        else
        {
            MessageRepository.Messages.Add(
                new Message("Couldn't find .nccsv extension.", 
                Severity.NonCritical));
            return false;
        }
    }
}