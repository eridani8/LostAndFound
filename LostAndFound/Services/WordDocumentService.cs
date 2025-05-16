using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using LostAndFound.Models;

namespace LostAndFound.Services;

public static class WordDocumentService
{
    public static Task<string> GenerateReturnReceiptAsync(ItemReturn itemReturn, string savePath)
    {
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        savePath = Path.Combine(
            savePath,
            $"Возврат_предмета_{itemReturn.ReturnId}_{DateTime.Now:yyyyMMdd_HHmmss}.docx"
        );

        using (
            var document = WordprocessingDocument.Create(
                savePath,
                WordprocessingDocumentType.Document
            )
        )
        {
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            var titlePara = body.AppendChild(new Paragraph());
            var titleRun = titlePara.AppendChild(new Run());
            titleRun.AppendChild(new Text("АКТ О ВОЗВРАТЕ НАЙДЕННОЙ ВЕЩИ"));

            titleRun.RunProperties = new RunProperties
            {
                Bold = new Bold(),
                FontSize = new FontSize { Val = "28" },
            };

            titlePara.ParagraphProperties = new ParagraphProperties
            {
                Justification = new Justification { Val = JustificationValues.Center },
                SpacingBetweenLines = new SpacingBetweenLines { After = "200" },
            };

            var datePara = body.AppendChild(new Paragraph());
            var dateRun = datePara.AppendChild(new Run());
            dateRun.AppendChild(new Text($"Дата: {DateTime.Now:dd.MM.yyyy}"));

            datePara.ParagraphProperties = new ParagraphProperties
            {
                Justification = new Justification { Val = JustificationValues.Right },
                SpacingBetweenLines = new SpacingBetweenLines { After = "400" },
            };

            AddParagraphWithText(body, $"Номер регистрации возврата: {itemReturn.ReturnId}");
            AddParagraphWithText(
                body,
                $"Наименование предмета: {itemReturn.LostItem?.ItemName ?? "Неизвестно"}"
            );
            AddParagraphWithText(
                body,
                $"Дата находки предмета: {itemReturn.LostItem?.FoundDate.ToString("dd.MM.yyyy") ?? "Неизвестно"}"
            );
            AddParagraphWithText(
                body,
                $"Место находки: {itemReturn.LostItem?.FoundLocation ?? "Неизвестно"}"
            );
            AddParagraphWithText(
                body,
                $"Категория предмета: {itemReturn.LostItem?.Category?.CategoryName ?? "Не указана"}"
            );
            AddParagraphWithText(body, $"Предмет выдан: {itemReturn.ReturnedTo}");
            AddParagraphWithText(body, $"Дата выдачи: {itemReturn.ReturnDate:dd.MM.yyyy HH:mm:ss}");
            AddParagraphWithText(
                body,
                $"Контактная информация получателя: {itemReturn.ContactInfo ?? "Не указана"}"
            );

            if (!string.IsNullOrEmpty(itemReturn.Notes))
            {
                AddParagraphWithText(body, $"Примечания: {itemReturn.Notes}");
            }

            body.AppendChild(new Paragraph());

            var signaturePara = body.AppendChild(new Paragraph());
            var signatureRun = signaturePara.AppendChild(new Run());
            signatureRun.AppendChild(new Text("Выдал: ______________ / ________________"));

            var receiverPara = body.AppendChild(new Paragraph());
            var receiverRun = receiverPara.AppendChild(new Run());
            receiverRun.AppendChild(new Text("Получил: ______________ / ________________"));

            receiverPara.ParagraphProperties = new ParagraphProperties
            {
                SpacingBetweenLines = new SpacingBetweenLines { After = "800" },
            };

            var operatorPara = body.AppendChild(new Paragraph());
            var operatorRun = operatorPara.AppendChild(new Run());
            operatorRun.AppendChild(
                new Text($"Оператор: {itemReturn.ReceivedByUser?.Login ?? "Неизвестно"}")
            );
        }

        return Task.FromResult(savePath);
    }

    public static async Task<string> SaveReportToWordAsync(
        string reportTitle,
        DateTime startDate,
        DateTime endDate,
        IEnumerable<object> reportItems,
        string filePath
    )
    {
        using (
            var document = WordprocessingDocument.Create(
                filePath,
                WordprocessingDocumentType.Document
            )
        )
        {
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            var titlePara = body.AppendChild(new Paragraph());
            var titleRun = titlePara.AppendChild(new Run());
            titleRun.AppendChild(new Text(reportTitle));

            titleRun.RunProperties = new RunProperties
            {
                Bold = new Bold(),
                FontSize = new FontSize { Val = "28" },
            };

            titlePara.ParagraphProperties = new ParagraphProperties
            {
                Justification = new Justification { Val = JustificationValues.Center },
                SpacingBetweenLines = new SpacingBetweenLines { After = "200" },
            };

            var datePara = body.AppendChild(new Paragraph());
            var dateRun = datePara.AppendChild(new Run());
            dateRun.AppendChild(
                new Text($"Период: {startDate.ToLongDateString()} - {endDate.ToLongDateString()}")
            );

            datePara.ParagraphProperties = new ParagraphProperties
            {
                Justification = new Justification { Val = JustificationValues.Center },
                SpacingBetweenLines = new SpacingBetweenLines { After = "400" },
            };

            body.AppendChild(new Paragraph());

            foreach (var item in reportItems)
            {
                switch (item)
                {
                    case LostItem lostItem:
                        AddParagraphWithText(
                            body,
                            $"ID: {lostItem.ItemId}, Название: {lostItem.ItemName}, "
                                + $"Категория: {lostItem.Category?.CategoryName ?? "Не указана"}, "
                                + $"Найдено: {lostItem.FoundDate:dd.MM.yyyy}, "
                                + $"Место: {lostItem.FoundLocation}, "
                                + $"Статус: {lostItem.Status}"
                        );
                        break;
                    case ItemReturn itemReturn:
                        AddParagraphWithText(
                            body,
                            $"ID: {itemReturn.ReturnId}, "
                                + $"Предмет: {itemReturn.LostItem?.ItemName ?? "Неизвестно"}, "
                                + $"Кому возвращен: {itemReturn.ReturnedTo}, "
                                + $"Дата возврата: {itemReturn.ReturnDate:dd.MM.yyyy}, "
                                + $"Контактная информация: {itemReturn.ContactInfo}"
                        );
                        break;
                }
            }

            var footerPara = body.AppendChild(new Paragraph());
            var footerRun = footerPara.AppendChild(new Run());
            footerRun.AppendChild(
                new Text($"Отчет сгенерирован: {DateTime.Now:dd.MM.yyyy HH:mm:ss}")
            );

            footerPara.ParagraphProperties = new ParagraphProperties
            {
                Justification = new Justification { Val = JustificationValues.Right },
                SpacingBetweenLines = new SpacingBetweenLines { Before = "400" },
            };
        }

        return await Task.FromResult(filePath);
    }

    private static void AddParagraphWithText(Body body, string text)
    {
        var para = body.AppendChild(new Paragraph());
        var run = para.AppendChild(new Run());
        run.AppendChild(new Text(text));

        para.ParagraphProperties = new ParagraphProperties
        {
            SpacingBetweenLines = new SpacingBetweenLines { After = "200" },
        };
    }
}
