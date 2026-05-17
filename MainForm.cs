using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AutopartsSystemBD
{
    public class MainForm : Form
    {
        private readonly User currentUser;

        private readonly TabControl tabs = new TabControl();

        private readonly DataGridView suppliersGrid = new DataGridView();
        private readonly TextBox supplierNameBox = new TextBox();
        private readonly TextBox supplierAddressBox = new TextBox();
        private readonly TextBox supplierPhoneBox = new TextBox();

        private readonly DataGridView partsGrid = new DataGridView();
        private readonly TextBox partNameBox = new TextBox();
        private readonly TextBox partArticleBox = new TextBox();

        private readonly DataGridView suppliedPartsGrid = new DataGridView();
        private readonly ComboBox suppliedSupplierBox = new ComboBox();
        private readonly ComboBox suppliedPartBox = new ComboBox();
        private readonly NumericUpDown suppliedPriceBox = new NumericUpDown();
        private readonly DateTimePicker suppliedStartDatePicker = new DateTimePicker();

        private readonly DataGridView purchasesGrid = new DataGridView();
        private readonly ComboBox purchaseSupplierBox = new ComboBox();
        private readonly ComboBox purchasePartBox = new ComboBox();
        private readonly DateTimePicker purchaseDatePicker = new DateTimePicker();
        private readonly NumericUpDown purchaseQuantityBox = new NumericUpDown();

        private readonly DataGridView priceHistoryGrid = new DataGridView();
        private readonly ComboBox priceSupplierBox = new ComboBox();
        private readonly ComboBox pricePartBox = new ComboBox();
        private readonly NumericUpDown newPriceBox = new NumericUpDown();
        private readonly DateTimePicker notificationDatePicker = new DateTimePicker();
        private readonly DateTimePicker priceStartDatePicker = new DateTimePicker();

        private readonly DataGridView usersGrid = new DataGridView();
        private readonly TextBox userLoginBox = new TextBox();
        private readonly TextBox userPasswordBox = new TextBox();
        private readonly ComboBox userRoleBox = new ComboBox();

        private bool IsAdmin => currentUser.Role == "Администратор";
        private bool IsProcurement => currentUser.Role == "Сотрудник отдела закупок";
        private bool IsLeadership => currentUser.Role == "Руководство";

        private bool CanEditData => IsAdmin || IsProcurement;

        public MainForm(User user)
        {
            currentUser = user;

            Text = "Информационная система учета автозапчастей - " +
                   currentUser.Login + " (" + currentUser.Role + ")";

            Width = 1100;
            Height = 700;
            StartPosition = FormStartPosition.CenterScreen;

            tabs.Dock = DockStyle.Fill;
            Controls.Add(tabs);

            CreateSuppliersTab();
            CreatePartsTab();
            CreateSuppliedPartsTab();
            CreatePurchasesTab();
            CreatePriceHistoryTab();

            if (IsAdmin)
            {
                CreateUsersTab();
            }

            try
            {
                DataStore.Load();
                RefreshAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось подключиться к базе данных PostgreSQL.\n\n" +
                    "Проверьте, что PostgreSQL запущен, база autoparts_db создана, " +
                    "а пароль в DataStore.cs указан правильно.\n\n" +
                    "Ошибка: " + ex.Message,
                    "Ошибка подключения",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void CreateSuppliersTab()
        {
            var page = new TabPage("Поставщики");
            tabs.TabPages.Add(page);

            suppliersGrid.Dock = DockStyle.Top;
            suppliersGrid.Height = 350;
            suppliersGrid.ReadOnly = true;
            suppliersGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            page.Controls.Add(suppliersGrid);

            int y = 370;

            AddLabel(page, "Название:", 20, y);
            supplierNameBox.SetBounds(200, y, 250, 25);
            supplierNameBox.Enabled = CanEditData;
            page.Controls.Add(supplierNameBox);

            y += 35;
            AddLabel(page, "Адрес:", 20, y);
            supplierAddressBox.SetBounds(200, y, 250, 25);
            supplierAddressBox.Enabled = CanEditData;
            page.Controls.Add(supplierAddressBox);

            y += 35;
            AddLabel(page, "Телефон:", 20, y);
            supplierPhoneBox.SetBounds(200, y, 250, 25);
            supplierPhoneBox.Enabled = CanEditData;
            page.Controls.Add(supplierPhoneBox);

            y += 45;

            var addButton = new Button();
            addButton.Text = "Добавить поставщика";
            addButton.SetBounds(200, y, 250, 35);
            addButton.Enabled = CanEditData;
            addButton.Click += AddSupplier;
            page.Controls.Add(addButton);

            var clearButton = new Button();
            clearButton.Text = "Очистить все данные";
            clearButton.SetBounds(470, y, 250, 35);
            clearButton.Enabled = IsAdmin;
            clearButton.Visible = IsAdmin;
            clearButton.Click += ClearAllData;
            page.Controls.Add(clearButton);
        }

        private void CreatePartsTab()
        {
            var page = new TabPage("Детали");
            tabs.TabPages.Add(page);

            partsGrid.Dock = DockStyle.Top;
            partsGrid.Height = 350;
            partsGrid.ReadOnly = true;
            partsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            page.Controls.Add(partsGrid);

            int y = 370;

            AddLabel(page, "Наименование:", 20, y);
            partNameBox.SetBounds(200, y, 250, 25);
            partNameBox.Enabled = CanEditData;
            page.Controls.Add(partNameBox);

            y += 35;
            AddLabel(page, "Артикул:", 20, y);
            partArticleBox.SetBounds(200, y, 250, 25);
            partArticleBox.Enabled = CanEditData;
            page.Controls.Add(partArticleBox);

            y += 45;
            var addButton = new Button();
            addButton.Text = "Добавить деталь";
            addButton.SetBounds(200, y, 250, 35);
            addButton.Enabled = CanEditData;
            addButton.Click += AddPart;
            page.Controls.Add(addButton);
        }

        private void CreateSuppliedPartsTab()
        {
            var page = new TabPage("Поставляемые детали");
            tabs.TabPages.Add(page);

            suppliedPartsGrid.Dock = DockStyle.Top;
            suppliedPartsGrid.Height = 350;
            suppliedPartsGrid.ReadOnly = true;
            suppliedPartsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            page.Controls.Add(suppliedPartsGrid);

            int y = 370;

            AddLabel(page, "Поставщик:", 20, y);
            suppliedSupplierBox.SetBounds(200, y, 300, 25);
            suppliedSupplierBox.Enabled = CanEditData;
            page.Controls.Add(suppliedSupplierBox);

            y += 35;
            AddLabel(page, "Деталь:", 20, y);
            suppliedPartBox.SetBounds(200, y, 300, 25);
            suppliedPartBox.Enabled = CanEditData;
            page.Controls.Add(suppliedPartBox);

            y += 35;
            AddLabel(page, "Текущая цена:", 20, y);
            suppliedPriceBox.SetBounds(200, y, 300, 25);
            suppliedPriceBox.Maximum = 100000000;
            suppliedPriceBox.DecimalPlaces = 2;
            suppliedPriceBox.Enabled = CanEditData;
            page.Controls.Add(suppliedPriceBox);

            y += 35;
            AddLabel(page, "Дата начала цены:", 20, y);
            suppliedStartDatePicker.SetBounds(200, y, 300, 25);
            suppliedStartDatePicker.Enabled = CanEditData;
            page.Controls.Add(suppliedStartDatePicker);

            y += 45;
            var addButton = new Button();
            addButton.Text = "Добавить поставляемую деталь";
            addButton.SetBounds(200, y, 300, 35);
            addButton.Enabled = CanEditData;
            addButton.Click += AddSuppliedPart;
            page.Controls.Add(addButton);
        }

        private void CreatePurchasesTab()
        {
            var page = new TabPage("Закупки");
            tabs.TabPages.Add(page);

            purchasesGrid.Dock = DockStyle.Top;
            purchasesGrid.Height = 350;
            purchasesGrid.ReadOnly = true;
            purchasesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            page.Controls.Add(purchasesGrid);

            int y = 370;

            AddLabel(page, "Поставщик:", 20, y);
            purchaseSupplierBox.SetBounds(200, y, 300, 25);
            purchaseSupplierBox.SelectedIndexChanged += (s, e) => RefreshPurchaseParts();
            purchaseSupplierBox.Enabled = CanEditData;
            page.Controls.Add(purchaseSupplierBox);

            y += 35;
            AddLabel(page, "Деталь:", 20, y);
            purchasePartBox.SetBounds(200, y, 300, 25);
            purchasePartBox.Enabled = CanEditData;
            page.Controls.Add(purchasePartBox);

            y += 35;
            AddLabel(page, "Дата закупки:", 20, y);
            purchaseDatePicker.SetBounds(200, y, 300, 25);
            purchaseDatePicker.Enabled = CanEditData;
            page.Controls.Add(purchaseDatePicker);

            y += 35;
            AddLabel(page, "Количество:", 20, y);
            purchaseQuantityBox.SetBounds(200, y, 300, 25);
            purchaseQuantityBox.Minimum = 1;
            purchaseQuantityBox.Maximum = 100000;
            purchaseQuantityBox.Enabled = CanEditData;
            page.Controls.Add(purchaseQuantityBox);

            y += 45;
            var addButton = new Button();
            addButton.Text = "Зарегистрировать закупку";
            addButton.SetBounds(200, y, 300, 35);
            addButton.Enabled = CanEditData;
            addButton.Click += AddPurchase;
            page.Controls.Add(addButton);
        }

        private void CreatePriceHistoryTab()
        {
            var page = new TabPage("История цен");
            tabs.TabPages.Add(page);

            priceHistoryGrid.Dock = DockStyle.Top;
            priceHistoryGrid.Height = 350;
            priceHistoryGrid.ReadOnly = true;
            priceHistoryGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            page.Controls.Add(priceHistoryGrid);

            int y = 370;

            AddLabel(page, "Поставщик:", 20, y);
            priceSupplierBox.SetBounds(200, y, 300, 25);
            priceSupplierBox.SelectedIndexChanged += (s, e) =>
            {
                RefreshPriceParts();
                RefreshPriceHistoryGrid();
            };
            page.Controls.Add(priceSupplierBox);

            y += 35;
            AddLabel(page, "Деталь:", 20, y);
            pricePartBox.SetBounds(200, y, 300, 25);
            pricePartBox.SelectedIndexChanged += (s, e) => RefreshPriceHistoryGrid();
            page.Controls.Add(pricePartBox);

            y += 35;
            AddLabel(page, "Новая цена:", 20, y);
            newPriceBox.SetBounds(200, y, 300, 25);
            newPriceBox.Maximum = 100000000;
            newPriceBox.DecimalPlaces = 2;
            newPriceBox.Enabled = CanEditData;
            page.Controls.Add(newPriceBox);

            y += 35;
            AddLabel(page, "Дата уведомления:", 20, y);
            notificationDatePicker.SetBounds(200, y, 300, 25);
            notificationDatePicker.Enabled = CanEditData;
            page.Controls.Add(notificationDatePicker);

            y += 35;
            AddLabel(page, "Дата начала действия:", 20, y);
            priceStartDatePicker.SetBounds(200, y, 300, 25);
            priceStartDatePicker.Enabled = CanEditData;
            page.Controls.Add(priceStartDatePicker);

            y += 45;
            var addButton = new Button();
            addButton.Text = "Добавить изменение цены";
            addButton.SetBounds(200, y, 300, 35);
            addButton.Enabled = CanEditData;
            addButton.Click += AddPriceChange;
            page.Controls.Add(addButton);
        }

        private void CreateUsersTab()
        {
            var page = new TabPage("Пользователи");
            tabs.TabPages.Add(page);

            usersGrid.Dock = DockStyle.Top;
            usersGrid.Height = 350;
            usersGrid.ReadOnly = true;
            usersGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            usersGrid.MultiSelect = false;
            usersGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            page.Controls.Add(usersGrid);

            int y = 370;

            AddLabel(page, "Логин:", 20, y);
            userLoginBox.SetBounds(200, y, 250, 25);
            page.Controls.Add(userLoginBox);

            y += 35;
            AddLabel(page, "Пароль:", 20, y);
            userPasswordBox.SetBounds(200, y, 250, 25);
            page.Controls.Add(userPasswordBox);

            y += 35;
            AddLabel(page, "Роль:", 20, y);
            userRoleBox.SetBounds(200, y, 250, 25);
            userRoleBox.DropDownStyle = ComboBoxStyle.DropDownList;
            userRoleBox.Items.Add("Администратор");
            userRoleBox.Items.Add("Сотрудник отдела закупок");
            userRoleBox.Items.Add("Руководство");
            userRoleBox.SelectedIndex = 1;
            page.Controls.Add(userRoleBox);

            y += 45;

            var addButton = new Button();
            addButton.Text = "Добавить пользователя";
            addButton.SetBounds(200, y, 250, 35);
            addButton.Click += AddUser;
            page.Controls.Add(addButton);

            var changeRoleButton = new Button();
            changeRoleButton.Text = "Изменить роль";
            changeRoleButton.SetBounds(470, y, 200, 35);
            changeRoleButton.Click += ChangeUserRole;
            page.Controls.Add(changeRoleButton);

            var changePasswordButton = new Button();
            changePasswordButton.Text = "Изменить пароль";
            changePasswordButton.SetBounds(690, y, 200, 35);
            changePasswordButton.Click += ChangeUserPassword;
            page.Controls.Add(changePasswordButton);

            y += 45;

            var deleteButton = new Button();
            deleteButton.Text = "Удалить пользователя";
            deleteButton.SetBounds(200, y, 250, 35);
            deleteButton.Click += DeleteUser;
            page.Controls.Add(deleteButton);
        }

        private void AddSupplier(object? sender, EventArgs e)
        {
            if (!CanEditData)
            {
                MessageBox.Show("У вас нет прав на добавление поставщиков");
                return;
            }

            try
            {
                string name = supplierNameBox.Text.Trim();
                string address = supplierAddressBox.Text.Trim();
                string phone = supplierPhoneBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(name) ||
                    string.IsNullOrWhiteSpace(address) ||
                    string.IsNullOrWhiteSpace(phone))
                {
                    MessageBox.Show("Заполните все поля поставщика");
                    return;
                }

                DataStore.AddSupplier(name, address, phone);

                supplierNameBox.Clear();
                supplierAddressBox.Clear();
                supplierPhoneBox.Clear();

                RefreshAll();
                MessageBox.Show("Поставщик добавлен");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления поставщика: " + ex.Message);
            }
        }

        private void AddPart(object? sender, EventArgs e)
        {
            if (!CanEditData)
            {
                MessageBox.Show("У вас нет прав на добавление деталей");
                return;
            }

            try
            {
                string name = partNameBox.Text.Trim();
                string article = partArticleBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(article))
                {
                    MessageBox.Show("Заполните наименование и артикул");
                    return;
                }

                bool articleExists = DataStore.Parts
                    .Any(x => x.Article.Equals(article, StringComparison.OrdinalIgnoreCase));

                if (articleExists)
                {
                    MessageBox.Show("Деталь с таким артикулом уже существует");
                    return;
                }

                DataStore.AddPart(name, article);

                partNameBox.Clear();
                partArticleBox.Clear();

                RefreshAll();
                MessageBox.Show("Деталь добавлена");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления детали: " + ex.Message);
            }
        }

        private void AddSuppliedPart(object? sender, EventArgs e)
        {
            if (!CanEditData)
            {
                MessageBox.Show("У вас нет прав на добавление поставляемых деталей");
                return;
            }

            try
            {
                if (suppliedSupplierBox.SelectedItem is not Supplier supplier ||
                    suppliedPartBox.SelectedItem is not Part part)
                {
                    MessageBox.Show("Выберите поставщика и деталь");
                    return;
                }

                if (suppliedPriceBox.Value <= 0)
                {
                    MessageBox.Show("Цена должна быть больше нуля");
                    return;
                }

                bool exists = DataStore.SuppliedParts
                    .Any(x => x.SupplierId == supplier.Id && x.PartId == part.Id);

                if (exists)
                {
                    MessageBox.Show("Этот поставщик уже поставляет выбранную деталь");
                    return;
                }

                DataStore.AddSuppliedPart(
                    supplier.Id,
                    part.Id,
                    suppliedPriceBox.Value,
                    suppliedStartDatePicker.Value.Date
                );

                RefreshAll();
                MessageBox.Show("Поставляемая деталь добавлена");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления поставляемой детали: " + ex.Message);
            }
        }

        private void AddPurchase(object? sender, EventArgs e)
        {
            if (!CanEditData)
            {
                MessageBox.Show("У вас нет прав на регистрацию закупок");
                return;
            }

            try
            {
                if (purchaseSupplierBox.SelectedItem is not Supplier supplier ||
                    purchasePartBox.SelectedItem is not Part part)
                {
                    MessageBox.Show("Выберите поставщика и деталь");
                    return;
                }

                var suppliedPart = DataStore.SuppliedParts
                    .FirstOrDefault(x => x.SupplierId == supplier.Id && x.PartId == part.Id);

                if (suppliedPart == null)
                {
                    MessageBox.Show("Выбранная деталь не поставляется выбранным поставщиком");
                    return;
                }

                DateTime purchaseDate = purchaseDatePicker.Value.Date;
                int quantity = (int)purchaseQuantityBox.Value;

                DataStore.AddPurchase(suppliedPart.Id, currentUser.Id, purchaseDate, quantity);

                RefreshAll();
                MessageBox.Show("Закупка сохранена");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка регистрации закупки: " + ex.Message);
            }
        }

        private void AddPriceChange(object? sender, EventArgs e)
        {
            if (!CanEditData)
            {
                MessageBox.Show("У вас нет прав на изменение цен");
                return;
            }

            try
            {
                if (priceSupplierBox.SelectedItem is not Supplier supplier ||
                    pricePartBox.SelectedItem is not Part part)
                {
                    MessageBox.Show("Выберите поставщика и деталь");
                    return;
                }

                if (newPriceBox.Value <= 0)
                {
                    MessageBox.Show("Цена должна быть больше нуля");
                    return;
                }

                DateTime notificationDate = notificationDatePicker.Value.Date;
                DateTime startDate = priceStartDatePicker.Value.Date;

                if (notificationDate > startDate)
                {
                    MessageBox.Show("Дата уведомления не может быть позже даты начала действия цены");
                    return;
                }

                var suppliedPart = DataStore.SuppliedParts
                    .FirstOrDefault(x => x.SupplierId == supplier.Id && x.PartId == part.Id);

                if (suppliedPart == null)
                {
                    MessageBox.Show("Выбранная деталь не поставляется выбранным поставщиком");
                    return;
                }

                bool sameDateExists = DataStore.PriceHistory
                    .Any(x => x.SuppliedPartId == suppliedPart.Id && x.StartDate.Date == startDate);

                if (sameDateExists)
                {
                    MessageBox.Show("На выбранную дату уже есть изменение цены");
                    return;
                }

                DataStore.AddPriceChange(
                    suppliedPart.Id,
                    newPriceBox.Value,
                    notificationDate,
                    startDate
                );

                RefreshAll();
                MessageBox.Show("Изменение цены добавлено");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления изменения цены: " + ex.Message);
            }
        }

        private void AddUser(object? sender, EventArgs e)
        {
            try
            {
                string login = userLoginBox.Text.Trim();
                string password = userPasswordBox.Text.Trim();

                if (userRoleBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите роль пользователя");
                    return;
                }

                string role = userRoleBox.SelectedItem.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Введите логин и пароль");
                    return;
                }

                bool exists = DataStore.Users
                    .Any(x => x.Login.Equals(login, StringComparison.OrdinalIgnoreCase));

                if (exists)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует");
                    return;
                }

                DataStore.AddUser(login, password, role);

                userLoginBox.Clear();
                userPasswordBox.Clear();

                RefreshAll();
                MessageBox.Show("Пользователь добавлен");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления пользователя: " + ex.Message);
            }
        }

        private void ChangeUserRole(object? sender, EventArgs e)
        {
            try
            {
                var user = GetSelectedUser();

                if (user == null)
                {
                    MessageBox.Show("Выберите пользователя в таблице");
                    return;
                }

                if (userRoleBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите новую роль");
                    return;
                }

                string newRole = userRoleBox.SelectedItem.ToString() ?? "";

                if (user.Id == currentUser.Id && newRole != "Администратор")
                {
                    MessageBox.Show("Нельзя снять роль администратора с самого себя");
                    return;
                }

                DataStore.UpdateUserRole(user.Id, newRole);

                RefreshAll();
                MessageBox.Show("Роль пользователя изменена");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка изменения роли: " + ex.Message);
            }
        }

        private void ChangeUserPassword(object? sender, EventArgs e)
        {
            try
            {
                var user = GetSelectedUser();

                if (user == null)
                {
                    MessageBox.Show("Выберите пользователя в таблице");
                    return;
                }

                string newPassword = userPasswordBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    MessageBox.Show("Введите новый пароль в поле пароль");
                    return;
                }

                DataStore.UpdateUserPassword(user.Id, newPassword);

                userPasswordBox.Clear();

                RefreshAll();
                MessageBox.Show("Пароль пользователя изменен");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка изменения пароля: " + ex.Message);
            }
        }

        private void DeleteUser(object? sender, EventArgs e)
        {
            try
            {
                var user = GetSelectedUser();

                if (user == null)
                {
                    MessageBox.Show("Выберите пользователя в таблице");
                    return;
                }

                if (user.Id == currentUser.Id)
                {
                    MessageBox.Show("Нельзя удалить самого себя");
                    return;
                }

                bool hasPurchases = DataStore.Purchases.Any(x => x.UserId == user.Id);

                if (hasPurchases)
                {
                    MessageBox.Show("Нельзя удалить пользователя, у которого уже есть закупки");
                    return;
                }

                var result = MessageBox.Show(
                    "Вы точно хотите удалить пользователя " + user.Login + "?",
                    "Удаление пользователя",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result != DialogResult.Yes)
                {
                    return;
                }

                DataStore.DeleteUser(user.Id);

                RefreshAll();
                MessageBox.Show("Пользователь удален");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления пользователя: " + ex.Message);
            }
        }

        private User? GetSelectedUser()
        {
            if (usersGrid.CurrentRow == null)
            {
                return null;
            }

            var idValue = usersGrid.CurrentRow.Cells["Id"].Value;

            if (idValue == null)
            {
                return null;
            }

            int userId = Convert.ToInt32(idValue);

            return DataStore.Users.FirstOrDefault(x => x.Id == userId);
        }

        private void ClearAllData(object? sender, EventArgs e)
        {
            if (!IsAdmin)
            {
                MessageBox.Show("Только администратор может очищать данные");
                return;
            }

            try
            {
                var result = MessageBox.Show(
                    "Вы точно хотите удалить все данные? Пользователи останутся.",
                    "Очистка данных",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result != DialogResult.Yes)
                {
                    return;
                }

                DataStore.ClearAll();
                RefreshAll();

                MessageBox.Show("Все данные удалены");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка очистки данных: " + ex.Message);
            }
        }

        private void RefreshAll()
        {
            try
            {
                DataStore.RefreshAll();

                RefreshSuppliersGrid();
                RefreshPartsGrid();
                RefreshComboBoxes();
                RefreshSuppliedPartsGrid();
                RefreshPurchaseParts();
                RefreshPurchasesGrid();
                RefreshPriceParts();
                RefreshPriceHistoryGrid();

                if (IsAdmin)
                {
                    RefreshUsersGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка обновления данных: " + ex.Message);
            }
        }

        private void RefreshSuppliersGrid()
        {
            suppliersGrid.DataSource = null;
            suppliersGrid.DataSource = DataStore.Suppliers
                .Select(x => new
                {
                    x.Id,
                    Название = x.Name,
                    Адрес = x.Address,
                    Телефон = x.Phone
                })
                .ToList();
        }

        private void RefreshPartsGrid()
        {
            partsGrid.DataSource = null;
            partsGrid.DataSource = DataStore.Parts
                .Select(x => new
                {
                    x.Id,
                    Наименование = x.Name,
                    Артикул = x.Article
                })
                .ToList();
        }

        private void RefreshSuppliedPartsGrid()
        {
            suppliedPartsGrid.DataSource = null;
            suppliedPartsGrid.DataSource = DataStore.SuppliedParts
                .Select(x =>
                {
                    var supplier = DataStore.Suppliers.FirstOrDefault(s => s.Id == x.SupplierId);
                    var part = DataStore.Parts.FirstOrDefault(p => p.Id == x.PartId);

                    return new
                    {
                        x.Id,
                        Поставщик = supplier?.Name ?? "",
                        Деталь = part?.Name ?? "",
                        Артикул = part?.Article ?? "",
                        ТекущаяЦена = x.CurrentPrice
                    };
                })
                .ToList();
        }

        private void RefreshPurchasesGrid()
        {
            purchasesGrid.DataSource = null;
            purchasesGrid.DataSource = DataStore.Purchases
                .Select(x =>
                {
                    var supplied = DataStore.SuppliedParts.FirstOrDefault(sp => sp.Id == x.SuppliedPartId);
                    var supplier = DataStore.Suppliers.FirstOrDefault(s => s.Id == supplied?.SupplierId);
                    var part = DataStore.Parts.FirstOrDefault(p => p.Id == supplied?.PartId);
                    var user = DataStore.Users.FirstOrDefault(u => u.Id == x.UserId);

                    return new
                    {
                        x.Id,
                        Пользователь = user?.Login ?? "",
                        Поставщик = supplier?.Name ?? "",
                        Деталь = part?.Name ?? "",
                        Артикул = part?.Article ?? "",
                        Дата = x.PurchaseDate.ToShortDateString(),
                        Количество = x.Quantity,
                        Цена = x.UnitPrice,
                        Сумма = x.TotalSum
                    };
                })
                .ToList();
        }

        private void RefreshPriceHistoryGrid()
        {
            priceHistoryGrid.DataSource = null;

            if (priceSupplierBox.SelectedItem is Supplier supplier &&
                pricePartBox.SelectedItem is Part part)
            {
                var supplied = DataStore.SuppliedParts
                    .FirstOrDefault(x => x.SupplierId == supplier.Id && x.PartId == part.Id);

                if (supplied == null)
                {
                    priceHistoryGrid.DataSource = new List<object>();
                    return;
                }

                priceHistoryGrid.DataSource = DataStore.PriceHistory
                    .Where(x => x.SuppliedPartId == supplied.Id)
                    .OrderBy(x => x.StartDate)
                    .Select(x => new
                    {
                        x.Id,
                        Поставщик = supplier.Name,
                        Деталь = part.Name,
                        Цена = x.NewPrice,
                        ДатаУведомления = x.NotificationDate.ToShortDateString(),
                        ДатаНачалаДействия = x.StartDate.ToShortDateString()
                    })
                    .ToList();
            }
            else
            {
                priceHistoryGrid.DataSource = DataStore.PriceHistory
                    .Select(x =>
                    {
                        var supplied = DataStore.SuppliedParts.FirstOrDefault(sp => sp.Id == x.SuppliedPartId);
                        var supplier = DataStore.Suppliers.FirstOrDefault(s => s.Id == supplied?.SupplierId);
                        var part = DataStore.Parts.FirstOrDefault(p => p.Id == supplied?.PartId);

                        return new
                        {
                            x.Id,
                            Поставщик = supplier?.Name ?? "",
                            Деталь = part?.Name ?? "",
                            Цена = x.NewPrice,
                            ДатаУведомления = x.NotificationDate.ToShortDateString(),
                            ДатаНачалаДействия = x.StartDate.ToShortDateString()
                        };
                    })
                    .ToList();
            }
        }

        private void RefreshUsersGrid()
        {
            usersGrid.DataSource = null;
            usersGrid.DataSource = DataStore.Users
                .Select(x => new
                {
                    x.Id,
                    Логин = x.Login,
                    Роль = x.Role
                })
                .ToList();
        }

        private void RefreshComboBoxes()
        {
            FillComboBox(suppliedSupplierBox, DataStore.Suppliers);
            FillComboBox(suppliedPartBox, DataStore.Parts);

            FillComboBox(purchaseSupplierBox, DataStore.Suppliers);
            FillComboBox(priceSupplierBox, DataStore.Suppliers);
        }

        private void RefreshPurchaseParts()
        {
            if (purchaseSupplierBox.SelectedItem is not Supplier supplier)
            {
                FillComboBox(purchasePartBox, new List<Part>());
                return;
            }

            var partIds = DataStore.SuppliedParts
                .Where(x => x.SupplierId == supplier.Id)
                .Select(x => x.PartId)
                .ToList();

            var parts = DataStore.Parts
                .Where(x => partIds.Contains(x.Id))
                .ToList();

            FillComboBox(purchasePartBox, parts);
        }

        private void RefreshPriceParts()
        {
            if (priceSupplierBox.SelectedItem is not Supplier supplier)
            {
                FillComboBox(pricePartBox, new List<Part>());
                return;
            }

            var partIds = DataStore.SuppliedParts
                .Where(x => x.SupplierId == supplier.Id)
                .Select(x => x.PartId)
                .ToList();

            var parts = DataStore.Parts
                .Where(x => partIds.Contains(x.Id))
                .ToList();

            FillComboBox(pricePartBox, parts);
        }

        private void FillComboBox<T>(ComboBox comboBox, List<T> items)
        {
            comboBox.DataSource = null;
            comboBox.DataSource = items;

            if (typeof(T) == typeof(Supplier))
            {
                comboBox.DisplayMember = "Name";
            }

            if (typeof(T) == typeof(Part))
            {
                comboBox.DisplayMember = "Name";
            }
        }

        private void AddLabel(Control parent, string text, int x, int y)
        {
            var label = new Label();
            label.Text = text;
            label.SetBounds(x, y + 3, 170, 25);
            parent.Controls.Add(label);
        }
    }
}
