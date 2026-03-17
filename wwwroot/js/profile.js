// ============================================
// ГЛОБАЛЬНЫЕ ФУНКЦИИ
// ============================================

/**
 * Обновляет интерфейс после входа пользователя
 * @param {Object} userData - Данные пользователя
 */
function updateUserInterface(userData) {
    const accountBtn = document.getElementById('accountBtn');
    if (accountBtn) {
        accountBtn.outerHTML = `
            <a href="Profile" class="account-action-btn" id="accountBtn">
                <img src="/img/header/check-profile.svg" alt="Профиль" class="header">
            </a>
        `;
    }
}

/**
 * Обновляет кнопку профиля в зависимости от авторизации
 * @param {boolean} isAuthenticated - Флаг авторизации
 */
function updateProfileButton(isAuthenticated) {
    const accountBtn = document.getElementById('accountBtn');
    if (!accountBtn) return;

    if (isAuthenticated) {
        accountBtn.outerHTML = `
            <a href="Profile" class="account-action-btn" id="accountBtn">
                <img src="/img/header/check-profile.svg" alt="Профиль" class="header">
            </a>
        `;
    } else {
        accountBtn.outerHTML = `
            <button class="account-action-btn" id="accountBtn">
                <img src="/img/header/profile.svg" alt="Войти" class="header">
            </button>
        `;
    }
}

/** Закрывает все модальные окна **/
function closeAllModals() {
    const allModals = document.querySelectorAll('.modal');
    allModals.forEach(modal => {
        modal.style.display = 'none';

        const modalBody = modal.querySelector('.modal-body');
        if (modalBody) {
            modalBody.classList.remove('modal-body-scrollable');
        }
    });

    const addressForms = document.querySelectorAll('.address-form');
    addressForms.forEach(form => {
        form.style.display = 'none';
    });

    const addAddressBtns = document.querySelectorAll('.add-address-btn');
    addAddressBtns.forEach(btn => {
        btn.style.display = 'block';
    });

    const addressLists = document.querySelectorAll('.address-list-scroll');
    addressLists.forEach(list => {
        list.classList.remove('address-list-scroll');
    });

    const promoLists = document.querySelectorAll('.promo-list-scroll');
    promoLists.forEach(list => {
        list.classList.remove('promo-list-scroll');
    });

    document.body.style.overflow = 'auto';
}

/**
 * Экранирует HTML-символы
 * @param {string} text - Текст для экранирования
 * @returns {string} Экранированный текст
 */
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

/**
 * Показывает уведомление
 * @param {string} message - Сообщение
 * @param {string} type - Тип уведомления (success, error, warning, info)
 */
function showNotification(message, type = 'info') {
    const existingNotifications = document.querySelectorAll('.notification');
    existingNotifications.forEach(notification => {
        notification.remove();
    });

    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.textContent = message;

    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 12px 20px;
        border-radius: 4px;
        color: white;
        font-weight: 500;
        z-index: 10000;
        animation: slideInRight 0.3s ease;
        max-width: 300px;
        box-shadow: 0 2px 10px rgba(0,0,0,0.2);
    `;

    if (type === 'success') {
        notification.style.backgroundColor = '#28a745';
    } else if (type === 'error') {
        notification.style.backgroundColor = '#dc3545';
    } else if (type === 'warning') {
        notification.style.backgroundColor = '#ffc107';
        notification.style.color = '#212529';
    } else {
        notification.style.backgroundColor = '#17a2b8';
    }

    document.body.appendChild(notification);

    if (!document.querySelector('#notification-animations')) {
        const style = document.createElement('style');
        style.id = 'notification-animations';
        style.textContent = `
            @keyframes slideInRight {
                from {
                    transform: translateX(100%);
                    opacity: 0;
                }
                to {
                    transform: translateX(0);
                    opacity: 1;
                }
            }
            @keyframes slideOutRight {
                from {
                    transform: translateX(0);
                    opacity: 1;
                }
                to {
                    transform: translateX(100%);
                    opacity: 0;
                }
            }
        `;
        document.head.appendChild(style);
    }

    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.3s ease';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }, 3000);
}

/**
 * Валидация промокода
 * @param {string} code - Промокод
 * @returns {boolean} Результат валидации
 */
function validatePromoCode(code) {
    const validCodes = ['SALE2025', 'FREE50', 'WELCOME52', 'WINTER26'];
    return code.length >= 4 && validCodes.includes(code.toUpperCase());
}

// ============================================
// ОСНОВНОЙ КОД ПРИ ЗАГРУЗКЕ DOM
// ============================================
document.addEventListener('DOMContentLoaded', function () {
    // ============================================
    // ИНИЦИАЛИЗАЦИЯ ЭЛЕМЕНТОВ
    // ============================================

    // Модальные окна для хэдера
    const registerModal = document.getElementById('registerModal');
    const loginModal = document.getElementById('loginModal');

    // Модальные окна для профиля
    const unavailableModal = document.getElementById('unavailableModal');
    const addressModal = document.getElementById('addressModal');
    const promoModal = document.getElementById('promoModal');

    // Модальные окна для корзины
    const addressModalBasket = document.getElementById('addressModalBasket');

    // Кнопки
    const accountBtn = document.getElementById('accountBtn');
    const paymentMethodsBtn = document.getElementById('paymentMethodsBtn');
    const addressBtn = document.getElementById('addressBtn');
    const promoCodesBtn = document.getElementById('promoCodesBtn');
    const addressBtnBasket = document.getElementById('addressBtnBasket');
    const paymentBtnBasket = document.getElementById('paymentBtnBasket');
    const switchToLoginBtn = document.getElementById('switchToLoginBtn');
    const switchToRegisterBtn = document.getElementById('switchToRegisterBtn');
    const registerBtn = document.getElementById('registerBtn');
    const loginBtn = document.getElementById('loginBtn');
    const closeButtons = document.querySelectorAll('.close-modal');

    // Элементы форм адресов
    const addAddressBtn = document.getElementById('addAddressBtn');
    const addressForm = document.getElementById('addressForm');
    const cancelAddressBtn = document.getElementById('cancelAddressBtn');
    const newAddressForm = document.getElementById('newAddressForm');
    const addressList = document.getElementById('addressList');

    const addAddressBtnBasket = document.getElementById('addAddressBtnBasket');
    const addressFormBasket = document.getElementById('addressFormBasket');
    const cancelAddressBtnBasket = document.getElementById('cancelAddressBtnBasket');
    const newAddressFormBasket = document.getElementById('newAddressFormBasket');
    const addressListBasket = document.getElementById('addressListBasket');

    const promoForm = document.getElementById('promoForm');
    const promoCodeInput = document.getElementById('promoCode');

    // ============================================
    // ДОБАВЛЕНИЕ СТИЛЕЙ ДЛЯ СКРОЛЛА
    // ============================================
    if (!document.querySelector('#modal-scroll-styles')) {
        const style = document.createElement('style');
        style.id = 'modal-scroll-styles';
        style.textContent = `
            .modal-scrollable {
                max-height: 400px;
                overflow-y: auto;
                padding-right: 8px;
                margin: 10px 0;
            }
            
            .modal-scrollable::-webkit-scrollbar {
                width: 6px;
            }
            
            .modal-scrollable::-webkit-scrollbar-track {
                background: #f1f1f1;
                border-radius: 3px;
            }
            
            .modal-scrollable::-webkit-scrollbar-thumb {
                background: #8a2828;
                border-radius: 3px;
            }
            
            .modal-scrollable::-webkit-scrollbar-thumb:hover {
                background: #6a1d1d;
            }
            
            .modal-scrollable {
                scrollbar-width: thin;
                scrollbar-color: #8a2828 #f1f1f1;
            }
            
            .address-list-scroll {
                max-height: 250px;
                overflow-y: auto;
                padding-right: 8px;
                margin-bottom: 15px;
            }
            
            .promo-list-scroll {
                overflow-y: auto;
                padding-right: 8px;
                margin-top: 10px;
            }
            
            .address-form {
                max-height: 320px;
                overflow-y: auto;
                padding-right: 8px;
            }
            
            .address-form h3 {
                margin: 5px 0 12px 0;
                font-size: 16px;
            }
            
            .address-form .form-input {
                padding: 8px 10px;
                margin-bottom: 8px;
                font-size: 14px;
            }
            
            .address-form .form-actions {
                margin-top: 12px;
                padding-top: 10px;
                border-top: 1px solid #eee;
            }
            
            .modal-body-scrollable {
                max-height: 60vh;
                overflow-y: auto;
                padding-right: 8px;
            }
            
            @media (max-height: 700px) {
                .address-form {
                    max-height: 250px;
                }
                
                .address-list-scroll {
                    max-height: 250px;
                }
                
                .promo-list-scroll {
                    max-height: 393px;
                }
                
                .modal-body-scrollable {
                    max-height: 50vh;
                }
            }
            
            @media (max-height: 550px) {
                .address-form {
                    max-height: 200px;
                }
                
                .address-list-scroll {
                    max-height: 140px;
                }
                
                .promo-list-scroll {
                    max-height: 120px;
                }
                
                .modal-body-scrollable {
                    max-height: 40vh;
                }
            }
        `;
        document.head.appendChild(style);
    }

    // ============================================
    // ПРОВЕРКА АВТОРИЗАЦИИ ПРИ ЗАГРУЗКЕ
    // ============================================
    const savedUser = localStorage.getItem('user');
    if (savedUser) {
        try {
            const user = JSON.parse(savedUser);
            updateUserInterface(user);
        } catch (e) {
            localStorage.removeItem('user'); // Удаляем некорректные данные
        }
    }

    // ============================================
    // ОТКРЫТИЕ МОДАЛЬНЫХ ОКОН
    // ============================================
    if (accountBtn && registerModal) {
        accountBtn.addEventListener('click', function (e) {
            e.preventDefault();
            registerModal.style.display = 'block';
            document.body.style.overflow = 'hidden';
        });
    }

    if (paymentMethodsBtn && unavailableModal) {
        paymentMethodsBtn.addEventListener('click', () => {
            unavailableModal.style.display = 'block';
            document.body.style.overflow = 'hidden';
        });
    }

    if (addressBtn && addressModal) {
        addressBtn.addEventListener('click', () => {
            addressModal.style.display = 'block';
            document.body.style.overflow = 'hidden';

            setTimeout(() => {
                const addressList = addressModal.querySelector('[id*="addressList"]');
                const modalBody = addressModal.querySelector('.modal-body');

                if (addressList) {
                    addressList.classList.add('address-list-scroll');
                }

                if (modalBody && modalBody.scrollHeight > window.innerHeight * 0.6) {
                    modalBody.classList.add('modal-body-scrollable');
                }
            }, 50);
        });
    }

    if (promoCodesBtn && promoModal) {
        promoCodesBtn.addEventListener('click', () => {
            promoModal.style.display = 'block';
            document.body.style.overflow = 'hidden';

            setTimeout(() => {
                const promoList = promoModal.querySelector('#promoList');
                const modalBody = promoModal.querySelector('.modal-body');

                if (promoList) {
                    promoList.classList.add('promo-list-scroll');
                }

                if (modalBody && modalBody.scrollHeight > window.innerHeight * 0.6) {
                    modalBody.classList.add('modal-body-scrollable');
                }
            }, 50);
        });
    }

    if (addressBtnBasket && addressModalBasket) {
        addressBtnBasket.addEventListener('click', () => {
            addressModalBasket.style.display = 'block';
            document.body.style.overflow = 'hidden';

            setTimeout(() => {
                const addressList = addressModalBasket.querySelector('[id*="addressList"]');
                const modalBody = addressModalBasket.querySelector('.modal-body');

                if (addressList) {
                    addressList.classList.add('address-list-scroll');
                }

                if (modalBody && modalBody.scrollHeight > window.innerHeight * 0.6) {
                    modalBody.classList.add('modal-body-scrollable');
                }
            }, 50);
        });
    }

    if (paymentBtnBasket && unavailableModal) {
        paymentBtnBasket.addEventListener('click', () => {
            unavailableModal.style.display = 'block';
            document.body.style.overflow = 'hidden';
        });
    }

    // ============================================
    // ПЕРЕКЛЮЧЕНИЕ МЕЖДУ ФОРМАМИ РЕГИСТРАЦИИ/ВХОДА
    // ============================================
    if (switchToLoginBtn && registerModal && loginModal) {
        switchToLoginBtn.addEventListener('click', () => {
            registerModal.style.display = 'none';
            loginModal.style.display = 'block';
        });
    }

    if (switchToRegisterBtn && registerModal && loginModal) {
        switchToRegisterBtn.addEventListener('click', function (e) {
            e.preventDefault();
            loginModal.style.display = 'none';
            registerModal.style.display = 'block';
        });
    }

    // ============================================
    // РЕГИСТРАЦИЯ ПОЛЬЗОВАТЕЛЯ
    // ============================================
    if (registerBtn) {
        registerBtn.addEventListener('click', async function (e) {
            e.preventDefault();
            const lastName = document.getElementById('regLastName');
            const firstName = document.getElementById('regFirstName');
            const middleName = document.getElementById('regMiddleName');
            const phone = document.getElementById('regPhone');
            const email = document.getElementById('regEmail');
            const genderSelect = document.getElementById('regGender');
            const password = document.getElementById('regPassword');
            const confirmPassword = document.getElementById('regConfirmPassword');
            const privacyCheckbox = document.getElementById('privacyCheckbox');
            if (!lastName || !firstName || !phone || !password || !confirmPassword) {
                alert('Ошибка: не найдены необходимые поля формы');
                return;
            }

            let errors = [];

            if (!lastName.value.trim()) errors.push('Фамилия обязательна');
            if (!firstName.value.trim()) errors.push('Имя обязательно');
            if (!phone.value.trim()) errors.push('Телефон обязателен');
            if (!password.value) errors.push('Пароль обязателен');
            if (password.value.length < 8) errors.push('Пароль должен быть не менее 8 символов');
            if (password.value !== confirmPassword.value) errors.push('Пароли не совпадают');
            if (!privacyCheckbox || !privacyCheckbox.checked) errors.push('Необходимо принять политику конфиденциальности');

            if (errors.length > 0) {
                alert('Ошибки:\n' + errors.join('\n'));
                return;
            }

            let genderValue = 0;
            if (genderSelect && genderSelect.value) {
                genderValue = parseInt(genderSelect.value) || 0;
            }

            const registrationData = {
                Phone: phone.value.trim(),
                Email: email ? email.value.trim() : '',
                FName: firstName.value.trim(),
                LName: lastName.value.trim(),
                SName: middleName ? middleName.value.trim() : '',
                Password: password.value,
                ConfirmPassword: confirmPassword.value,
                Gender: genderValue,
                AcceptPrivacyPolicy: true
            };

            try {
                const response = await fetch('/api/account/register', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(registrationData)
                });

                const result = await response.json();

                if (result.success) {
                    alert('Регистрация успешна! Теперь вы можете войти в аккаунт.');
                    if (registerModal) {
                        registerModal.style.display = 'none';
                        document.body.style.overflow = 'auto';
                    }

                    const form = document.getElementById('registerForm') || document.getElementById('newAccountForm');
                    if (form) form.reset();

                    if (loginModal) {
                        loginModal.style.display = 'block';
                        document.body.style.overflow = 'hidden';
                    }
                } else {
                    alert('Ошибка регистрации: ' + result.message);
                }
            } catch (error) {
                alert('Ошибка сети. Проверьте подключение к интернету.');
            }
        });
    }

    // ============================================
    // ВХОД ПОЛЬЗОВАТЕЛЯ
    // ============================================
    if (loginBtn) {
        loginBtn.addEventListener('click', async function (e) {
            e.preventDefault();

            const phone = document.getElementById('loginPhone');
            const password = document.getElementById('loginPassword');

            if (!phone || !password) {
                alert('Не найдены поля формы входа');
                return;
            }

            if (!phone.value.trim() || !password.value) {
                alert('Заполните телефон и пароль');
                return;
            }

            const loginData = {
                Phone: phone.value.trim(),
                Password: password.value
            };

            try {
                const response = await fetch('/api/account/login', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(loginData)
                });

                const result = await response.json();

                if (result.success) {
                    alert('Вход выполнен успешно!');

                    localStorage.setItem('user', JSON.stringify(result.data));

                    if (loginModal) {
                        loginModal.style.display = 'none';
                        document.body.style.overflow = 'auto';
                    }

                    const form = document.getElementById('loginForm');
                    if (form) form.reset();

                    updateUserInterface(result.data);
                } else {
                    alert('Ошибка входа: ' + result.message);
                }
            } catch (error) {
                alert('Ошибка сети при входе');
            }
        });
    }

    // ============================================
    // ЗАКРЫТИЕ МОДАЛЬНЫХ ОКОН
    // ============================================
    closeButtons.forEach(button => {
        button.addEventListener('click', (e) => {
            e.stopPropagation();
            closeAllModals();
        });
    });

    window.addEventListener('click', (event) => {
        if (event.target.classList.contains('modal')) {
            closeAllModals();
        }
    });

    // ============================================
    // ФУНКЦИОНАЛ АДРЕСОВ ДОСТАВКИ (ПРОФИЛЬ)
    // ============================================
    if (addAddressBtn && addressForm) {
        addAddressBtn.addEventListener('click', () => {
            addressForm.style.display = 'block';
            addAddressBtn.style.display = 'none';
            addressForm.querySelector('input')?.focus();
        });
    }

    if (cancelAddressBtn) {
        cancelAddressBtn.addEventListener('click', () => {
            if (addressForm) addressForm.style.display = 'none';
            if (addAddressBtn) addAddressBtn.style.display = 'block';
            if (newAddressForm) newAddressForm.reset();
        });
    }

    if (newAddressForm) {
        newAddressForm.addEventListener('submit', (e) => {
            e.preventDefault();

            const inputs = newAddressForm.querySelectorAll('input');
            const city = inputs[0]?.value.trim() || '';
            const street = inputs[1]?.value.trim() || '';
            const house = inputs[2]?.value.trim() || '';
            const apartment = inputs[3]?.value.trim() || '';
            const name = inputs[4]?.value.trim() || 'Новый адрес';

            if (!city || !street || !house) {
                alert('Пожалуйста, заполните обязательные поля: город, улица и дом');
                return;
            }

            const addressItem = document.createElement('div');
            addressItem.className = 'address-item';
            addressItem.innerHTML = `
                <p><strong>${escapeHtml(name)}</strong></p>
                <p>${escapeHtml(city)}, ${escapeHtml(street)}, д. ${escapeHtml(house)}${apartment ? ', кв. ' + escapeHtml(apartment) : ''}</p>
                <div class="address-actions">
                    <button class="btn-edit" type="button">Изменить</button>
                    <button class="btn-delete" type="button">Удалить</button>
                </div>
            `;

            if (addressList) {
                addressList.appendChild(addressItem);

                setTimeout(() => {
                    addressItem.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                }, 100);
            }

            newAddressForm.reset();
            if (addressForm) addressForm.style.display = 'none';
            if (addAddressBtn) addAddressBtn.style.display = 'block';

            showNotification('Адрес успешно добавлен!', 'success');
        });
    }

    // ============================================
    // ФУНКЦИОНАЛ АДРЕСОВ ДОСТАВКИ (КОРЗИНА)
    // ============================================
    if (addAddressBtnBasket && addressFormBasket) {
        addAddressBtnBasket.addEventListener('click', () => {
            addressFormBasket.style.display = 'block';
            addAddressBtnBasket.style.display = 'none';
            addressFormBasket.querySelector('input')?.focus();
        });
    }

    if (cancelAddressBtnBasket) {
        cancelAddressBtnBasket.addEventListener('click', () => {
            if (addressFormBasket) addressFormBasket.style.display = 'none';
            if (addAddressBtnBasket) addAddressBtnBasket.style.display = 'block';
            if (newAddressFormBasket) newAddressFormBasket.reset();
        });
    }

    if (newAddressFormBasket) {
        newAddressFormBasket.addEventListener('submit', (e) => {
            e.preventDefault();

            const inputs = newAddressFormBasket.querySelectorAll('input');
            const city = inputs[0]?.value.trim() || '';
            const street = inputs[1]?.value.trim() || '';
            const house = inputs[2]?.value.trim() || '';
            const apartment = inputs[3]?.value.trim() || '';
            const name = inputs[4]?.value.trim() || 'Новый адрес';

            if (!city || !street || !house) {
                alert('Пожалуйста, заполните обязательные поля: город, улица и дом');
                return;
            }

            const addressItem = document.createElement('div');
            addressItem.className = 'address-item';
            addressItem.innerHTML = `
                <p><strong>${escapeHtml(name)}</strong></p>
                <p>${escapeHtml(city)}, ${escapeHtml(street)}, д. ${escapeHtml(house)}${apartment ? ', кв. ' + escapeHtml(apartment) : ''}</p>
                <div class="address-actions">
                    <button class="btn-edit" type="button">Изменить</button>
                    <button class="btn-delete" type="button">Удалить</button>
                </div>
            `;

            if (addressListBasket) {
                addressListBasket.appendChild(addressItem);

                setTimeout(() => {
                    addressItem.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                }, 100);
            }

            newAddressFormBasket.reset();
            if (addressFormBasket) addressFormBasket.style.display = 'none';
            if (addAddressBtnBasket) addAddressBtnBasket.style.display = 'block';

            showNotification('Адрес успешно добавлен!', 'success');
        });
    }

    // ============================================
    // ОБРАБОТКА УДАЛЕНИЯ АДРЕСОВ
    // ============================================
    if (addressList) {
        addressList.addEventListener('click', (e) => {
            const addressItem = e.target.closest('.address-item');
            if (!addressItem) return;

            if (e.target.classList.contains('btn-delete')) {
                if (confirm('Вы уверены, что хотите удалить этот адрес?')) {
                    addressItem.remove();
                    showNotification('Адрес удален', 'info');
                }
            }

            if (e.target.classList.contains('btn-edit')) {
                const addressText = addressItem.querySelector('p:nth-child(2)').textContent;
                alert('Редактирование адреса\nТекущий адрес: ' + addressText + '\n\nФункция редактирования в разработке.');
            }
        });
    }

    if (addressListBasket) {
        addressListBasket.addEventListener('click', (e) => {
            const addressItem = e.target.closest('.address-item');
            if (!addressItem) return;

            if (e.target.classList.contains('btn-delete')) {
                if (confirm('Вы уверены, что хотите удалить этот адрес?')) {
                    addressItem.remove();
                    showNotification('Адрес удален', 'info');
                }
            }

            if (e.target.classList.contains('btn-edit')) {
                const addressText = addressItem.querySelector('p:nth-child(2)').textContent;
                alert('Редактирование адреса\nТекущий адрес: ' + addressText + '\n\nФункция редактирования в разработке.');
            }
        });
    }

    // ============================================
    // ОБРАБОТКА ПРОМОКОДОВ
    // ============================================
    if (promoForm && promoCodeInput) {
        promoForm.addEventListener('submit', (e) => {
            e.preventDefault();

            const promoCode = promoCodeInput.value.trim();

            if (promoCode) {
                const isValid = validatePromoCode(promoCode);

                if (isValid) {
                    const promoList = document.getElementById('promoList');
                    if (promoList) {
                        const newPromoItem = document.createElement('div');
                        newPromoItem.className = 'promo-item';
                        newPromoItem.innerHTML = `
                            <p><strong>${escapeHtml(promoCode)}</strong> - Новый промокод</p>
                            <span class="promo-status active">Активен</span>
                        `;

                        promoList.appendChild(newPromoItem);
                        promoCodeInput.value = '';

                        setTimeout(() => {
                            newPromoItem.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                        }, 100);

                        showNotification(`Промокод "${promoCode}" успешно применен!`, 'success');
                    }
                } else {
                    showNotification('Неверный промокод. Попробуйте другой.', 'error');
                }
            } else {
                showNotification('Введите промокод', 'warning');
            }
        });
    }

    // ============================================
    // CSRF ТОКЕН ДЛЯ ФОРМ
    // ============================================
    document.querySelectorAll('form').forEach(form => {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (token && !form.querySelector('input[name="__RequestVerificationToken"]')) {
            const tokenClone = token.cloneNode(true);
            form.appendChild(tokenClone);
        }
    });
});

// ============================================
// ДОПОЛНИТЕЛЬНЫЕ ГЛОБАЛЬНЫЕ ФУНКЦИИ
// ============================================
function removeFromFavourites(event, form, productId) {
    event.preventDefault();
    event.stopPropagation();

    if (confirm('Удалить товар из избранного?')) {
        const btn = form.querySelector('.favourites-btn');
        if (btn) {
            const originalHTML = btn.innerHTML;
            btn.innerHTML = '<span style="font-size:12px;">Удаление...</span>';
            btn.disabled = true;

            form.submit();

            setTimeout(() => {
                btn.innerHTML = originalHTML;
                btn.disabled = false;
            }, 3000);
        } else {
            form.submit();
        }
    }
    return false;
}
function addToCart(event, button, productId) {
    event.preventDefault();
    event.stopPropagation();

    const originalText = button.innerHTML;
    button.innerHTML = '<span>Добавляем...</span>';
    button.disabled = true;

    const form = button.closest('.add-to-cart-form');
    if (form) {
        form.submit();
    }

    setTimeout(() => {
        button.innerHTML = originalText;
        button.disabled = false;
    }, 3000);
}

function sortFavorites(sortType) {
    const container = document.querySelector('.cards-container');
    if (!container) return;

    const items = Array.from(container.querySelectorAll('.favorite-item'));

    items.sort((a, b) => {
        switch (sortType) {
            case 'newest':
                return new Date(b.dataset.addedDate) - new Date(a.dataset.addedDate);
            case 'oldest':
                return new Date(a.dataset.addedDate) - new Date(b.dataset.addedDate);
            case 'price_high':
                return parseFloat(b.dataset.price) - parseFloat(a.dataset.price);
            case 'price_low':
                return parseFloat(a.dataset.price) - parseFloat(b.dataset.price);
            case 'rating_high':
                return parseFloat(b.dataset.rating) - parseFloat(a.dataset.rating);
            case 'rating_low':
                return parseFloat(a.dataset.rating) - parseFloat(b.dataset.rating);
            default:
                return 0;
        }
    });

    container.innerHTML = '';
    items.forEach(item => container.appendChild(item));
}

// ============================================
// ДОПОЛНИТЕЛЬНЫЕ ОБРАБОТЧИКИ СОБЫТИЙ
// ============================================
document.addEventListener('DOMContentLoaded', function () {
    const sortSelect = document.getElementById('sortSelect');
    if (sortSelect) {
        sortSelect.addEventListener('change', function () {
            sortFavorites(this.value);
        });
    }

    const loginLink = document.getElementById('loginLinkFav');
    if (loginLink) {
        loginLink.addEventListener('click', function (e) {
            e.preventDefault();

            const loginModal = document.getElementById('loginModal');
            if (loginModal) {
                loginModal.style.display = 'block';
                document.body.style.overflow = 'hidden';
            }
        });
    }
});