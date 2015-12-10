using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;


namespace Spen_metrix
{
    class Program
    {
        public const int MAX_COUNT_VARS = 30;
        public static string[] arrayOfTypes = { "int", "char", "boolean", "float", "double", "String" };
        public const int numberOfStandartTypes = 6;
        public struct variableOfGlobalNames
        {
            public string nameOfVariable;
            public string typeOfVariable;
            public int countOfTreatment;
        }

        public struct structForGlobalArrayAndCount
        {
            public variableOfGlobalNames[] arrayOfGlobalVars;
            public int countOfGlobalVars;
        }

        public const string emptyString = "";
        public const char doubleQuotes = '"';
        public const char backSlash = '\\';
        public const char openCurvedBracket = '{';
        public const char closeCurvedBracket = '}';

        public const String multiLineCommentRegEx = @"\/\*[\s\S]*?\*\/";
        public const String singleLineCommentRegEx = @"\/\/[^\n\r]*";
        public const String compilierInstructions = @"#.*";

        public const String identifierRegEx = @"\b([a-zA-Z_][a-zA-Z0-9_]*)\b";
        public const String pointerRegEx = @"\s*[\*|\&]*\s*";
        public const String functionParamsRegEx = @"\(.*\)";
        public const String typeOfVariableRegEx = @"((int)|(char)|(String)|(boolean)|(byte)|(float)|(double))\s*";
        public const String variableRegex = typeOfVariableRegEx + identifierRegEx;
        public const String signatureFunctionRegEx = identifierRegEx + pointerRegEx + identifierRegEx +  functionParamsRegEx + @"\s*({)";



        public static String deleteStringLiterals(String codeString)  //delete all string literals
        {
            StringBuilder additionalStringForCode = new StringBuilder(emptyString, codeString.Length);

            int startDoubleQuotes = 0, endDoubleQuotes = 0;
            int counterForAddStr = 0;
            int i = 0;
            Boolean checkDeletingLiterals = true;
            for (i = 0; i < codeString.Length; i++)
            {
                if (codeString[i] == doubleQuotes)
                {
                    if (codeString[i - 1] != backSlash)
                    {
                        if (startDoubleQuotes == 0)
                            startDoubleQuotes = i;
                        else
                        {
                            endDoubleQuotes = i;
                            additionalStringForCode.Append(codeString.Substring(i - counterForAddStr, counterForAddStr - (endDoubleQuotes - startDoubleQuotes)));
                            checkDeletingLiterals = false;
                            counterForAddStr = -1;
                            startDoubleQuotes = endDoubleQuotes = 0;
                        }
                    }
                }
                counterForAddStr++;
            }
            if (checkDeletingLiterals == false)
            {
                additionalStringForCode.Append(codeString.Substring(i - counterForAddStr, counterForAddStr));
                codeString = additionalStringForCode.ToString();
            }
            return codeString;
        }
        public static String deleteComments(String codeString, String pattern) //delete all comments 
        {

            Regex regularExpression = new Regex(pattern);
            string replacement = emptyString;
            codeString = regularExpression.Replace(codeString, replacement);

            return codeString;
        }


        public static string readCodeFromFile()    // read code from file and write to string
        {
            string stringWithCode;
            string addressOfCode = "C:\\Users\\Alex\\Downloads\\Persona.java";

            StreamReader reader = new StreamReader(addressOfCode);
            stringWithCode = reader.ReadToEnd();
            return stringWithCode;
        }

        public static bool isStandartType(string stringOfType)
        {
            bool isType = false;
            int counterForCycle = 0;

            while ((counterForCycle < numberOfStandartTypes) && (!(isType)))
            {
                if ((string.Compare(arrayOfTypes[counterForCycle], stringOfType, true)) == 0)
                {
                    isType = true;
                }
                counterForCycle++;
            }
            return isType;
        }
        public static structForGlobalArrayAndCount getCountAndArrayOfGlobalVars(String codeString, variableOfGlobalNames[] arrayOfGlobalVars, String pattern) // get number of global params
        {
            int countOfGlobalVars = 0;
            int additionalCounter = 0;
            int countOfCurvedBrackets = 0;
            int index;
            String signatureFunction;
            structForGlobalArrayAndCount structForReturning = new structForGlobalArrayAndCount();
            StringBuilder additionalCodeString = new StringBuilder(emptyString,codeString.Length);
            Regex regularExpression = new Regex(pattern);
            Match match = regularExpression.Match(codeString);
            
            while (match.Success)               // delete all functions in our code
            {              
                signatureFunction = match.Groups[0].Value;
                index = codeString.IndexOf(signatureFunction);
                additionalCodeString.Append(codeString.Substring(additionalCounter, index - additionalCounter));

                while (codeString[index] != openCurvedBracket)
                    index++;

                while (countOfCurvedBrackets >= 0)
                {
                    if (codeString[index] == openCurvedBracket)
                        countOfCurvedBrackets++;
                    else if (codeString[index] == closeCurvedBracket)
                        countOfCurvedBrackets--;
                    if (countOfCurvedBrackets == 0)
                        countOfCurvedBrackets = -1;
                    index++;
                }
                    additionalCounter = index;
                    countOfCurvedBrackets = 0;
                match = match.NextMatch();
            }

            additionalCodeString.Append(codeString.Substring(additionalCounter, codeString.Length - additionalCounter));
            codeString = additionalCodeString.ToString();
            // create new RegEx fo finding all variables in codeString(all code after deliting comments, strings, functions)
            
            pattern = identifierRegEx + pointerRegEx + identifierRegEx;
            regularExpression = new Regex(pattern);
            match = regularExpression.Match(codeString);

            index = 0;
            while (match.Success)
            {
                if (isStandartType(match.Groups[1].Value))
                {
                    arrayOfGlobalVars[index].typeOfVariable = match.Groups[1].Value;
                    arrayOfGlobalVars[index].nameOfVariable = match.Groups[2].Value;             
                    index++;
                    countOfGlobalVars++;
                }
                match = match.NextMatch();
            }
            structForReturning.countOfGlobalVars = countOfGlobalVars;
            structForReturning.arrayOfGlobalVars = arrayOfGlobalVars; 
            return structForReturning;
        }

        public static void countSpenNumber(String codeString, structForGlobalArrayAndCount structArrayOfGlobalVars)
        {
            String signatureFunction;
            String methodString;
            
            int countOfCurvedBrackets = 0;
            int index;
            int additionalCounter = 0;
            StringBuilder additionalCodeString = new StringBuilder(emptyString, codeString.Length); 
            Regex regularExpressionSignature = new Regex(signatureFunctionRegEx);
            Match matchSignature = regularExpressionSignature.Match(codeString);

            while (matchSignature.Success)               // find method
            {
                String[] arrayOfLocalVars = new String[MAX_COUNT_VARS];
                int numberOfLocalVarsInMethod = 0;

                signatureFunction = matchSignature.Groups[0].Value;
                index = codeString.IndexOf(signatureFunction);
                additionalCounter = index;

                while (codeString[additionalCounter] != openCurvedBracket)
                    additionalCounter++;
                while (countOfCurvedBrackets >= 0)
                {
                    if (codeString[additionalCounter] == openCurvedBracket)
                        countOfCurvedBrackets++;
                    else if (codeString[additionalCounter] == closeCurvedBracket)
                        countOfCurvedBrackets--;
                    if (countOfCurvedBrackets == 0)
                        countOfCurvedBrackets = -1;
                    additionalCounter++;
                }
                additionalCodeString.Append(codeString.Substring(index, additionalCounter - index));
                methodString = additionalCodeString.ToString(); // method in methodString
                Console.WriteLine(signatureFunction.Substring(0,signatureFunction.Length - 1));

                Regex regularExpressionVariable = new Regex(variableRegex);
                Match matchVariable = regularExpressionVariable.Match(methodString); //find all local variables in methodString

                additionalCodeString.Replace(additionalCodeString.ToString(), emptyString);
                additionalCodeString.Append(methodString);

                while (matchVariable.Success)
                {
                    //int indexOfVariable;
                    additionalCounter = 0;
                    
                    String parseString;
                    StringBuilder variableInMethod = new StringBuilder(emptyString, methodString.Length);


                    index = additionalCodeString.ToString().IndexOf( matchVariable.Groups[1].Value );
                    additionalCodeString.Replace(additionalCodeString.ToString(), additionalCodeString.ToString().Substring(index + 1));
                    parseString = additionalCodeString.ToString();
                    index = 0;

                    while ((parseString[index] != ' ') && (parseString[index] != '\t'))
                        index++;
                    while ((parseString[index] == ' ') || (parseString[index] == '\t'))
                        index++;

                    additionalCounter = index;

                    while ((parseString[index] != '=') && (parseString[index] != ';') && (parseString[index] != ',') && (parseString[index] != ')') &&
                           (parseString[index] != '(') && (parseString[index] != ' ') && (parseString[index] != '\t'))
                        index++;

                    variableInMethod.Append(parseString.Substring(additionalCounter, index - additionalCounter));//необходимо проверить, переменная это или объявление метода

                    while ((parseString[index] != '=') && (parseString[index] == ';') && (parseString[index] != ',') && (parseString[index] != ')') &&
                           (parseString[index] != '('))
                        index++;
                    if (parseString[index] != '(')
                    {
                        arrayOfLocalVars[numberOfLocalVarsInMethod] = variableInMethod.ToString();
                        numberOfLocalVarsInMethod++;
                    }

                        matchVariable = matchVariable.NextMatch();
                }


                for (int i = 0; i < numberOfLocalVarsInMethod; i++)
                {
                    int SpenCount = -1;
                    StringBuilder additionalMethodString = new StringBuilder(emptyString, methodString.Length);
                    String stringForCountLocalVars = methodString;

                    index = methodString.IndexOf(arrayOfLocalVars[i]);
                    while (index != -1)
                    {
                        SpenCount++;
                        additionalMethodString.Append(stringForCountLocalVars.Substring(index + 1));
                        stringForCountLocalVars = additionalMethodString.ToString();
                        additionalMethodString.Replace(stringForCountLocalVars, emptyString);
                        index = stringForCountLocalVars.IndexOf(arrayOfLocalVars[i]);
                    }
                    Console.Write(arrayOfLocalVars[i] + " ");
                    Console.WriteLine(SpenCount);
                   
                }
                for (int i = 0; i < structArrayOfGlobalVars.countOfGlobalVars; i++ )
                {
                    int counterForSpenNumber = 0;
                    Boolean isGlobal = true;
                    for (int j = 0; j < numberOfLocalVarsInMethod; j++)
                        if (structArrayOfGlobalVars.arrayOfGlobalVars[i].nameOfVariable == arrayOfLocalVars[j])
                            isGlobal = false;   
                    if (isGlobal)
                    {                     
                            index = methodString.IndexOf(structArrayOfGlobalVars.arrayOfGlobalVars[i].nameOfVariable);
                            String stringForCountGlobalSpen = methodString;
                            while (index != -1)
                            {
                                counterForSpenNumber++;
                                stringForCountGlobalSpen = stringForCountGlobalSpen.Substring(index + 1);
                                index = stringForCountGlobalSpen.IndexOf(structArrayOfGlobalVars.arrayOfGlobalVars[i].nameOfVariable);
                            }
                            structArrayOfGlobalVars.arrayOfGlobalVars[i].countOfTreatment += counterForSpenNumber;                    
                    }
                }
                    countOfCurvedBrackets = 0;
                additionalCodeString.Replace(additionalCodeString.ToString(), emptyString);
                methodString = additionalCodeString.ToString();
                matchSignature = matchSignature.NextMatch();
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            string codeString;
            structForGlobalArrayAndCount structArrayOfGlobalVars = new structForGlobalArrayAndCount();

            variableOfGlobalNames[] arrayOfGlobalVars = new variableOfGlobalNames[MAX_COUNT_VARS];

            codeString = readCodeFromFile();
            codeString = deleteStringLiterals(codeString);
            codeString = deleteComments(codeString, multiLineCommentRegEx);
            codeString = deleteComments(codeString, singleLineCommentRegEx);
            structArrayOfGlobalVars = getCountAndArrayOfGlobalVars(codeString, arrayOfGlobalVars, signatureFunctionRegEx);
            countSpenNumber(codeString, structArrayOfGlobalVars);
            for (int i = 0; i < structArrayOfGlobalVars.countOfGlobalVars;i++ )
            {
                Console.Write(structArrayOfGlobalVars.arrayOfGlobalVars[i].nameOfVariable + " ");
                Console.WriteLine(structArrayOfGlobalVars.arrayOfGlobalVars[i].countOfTreatment);
            }
                Console.ReadLine();
        }
    }
}
