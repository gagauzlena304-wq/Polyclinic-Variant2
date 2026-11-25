-- ============================================================================
-- БАЗА ДАННЫХ: ПОЛИКЛИНИКА (POLYCLINIC)
-- Демонстрационный экзамен / Программные решения для бизнеса
-- Модуль 1: Разработка, администрирование и защита баз данных
-- Требования: 3-я нормальная форма, ссылочная целостность, согласованные имена
-- ============================================================================

-- Удаление таблиц в правильном порядке при повторном запуске
DROP TABLE IF EXISTS medicine_deliveries;
DROP TABLE IF EXISTS appointments;
DROP TABLE IF EXISTS doctors;
DROP TABLE IF EXISTS specializations;
DROP TABLE IF EXISTS patients;
DROP TABLE IF EXISTS genders;
DROP TABLE IF EXISTS material_types;
DROP TABLE IF EXISTS treatment_types;
DROP TABLE IF EXISTS partners;

-- 1. Справочник полов пациентов
CREATE TABLE genders (
    gender_id INT PRIMARY KEY,
    gender_name VARCHAR(20) NOT NULL UNIQUE
);

INSERT INTO genders (gender_id, gender_name) VALUES
(1, 'Мужской'),
(2, 'Женский');

-- 2. Справочник специализаций врачей
CREATE TABLE specializations (
    specialization_id INT PRIMARY KEY,
    specialization_name VARCHAR(100) NOT NULL UNIQUE
);

INSERT INTO specializations (specialization_id, specialization_name) VALUES
(1, 'Терапевт'),
(2, 'Кардиолог'),
(3, 'Невролог'),
(4, 'Хирург'),
(5, 'Офтальмолог'),
(6, 'Отоларинголог'),
(7, 'Эндокринолог');

-- 3. Таблица врачей
CREATE TABLE doctors (
    doctor_id INT PRIMARY KEY,
    full_name VARCHAR(150) NOT NULL,
    specialization_id INT NOT NULL,
    cabinet VARCHAR(10) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    is_active INT NOT NULL DEFAULT 1,
    CONSTRAINT fk_doctor_specialization FOREIGN KEY (specialization_id) 
        REFERENCES specializations(specialization_id) ON UPDATE CASCADE ON DELETE RESTRICT
);

INSERT INTO doctors (doctor_id, full_name, specialization_id, cabinet, phone, is_active) VALUES
(1, 'Иванов Сергей Петрович', 1, '101', '+7 (900) 111-22-33', 1),
(2, 'Смирнова Елена Васильевна', 2, '205', '+7 (900) 222-33-44', 1),
(3, 'Кузнецов Михаил Александрович', 3, '312', '+7 (900) 333-44-55', 1),
(4, 'Попова Ольга Дмитриевна', 4, '115', '+7 (900) 444-55-66', 1),
(5, 'Васильев Андрей Николаевич', 5, '402', '+7 (900) 555-66-77', 1);

-- 4. Таблица пациентов
CREATE TABLE patients (
    patient_id INT PRIMARY KEY,
    full_name VARCHAR(150) NOT NULL,
    birth_date DATE NOT NULL,
    gender_id INT NOT NULL,
    policy_number VARCHAR(16) NOT NULL UNIQUE,
    address VARCHAR(255) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_patient_gender FOREIGN KEY (gender_id) 
        REFERENCES genders(gender_id) ON UPDATE CASCADE ON DELETE RESTRICT
);

-- Индекс для быстрого поиска пациентов по ФИО, полису и телефону
CREATE INDEX idx_patients_fullname ON patients(full_name);
CREATE INDEX idx_patients_policy ON patients(policy_number);
CREATE INDEX idx_patients_phone ON patients(phone);

INSERT INTO patients (patient_id, full_name, birth_date, gender_id, policy_number, address, phone, email) VALUES
(1, 'Сидоров Алексей Михайлович', '1985-04-12', 1, '1111222233334444', 'г. Москва, ул. Ленина, д. 10, кв. 15', '+7 (916) 123-45-67', 'sidorov@mail.ru'),
(2, 'Ковалева Мария Ивановна', '1992-08-25', 2, '2222333344445555', 'г. Москва, ул. Мира, д. 25, кв. 42', '+7 (926) 234-56-78', 'kovaleva@yandex.ru'),
(3, 'Морозов Дмитрий Сергеевич', '1978-11-03', 1, '3333444455556666', 'г. Москва, пр. Гагарина, д. 5, кв. 8', '+7 (903) 345-67-89', 'morozov@inbox.ru'),
(4, 'Новикова Анна Павловна', '2001-02-18', 2, '4444555566667777', 'г. Москва, ул. Пушкина, д. 14, кв. 99', '+7 (915) 456-78-90', 'novikova@gmail.com'),
(5, 'Федоров Игорь Владимирович', '1965-07-30', 1, '5555666677778888', 'г. Москва, б-р Победы, д. 3, кв. 21', '+7 (925) 567-89-01', 'fedorov@bk.ru');

-- 5. Таблица записей на прием (история посещений и диагнозов)
CREATE TABLE appointments (
    appointment_id INT PRIMARY KEY,
    patient_id INT NOT NULL,
    doctor_id INT NOT NULL,
    appointment_date TIMESTAMP NOT NULL,
    status VARCHAR(30) NOT NULL DEFAULT 'Завершен',
    complaints TEXT,
    diagnosis TEXT,
    recommendations TEXT,
    base_cost DECIMAL(10, 2) NOT NULL DEFAULT 1500.00,
    discount_percent INT NOT NULL DEFAULT 0,
    final_cost DECIMAL(10, 2) NOT NULL DEFAULT 1500.00,
    CONSTRAINT fk_appointment_patient FOREIGN KEY (patient_id) 
        REFERENCES patients(patient_id) ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_appointment_doctor FOREIGN KEY (doctor_id) 
        REFERENCES doctors(doctor_id) ON UPDATE CASCADE ON DELETE RESTRICT
);

CREATE INDEX idx_appointments_patient ON appointments(patient_id);
CREATE INDEX idx_appointments_date ON appointments(appointment_date);

-- Тестовые приемы для демонстрации градаций скидок (за последние 12 месяцев):
-- Пациент 1: 2 визита (скидка 0%)
-- Пациент 2: 5 визитов (скидка 5%)
-- Пациент 3: 10 визитов (скидка 10%)
-- Пациент 5: 16 визитов (скидка 15%)
-- Пациент 4: 0 визитов (скидка 0%)

-- Пациент 1 (2 визита)
INSERT INTO appointments (appointment_id, patient_id, doctor_id, appointment_date, status, complaints, diagnosis, recommendations, base_cost, discount_percent, final_cost) VALUES
(1, 1, 1, '2026-02-10 10:00:00', 'Завершен', 'Головная боль, общая слабость', 'ОРВИ легкой степени', 'Постельный режим, обильное питье, парацетамол 500мг', 1500.00, 0, 1500.00),
(2, 1, 1, '2026-05-15 11:30:00', 'Завершен', 'Повторный осмотр после болезни', 'Клиническое выздоровление', 'Витаминотерапия', 1200.00, 0, 1200.00);

-- Пациент 2 (5 визитов за 12 месяцев -> скидка 5%)
INSERT INTO appointments (appointment_id, patient_id, doctor_id, appointment_date, status, complaints, diagnosis, recommendations, base_cost, discount_percent, final_cost) VALUES
(3, 2, 2, '2026-01-14 09:00:00', 'Завершен', 'Учащенное сердцебиение, одышка', 'Тахикардия синусовая', 'ЭКГ, холтеровское мониторирование, бета-блокаторы', 2000.00, 0, 2000.00),
(4, 2, 2, '2026-03-20 10:00:00', 'Завершен', 'Контроль показателей давления', 'Артериальная гипертензия I ст.', 'Коррекция дозы ингибиторов АПФ', 1800.00, 0, 1800.00),
(5, 2, 1, '2026-06-11 12:00:00', 'Завершен', 'Сезонная аллергия, насморк', 'Аллергический ринит', 'Антигистаминные препараты, назальный спрей', 1500.00, 0, 1500.00),
(6, 2, 2, '2026-08-05 14:30:00', 'Завершен', 'Плановый прием кардиолога', 'Стабилизация показателей АД', 'Продолжить текущую схему лечения', 1800.00, 5, 1710.00),
(7, 2, 5, '2026-10-18 16:00:00', 'Завершен', 'Проверка остроты зрения', 'Миопия слабой степени', 'Гимнастика для глаз, рецепт на очки', 1600.00, 5, 1520.00);

-- Пациент 3 (10 визитов за 12 месяцев -> скидка 10%)
INSERT INTO appointments (appointment_id, patient_id, doctor_id, appointment_date, status, complaints, diagnosis, recommendations, base_cost, discount_percent, final_cost) VALUES
(8, 3, 3, '2026-01-10 09:30:00', 'Завершен', 'Боли в поясничном отделе спины', 'Остеохондроз поясничного отдела', 'НПВП, ЛФК, физиотерапия', 2200.00, 0, 2200.00),
(9, 3, 3, '2026-02-12 10:00:00', 'Завершен', 'Контроль после курса инъекций', 'Положительная динамика', 'Массаж спины', 1800.00, 0, 1800.00),
(10, 3, 4, '2026-03-15 11:00:00', 'Завершен', 'Консультация хирурга по суставам', 'Артроз коленного сустава I ст.', 'Хондропротекторы', 2000.00, 0, 2000.00),
(11, 3, 1, '2026-04-18 14:00:00', 'Завершен', 'Плановый осмотр терапевта', 'Здоров в рамках диспансеризации', 'Сдать ОАК, ОАМ', 1500.00, 5, 1425.00),
(12, 3, 3, '2026-05-22 15:30:00', 'Завершен', 'Обострение болей при нагрузке', 'Дорсопатия', 'Миорелаксанты 10 дней', 1800.00, 5, 1710.00),
(13, 3, 1, '2026-06-25 10:15:00', 'Завершен', 'Разбор результатов анализов', 'Дислипидемия умеренная', 'Диета №10, Омега-3', 1500.00, 5, 1425.00),
(14, 3, 3, '2026-07-28 12:00:00', 'Завершен', 'Осмотр невролога', 'Умеренный болевой синдром', 'Курс физиотерапии', 1800.00, 5, 1710.00),
(15, 3, 4, '2026-08-30 09:00:00', 'Завершен', 'Повторный прием хирурга', 'Улучшение подвижности', 'Умеренные нагрузки', 2000.00, 10, 1800.00),
(16, 3, 2, '2026-09-24 16:45:00', 'Завершен', 'Профосмотр кардиолога', 'Без патологии', 'Контроль давления 1 раз в мес.', 1800.00, 10, 1620.00),
(17, 3, 1, '2026-11-05 11:00:00', 'Завершен', 'Жалобы на бессонницу', 'Астенический синдром', 'Седативные растительные сборы', 1500.00, 10, 1350.00);

-- Пациент 5 (16 визитов за 12 месяцев -> скидка 15%)
INSERT INTO appointments (appointment_id, patient_id, doctor_id, appointment_date, status, complaints, diagnosis, recommendations, base_cost, discount_percent, final_cost) VALUES
(18, 5, 1, '2025-12-05 10:00:00', 'Завершен', 'Кашель, температура 37.8', 'Острый бронхит', 'Амоксиклав 1000мг, муколитики', 1500.00, 0, 1500.00),
(19, 5, 1, '2025-12-15 11:00:00', 'Завершен', 'Повторный прием', 'Остаточный кашель', 'Ингаляции с физраствором', 1200.00, 0, 1200.00),
(20, 5, 2, '2026-01-12 09:30:00', 'Завершен', 'Давление 150/95', 'Гипертоническая болезнь II ст.', 'Амлодипин 5мг, индапамид', 1900.00, 0, 1900.00),
(21, 5, 2, '2026-02-02 10:15:00', 'Завершен', 'Контроль давления', 'АД 130/80, стабильно', 'Продолжить терапию', 1600.00, 5, 1520.00),
(22, 5, 4, '2026-02-20 14:00:00', 'Завершен', 'Боль в правом подреберье', 'Хронический бескаменный холецистит', 'Диета №5, спазмолитики', 2000.00, 5, 1900.00),
(23, 5, 1, '2026-03-10 11:30:00', 'Завершен', 'Слабость по утрам', 'Анемия легкой степени', 'Препараты железа 2 месяца', 1500.00, 5, 1425.00),
(24, 5, 3, '2026-04-05 15:00:00', 'Завершен', 'Головокружение', 'Цереброваскулярная недостаточность', 'Винпоцетин, сосудистая терапия', 2100.00, 5, 1995.00),
(25, 5, 1, '2026-05-04 09:00:00', 'Завершен', 'Контроль ферритина и гемоглобина', 'Положительная динамика', 'Продолжить прием железа', 1400.00, 10, 1260.00),
(26, 5, 2, '2026-05-25 10:30:00', 'Завершен', 'Плановый осмотр кардиолога', 'Состояние компенсированное', 'ЭКГ через полгода', 1800.00, 10, 1620.00),
(27, 5, 5, '2026-06-16 16:00:00', 'Завершен', 'Проверка глазного дна при гипертонии', 'Ангиопатия сетчатки', 'Капли тауфон, витамины', 1700.00, 10, 1530.00),
(28, 5, 4, '2026-07-08 11:00:00', 'Завершен', 'Контроль органов брюшной полости', 'Ремиссия холецистита', 'Минеральная вода, режим питания', 1800.00, 10, 1620.00),
(29, 5, 1, '2026-08-14 12:30:00', 'Завершен', 'Вакцинация от гриппа', 'Осмотр перед вакцинацией', 'Вакцинация разрешена', 1000.00, 10, 900.00),
(30, 5, 3, '2026-09-02 14:15:00', 'Завершен', 'Онемение пальцев рук', 'Шейный радикулит', 'Витамины группы В, ЛФК', 2000.00, 10, 1800.00),
(31, 5, 2, '2026-09-28 09:45:00', 'Завершен', 'Контрольный визит', 'АД 125/80', 'Коррекция терапии не требуется', 1600.00, 10, 1440.00),
(32, 5, 1, '2026-10-20 11:15:00', 'Завершен', 'Осенняя диспансеризация', 'Общее состояние удовлетворительное', 'Направление на биохимию', 1500.00, 10, 1350.00),
(33, 5, 1, '2026-11-12 10:00:00', 'Завершен', 'Осмотр по результатам биохимии', 'Показатели в пределах нормы', 'Здоровый образ жизни', 1500.00, 15, 1275.00);

-- ============================================================================
-- МОДУЛЬ 4: ТАБЛИЦЫ ВЗАИМОДЕЙСТВИЯ С ПАРТНЕРАМИ И РАСЧЕТА МЕДИКАМЕНТОВ
-- ============================================================================

-- 6. Справочник партнеров (фармацевтических компаний и дистрибьюторов)
CREATE TABLE partners (
    partner_id INT PRIMARY KEY,
    partner_name VARCHAR(150) NOT NULL UNIQUE,
    contact_person VARCHAR(150) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(100) NOT NULL,
    address VARCHAR(255) NOT NULL
);

INSERT INTO partners (partner_id, partner_name, contact_person, phone, email, address) VALUES
(1, 'АО «Фармстандарт»', 'Григорьев Артем Юрьевич', '+7 (495) 787-12-12', 'supply@pharmstd.ru', 'г. Москва, ул. Тестовая, д. 10'),
(2, 'ЗАО «Биокад»', 'Семенова Татьяна Викторовна', '+7 (812) 380-49-33', 'partners@biocad.ru', 'г. Санкт-Петербург, п. Стрельна, ул. Связи, д. 34'),
(3, 'ООО «ПРОТЕК»', 'Лазарев Константин Сергеевич', '+7 (495) 737-75-00', 'distrib@protek.ru', 'г. Москва, Чермянский пр-д, д. 5'),
(4, 'ООО «Гротекс» (Solopharm)', 'Мартынова Юлия Сергеевна', '+7 (812) 385-47-87', 'orders@solopharm.com', 'г. Санкт-Петербург, Индустриальный пр-кт, д. 71'),
(5, 'АО «Р-Фарм»', 'Климов Денис Олегович', '+7 (495) 956-79-37', 'sales@r-pharm.ru', 'г. Москва, ул. Берзарина, д. 19');

-- 7. Справочник типов лечения / назначения (product_type_id)
CREATE TABLE treatment_types (
    treatment_type_id INT PRIMARY KEY,
    type_name VARCHAR(150) NOT NULL UNIQUE,
    treatment_coefficient DECIMAL(5, 3) NOT NULL -- коэффициент типа лечения (например 1.1)
);

INSERT INTO treatment_types (treatment_type_id, type_name, treatment_coefficient) VALUES
(1, 'Антибактериальная терапия', 1.050),
(2, 'Инфузионная терапия (капельницы)', 1.100),
(3, 'Физиотерапия и ингаляции', 1.000),
(4, 'Инъекционная терапия', 1.080),
(5, 'Химиотерапия и таргетное лечение', 1.150);

-- 8. Справочник типов препаратов / лекарств (material_type_id)
CREATE TABLE material_types (
    material_type_id INT PRIMARY KEY,
    material_name VARCHAR(150) NOT NULL UNIQUE,
    defect_rate DECIMAL(5, 3) NOT NULL -- процент брака / потерь при хранении/транспортировке (например 0.05)
);

INSERT INTO material_types (material_type_id, material_name, defect_rate) VALUES
(1, 'Антибиотик широкого спектра (флаконы)', 0.035),
(2, 'Изотонический физраствор (0.9% NaCl 500мл)', 0.020),
(3, 'Раствор глюкозы (5% 400мл)', 0.025),
(4, 'Обезболивающий раствор (ампулы)', 0.040),
(5, 'Витаминный комплекс для инъекций', 0.015);

-- 9. Таблица истории поставок медикаментов от партнеров
CREATE TABLE medicine_deliveries (
    delivery_id INT PRIMARY KEY,
    partner_id INT NOT NULL,
    material_type_id INT NOT NULL,
    medicine_name VARCHAR(150) NOT NULL,
    quantity INT NOT NULL,
    delivery_date DATE NOT NULL,
    batch_number VARCHAR(50) NOT NULL,
    CONSTRAINT fk_delivery_partner FOREIGN KEY (partner_id) 
        REFERENCES partners(partner_id) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_delivery_material FOREIGN KEY (material_type_id) 
        REFERENCES material_types(material_type_id) ON UPDATE CASCADE ON DELETE RESTRICT
);

CREATE INDEX idx_deliveries_partner ON medicine_deliveries(partner_id);
CREATE INDEX idx_deliveries_date ON medicine_deliveries(delivery_date);

INSERT INTO medicine_deliveries (delivery_id, partner_id, material_type_id, medicine_name, quantity, delivery_date, batch_number) VALUES
(1, 1, 1, 'Цефтриаксон 1.0 г пор. д/приг. р-ра', 500, '2026-10-05', 'BAT-2026-091'),
(2, 4, 2, 'Натрия хлорид 0.9% 500 мл Solopharm', 1200, '2026-10-12', 'SOL-77412'),
(3, 2, 4, 'Кеторолак 30 мг/мл ампулы 1 мл №10', 350, '2026-10-20', 'BIO-44910'),
(4, 3, 3, 'Глюкоза р-р для инфузий 5% 400 мл', 800, '2026-11-01', 'PRT-11029'),
(5, 5, 1, 'Амоксициллин + клавулановая к-та 1.2 г', 450, '2026-11-08', 'RPH-33018'),
(6, 4, 2, 'Натрия хлорид 0.9% 250 мл Solopharm', 950, '2026-11-15', 'SOL-78099'),
(7, 1, 5, 'Комбилипен р-р д/ин. амп. 2 мл №10', 400, '2026-11-20', 'BAT-2026-105');
