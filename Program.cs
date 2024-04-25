
NewCoder128 nc =  new NewCoder128();
Console.WriteLine(nc.stringToBarcode("123456789"));


public class NewCoder128
{
    public string stringToBarcode(string value)
    {
        // Parameters : a string
        // Return     : a string which give the bar code when it is dispayed with CODE128.TTF font
        //              : an empty string if the supplied parameter is no good
        int charPos, minCharPos;
        int currentChar, checksum;
        Boolean isTableB = true, isValid = true;
        string returnValue = "";

        if (value.Length > 0)
        {
            // Check for valid characters
            for (int charCount = 0; charCount < value.Length; charCount++)
            {
                //currentChar = char.GetNumericValue(value, charPos);
                //            (int)char.Parse(value.Substring(charCount, 1));
                Console.WriteLine("count " + charCount);
                currentChar = value[charCount];
                if (!(currentChar >= 32 && currentChar <= 126))
                {
                    isValid = false;
                    break;
                }
            }

            // Barcode is full of ascii characters, we can now process it
            if (isValid)
            {
                charPos = 0;
                while (charPos < value.Length)
                {
                    if (isTableB)
                    {
                        // See if interesting to switch to table C
                        // yes for 4 digits at start or end, else if 6 digits
                        if (charPos == 0 || charPos + 4 == value.Length)
                            minCharPos = 4;
                        else
                            minCharPos = 6;

                        BarcodeConverter128 brc128 = new BarcodeConverter128();

                        minCharPos = brc128.IsNumber(value, charPos, minCharPos);

                        if (minCharPos < 0)
                        {
                            // Choice table C
                            if (charPos == 0)
                            {
                                // Starting with table C
                                returnValue = ((char)205).ToString(); // char.ConvertFromUtf32(205);
                            }
                            else
                            {
                                // Switch to table C
                                returnValue = returnValue + ((char)199).ToString();
                            }
                            isTableB = false;
                        }
                        else
                        {
                            if (charPos == 0)
                            {
                                // Starting with table B
                                returnValue = ((char)204).ToString(); // char.ConvertFromUtf32(204);
                            }

                        }
                    }

                    if (!isTableB)
                    {
                        // We are on table C, try to process 2 digits
                        minCharPos = 2;
                        BarcodeConverter128 brcd = new BarcodeConverter128();
                        minCharPos = brcd.IsNumber(value, charPos, minCharPos);
                        if (minCharPos < 0) // OK for 2 digits, process it
                        {

                            currentChar = int.Parse(value.Substring(charPos, 2));
                            currentChar = currentChar < 95 ? currentChar + 32 : currentChar + 100;
                            returnValue = returnValue + ((char)currentChar).ToString();
                            charPos += 2;
                        }
                        else
                        {
                            // We haven't 2 digits, switch to table B
                            returnValue = returnValue + ((char)200).ToString();
                            isTableB = true;
                        }
                    }
                    if (isTableB)
                    {
                        // Process 1 digit with table B
                        returnValue = returnValue + value.Substring(charPos, 1);
                        charPos++;
                    }
                }

                // Calculation of the checksum
                checksum = 0;
                for (int loop = 0; loop < returnValue.Length; loop++)
                {
                    currentChar = returnValue[loop];
                    currentChar = currentChar < 127 ? currentChar - 32 : currentChar - 100;
                    if (loop == 0)
                        checksum = currentChar;
                    else
                        checksum = (checksum + (loop * currentChar)) % 103;
                }

                // Calculation of the checksum ASCII code
                checksum = checksum < 95 ? checksum + 32 : checksum + 100;
                // Add the checksum and the STOP
                returnValue = returnValue +
                        (char)checksum +
                        (char)206;
            }
        }

        return returnValue;
    }



}


class BarcodeConverter128
{
    /// <summary>
    /// Converts an input string to the equivilant string, that need to be produced using the 'Code 128' font.
    /// </summary>
    /// <param name="value">string to be encoded</param>
    /// <returns>Encoded string start/stop and checksum characters included</returns>
    public BarcodeConverter128() { }

    public int IsNumber(string InputValue, int CharPos, int MinCharPos)
    {
        // if the MinCharPos characters from CharPos are numeric, then MinCharPos = -1
        MinCharPos--;
        if (CharPos + MinCharPos < InputValue.Length)
        {
            while (MinCharPos >= 0)
            {
                if ((int)(InputValue[CharPos + MinCharPos]) < 48
                    || (int)(InputValue[CharPos + MinCharPos]) > 57)
                {
                    break;
                }
                MinCharPos--;
            }
        }
        return MinCharPos;
    }


}