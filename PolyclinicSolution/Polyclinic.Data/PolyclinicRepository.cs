using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using Polyclinic.Core.Models;
using Polyclinic.Core.Services;

namespace Polyclinic.Data;

/// <summary>
/// Репозиторий доступа к данным поликлиники (SQLite / ADO.NET).
/// Обеспечивает полную автономность и работу без внешних серверов.
/// </summary>
public class PolyclinicRepository
{
    private readonly string _connectionString;
    private readonly string _dbPath;

    public PolyclinicRepository(string? dbPath = null)
    {
        _dbPath = dbPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "polyclinic.db");
        _connectionString = $"Data Source={_dbPath}";
        InitializeDatabase();
    }

    /// <summary>
    /// Инициализация структуры базы данных и наполнение тестовыми данными при первом старте.
    /// </summary>
    public void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Проверяем, существует ли уже таблица patients
        var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='patients';";
        var tableExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

        if (!tableExists)
        {
            var initScript = @"
                PRAGMA foreign_keys = ON;

                CREATE TABLE genders (
                    gender_id INTEGER PRIMARY KEY,
                    gender_name TEXT NOT NULL UNIQUE
                );

                INSERT INTO genders (gender_id, gender_name) VALUES
                (1, 'Мужской'),
                (2, 'Женский');

                CREATE TABLE specializations (
                    specialization_id INTEGER PRIMARY KEY,
                    specialization_name TEXT NOT NULL UNIQUE
                );

                INSERT INTO specializations (specialization_id, specialization_name) VALUES
                (1, 'Терапевт'),
                (2, 'Кардиолог'),
                (3, 'Невролог'),
                (4, 'Хирург'),
                (5, 'Офтальмолог'),
                (6, 'Отоларинголог'),
                (7, 'Эндокринолог');

                CREATE TABLE doctors (
                    doctor_id INTEGER PRIMARY KEY,
                    full_name TEXT NOT NULL,
                    specialization_id INTEGER NOT NULL,
                    cabinet TEXT NOT NULL,
                    phone TEXT NOT NULL,
                    is_active INTEGER NOT NULL DEFAULT 1,
                    FOREIGN KEY (specialization_id) REFERENCES specializations(specialization_id)
                );

                INSERT INTO doctors (doctor_id, full_name, specialization_id, cabinet, phone, is_active) VALUES
                (1, 'Иванов Сергей Петрович', 1, '101', '+7 (900) 111-22-33', 1),
                (2, 'Смирнова Елена Васильевна', 2, '205', '+7 (900) 222-33-44', 1),
                (3, 'Кузнецов Михаил Александрович', 3, '312', '+7 (900) 333-44-55', 1),
                (4, 'Попова Ольга Дмитриевна', 4, '115', '+7 (900) 444-55-66', 1),
                (5, 'Васильев Андрей Николаевич', 5, '402', '+7 (900) 555-66-77', 1);

                CREATE TABLE patients (
                    patient_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    full_name TEXT NOT NULL,
                    birth_date TEXT NOT NULL,
                    gender_id INTEGER NOT NULL,
                    policy_number TEXT NOT NULL UNIQUE,
                    address TEXT NOT NULL,
                    phone TEXT NOT NULL,
                    email TEXT NOT NULL,
                    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (gender_id) REFERENCES genders(gender_id)
                );

                INSERT INTO patients (patient_id, full_name, birth_date, gender_id, policy_number, address, phone, email) VALUES
                (1, 'Сидоров Алексей Михайлович', '1985-04-12', 1, '1111222233334444', 'г. Москва, ул. Ленина, д. 10, кв. 15', '+7 (916) 123-45-67', 'sidorov@mail.ru'),
                (2, 'Ковалева Мария Ивановна', '1992-08-25', 2, '2222333344445555', 'г. Москва, ул. Мира, д. 25, кв. 42', '+7 (926) 234-56-78', 'kovaleva@yandex.ru'),
                (3, 'Морозов Дмитрий Сергеевич', '1978-11-03', 1, '3333444455556666', 'г. Москва, пр. Гагарина, д. 5, кв. 8', '+7 (903) 345-67-89', 'morozov@inbox.ru'),
                (4, 'Новикова Анна Павловна', '2001-02-18', 2, '4444555566667777', 'г. Москва, ул. Пушкина, д. 14, кв. 99', '+7 (915) 456-78-90', 'novikova@gmail.com'),
                (5, 'Федоров Игорь Владимирович', '1965-07-30', 1, '5555666677778888', 'г. Москва, б-р Победы, д. 3, кв. 21', '+7 (925) 567-89-01', 'fedorov@bk.ru');

                CREATE TABLE appointments (
                    appointment_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    patient_id INTEGER NOT NULL,
                    doctor_id INTEGER NOT NULL,
                    appointment_date TEXT NOT NULL,
                    status TEXT NOT NULL DEFAULT 'Завершен',
                    complaints TEXT,
                    diagnosis TEXT,
                    recommendations TEXT,
                    base_cost REAL NOT NULL DEFAULT 1500.0,
                    discount_percent INTEGER NOT NULL DEFAULT 0,
                    final_cost REAL NOT NULL DEFAULT 1500.0,
                    FOREIGN KEY (patient_id) REFERENCES patients(patient_id) ON DELETE CASCADE,
                    FOREIGN KEY (doctor_id) REFERENCES doctors(doctor_id)
                );

                -- Вставка приемов для пациентов (для демонстрации расчета скидок)
                INSERT INTO appointments (patient_id, doctor_id, appointment_date, status, complaints, diagnosis, recommendations, base_cost, discount_percent, final_cost) VALUES
                (1, 1, '2026-02-10 10:00:00', 'Завершен', 'Головная боль, общая слабость', 'ОРВИ легкой степени', 'Постельный режим, обильное питье', 1500.0, 0, 1500.0),
                (1, 1, '2026-05-15 11:30:00', 'Завершен', 'Повторный осмотр', 'Клиническое выздоровление', 'Витаминотерапия', 1200.0, 0, 1200.0),

                (2, 2, '2026-01-14 09:00:00', 'Завершен', 'Учащенное сердцебиение', 'Тахикардия синусовая', 'ЭКГ, бета-блокаторы', 2000.0, 0, 2000.0),
                (2, 2, '2026-03-20 10:00:00', 'Завершен', 'Контроль давления', 'Артериальная гипертензия I ст.', 'Ингибиторы АПФ', 1800.0, 0, 1800.0),
                (2, 1, '2026-06-11 12:00:00', 'Завершен', 'Сезонная аллергия', 'Аллергический ринит', 'Антигистаминные', 1500.0, 0, 1500.0),
                (2, 2, '2026-08-05 14:30:00', 'Завершен', 'Плановый прием кардиолога', 'Стабилизация АД', 'Продолжить схему', 1800.0, 5, 1710.0),
                (2, 5, '2026-10-18 16:00:00', 'Завершен', 'Проверка зрения', 'Миопия слабой степени', 'Гимнастика для глаз', 1600.0, 5, 1520.0),

                (3, 3, '2026-01-10 09:30:00', 'Завершен', 'Боли в спине', 'Остеохондроз поясничный', 'НПВП, ЛФК', 2200.0, 0, 2200.0),
                (3, 3, '2026-02-12 10:00:00', 'Завершен', 'Контроль инъекций', 'Положительная динамика', 'Массаж спины', 1800.0, 0, 1800.0),
                (3, 4, '2026-03-15 11:00:00', 'Завершен', 'Боли в суставе', 'Артроз коленного сустава', 'Хондропротекторы', 2000.0, 0, 2000.0),
                (3, 1, '2026-04-18 14:00:00', 'Завершен', 'Диспансеризация', 'Без выраженной патологии', 'Анализы ОАК, ОАМ', 1500.0, 5, 1425.0),
                (3, 3, '2026-05-22 15:30:00', 'Завершен', 'Обострение болей', 'Дорсопатия', 'Миорелаксанты', 1800.0, 5, 1710.0),
                (3, 1, '2026-06-25 10:15:00', 'Завершен', 'Анализы', 'Дислипидемия', 'Диета №10', 1500.0, 5, 1425.0),
                (3, 3, '2026-07-28 12:00:00', 'Завершен', 'Осмотр невролога', 'Умеренный синдром', 'Физиотерапия', 1800.0, 5, 1710.0),
                (3, 4, '2026-08-30 09:00:00', 'Завершен', 'Прием хирурга', 'Улучшение подвижности', 'Ограничение нагрузок', 2000.0, 10, 1800.0),
                (3, 2, '2026-09-24 16:45:00', 'Завершен', 'Профосмотр', 'Норма', 'Контроль АД', 1800.0, 10, 1620.0),
                (3, 1, '2026-11-05 11:00:00', 'Завершен', 'Бессонница', 'Астенический синдром', 'Седативные сборы', 1500.0, 10, 1350.0),

                (5, 1, '2025-12-05 10:00:00', 'Завершен', 'Кашель, температура', 'Острый бронхит', 'Антибиотики', 1500.0, 0, 1500.0),
                (5, 1, '2025-12-15 11:00:00', 'Завершен', 'Повторный осмотр', 'Ремиссия', 'Ингаляции', 1200.0, 0, 1200.0),
                (5, 2, '2026-01-12 09:30:00', 'Завершен', 'АД 150/95', 'Гипертония II ст.', 'Амлодипин', 1900.0, 0, 1900.0),
                (5, 2, '2026-02-02 10:15:00', 'Завершен', 'Контроль АД', 'Стабильно 130/80', 'Продолжить', 1600.0, 5, 1520.0),
                (5, 4, '2026-02-20 14:00:00', 'Завершен', 'Боль в подреберье', 'Холецистит', 'Диета №5', 2000.0, 5, 1900.0),
                (5, 1, '2026-03-10 11:30:00', 'Завершен', 'Слабость', 'Анемия', 'Препараты железа', 1500.0, 5, 1425.0),
                (5, 3, '2026-04-05 15:00:00', 'Завершен', 'Головокружение', 'ЦВБ', 'Винпоцетин', 2100.0, 5, 1995.0),
                (5, 1, '2026-05-04 09:00:00', 'Завершен', 'Контроль крови', 'Улучшение', 'Продолжить железо', 1400.0, 10, 1260.0),
                (5, 2, '2026-05-25 10:30:00', 'Завершен', 'Осмотр', 'Компенсирован', 'ЭКГ', 1800.0, 10, 1620.0),
                (5, 5, '2026-06-16 16:00:00', 'Завершен', 'Осмотр глаз', 'Ангиопатия', 'Тауфон', 1700.0, 10, 1530.0),
                (5, 4, '2026-07-08 11:00:00', 'Завершен', 'Осмотр живота', 'Ремиссия', 'Мин. вода', 1800.0, 10, 1620.0),
                (5, 1, '2026-08-14 12:30:00', 'Завершен', 'Вакцинация', 'Допущен', 'Вакцина Гриппол', 1000.0, 10, 900.0),
                (5, 3, '2026-09-02 14:15:00', 'Завершен', 'Онемение рук', 'Радикулопатия', 'Нейромультивит', 2000.0, 10, 1800.0),
                (5, 2, '2026-09-28 09:45:00', 'Завершен', 'Контроль', 'АД 125/80', 'Без изменений', 1600.0, 10, 1440.0),
                (5, 1, '2026-10-20 11:15:00', 'Завершен', 'Диспансеризация', 'Удовл.', 'Биохимия', 1500.0, 10, 1350.0),
                (5, 1, '2026-11-12 10:00:00', 'Завершен', 'Итоги анализов', 'В норме', 'ЗОЖ', 1500.0, 15, 1275.0);

                -- МОДУЛЬ 4: Справочники партнеров и поставок
                CREATE TABLE partners (
                    partner_id INTEGER PRIMARY KEY,
                    partner_name TEXT NOT NULL UNIQUE,
                    contact_person TEXT NOT NULL,
                    phone TEXT NOT NULL,
                    email TEXT NOT NULL,
                    address TEXT NOT NULL
                );

                INSERT INTO partners (partner_id, partner_name, contact_person, phone, email, address) VALUES
                (1, 'АО «Фармстандарт»', 'Григорьев Артем Юрьевич', '+7 (495) 787-12-12', 'supply@pharmstd.ru', 'г. Москва, ул. Тестовая, д. 10'),
                (2, 'ЗАО «Биокад»', 'Семенова Татьяна Викторовна', '+7 (812) 380-49-33', 'partners@biocad.ru', 'г. Санкт-Петербург, ул. Связи, д. 34'),
                (3, 'ООО «ПРОТЕК»', 'Лазарев Константин Сергеевич', '+7 (495) 737-75-00', 'distrib@protek.ru', 'г. Москва, Чермянский пр-д, д. 5'),
                (4, 'ООО «Гротекс» (Solopharm)', 'Мартынова Юлия Сергеевна', '+7 (812) 385-47-87', 'orders@solopharm.com', 'г. Санкт-Петербург, Индустриальный пр., д. 71'),
                (5, 'АО «Р-Фарм»', 'Климов Денис Олегович', '+7 (495) 956-79-37', 'sales@r-pharm.ru', 'г. Москва, ул. Берзарина, д. 19');

                CREATE TABLE treatment_types (
                    treatment_type_id INTEGER PRIMARY KEY,
                    type_name TEXT NOT NULL UNIQUE,
                    treatment_coefficient REAL NOT NULL
                );

                INSERT INTO treatment_types (treatment_type_id, type_name, treatment_coefficient) VALUES
                (1, 'Антибактериальная терапия', 1.050),
                (2, 'Инфузионная терапия (капельницы)', 1.100),
                (3, 'Физиотерапия и ингаляции', 1.000),
                (4, 'Инъекционная терапия', 1.080),
                (5, 'Химиотерапия и таргетное лечение', 1.150);

                CREATE TABLE material_types (
                    material_type_id INTEGER PRIMARY KEY,
                    material_name TEXT NOT NULL UNIQUE,
                    defect_rate REAL NOT NULL
                );

                INSERT INTO material_types (material_type_id, material_name, defect_rate) VALUES
                (1, 'Антибиотик широкого спектра (флаконы)', 0.035),
                (2, 'Изотонический физраствор (0.9% NaCl 500мл)', 0.020),
                (3, 'Раствор глюкозы (5% 400мл)', 0.025),
                (4, 'Обезболивающий раствор (ампулы)', 0.040),
                (5, 'Витаминный комплекс для инъекций', 0.015);

                CREATE TABLE medicine_deliveries (
                    delivery_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    partner_id INTEGER NOT NULL,
                    material_type_id INTEGER NOT NULL,
                    medicine_name TEXT NOT NULL,
                    quantity INTEGER NOT NULL,
                    delivery_date TEXT NOT NULL,
                    batch_number TEXT NOT NULL,
                    FOREIGN KEY (partner_id) REFERENCES partners(partner_id),
                    FOREIGN KEY (material_type_id) REFERENCES material_types(material_type_id)
                );

                INSERT INTO medicine_deliveries (partner_id, material_type_id, medicine_name, quantity, delivery_date, batch_number) VALUES
                (1, 1, 'Цефтриаксон 1.0 г пор. д/приг. р-ра', 500, '2026-10-05', 'BAT-2026-091'),
                (4, 2, 'Натрия хлорид 0.9% 500 мл Solopharm', 1200, '2026-10-12', 'SOL-77412'),
                (2, 4, 'Кеторолак 30 мг/мл ампулы 1 мл №10', 350, '2026-10-20', 'BIO-44910'),
                (3, 3, 'Глюкоза р-р для инфузий 5% 400 мл', 800, '2026-11-01', 'PRT-11029'),
                (5, 1, 'Амоксициллин + клавулановая к-та 1.2 г', 450, '2026-11-08', 'RPH-33018'),
                (4, 2, 'Натрия хлорид 0.9% 250 мл Solopharm', 950, '2026-11-15', 'SOL-78099'),
                (1, 5, 'Комбилипен р-р д/ин. амп. 2 мл №10', 400, '2026-11-20', 'BAT-2026-105');
            ";

            var createCmd = connection.CreateCommand();
            createCmd.CommandText = initScript;
            createCmd.ExecuteNonQuery();
        }
    }

    #region Пациенты (Модуль 2 и 3)

    public List<Patient> GetPatients(string? searchTerm = null)
    {
        var list = new List<Patient>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var sql = @"
            SELECT p.patient_id, p.full_name, p.birth_date, p.gender_id, g.gender_name, 
                   p.policy_number, p.address, p.phone, p.email, p.created_at
            FROM patients p
            JOIN genders g ON p.gender_id = g.gender_id
        ";

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            sql += " WHERE p.full_name LIKE @s OR p.policy_number LIKE @s OR p.phone LIKE @s";
        }

        sql += " ORDER BY p.full_name ASC;";

        using var cmd = new SqliteCommand(sql, conn);
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            cmd.Parameters.AddWithValue("@s", $"%{searchTerm.Trim()}%");
        }

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var p = new Patient
            {
                PatientId = reader.GetInt32(0),
                FullName = reader.GetString(1),
                BirthDate = DateTime.Parse(reader.GetString(2)),
                GenderId = reader.GetInt32(3),
                GenderName = reader.GetString(4),
                PolicyNumber = reader.GetString(5),
                Address = reader.GetString(6),
                Phone = reader.GetString(7),
                Email = reader.GetString(8),
                CreatedAt = DateTime.Parse(reader.GetString(9))
            };
            list.Add(p);
        }

        // Подгружаем визиты и скидку для каждого пациента
        foreach (var p in list)
        {
            p.VisitsLast12Months = GetVisitsCountLast12Months(p.PatientId, conn);
            p.CurrentDiscountPercent = DiscountCalculator.CalculateDiscount(p.VisitsLast12Months);
        }

        return list;
    }

    public Patient? GetPatientById(int patientId)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        const string sql = @"
            SELECT p.patient_id, p.full_name, p.birth_date, p.gender_id, g.gender_name, 
                   p.policy_number, p.address, p.phone, p.email, p.created_at
            FROM patients p
            JOIN genders g ON p.gender_id = g.gender_id
            WHERE p.patient_id = @id;
        ";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", patientId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var p = new Patient
            {
                PatientId = reader.GetInt32(0),
                FullName = reader.GetString(1),
                BirthDate = DateTime.Parse(reader.GetString(2)),
                GenderId = reader.GetInt32(3),
                GenderName = reader.GetString(4),
                PolicyNumber = reader.GetString(5),
                Address = reader.GetString(6),
                Phone = reader.GetString(7),
                Email = reader.GetString(8),
                CreatedAt = DateTime.Parse(reader.GetString(9))
            };
            p.VisitsLast12Months = GetVisitsCountLast12Months(p.PatientId, conn);
            p.CurrentDiscountPercent = DiscountCalculator.CalculateDiscount(p.VisitsLast12Months);
            return p;
        }

        return null;
    }

    public int AddPatient(Patient patient)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        const string sql = @"
            INSERT INTO patients (full_name, birth_date, gender_id, policy_number, address, phone, email)
            VALUES (@full_name, @birth_date, @gender_id, @policy_number, @address, @phone, @email);
            SELECT last_insert_rowid();
        ";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@full_name", patient.FullName.Trim());
        cmd.Parameters.AddWithValue("@birth_date", patient.BirthDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@gender_id", patient.GenderId);
        cmd.Parameters.AddWithValue("@policy_number", patient.PolicyNumber.Trim());
        cmd.Parameters.AddWithValue("@address", patient.Address.Trim());
        cmd.Parameters.AddWithValue("@phone", patient.Phone.Trim());
        cmd.Parameters.AddWithValue("@email", patient.Email.Trim());

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void UpdatePatient(Patient patient)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        const string sql = @"
            UPDATE patients
            SET full_name = @full_name,
                birth_date = @birth_date,
                gender_id = @gender_id,
                policy_number = @policy_number,
                address = @address,
                phone = @phone,
                email = @email
            WHERE patient_id = @id;
        ";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@full_name", patient.FullName.Trim());
        cmd.Parameters.AddWithValue("@birth_date", patient.BirthDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@gender_id", patient.GenderId);
        cmd.Parameters.AddWithValue("@policy_number", patient.PolicyNumber.Trim());
        cmd.Parameters.AddWithValue("@address", patient.Address.Trim());
        cmd.Parameters.AddWithValue("@phone", patient.Phone.Trim());
        cmd.Parameters.AddWithValue("@email", patient.Email.Trim());
        cmd.Parameters.AddWithValue("@id", patient.PatientId);

        cmd.ExecuteNonQuery();
    }

    public void DeletePatient(int patientId)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = new SqliteCommand("DELETE FROM patients WHERE patient_id = @id;", conn);
        cmd.Parameters.AddWithValue("@id", patientId);
        cmd.ExecuteNonQuery();
    }

    #endregion

    #region Приёмы и история посещений (Модуль 1 и 2)

    public int GetVisitsCountLast12Months(int patientId, SqliteConnection? openConn = null)
    {
        var conn = openConn ?? new SqliteConnection(_connectionString);
        bool shouldClose = openConn == null;
        if (shouldClose) conn.Open();

        try
        {
            // Считаем завершенные визиты за последние 12 месяцев от текущей даты
            var thresholdDate = DateTime.Now.AddMonths(-12).ToString("yyyy-MM-dd HH:mm:ss");
            const string sql = @"
                SELECT COUNT(*) 
                FROM appointments 
                WHERE patient_id = @pid 
                  AND status = 'Завершен' 
                  AND appointment_date >= @dt;
            ";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pid", patientId);
            cmd.Parameters.AddWithValue("@dt", thresholdDate);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        finally
        {
            if (shouldClose) conn.Dispose();
        }
    }

    public List<Appointment> GetAppointmentsByPatientId(int patientId)
    {
        var list = new List<Appointment>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        const string sql = @"
            SELECT a.appointment_id, a.patient_id, p.full_name, a.doctor_id, d.full_name, 
                   s.specialization_name, d.cabinet, a.appointment_date, a.status,
                   a.complaints, a.diagnosis, a.recommendations, a.base_cost, a.discount_percent, a.final_cost
            FROM appointments a
            JOIN patients p ON a.patient_id = p.patient_id
            JOIN doctors d ON a.doctor_id = d.doctor_id
            JOIN specializations s ON d.specialization_id = s.specialization_id
            WHERE a.patient_id = @pid
            ORDER BY a.appointment_date DESC;
        ";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@pid", patientId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var app = new Appointment
            {
                AppointmentId = reader.GetInt32(0),
                PatientId = reader.GetInt32(1),
                PatientName = reader.GetString(2),
                DoctorId = reader.GetInt32(3),
                DoctorName = reader.GetString(4),
                SpecializationName = reader.GetString(5),
                Cabinet = reader.GetString(6),
                AppointmentDate = DateTime.Parse(reader.GetString(7)),
                Status = reader.GetString(8),
                Complaints = reader.IsDBNull(9) ? "" : reader.GetString(9),
                Diagnosis = reader.IsDBNull(10) ? "" : reader.GetString(10),
                Recommendations = reader.IsDBNull(11) ? "" : reader.GetString(11),
                BaseCost = Convert.ToDecimal(reader.GetDouble(12)),
                DiscountPercent = reader.GetInt32(13),
                FinalCost = Convert.ToDecimal(reader.GetDouble(14))
            };
            list.Add(app);
        }

        return list;
    }

    public void AddAppointment(Appointment appointment)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        const string sql = @"
            INSERT INTO appointments (patient_id, doctor_id, appointment_date, status, complaints, diagnosis, recommendations, base_cost, discount_percent, final_cost)
            VALUES (@patient_id, @doctor_id, @appointment_date, @status, @complaints, @diagnosis, @recommendations, @base_cost, @discount_percent, @final_cost);
        ";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@patient_id", appointment.PatientId);
        cmd.Parameters.AddWithValue("@doctor_id", appointment.DoctorId);
        cmd.Parameters.AddWithValue("@appointment_date", appointment.AppointmentDate.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("@status", appointment.Status);
        cmd.Parameters.AddWithValue("@complaints", appointment.Complaints ?? "");
        cmd.Parameters.AddWithValue("@diagnosis", appointment.Diagnosis ?? "");
        cmd.Parameters.AddWithValue("@recommendations", appointment.Recommendations ?? "");
        cmd.Parameters.AddWithValue("@base_cost", Convert.ToDouble(appointment.BaseCost));
        cmd.Parameters.AddWithValue("@discount_percent", appointment.DiscountPercent);
        cmd.Parameters.AddWithValue("@final_cost", Convert.ToDouble(appointment.FinalCost));

        cmd.ExecuteNonQuery();
    }

    #endregion

    #region Справочники (Врачи, Специализации, Пол)

    public List<Gender> GetGenders()
    {
        var list = new List<Gender>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = new SqliteCommand("SELECT gender_id, gender_name FROM genders ORDER BY gender_id;", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Gender { GenderId = reader.GetInt32(0), GenderName = reader.GetString(1) });
        }
        return list;
    }

    public List<Doctor> GetDoctors()
    {
        var list = new List<Doctor>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        const string sql = @"
            SELECT d.doctor_id, d.full_name, d.specialization_id, s.specialization_name, d.cabinet, d.phone, d.is_active
            FROM doctors d
            JOIN specializations s ON d.specialization_id = s.specialization_id
            WHERE d.is_active = 1
            ORDER BY d.full_name ASC;
        ";

        using var cmd = new SqliteCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Doctor
            {
                DoctorId = reader.GetInt32(0),
                FullName = reader.GetString(1),
                SpecializationId = reader.GetInt32(2),
                SpecializationName = reader.GetString(3),
                Cabinet = reader.GetString(4),
                Phone = reader.GetString(5),
                IsActive = reader.GetInt32(6) == 1
            });
        }
        return list;
    }

    #endregion

    #region Модуль 4: Партнеры, Поставки медикаментов и Справочники

    public List<Partner> GetPartners()
    {
        var list = new List<Partner>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = new SqliteCommand("SELECT partner_id, partner_name, contact_person, phone, email, address FROM partners ORDER BY partner_name;", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Partner
            {
                PartnerId = reader.GetInt32(0),
                PartnerName = reader.GetString(1),
                ContactPerson = reader.GetString(2),
                Phone = reader.GetString(3),
                Email = reader.GetString(4),
                Address = reader.GetString(5)
            });
        }
        return list;
    }

    public List<TreatmentType> GetTreatmentTypes()
    {
        var list = new List<TreatmentType>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = new SqliteCommand("SELECT treatment_type_id, type_name, treatment_coefficient FROM treatment_types ORDER BY treatment_type_id;", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new TreatmentType
            {
                TreatmentTypeId = reader.GetInt32(0),
                TypeName = reader.GetString(1),
                TreatmentCoefficient = reader.GetDouble(2)
            });
        }
        return list;
    }

    public List<MaterialType> GetMaterialTypes()
    {
        var list = new List<MaterialType>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = new SqliteCommand("SELECT material_type_id, material_name, defect_rate FROM material_types ORDER BY material_type_id;", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new MaterialType
            {
                MaterialTypeId = reader.GetInt32(0),
                MaterialName = reader.GetString(1),
                DefectRate = reader.GetDouble(2)
            });
        }
        return list;
    }

    public List<MedicineDelivery> GetMedicineDeliveries(int? partnerId = null, string? search = null)
    {
        var list = new List<MedicineDelivery>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var sql = @"
            SELECT md.delivery_id, md.partner_id, p.partner_name, md.material_type_id, mt.material_name,
                   md.medicine_name, md.quantity, md.delivery_date, md.batch_number
            FROM medicine_deliveries md
            JOIN partners p ON md.partner_id = p.partner_id
            JOIN material_types mt ON md.material_type_id = mt.material_type_id
            WHERE 1=1
        ";

        if (partnerId.HasValue && partnerId.Value > 0)
        {
            sql += " AND md.partner_id = @pid";
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " AND (md.medicine_name LIKE @s OR md.batch_number LIKE @s)";
        }

        sql += " ORDER BY md.delivery_date DESC;";

        using var cmd = new SqliteCommand(sql, conn);
        if (partnerId.HasValue && partnerId.Value > 0)
        {
            cmd.Parameters.AddWithValue("@pid", partnerId.Value);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            cmd.Parameters.AddWithValue("@s", $"%{search.Trim()}%");
        }

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new MedicineDelivery
            {
                DeliveryId = reader.GetInt32(0),
                PartnerId = reader.GetInt32(1),
                PartnerName = reader.GetString(2),
                MaterialTypeId = reader.GetInt32(3),
                MaterialName = reader.GetString(4),
                MedicineName = reader.GetString(5),
                Quantity = reader.GetInt32(6),
                DeliveryDate = DateTime.Parse(reader.GetString(7)),
                BatchNumber = reader.GetString(8)
            });
        }

        return list;
    }

    public void AddMedicineDelivery(MedicineDelivery delivery)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        const string sql = @"
            INSERT INTO medicine_deliveries (partner_id, material_type_id, medicine_name, quantity, delivery_date, batch_number)
            VALUES (@partner_id, @material_type_id, @medicine_name, @quantity, @delivery_date, @batch_number);
        ";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@partner_id", delivery.PartnerId);
        cmd.Parameters.AddWithValue("@material_type_id", delivery.MaterialTypeId);
        cmd.Parameters.AddWithValue("@medicine_name", delivery.MedicineName.Trim());
        cmd.Parameters.AddWithValue("@quantity", delivery.Quantity);
        cmd.Parameters.AddWithValue("@delivery_date", delivery.DeliveryDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@batch_number", delivery.BatchNumber.Trim());

        cmd.ExecuteNonQuery();
    }

    #endregion
}
