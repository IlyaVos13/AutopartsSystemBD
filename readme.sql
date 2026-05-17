CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    login VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(100) NOT NULL,
    role VARCHAR(50) NOT NULL
);

CREATE TABLE suppliers (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    address VARCHAR(200) NOT NULL,
    phone VARCHAR(30) NOT NULL
);

CREATE TABLE parts (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    article VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE supplied_parts (
    id SERIAL PRIMARY KEY,
    supplier_id INT NOT NULL REFERENCES suppliers(id) ON DELETE CASCADE,
    part_id INT NOT NULL REFERENCES parts(id) ON DELETE CASCADE,
    current_price NUMERIC(10,2) NOT NULL CHECK (current_price > 0),
    UNIQUE (supplier_id, part_id)
);

CREATE TABLE purchases (
    id SERIAL PRIMARY KEY,
    supplied_part_id INT NOT NULL REFERENCES supplied_parts(id) ON DELETE CASCADE,
    user_id INT NOT NULL REFERENCES users(id),
    purchase_date DATE NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0),
    unit_price NUMERIC(10,2) NOT NULL CHECK (unit_price > 0),
    total_sum NUMERIC(10,2) NOT NULL CHECK (total_sum > 0)
);

CREATE TABLE price_history (
    id SERIAL PRIMARY KEY,
    supplied_part_id INT NOT NULL REFERENCES supplied_parts(id) ON DELETE CASCADE,
    new_price NUMERIC(10,2) NOT NULL CHECK (new_price > 0),
    notification_date DATE NOT NULL,
    start_date DATE NOT NULL,
    CHECK (notification_date <= start_date)
);

INSERT INTO users (login, password, role)
VALUES ('admin', 'admin', 'Администратор');