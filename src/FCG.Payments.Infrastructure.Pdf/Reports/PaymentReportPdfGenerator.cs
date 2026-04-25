using FCG.Payments.Application.Abstractions.Reports;
using FCG.Payments.Application.UseCases.Payments.ExportPaymentReport;
using FCG.Payments.Application.UseCases.Payments.GetPaymentReport;
using FCG.Payments.Domain.Payments;
using FCG.Payments.Domain.Payments.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace FCG.Payments.Infrastructure.Pdf.Reports
{
    public sealed class PaymentReportPdfGenerator : IPaymentReportPdfGenerator
    {
        private static readonly CultureInfo Culture = new("pt-BR");

        public byte[] Generate(PaymentReportPdfData data)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(24);
                    page.DefaultTextStyle(text => text.FontSize(8));

                    page.Header().Element(header => ComposeHeader(header, data.GeneratedAt));
                    page.Content().Column(column =>
                    {
                        column.Spacing(10);
                        if (data.TotalMatched > data.Items.Count)
                            column.Item().Element(content => ComposeLimitWarning(content, data.Items.Count, data.TotalMatched));
                        column.Item().Element(content => ComposeFilters(content, data.Filter));
                        column.Item().Element(content => ComposeSummary(content, data.Summary));
                        column.Item().Element(content => ComposeTable(content, data.Items));
                    });
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Página ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        private static void ComposeHeader(IContainer container, DateTime generatedAt)
        {
            container.Column(column =>
            {
                column.Item().Text("Relatório de Pagamentos").FontSize(18).Bold();
                column.Item().Text($"Gerado em {generatedAt:dd/MM/yyyy HH:mm:ss} UTC")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken2);
            });
        }

        private static void ComposeLimitWarning(IContainer container, int displayed, int total)
        {
            container
                .Background(Colors.Orange.Lighten4)
                .Border(1)
                .BorderColor(Colors.Orange.Medium)
                .Padding(8)
                .Column(column =>
                {
                    column.Item()
                        .Text($"⚠ Este relatório exibe apenas os primeiros {displayed} registros. Foram encontrados {total} registros com os filtros aplicados. Utilize filtros mais específicos para visualizar todos os resultados.")
                        .FontColor(Colors.Orange.Darken4)
                        .SemiBold();
                });
        }

        private static void ComposeFilters(IContainer container, PaymentReportFilter filter)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(column =>
            {
                column.Spacing(3);
                column.Item().Text("Filtros Aplicados").SemiBold();
                column.Item().Text(
                    $"Status: {Format(filter.Status)} | De: {Format(filter.DateFrom)} | Até: {Format(filter.DateTo)} | ID do Usuário: {Format(filter.UserId)} | ID do Jogo: {Format(filter.GameId)}");
            });
        }

        private static void ComposeSummary(IContainer container, GetPaymentReportSummaryResponse summary)
        {
            container.Row(row =>
            {
                row.RelativeItem().Element(cell => SummaryCell(cell, "Total", summary.TotalPayments.ToString(Culture)));
                row.RelativeItem().Element(cell => SummaryCell(cell, "Aprovados", summary.TotalApproved.ToString(Culture)));
                row.RelativeItem().Element(cell => SummaryCell(cell, "Rejeitados", summary.TotalRejected.ToString(Culture)));
                row.RelativeItem().Element(cell => SummaryCell(cell, "Valor Aprovado", summary.ApprovedAmount.ToString("C", Culture)));
                row.RelativeItem().Element(cell => SummaryCell(cell, "Valor Rejeitado", summary.RejectedAmount.ToString("C", Culture)));
            });
        }

        private static void ComposeTable(IContainer container, IReadOnlyList<GetPaymentReportItemResponse> items)
        {
            if (items.Count == 0)
            {
                container.PaddingTop(8).Text("Nenhum registro encontrado para os filtros selecionados.");
                return;
            }

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.1f);
                    columns.RelativeColumn(1.8f);
                    columns.RelativeColumn(1.8f);
                    columns.RelativeColumn(1.8f);
                    columns.RelativeColumn(0.8f);
                    columns.RelativeColumn(0.8f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Processado Em");
                    header.Cell().Element(HeaderCell).Text("ID do Pagamento");
                    header.Cell().Element(HeaderCell).Text("ID do Usuário");
                    header.Cell().Element(HeaderCell).Text("ID do Jogo");
                    header.Cell().Element(HeaderCell).Text("Status");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Valor");
                });

                foreach (var item in items)
                {
                    var rowStyle = RowBackground(item.Status);
                    table.Cell().Element(rowStyle).Text(item.ProcessedAt.ToString("dd/MM/yyyy HH:mm:ss", Culture));
                    table.Cell().Element(rowStyle).Text(item.PaymentId.ToString());
                    table.Cell().Element(rowStyle).Text(item.UserId.ToString());
                    table.Cell().Element(rowStyle).Text(item.GameId.ToString());
                    table.Cell().Element(rowStyle).Text(TranslateStatus(item.Status));
                    table.Cell().Element(rowStyle).AlignRight().Text(item.Amount.ToString("C", Culture));
                }
            });
        }

        private static void SummaryCell(IContainer container, string label, string value)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(column =>
            {
                column.Item().Text(label).FontColor(Colors.Grey.Darken2);
                column.Item().Text(value).FontSize(11).SemiBold();
            });
        }

        private static IContainer HeaderCell(IContainer container)
        {
            return container
                .DefaultTextStyle(text => text.SemiBold())
                .Background(Colors.Grey.Lighten3)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten1)
                .Padding(4);
        }

        private static IContainer BodyCell(IContainer container)
        {
            return container
                .BorderBottom(0.5f)
                .BorderColor(Colors.Grey.Lighten3)
                .Padding(4);
        }

        private static Func<IContainer, IContainer> RowBackground(PaymentStatus status)
        {
            var bg = status switch
            {
                PaymentStatus.Approved => Colors.Green.Lighten4,
                PaymentStatus.Rejected => Colors.Red.Lighten4,
                _                      => Colors.White
            };

            return container => container
                .Background(bg)
                .BorderBottom(0.5f)
                .BorderColor(Colors.Grey.Lighten3)
                .Padding(4);
        }

        private static string Format(object? value)
        {
            return value switch
            {
                null => "Todos",
                DateTime dateTime => dateTime.ToString("dd/MM/yyyy HH:mm:ss", Culture),
                _ => value.ToString() ?? "Todos"
            };
        }

        private static string TranslateStatus(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Approved => "Aprovado",
                PaymentStatus.Rejected => "Rejeitado",
                PaymentStatus.Pending  => "Pendente",
                _ => status.ToString()
            };
        }
    }
}
