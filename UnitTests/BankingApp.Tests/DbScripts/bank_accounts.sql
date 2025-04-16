CREATE TABLE bank_accounts (
    id UUID PRIMARY KEY,
    name TEXT NOT NULL,
    account_number TEXT NOT NULL UNIQUE,
    email TEXT NOT NULL UNIQUE,
    date_of_birth DATE NOT NULL,
    nominee TEXT,
    mobile_number TEXT NOT NULL UNIQUE,
    pan TEXT NOT NULL UNIQUE
);
