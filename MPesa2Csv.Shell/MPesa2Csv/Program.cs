﻿using CsvHelper;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using MPesa2Csv;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public class Program
{

    public static void Main(string[] args)
    {

        var source = args[0];
        var password = args[1];
        var outputFile = args[2];
        var readerProperties = new ReaderProperties().SetPassword(Encoding.UTF8.GetBytes(password));
        var pdfReader = new PdfReader(source, readerProperties);
        pdfReader.SetUnethicalReading(true);

        var pdfDoc = new PdfDocument(pdfReader);

        var numberOfPages = pdfDoc.GetNumberOfPages();
        var sb = new StringBuilder();
        for (int i = 1; i <= numberOfPages; i++)
        {
            var page = pdfDoc.GetPage(i);
            var pageText = PdfTextExtractor.GetTextFromPage(page);
            sb.Append(pageText);
        }

        var regexesToStrip = new List<Regex>()
        {   
            new Regex("^Page [0-9]+ of [0-9]+$",RegexOptions.Multiline),
            new Regex("^Twitter.*$",RegexOptions.Multiline),
            new Regex("^Status$",RegexOptions.Multiline),
            new Regex("^Disclaimer:.*$",RegexOptions.Multiline),
            new Regex("^Receipt No.*$",RegexOptions.Multiline),
            new Regex("^further guidance.*$",RegexOptions.Multiline),
            new Regex(@"^\s.*$",RegexOptions.Multiline)
        };


        var allText = sb.ToString();
        var allLines =
            Regex.Split(allText, "\r\n|\r|\n")
            .SkipWhile(l => !l.StartsWith("Status"))
            .ToList();

        foreach (var regex in regexesToStrip)
        {
            for (int i = 0; i < allLines.Count; i++)
            {
                allLines[i] = regex.Replace(allLines[i], String.Empty);
            }
        }

        allLines.RemoveAll(l => l == String.Empty);

        var joined = String.Join(Environment.NewLine, allLines);

        var groupedLines = GroupToRecordLines(allLines);

        var records = groupedLines
        .Select(l => ConvertLineSetToRecord(l.ToList()))
        .OrderByDescending(r => r.CompletionTime);


        using (var writer = new StreamWriter(outputFile))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteHeader<RowRecord>();
            csv.NextRecord();
            csv.WriteRecords(records);
        }

        bool IsTokenReciptNumber(string line)
        {
            var firstToken = String.Concat(line.TakeWhile(c => c != ' '));

            var matches = firstToken.Length == 10 && Regex.IsMatch(firstToken, "[0-9]") && Regex.IsMatch(firstToken, "[A-Z]");
            return matches;
        }

        IEnumerable<IEnumerable<string>> GroupToRecordLines(IEnumerable<string> lines)
        {
            IEnumerable<IEnumerable<string>> recurse(IEnumerable<string> lines, List<List<string>> acc)
            {
                if (!lines.Any())
                {
                    return acc;
                }

                else
                {
                    var group = new List<string>();
                    var head = lines.Take(1).First();
                    var tail = lines.Skip(1);
                    if (IsTokenReciptNumber(head))
                    {
                        group.Add(head);
                    }
                    var tailUntilNewRecord = tail.TakeWhile(l => !IsTokenReciptNumber(l));
                    if (tailUntilNewRecord.Any())
                    {

                        group.AddRange(tailUntilNewRecord);
                        tail = tail.Skip(tailUntilNewRecord.Count());

                    }
                    acc.Add(group);
                    return recurse(tail, acc);
                }

            }
            return recurse(lines, new List<List<string>>());
        }


        RowRecord ConvertLineSetToRecord(List<string> lines)
        {
            decimal ParseToDecimal(string v)
            {
                return v == String.Empty ? 0 : Decimal.Parse(v);
            }

            var line = lines.First();

            var splitBySpace = line.Split(' ');
            var length = splitBySpace.Length;
            var receiptNumber = splitBySpace[0];
            var date = splitBySpace[1];
            var time = splitBySpace[2];
            var balance = ParseToDecimal(splitBySpace[length - 1]);
            var paidInOrWithdrawn = ParseToDecimal(splitBySpace[length - 2]);
            var transactionType = splitBySpace[length - 3];
            var descriptionTokens = new ArraySegment<string>(splitBySpace, 3, length - 6).ToList();
            var description = String.Join(' ', descriptionTokens);
            if (lines.Count() >= 1)
            {
                var restOfDescr = String.Join(" ", lines.Skip(1));
                description += " " + restOfDescr;
            }


            var isWithdrawl = paidInOrWithdrawn < 0;

            return new RowRecord()
            {
                ReciptNo = receiptNumber,
                CompletionTime = DateTime.Parse(date + " " + time),
                Details = description,
                PaidIn = isWithdrawl ? 0 : paidInOrWithdrawn,
                WithDrawn = isWithdrawl ? paidInOrWithdrawn : 0,
                Balance = balance,
                TransactionStatus = transactionType
            };
        }
    }
}

