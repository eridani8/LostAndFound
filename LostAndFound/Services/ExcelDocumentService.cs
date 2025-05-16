using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using LostAndFound.Models;

namespace LostAndFound.Services;

public static class ExcelDocumentService
{
    public static async Task<string> SaveReportToExcelAsync(
        string reportTitle,
        DateTime startDate,
        DateTime endDate,
        IEnumerable<object> reportItems,
        string filePath
    )
    {
        using (
            var spreadsheetDocument = SpreadsheetDocument.Create(
                filePath,
                SpreadsheetDocumentType.Workbook
            )
        )
        {
            var workbookPart = spreadsheetDocument.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());

            var sheet = new Sheet()
            {
                Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Отчет",
            };
            sheets.Append(sheet);

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            var headerRow = new Row() { RowIndex = 1 };
            headerRow.Append(
                CreateCell(
                    $"{reportTitle} ({startDate.ToShortDateString()} - {endDate.ToShortDateString()})",
                    CellValues.String
                )
            );
            sheetData.Append(headerRow);

            var columnHeaderRow = new Row() { RowIndex = 3 };

            var firstItem = reportItems.GetEnumerator();
            if (firstItem.MoveNext())
            {
                switch (firstItem.Current)
                {
                    case LostItem _:
                        columnHeaderRow.Append(
                            CreateCell("ID", CellValues.String),
                            CreateCell("Название", CellValues.String),
                            CreateCell("Категория", CellValues.String),
                            CreateCell("Дата находки", CellValues.String),
                            CreateCell("Место находки", CellValues.String),
                            CreateCell("Статус", CellValues.String)
                        );
                        break;
                    case ItemReturn _:
                        columnHeaderRow.Append(
                            CreateCell("ID", CellValues.String),
                            CreateCell("Предмет", CellValues.String),
                            CreateCell("Кому возвращен", CellValues.String),
                            CreateCell("Дата возврата", CellValues.String),
                            CreateCell("Контактная информация", CellValues.String)
                        );
                        break;
                }
            }
            sheetData.Append(columnHeaderRow);

            uint rowIndex = 4;
            foreach (var item in reportItems)
            {
                var dataRow = new Row() { RowIndex = rowIndex++ };

                switch (item)
                {
                    case LostItem lostItem:
                        dataRow.Append(
                            CreateCell(lostItem.ItemId.ToString(), CellValues.Number),
                            CreateCell(lostItem.ItemName, CellValues.String),
                            CreateCell(
                                lostItem.Category?.CategoryName ?? "Не указана",
                                CellValues.String
                            ),
                            CreateCell(lostItem.FoundDate.ToShortDateString(), CellValues.String),
                            CreateCell(lostItem.FoundLocation, CellValues.String),
                            CreateCell(lostItem.Status, CellValues.String)
                        );
                        break;
                    case ItemReturn itemReturn:
                        dataRow.Append(
                            CreateCell(itemReturn.ReturnId.ToString(), CellValues.Number),
                            CreateCell(
                                itemReturn.LostItem?.ItemName ?? "Неизвестно",
                                CellValues.String
                            ),
                            CreateCell(itemReturn.ReturnedTo, CellValues.String),
                            CreateCell(
                                itemReturn.ReturnDate.ToShortDateString(),
                                CellValues.String
                            ),
                            CreateCell(itemReturn.ContactInfo ?? "Не указана", CellValues.String)
                        );
                        break;
                }
                sheetData.Append(dataRow);
            }

            var footerRow = new Row() { RowIndex = rowIndex + 1 };
            footerRow.Append(
                CreateCell(
                    $"Отчет сгенерирован: {DateTime.Now:dd.MM.yyyy HH:mm:ss}",
                    CellValues.String
                )
            );
            sheetData.Append(footerRow);

            workbookPart.Workbook.Save();
        }

        return await Task.FromResult(filePath);
    }

    private static Cell CreateCell(string value, CellValues dataType)
    {
        return new Cell()
        {
            CellValue = new CellValue(value),
            DataType = new EnumValue<CellValues>(dataType),
        };
    }
}
