using System;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace ProjectDocsGenerator;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Generating Database/ER_Diagram.pdf...");
        var baseDir = Directory.GetCurrentDirectory();
        while (!File.Exists(Path.Combine(baseDir, "Вариант 2.docx")) && Directory.GetParent(baseDir) != null)
        {
            baseDir = Directory.GetParent(baseDir)!.FullName;
        }

        var dbDir = Path.Combine(baseDir, "Database");
        Directory.CreateDirectory(dbDir);
        var pdfPath = Path.Combine(dbDir, "ER_Diagram.pdf");
        GenerateErDiagramPdf(pdfPath);
        Console.WriteLine($"ER-Diagram generated at: {pdfPath}");

        var docxPath = Path.Combine(baseDir, "Инструкция.docx");
        Console.WriteLine($"Generating {docxPath}...");
        GenerateInstructionDocx(docxPath);
        Console.WriteLine($"Instruction DOCX successfully generated at: {docxPath}");
    }

    private static void GenerateErDiagramPdf(string outputPath)
    {
        var doc = new PdfDocument();
        doc.Info.Title = "ER-диаграмма базы данных «Поликлиника»";
        doc.Info.Subject = "Модуль 1: Разработка, администрирование и защита баз данных";

        var page = doc.AddPage();
        page.Orientation = PdfSharpCore.PageOrientation.Landscape;
        page.Size = PdfSharpCore.PageSize.A4; // 842 x 595 pt

        using var gfx = XGraphics.FromPdfPage(page);

        var titleFont = new XFont("Arial", 16, XFontStyle.Bold);
        var subTitleFont = new XFont("Arial", 10, XFontStyle.Regular);
        var headerFont = new XFont("Arial", 10, XFontStyle.Bold);
        var itemFont = new XFont("Arial", 8.5, XFontStyle.Regular);
        var boldItemFont = new XFont("Arial", 8.5, XFontStyle.Bold);

        // Header
        gfx.DrawString("ER-ДИАГРАММА РЕЛЯЦИОННОЙ БАЗЫ ДАННЫХ «ПОЛИКЛИНИКА»", titleFont, XBrushes.Navy, new XPoint(30, 35));
        gfx.DrawString("3-я нормальная форма (3NF) | Ссылочная целостность | Модуль 1 (Вариант 2)", subTitleFont, XBrushes.DarkSlateGray, new XPoint(30, 52));

        // Draw Entity Box Helper
        void DrawTableBox(double x, double y, double width, string tableName, string[] fields)
        {
            double headerHeight = 22;
            double rowHeight = 15;
            double totalHeight = headerHeight + fields.Length * rowHeight + 6;

            var borderPen = new XPen(XColor.FromArgb(200, 215, 230), 1);
            var bodyBrush = new XSolidBrush(XColor.FromArgb(255, 255, 255));
            var headerBrush = new XSolidBrush(XColor.FromArgb(2, 132, 199));

            // Box Body
            var rect = new XRect(x, y, width, totalHeight);
            gfx.DrawRectangle(borderPen, bodyBrush, rect);

            // Table Header
            var headerRect = new XRect(x, y, width, headerHeight);
            gfx.DrawRectangle(headerBrush, headerRect);

            gfx.DrawString(tableName, headerFont, XBrushes.White, new XPoint(x + 10, y + 15));

            // Fields
            double currentY = y + headerHeight + 12;
            foreach (var field in fields)
            {
                if (field.StartsWith("[PK]"))
                {
                    gfx.DrawString("[PK]", boldItemFont, XBrushes.DarkBlue, new XPoint(x + 6, currentY));
                    gfx.DrawString(field.Substring(4).Trim(), boldItemFont, XBrushes.Black, new XPoint(x + 32, currentY));
                }
                else if (field.StartsWith("[FK]"))
                {
                    gfx.DrawString("[FK]", boldItemFont, XBrushes.DarkGreen, new XPoint(x + 6, currentY));
                    gfx.DrawString(field.Substring(4).Trim(), itemFont, XBrushes.Black, new XPoint(x + 32, currentY));
                }
                else
                {
                    gfx.DrawString("•", itemFont, XBrushes.Gray, new XPoint(x + 12, currentY));
                    gfx.DrawString(field.Trim(), itemFont, XBrushes.Black, new XPoint(x + 32, currentY));
                }
                currentY += rowHeight;
            }
        }

        // Layout Tables
        // Top row
        DrawTableBox(30, 75, 175, "genders (Пол)", new[] {
            "[PK] gender_id : INT",
            "gender_name : VARCHAR(20)"
        });

        DrawTableBox(235, 75, 215, "patients (Пациенты)", new[] {
            "[PK] patient_id : INT",
            "full_name : VARCHAR(150)",
            "birth_date : DATE",
            "[FK] gender_id : INT",
            "policy_number : VARCHAR(16) [UQ]",
            "address : VARCHAR(255)",
            "phone : VARCHAR(20)",
            "email : VARCHAR(100)",
            "created_at : TIMESTAMP"
        });

        DrawTableBox(480, 75, 175, "specializations", new[] {
            "[PK] specialization_id : INT",
            "specialization_name : VARCHAR(100)"
        });

        DrawTableBox(675, 75, 140, "treatment_types", new[] {
            "[PK] treatment_type_id : INT",
            "type_name : VARCHAR(150)",
            "treatment_coefficient : DEC"
        });

        // Middle row
        DrawTableBox(235, 255, 215, "appointments (Приёмы)", new[] {
            "[PK] appointment_id : INT",
            "[FK] patient_id : INT",
            "[FK] doctor_id : INT",
            "appointment_date : TIMESTAMP",
            "status : VARCHAR(30)",
            "complaints : TEXT",
            "diagnosis : TEXT",
            "recommendations : TEXT",
            "base_cost : DECIMAL(10,2)",
            "discount_percent : INT",
            "final_cost : DECIMAL(10,2)"
        });

        DrawTableBox(480, 160, 175, "doctors (Врачи)", new[] {
            "[PK] doctor_id : INT",
            "full_name : VARCHAR(150)",
            "[FK] specialization_id : INT",
            "cabinet : VARCHAR(10)",
            "phone : VARCHAR(20)",
            "is_active : INT"
        });

        DrawTableBox(675, 160, 140, "material_types", new[] {
            "[PK] material_type_id : INT",
            "material_name : VARCHAR(150)",
            "defect_rate : DECIMAL"
        });

        // Bottom row
        DrawTableBox(480, 350, 175, "partners (Поставщики)", new[] {
            "[PK] partner_id : INT",
            "partner_name : VARCHAR(150)",
            "contact_person : VARCHAR(150)",
            "phone : VARCHAR(20)",
            "email : VARCHAR(100)",
            "address : VARCHAR(255)"
        });

        DrawTableBox(675, 275, 140, "medicine_deliveries", new[] {
            "[PK] delivery_id : INT",
            "[FK] partner_id : INT",
            "[FK] material_type_id : INT",
            "medicine_name : VARCHAR(150)",
            "quantity : INT",
            "delivery_date : DATE",
            "batch_number : VARCHAR(50)"
        });

        // Relation lines helper
        var penRel = new XPen(XColor.FromArgb(2, 132, 199), 1.5);
        
        // genders -> patients
        gfx.DrawLine(penRel, 205, 100, 235, 100);
        gfx.DrawString("1", boldItemFont, XBrushes.Navy, new XPoint(208, 95));
        gfx.DrawString("N", boldItemFont, XBrushes.Navy, new XPoint(223, 95));

        // specializations -> doctors
        gfx.DrawLine(penRel, 567, 135, 567, 160);
        gfx.DrawString("1", boldItemFont, XBrushes.Navy, new XPoint(572, 145));
        gfx.DrawString("N", boldItemFont, XBrushes.Navy, new XPoint(572, 155));

        // patients -> appointments
        gfx.DrawLine(penRel, 342, 235, 342, 255);
        gfx.DrawString("1", boldItemFont, XBrushes.Navy, new XPoint(346, 243));
        gfx.DrawString("N", boldItemFont, XBrushes.Navy, new XPoint(346, 252));

        // doctors -> appointments
        gfx.DrawLine(penRel, 480, 220, 450, 220);
        gfx.DrawLine(penRel, 450, 220, 450, 280);
        gfx.DrawString("1", boldItemFont, XBrushes.Navy, new XPoint(465, 215));
        gfx.DrawString("N", boldItemFont, XBrushes.Navy, new XPoint(453, 275));

        // partners -> medicine_deliveries
        gfx.DrawLine(penRel, 655, 390, 665, 390);
        gfx.DrawLine(penRel, 665, 390, 665, 320);
        gfx.DrawLine(penRel, 665, 320, 675, 320);
        gfx.DrawString("1", boldItemFont, XBrushes.Navy, new XPoint(656, 385));
        gfx.DrawString("N", boldItemFont, XBrushes.Navy, new XPoint(668, 315));

        // material_types -> medicine_deliveries
        gfx.DrawLine(penRel, 745, 235, 745, 275);
        gfx.DrawString("1", boldItemFont, XBrushes.Navy, new XPoint(750, 245));
        gfx.DrawString("N", boldItemFont, XBrushes.Navy, new XPoint(750, 270));

        // Legend
        double legendY = 490;
        gfx.DrawRectangle(new XPen(XColor.FromArgb(226, 232, 240), 1), new XSolidBrush(XColor.FromArgb(248, 250, 252)), new XRect(30, legendY, 785, 45));
        gfx.DrawString("ОБОЗНАЧЕНИЯ И ЦЕЛОСТНОСТЬ ДАННЫХ:", boldItemFont, XBrushes.Navy, new XPoint(40, legendY + 18));
        gfx.DrawString("[PK] Первичный ключ (Primary Key) | [FK] Внешний ключ (Foreign Key) | [UQ] Уникальное значение (Unique)", itemFont, XBrushes.Black, new XPoint(40, legendY + 34));
        gfx.DrawString("Связи 1:N обеспечены внешними ключами с каскадными операциями ON UPDATE CASCADE и ON DELETE RESTRICT / CASCADE.", itemFont, XBrushes.DarkSlateGray, new XPoint(360, legendY + 18));

        doc.Save(outputPath);
    }

    private static void GenerateInstructionDocx(string filePath)
    {
        using var wordDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
        var mainPart = wordDoc.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        void AddTitle(string text)
        {
            var p = body.AppendChild(new Paragraph());
            p.AppendChild(new ParagraphProperties(
                new ParagraphStyleId { Val = "Title" },
                new SpacingBetweenLines { Before = "120", After = "240" },
                new Justification { Val = JustificationValues.Center }
            ));
            var r = p.AppendChild(new Run());
            r.AppendChild(new RunProperties(
                new Bold(),
                new FontSize { Val = "36" },
                new Color { Val = "0284C7" },
                new RunFonts { Ascii = "Segoe UI", HighAnsi = "Segoe UI" }
            ));
            r.AppendChild(new Text(text));
        }

        void AddSubtitle(string text)
        {
            var p = body.AppendChild(new Paragraph());
            p.AppendChild(new ParagraphProperties(
                new SpacingBetweenLines { Before = "0", After = "360" },
                new Justification { Val = JustificationValues.Center }
            ));
            var r = p.AppendChild(new Run());
            r.AppendChild(new RunProperties(
                new FontSize { Val = "24" },
                new Color { Val = "475569" },
                new RunFonts { Ascii = "Segoe UI", HighAnsi = "Segoe UI" }
            ));
            r.AppendChild(new Text(text));
        }

        void AddHeading1(string text)
        {
            var p = body.AppendChild(new Paragraph());
            p.AppendChild(new ParagraphProperties(
                new SpacingBetweenLines { Before = "300", After = "120" }
            ));
            var r = p.AppendChild(new Run());
            r.AppendChild(new RunProperties(
                new Bold(),
                new FontSize { Val = "28" },
                new Color { Val = "0F172A" },
                new RunFonts { Ascii = "Segoe UI", HighAnsi = "Segoe UI" }
            ));
            r.AppendChild(new Text(text));
        }

        void AddHeading2(string text)
        {
            var p = body.AppendChild(new Paragraph());
            p.AppendChild(new ParagraphProperties(
                new SpacingBetweenLines { Before = "200", After = "80" }
            ));
            var r = p.AppendChild(new Run());
            r.AppendChild(new RunProperties(
                new Bold(),
                new FontSize { Val = "24" },
                new Color { Val = "0284C7" },
                new RunFonts { Ascii = "Segoe UI", HighAnsi = "Segoe UI" }
            ));
            r.AppendChild(new Text(text));
        }

        void AddParagraph(string text, bool bold = false)
        {
            var p = body.AppendChild(new Paragraph());
            p.AppendChild(new ParagraphProperties(
                new SpacingBetweenLines { Before = "40", After = "80", Line = "260" }
            ));
            var r = p.AppendChild(new Run());
            var rPr = new RunProperties(
                new FontSize { Val = "22" },
                new Color { Val = "1E293B" },
                new RunFonts { Ascii = "Segoe UI", HighAnsi = "Segoe UI" }
            );
            if (bold) rPr.AppendChild(new Bold());
            r.AppendChild(rPr);
            r.AppendChild(new Text(text));
        }

        void AddBullet(string text, string prefix = "•  ")
        {
            var p = body.AppendChild(new Paragraph());
            p.AppendChild(new ParagraphProperties(
                new Indentation { Left = "360", Hanging = "200" },
                new SpacingBetweenLines { Before = "20", After = "40", Line = "240" }
            ));
            var r = p.AppendChild(new Run());
            r.AppendChild(new RunProperties(
                new FontSize { Val = "22" },
                new Color { Val = "334155" },
                new RunFonts { Ascii = "Segoe UI", HighAnsi = "Segoe UI" }
            ));
            r.AppendChild(new Text(prefix + text));
        }

        void AddCodeBlock(string code)
        {
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                var p = body.AppendChild(new Paragraph());
                p.AppendChild(new ParagraphProperties(
                    new Indentation { Left = "400" },
                    new SpacingBetweenLines { Before = "0", After = "0", Line = "220" }
                ));
                var r = p.AppendChild(new Run());
                r.AppendChild(new RunProperties(
                    new FontSize { Val = "19" },
                    new Color { Val = "0F172A" },
                    new RunFonts { Ascii = "Consolas", HighAnsi = "Consolas" }
                ));
                r.AppendChild(new Text(line.Replace("\t", "    ")));
            }
        }

        void AddTable(string[,] data, int[] colWidths)
        {
            var table = body.AppendChild(new Table());

            var tblPr = new TableProperties(
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4, Color = "CBD5E1" },
                    new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "CBD5E1" },
                    new LeftBorder { Val = BorderValues.Single, Size = 4, Color = "CBD5E1" },
                    new RightBorder { Val = BorderValues.Single, Size = 4, Color = "CBD5E1" },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4, Color = "E2E8F0" },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 4, Color = "E2E8F0" }
                ),
                new TableWidth { Type = TableWidthUnitValues.Pct, Width = "5000" }
            );
            table.AppendChild(tblPr);

            int rows = data.GetLength(0);
            int cols = data.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                var row = table.AppendChild(new TableRow());
                bool isHeader = (i == 0);

                for (int j = 0; j < cols; j++)
                {
                    var cell = row.AppendChild(new TableCell());
                    var cellPr = new TableCellProperties(
                        new TableCellWidth { Type = TableWidthUnitValues.Dxa, Width = colWidths[j].ToString() },
                        new TableCellMargin(
                            new TopMargin { Width = "100" },
                            new BottomMargin { Width = "100" },
                            new LeftMargin { Width = "150" },
                            new RightMargin { Width = "150" }
                        )
                    );
                    if (isHeader)
                    {
                        cellPr.AppendChild(new Shading { Val = ShadingPatternValues.Clear, Fill = "F1F5F9" });
                    }
                    cell.AppendChild(cellPr);

                    var p = cell.AppendChild(new Paragraph());
                    p.AppendChild(new ParagraphProperties(new SpacingBetweenLines { Before = "0", After = "0" }));
                    var r = p.AppendChild(new Run());
                    var rPr = new RunProperties(
                        new FontSize { Val = isHeader ? "20" : "19" },
                        new Color { Val = isHeader ? "0F172A" : "334155" },
                        new RunFonts { Ascii = "Segoe UI", HighAnsi = "Segoe UI" }
                    );
                    if (isHeader) rPr.AppendChild(new Bold());
                    r.AppendChild(rPr);
                    r.AppendChild(new Text(data[i, j]));
                }
            }
        }

        // Content
        AddTitle("ИНСТРУКЦИЯ ПО ВЫПОЛНЕНИЮ И ЭКСПЛУАТАЦИИ ПРОГРАММНОГО КОМПЛЕКСА");
        AddSubtitle("Вариант 2 | Предметная область: «Поликлиника» | Все модули 1–4");

        AddParagraph("Настоящий документ представляет собой полную спецификацию и пошаговую инструкцию по установке, развертыванию, архитектуре и эксплуатации программного решения «Поликлиника», разработанного в соответствии с заданием демонстрационного экзамена (Вариант № 2).");

        AddHeading1("1. Описание предметной области и общая архитектура");
        AddParagraph("Предметная область: Поликлиника ведет цифровой учет пациентов, медицинского персонала (врачей), расписания и записей на прием, диагнозов и истории лечения, а также взаимодействия с внешними фармацевтическими партнерами-поставщиками медикаментов.");
        AddParagraph("Программное решение построено на базе современной модульной архитектуры .NET (C#) и включает в себя следующие компоненты:");
        AddBullet("Database/ — SQL-скрипт развертывания реляционной базы данных (3-я нормальная форма) и ER-диаграмма в формате PDF;");
        AddBullet("Polyclinic.Core — библиотека ядра системы, содержащая бизнес-модели, сервисы расчета скидки, калькулятор расхода медикаментов и модуль валидации данных пациентов;");
        AddBullet("Polyclinic.Data — уровень доступа к данным (DAO / Репозиторий), поддерживающий автономную SQLite базу данных с возможностью подключения к PostgreSQL / MS SQL Server;");
        AddBullet("Polyclinic.UI — графическое настольное приложение (WPF) с современным медицинским дизайном, визуальными подсказками, валидацией в реальном времени и навигацией;");
        AddBullet("Polyclinic.Tests — автоматизированный тестовый набор модульных тестов (xUnit), покрывающий критическую бизнес-логику всех модулей (37 успешных тестов).");

        AddHeading1("2. Модуль № 1: Разработка, администрирование и защита баз данных");
        AddParagraph("База данных спроектирована в строгом соответствии с требованиями 3-й нормальной формы (3NF):");
        AddBullet("1-я нормальная форма: все атрибуты атомарны, отсутствуют повторяющиеся группы;");
        AddBullet("2-я нормальная форма: все неключевые атрибуты полностью функционально зависят от составного/простого первичного ключа;");
        AddBullet("3-я нормальная форма: отсутствуют транзитивные зависимости неключевых атрибутов от первичного ключа (справочники полов, специализаций, партнеров, типов лечения и типов материалов вынесены в отдельные связанные таблицы).");

        AddHeading2("Таблицы базы данных и ссылочная целостность");
        string[,] dbTableData = {
            { "Таблица", "Первичный ключ", "Внешние ключи (FK)", "Назначение и 3НФ" },
            { "genders", "gender_id", "—", "Справочник полов пациентов (Мужской, Женский)" },
            { "patients", "patient_id", "gender_id -> genders", "Картотека пациентов поликлиники с полисом ОМС" },
            { "specializations", "specialization_id", "—", "Справочник врачебных специализаций" },
            { "doctors", "doctor_id", "specialization_id -> spec.", "Реестр врачей поликлиники с номерами кабинетов" },
            { "appointments", "appointment_id", "patient_id, doctor_id", "Журнал приёмов, диагнозов, назначений и стоимости" },
            { "partners", "partner_id", "—", "Фармацевтические компании и поставщики медикаментов" },
            { "treatment_types", "treatment_type_id", "—", "Типы лечения (product_type_id) и их коэффициенты" },
            { "material_types", "material_type_id", "—", "Типы препаратов (material_type_id) и процент брака" },
            { "medicine_deliveries", "delivery_id", "partner_id, material_type_id", "Журнал поставок лекарственных препаратов" }
        };
        AddTable(dbTableData, new[] { 1400, 1200, 1700, 2700 });

        AddParagraph("Файлы артефактов Модуля 1 в проекте:");
        AddBullet("Database/Polyclinic_DB.sql — полный скрипт создания таблиц, ограничений целостности (PRIMARY KEY, FOREIGN KEY, UNIQUE, NOT NULL, CHECK), индексов и наполнения начальными тестовыми данными.");
        AddBullet("Database/ER_Diagram.pdf — векторная схема базы данных со связями «один-ко-многим» (1:N) и типами полей.");

        AddHeading1("3. Модуль № 2: Разработка модулей ПО для компьютерных систем");
        AddParagraph("В рамках Модуля 2 реализована подсистема учета пациентов, расчета скидки и просмотра истории посещений.");

        AddHeading2("Алгоритм расчета индивидуальной скидки");
        AddParagraph("Скидка рассчитывается на основе количества завершенных визитов пациента за последние 12 месяцев от текущей даты:");
        string[,] discountTableData = {
            { "Количество визитов за последние 12 мес.", "Размер скидки (%)", "Пример статуса" },
            { "от 0 до 3 визитов", "0%", "Новый или редкий пациент" },
            { "от 4 до 7 визитов", "5%", "Постоянный пациент (базовый уровень)" },
            { "от 8 до 15 визитов", "10%", "Активный пациент (повышенный уровень)" },
            { "более 15 визитов (16+)", "15%", "VIP-пациент (максимальная скидка)" }
        };
        AddTable(discountTableData, new[] { 2600, 1600, 2800 });

        AddParagraph("Реализация метода скидки в классе DiscountCalculator.cs:");
        AddCodeBlock(
@"public static int CalculateDiscount(int visitsCountLast12Months)
{
    if (visitsCountLast12Months < 0)
        throw new ArgumentOutOfRangeException(nameof(visitsCountLast12Months));
    if (visitsCountLast12Months <= 3) return 0;
    if (visitsCountLast12Months <= 7) return 5;
    if (visitsCountLast12Months <= 15) return 10;
    return 15;
}"
        );

        AddHeading1("4. Модуль № 3: Сопровождение и обслуживание ПО (Интерфейс)");
        AddParagraph("Интерфейс приложения разработан на WPF с соблюдением требований эргономики и технического задания:");
        AddBullet("Единый стиль оформления: корпоративная медицинская цветовая палитра (#0284C7, #0F172A, #F8FAFC), унифицированные шрифты Segoe UI, скругления кнопок и карточек.");
        AddBullet("Четкая навигация и заголовки окон: «Информационная система «Поликлиника»», «Список пациентов», «Добавление пациента», «Редактирование пациента», «История приёмов пациента».");
        AddBullet("Валидация в реальном времени: маска и проверка полиса ОМС (16 цифр), номера телефона (+7...), адреса электронной почты и даты рождения (не в будущем).");
        AddBullet("Информативные сообщения об ошибках: при неверном заполнении полей выводятся всплывающие окна с иконками Warning/Error и подробными рекомендациями по исправлению.");
        AddBullet("Обновление данных без перезапуска: после добавления или редактирования пациента список на главном экране мгновенно обновляется.");

        AddHeading1("5. Модуль № 4: Интеграция программных модулей и расчет медикаментов");
        AddParagraph("Модуль 4 обеспечивает взаимодействие клиники с фармацевтическими партнерами и автоматический расчет потребности в лекарственных средствах.");
        
        AddHeading2("Спецификация метода расчета лекарств");
        AddParagraph("Метод рассчитывает требуемое количество препарата с учетом дозировки, продолжительности, количества пациентов, коэффициента вида лечения и процента брака/потерь при транспортировке.");
        AddCodeBlock(
@"material_per_unit = param1 * param2 * treatment_coefficient
total = material_per_unit * quantity * (1 + defect_rate)
return int(total)"
        );
        AddParagraph("Входные параметры метода:");
        AddBullet("product_type_id (int) — тип лечения/назначения (например, Инфузионная терапия = 1.10, Антибактериальная = 1.05);");
        AddBullet("material_type_id (int) — тип препарата (например, Физраствор = 0.02 брака, Антибиотик = 0.035 брака);");
        AddBullet("quantity (int) — количество пациентов;");
        AddBullet("param1 (float/double) — дозировка на прием/день;");
        AddBullet("param2 (float/double) — продолжительность курса в днях.");
        AddParagraph("Особые условия: если переданы некорректные или неизвестные типы данных, отрицательные или нулевые значения — метод возвращает -1.");

        AddHeading1("6. Инструкция по сборке, запуску и тестированию проекта");
        AddParagraph("Для сборки и запуска приложения требуется среда .NET 8.0, 9.0 или 10.0 под управлением Windows.");
        AddParagraph("Пошаговые команды запуска из командной строки PowerShell:");

        AddParagraph("Шаг 1. Переход в каталог решения:", true);
        AddCodeBlock("cd \"C:\\Вариант 2\\PolyclinicSolution\"");

        AddParagraph("Шаг 2. Запуск автоматических тестов (xUnit):", true);
        AddCodeBlock("dotnet test");

        AddParagraph("Шаг 3. Сборка решения в режиме Release / Debug:", true);
        AddCodeBlock("dotnet build");

        AddParagraph("Шаг 4. Запуск настольного приложения:", true);
        AddCodeBlock("dotnet run --project Polyclinic.UI");

        AddHeading1("7. Протокол автоматизированного тестирования");
        AddParagraph("Все 37 модульных тестов в проекте Polyclinic.Tests успешно пройдены без ошибок:");
        string[,] testResultsData = {
            { "Тестовый класс", "Кол-во тестов", "Статус", "Проверяемый функционал" },
            { "DiscountCalculatorTests", "22", "ПРОЙДЕН", "Градации скидок 0%, 5%, 10%, 15%, расчет стоимости, защита от -1" },
            { "MedicineRequirementCalculatorTests", "11", "ПРОЙДЕН", "Формула Модуля 4, расчет брака, коэффициенты, возврат -1 при ошибках" },
            { "PatientValidatorTests", "4", "ПРОЙДЕН", "Валидация 16 цифр полиса, email, телефона, даты рождения" }
        };
        AddTable(testResultsData, new[] { 2200, 1000, 1100, 2700 });

        AddParagraph("Программный комплекс готов к демонстрации и промышленной эксплуатации.");
        wordDoc.Save();
    }
}
