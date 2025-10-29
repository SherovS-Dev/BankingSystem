-- =============================================
-- Banking System Database Schema for PostgreSQL
-- =============================================

-- Удаление существующих таблиц (если есть)
DROP TABLE IF EXISTS AuditLog CASCADE;
DROP TABLE IF EXISTS LoanPayments CASCADE;
DROP TABLE IF EXISTS Loans CASCADE;
DROP TABLE IF EXISTS Cards CASCADE;
DROP TABLE IF EXISTS Transactions CASCADE;
DROP TABLE IF EXISTS Accounts CASCADE;
DROP TABLE IF EXISTS Customers CASCADE;
DROP TABLE IF EXISTS Users CASCADE;

-- =============================================
-- Таблица: Customers (Клиенты)
-- =============================================
CREATE TABLE Customers (
                           CustomerId SERIAL PRIMARY KEY,
                           FirstName VARCHAR(50) NOT NULL,
                           LastName VARCHAR(50) NOT NULL,
                           MiddleName VARCHAR(50),
                           DateOfBirth DATE NOT NULL,
                           Email VARCHAR(100) UNIQUE NOT NULL,
                           PhoneNumber VARCHAR(20) NOT NULL,
                           PassportNumber VARCHAR(20) UNIQUE NOT NULL,
                           Address VARCHAR(200),
                           City VARCHAR(50),
                           Country VARCHAR(50) DEFAULT 'Tajikistan',
                           PostalCode VARCHAR(10),
                           CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                           UpdatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                           IsActive BOOLEAN DEFAULT TRUE,
                           CONSTRAINT CHK_Email CHECK (Email LIKE '%@%.%'),
                           CONSTRAINT CHK_DateOfBirth CHECK (DateOfBirth < CURRENT_DATE)
);

-- Индексы для таблицы Customers
CREATE INDEX idx_customers_email ON Customers(Email);
CREATE INDEX idx_customers_passport ON Customers(PassportNumber);
CREATE INDEX idx_customers_active ON Customers(IsActive);

-- =============================================
-- Таблица: Accounts (Счета)
-- =============================================
CREATE TABLE Accounts (
                          AccountId SERIAL PRIMARY KEY,
                          CustomerId INTEGER NOT NULL REFERENCES Customers(CustomerId),
                          AccountNumber VARCHAR(30) UNIQUE NOT NULL,
                          AccountType VARCHAR(20) NOT NULL,
                          Balance DECIMAL(18, 2) DEFAULT 0.00,
                          Currency VARCHAR(3) DEFAULT 'TJS',
                          InterestRate DECIMAL(5, 2) DEFAULT 0.00,
                          Status VARCHAR(20) DEFAULT 'Active',
                          OpenDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                          CloseDate TIMESTAMP,
                          LastTransactionDate TIMESTAMP,
                          CONSTRAINT CHK_Balance CHECK (Balance >= -100000),
                          CONSTRAINT CHK_AccountType CHECK (AccountType IN ('Checking', 'Savings', 'Credit')),
                          CONSTRAINT CHK_Status CHECK (Status IN ('Active', 'Frozen', 'Closed'))
);

-- Индексы для таблицы Accounts
CREATE INDEX idx_accounts_customer ON Accounts(CustomerId);
CREATE UNIQUE INDEX idx_accounts_number ON Accounts(AccountNumber);
CREATE INDEX idx_accounts_status ON Accounts(Status);

-- =============================================
-- Таблица: Transactions (Транзакции)
-- =============================================
CREATE TABLE Transactions (
                              TransactionId SERIAL PRIMARY KEY,
                              AccountId INTEGER NOT NULL REFERENCES Accounts(AccountId),
                              TransactionType VARCHAR(20) NOT NULL,
                              Amount DECIMAL(18, 2) NOT NULL,
                              Currency VARCHAR(3) DEFAULT 'TJS',
                              TransactionDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                              Description VARCHAR(200),
                              Status VARCHAR(20) DEFAULT 'Completed',
                              ToAccountId INTEGER REFERENCES Accounts(AccountId),
                              ReferenceNumber VARCHAR(50),
                              BalanceAfter DECIMAL(18, 2),
                              CreatedBy VARCHAR(50),
                              CONSTRAINT CHK_Amount CHECK (Amount > 0),
                              CONSTRAINT CHK_TransactionType CHECK (TransactionType IN ('Deposit', 'Withdrawal', 'Transfer')),
                              CONSTRAINT CHK_TransactionStatus CHECK (Status IN ('Pending', 'Completed', 'Failed', 'Cancelled'))
);

-- Индексы для таблицы Transactions
CREATE INDEX idx_transactions_account ON Transactions(AccountId);
CREATE INDEX idx_transactions_date ON Transactions(TransactionDate DESC);
CREATE INDEX idx_transactions_status ON Transactions(Status);
CREATE INDEX idx_transactions_referencenumber ON Transactions(ReferenceNumber)
    WHERE ReferenceNumber IS NOT NULL;

-- =============================================
-- Таблица: Cards (Карты)
-- =============================================
CREATE TABLE Cards (
                       CardId SERIAL PRIMARY KEY,
                       AccountId INTEGER NOT NULL REFERENCES Accounts(AccountId),
                       CardNumber VARCHAR(19) UNIQUE NOT NULL,
                       CardholderName VARCHAR(100) NOT NULL,
                       CardType VARCHAR(20) NOT NULL,
                       ExpiryDate DATE NOT NULL,
                       CVV VARCHAR(4) NOT NULL,
                       IssueDate DATE DEFAULT CURRENT_DATE,
                       Status VARCHAR(20) DEFAULT 'Active',
                       DailyLimit DECIMAL(18, 2) DEFAULT 10000.00,
                       MonthlyLimit DECIMAL(18, 2) DEFAULT 100000.00,
                       CONSTRAINT CHK_CardType CHECK (CardType IN ('Debit', 'Credit')),
                       CONSTRAINT CHK_CardStatus CHECK (Status IN ('Active', 'Blocked', 'Expired')),
                       CONSTRAINT CHK_ExpiryDate CHECK (ExpiryDate > IssueDate)
);

-- Индексы для таблицы Cards
CREATE UNIQUE INDEX idx_cards_number ON Cards(CardNumber);
CREATE INDEX idx_cards_account ON Cards(AccountId);
CREATE INDEX idx_cards_status ON Cards(Status);

-- =============================================
-- Таблица: Loans (Кредиты)
-- =============================================
CREATE TABLE Loans (
                       LoanId SERIAL PRIMARY KEY,
                       CustomerId INTEGER NOT NULL REFERENCES Customers(CustomerId),
                       LoanType VARCHAR(50) NOT NULL,
                       PrincipalAmount DECIMAL(18, 2) NOT NULL,
                       InterestRate DECIMAL(5, 2) NOT NULL,
                       TermMonths INTEGER NOT NULL,
                       MonthlyPayment DECIMAL(18, 2) NOT NULL,
                       RemainingBalance DECIMAL(18, 2) NOT NULL,
                       StartDate DATE DEFAULT CURRENT_DATE,
                       EndDate DATE,
                       Status VARCHAR(20) DEFAULT 'Active',
                       AccountId INTEGER REFERENCES Accounts(AccountId),
                       CONSTRAINT CHK_PrincipalAmount CHECK (PrincipalAmount > 0),
                       CONSTRAINT CHK_InterestRate CHECK (InterestRate >= 0),
                       CONSTRAINT CHK_TermMonths CHECK (TermMonths > 0),
                       CONSTRAINT CHK_LoanStatus CHECK (Status IN ('Active', 'PaidOff', 'Defaulted'))
);

-- Индексы для таблицы Loans
CREATE INDEX idx_loans_customer ON Loans(CustomerId);
CREATE INDEX idx_loans_status ON Loans(Status);

-- =============================================
-- Таблица: LoanPayments (Платежи по кредитам)
-- =============================================
CREATE TABLE LoanPayments (
                              PaymentId SERIAL PRIMARY KEY,
                              LoanId INTEGER NOT NULL REFERENCES Loans(LoanId),
                              PaymentDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                              PaymentAmount DECIMAL(18, 2) NOT NULL,
                              PrincipalAmount DECIMAL(18, 2) NOT NULL,
                              InterestAmount DECIMAL(18, 2) NOT NULL,
                              RemainingBalance DECIMAL(18, 2) NOT NULL,
                              Status VARCHAR(20) DEFAULT 'Completed',
                              CONSTRAINT CHK_PaymentAmount CHECK (PaymentAmount > 0)
);

-- Индексы для таблицы LoanPayments
CREATE INDEX idx_loanpayments_loan ON LoanPayments(LoanId);
CREATE INDEX idx_loanpayments_date ON LoanPayments(PaymentDate DESC);

-- =============================================
-- Таблица: Users (Пользователи системы)
-- =============================================
CREATE TABLE Users (
                       UserId SERIAL PRIMARY KEY,
                       Username VARCHAR(50) UNIQUE NOT NULL,
                       PasswordHash VARCHAR(255) NOT NULL,
                       Email VARCHAR(100) UNIQUE NOT NULL,
                       FullName VARCHAR(100) NOT NULL,
                       Role VARCHAR(20) NOT NULL,
                       IsActive BOOLEAN DEFAULT TRUE,
                       CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                       LastLoginDate TIMESTAMP,
                       FailedLoginAttempts INTEGER DEFAULT 0,
                       LockedUntil TIMESTAMP,
                       CONSTRAINT CHK_UserRole CHECK (Role IN ('Admin', 'Manager', 'Teller', 'Customer'))
);

-- Индексы для таблицы Users
CREATE UNIQUE INDEX idx_users_username ON Users(Username);
CREATE INDEX idx_users_email ON Users(Email);

-- =============================================
-- Таблица: AuditLog (Журнал аудита)
-- =============================================
CREATE TABLE AuditLog (
                          AuditId SERIAL PRIMARY KEY,
                          UserId INTEGER REFERENCES Users(UserId),
                          Action VARCHAR(100) NOT NULL,
                          EntityType VARCHAR(50),
                          EntityId INTEGER,
                          OldValue TEXT,
                          NewValue TEXT,
                          IPAddress VARCHAR(45),
                          Timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Индексы для таблицы AuditLog
CREATE INDEX idx_auditlog_user ON AuditLog(UserId);
CREATE INDEX idx_auditlog_timestamp ON AuditLog(Timestamp DESC);
CREATE INDEX idx_auditlog_entity ON AuditLog(EntityType);

-- =============================================
-- Функции для PostgreSQL
-- =============================================

-- =============================================
-- 1. ФУНКЦИЯ: Пополнение счета (улучшенная)
-- =============================================
DROP FUNCTION IF EXISTS sp_deposit_funds;

CREATE OR REPLACE FUNCTION sp_deposit_funds(
    p_account_id INTEGER,
    p_amount DECIMAL(18, 2),
    p_description VARCHAR(200) DEFAULT NULL,
    p_created_by VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    transaction_id INTEGER,
    new_balance DECIMAL(18, 2)
)
LANGUAGE plpgsql
AS $$
DECLARE
v_balance DECIMAL(18, 2);
    v_reference VARCHAR(50);
    v_transaction_id INTEGER;
    v_random_suffix INTEGER;
BEGIN
    -- Проверка существования счета
SELECT Balance INTO v_balance FROM Accounts WHERE AccountId = p_account_id AND Status = 'Active';

IF v_balance IS NULL THEN
        RETURN QUERY SELECT FALSE, 'Account not found or inactive'::TEXT, NULL::INTEGER, NULL::DECIMAL;
RETURN;
END IF;
    
    -- Генерация уникального референса
    v_random_suffix := FLOOR(RANDOM() * 10000)::INTEGER;
    v_reference := 'DEP' || 
                   TO_CHAR(CURRENT_TIMESTAMP, 'YYYYMMDDHH24MISSUS') || 
                   LPAD(v_random_suffix::TEXT, 4, '0');
    
    -- Обновление баланса
UPDATE Accounts
SET Balance = Balance + p_amount,
    LastTransactionDate = CURRENT_TIMESTAMP
WHERE AccountId = p_account_id
    RETURNING Balance INTO v_balance;

-- Создание транзакции
INSERT INTO Transactions (AccountId, TransactionType, Amount, TransactionDate, Description, Status, ReferenceNumber, BalanceAfter, CreatedBy)
VALUES (p_account_id, 'Deposit', p_amount, CURRENT_TIMESTAMP, p_description, 'Completed', v_reference, v_balance, p_created_by)
    RETURNING TransactionId INTO v_transaction_id;

RETURN QUERY SELECT TRUE, 'Deposit completed successfully'::TEXT, v_transaction_id, v_balance;
END;
$$;

-- =============================================
-- 2. ФУНКЦИЯ: Снятие средств (улучшенная)
-- =============================================
DROP FUNCTION IF EXISTS sp_withdraw_funds;

CREATE OR REPLACE FUNCTION sp_withdraw_funds(
    p_account_id INTEGER,
    p_amount DECIMAL(18, 2),
    p_description VARCHAR(200) DEFAULT NULL,
    p_created_by VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    transaction_id INTEGER,
    new_balance DECIMAL(18, 2)
)
LANGUAGE plpgsql
AS $$
DECLARE
v_balance DECIMAL(18, 2);
    v_reference VARCHAR(50);
    v_transaction_id INTEGER;
    v_random_suffix INTEGER;
BEGIN
    -- Проверка существования счета и баланса
SELECT Balance INTO v_balance FROM Accounts WHERE AccountId = p_account_id AND Status = 'Active';

IF v_balance IS NULL THEN
        RETURN QUERY SELECT FALSE, 'Account not found or inactive'::TEXT, NULL::INTEGER, NULL::DECIMAL;
RETURN;
END IF;
    
    IF v_balance < p_amount THEN
        RETURN QUERY SELECT FALSE, 'Insufficient funds'::TEXT, NULL::INTEGER, NULL::DECIMAL;
RETURN;
END IF;
    
    -- Генерация уникального референса
    v_random_suffix := FLOOR(RANDOM() * 10000)::INTEGER;
    v_reference := 'WTD' || 
                   TO_CHAR(CURRENT_TIMESTAMP, 'YYYYMMDDHH24MISSUS') || 
                   LPAD(v_random_suffix::TEXT, 4, '0');
    
    -- Обновление баланса
UPDATE Accounts
SET Balance = Balance - p_amount,
    LastTransactionDate = CURRENT_TIMESTAMP
WHERE AccountId = p_account_id
    RETURNING Balance INTO v_balance;

-- Создание транзакции
INSERT INTO Transactions (AccountId, TransactionType, Amount, TransactionDate, Description, Status, ReferenceNumber, BalanceAfter, CreatedBy)
VALUES (p_account_id, 'Withdrawal', p_amount, CURRENT_TIMESTAMP, p_description, 'Completed', v_reference, v_balance, p_created_by)
    RETURNING TransactionId INTO v_transaction_id;

RETURN QUERY SELECT TRUE, 'Withdrawal completed successfully'::TEXT, v_transaction_id, v_balance;
END;
$$;

-- =============================================
-- 3. ФУНКЦИЯ: Перевод средств (улучшенная)
-- =============================================
DROP FUNCTION IF EXISTS sp_transfer_funds;

CREATE OR REPLACE FUNCTION sp_transfer_funds(
    p_from_account_id INTEGER,
    p_to_account_id INTEGER,
    p_amount DECIMAL(18, 2),
    p_description VARCHAR(200) DEFAULT NULL,
    p_created_by VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    transaction_id INTEGER
)
LANGUAGE plpgsql
AS $$
DECLARE
v_from_balance DECIMAL(18, 2);
    v_to_balance DECIMAL(18, 2);
    v_reference VARCHAR(50);
    v_transaction_id INTEGER;
    v_random_suffix INTEGER;
BEGIN
    -- Проверка счетов
SELECT Balance INTO v_from_balance FROM Accounts WHERE AccountId = p_from_account_id AND Status = 'Active';
SELECT Balance INTO v_to_balance FROM Accounts WHERE AccountId = p_to_account_id AND Status = 'Active';

IF v_from_balance IS NULL THEN
        RETURN QUERY SELECT FALSE, 'Source account not found or inactive'::TEXT, NULL::INTEGER;
RETURN;
END IF;
    
    IF v_to_balance IS NULL THEN
        RETURN QUERY SELECT FALSE, 'Destination account not found or inactive'::TEXT, NULL::INTEGER;
RETURN;
END IF;
    
    IF v_from_balance < p_amount THEN
        RETURN QUERY SELECT FALSE, 'Insufficient funds'::TEXT, NULL::INTEGER;
RETURN;
END IF;
    
    -- Генерация УНИКАЛЬНОГО референса с микросекундами и случайным числом
    v_random_suffix := FLOOR(RANDOM() * 10000)::INTEGER;
    v_reference := 'TRF' || 
                   TO_CHAR(CURRENT_TIMESTAMP, 'YYYYMMDDHH24MISSUS') || 
                   LPAD(v_random_suffix::TEXT, 4, '0');
    
    -- Списание со счета отправителя
UPDATE Accounts
SET Balance = Balance - p_amount,
    LastTransactionDate = CURRENT_TIMESTAMP
WHERE AccountId = p_from_account_id
    RETURNING Balance INTO v_from_balance;

-- Зачисление на счет получателя
UPDATE Accounts
SET Balance = Balance + p_amount,
    LastTransactionDate = CURRENT_TIMESTAMP
WHERE AccountId = p_to_account_id
    RETURNING Balance INTO v_to_balance;

-- Создание транзакции списания
INSERT INTO Transactions (AccountId, TransactionType, Amount, TransactionDate, Description, Status, ToAccountId, ReferenceNumber, BalanceAfter, CreatedBy)
VALUES (p_from_account_id, 'Transfer', p_amount, CURRENT_TIMESTAMP, p_description, 'Completed', p_to_account_id, v_reference, v_from_balance, p_created_by)
    RETURNING TransactionId INTO v_transaction_id;

-- Создание транзакции зачисления
INSERT INTO Transactions (AccountId, TransactionType, Amount, TransactionDate, Description, Status, ToAccountId, ReferenceNumber, BalanceAfter, CreatedBy)
VALUES (p_to_account_id, 'Deposit', p_amount, CURRENT_TIMESTAMP, 'Transfer from account ' || p_from_account_id, 'Completed', p_from_account_id, v_reference, v_to_balance, p_created_by);

RETURN QUERY SELECT TRUE, 'Transfer completed successfully'::TEXT, v_transaction_id;
END;
$$;

-- =============================================
-- Представления (Views)
-- =============================================

CREATE OR REPLACE VIEW vw_customer_account_summary AS
SELECT
    c.CustomerId,
    c.FirstName || ' ' || c.LastName AS FullName,
    c.Email,
    c.PhoneNumber,
    COUNT(a.AccountId) AS TotalAccounts,
    COALESCE(SUM(a.Balance), 0) AS TotalBalance,
    MAX(a.LastTransactionDate) AS LastTransactionDate
FROM Customers c
         LEFT JOIN Accounts a ON c.CustomerId = a.CustomerId AND a.Status = 'Active'
WHERE c.IsActive = TRUE
GROUP BY c.CustomerId, c.FirstName, c.LastName, c.Email, c.PhoneNumber;

-- =============================================
-- Тестовые данные
-- =============================================

INSERT INTO Customers (FirstName, LastName, MiddleName, DateOfBirth, Email, PhoneNumber, PassportNumber, Address, City, Country)
VALUES
    ('Фаррух', 'Исмоилов', 'Шерович', '1990-05-15', 'farrukh.ismoilov@example.com', '+992918123456', 'AA1234567', 'ул. Рудаки 50', 'Душанбе', 'Таджикистан'),
    ('Нигина', 'Шарипова', 'Алиевна', '1985-03-20', 'nigina.sharipova@example.com', '+992927234567', 'BB2345678', 'пр. Саъди Шерози 25', 'Душанбе', 'Таджикистан'),
    ('Джамшед', 'Назаров', 'Рахимович', '1992-11-08', 'jamshed.nazarov@example.com', '+992935345678', 'CC3456789', 'ул. Исмоили Сомони 10', 'Душанбе', 'Таджикистан');

INSERT INTO Accounts (CustomerId, AccountNumber, AccountType, Balance, Currency, InterestRate, Status)
VALUES
    (1, 'TJ72001234567890123456', 'Checking', 15000.00, 'TJS', 0.00, 'Active'),
    (1, 'TJ72001234567890123457', 'Savings', 50000.00, 'TJS', 5.50, 'Active'),
    (2, 'TJ72001234567890123458', 'Checking', 8000.00, 'TJS', 0.00, 'Active'),
    (3, 'TJ72001234567890123459', 'Savings', 120000.00, 'TJS', 6.00, 'Active');

INSERT INTO Transactions (AccountId, TransactionType, Amount, Description, ReferenceNumber, BalanceAfter, Status)
VALUES
    (1, 'Deposit', 15000.00, 'Initial deposit', 'DEP20250101001', 15000.00, 'Completed'),
    (2, 'Deposit', 50000.00, 'Initial deposit', 'DEP20250101002', 50000.00, 'Completed'),
    (3, 'Deposit', 8000.00, 'Initial deposit', 'DEP20250101003', 8000.00, 'Completed'),
    (4, 'Deposit', 120000.00, 'Initial deposit', 'DEP20250101004', 120000.00, 'Completed');

-- Пользователи (пароль: Admin123! - хеш упрощен для примера)
INSERT INTO Users (Username, PasswordHash, Email, FullName, Role)
VALUES
    ('admin', '$2a$11$abcdefghijklmnopqrstuvwxyz1234567890', 'admin@bank.tj', 'Администратор Системы', 'Admin'),
    ('manager', '$2a$11$abcdefghijklmnopqrstuvwxyz1234567890', 'manager@bank.tj', 'Менеджер Банка', 'Manager');