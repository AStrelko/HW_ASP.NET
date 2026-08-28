# Meetings API — ролі та доступ до endpoint-ів

## Ролі

У застосунку використовуються дві ролі:

- **User** — звичайний авторизований користувач.
- **Admin** — адміністратор із правами на створення, зміну та видалення даних, а також керування ролями.

Після реєстрації новому користувачу автоматично призначається роль `User`.
Змінювати роль користувача може лише `Admin`.

Для захищених endpoint-ів використовується JWT Bearer Authentication.

### HTTP-коди авторизації

- `401 Unauthorized` — користувач не автентифікований або JWT відсутній / недійсний.
- `403 Forbidden` — користувач автентифікований, але не має необхідних прав.

---

## Auth

| Метод | Endpoint | Доступ |
|---|---|---|
| POST | `/api/auth/register` | Усі |
| POST | `/api/auth/login` | Усі |

---

## Roles

Усі endpoint-и цього контролера доступні лише ролі `Admin`.

| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/api/roles` | Admin |
| GET | `/api/roles/participants/{participantId}` | Admin |
| PUT | `/api/roles/assign` | Admin |

---

## Meetings API v1

| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/api/v1/meetings` | Усі |
| GET | `/api/v1/meetings/{id}` | Авторизований User / Admin |
| POST | `/api/v1/meetings` | Admin |
| PUT | `/api/v1/meetings/{id}` | Admin |
| DELETE | `/api/v1/meetings/{id}` | Admin |

---

## Meetings API v2

| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/api/v2/meetings` | Усі |
| GET | `/api/v2/meetings/{id}` | Авторизований User / Admin |
| POST | `/api/v2/meetings` | Admin |
| PUT | `/api/v2/meetings/{id}` | Admin |
| PATCH | `/api/v2/meetings/{id}` | Admin |
| DELETE | `/api/v2/meetings/{id}` | Admin |
| DELETE | `/api/v2/meetings/delete-many` | Admin |
| GET | `/api/v2/meetings/by-participant/{participantId}` | Авторизований User / Admin |

---

## Participants API v2

| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/api/v2/participants` | Усі |
| GET | `/api/v2/participants/{id}` | Авторизований User / Admin |
| GET | `/api/v2/participants/{participantId}/avatar/file` | Авторизований User / Admin |
| PUT | `/api/v2/participants/{participantId}/avatar` | Admin |
| DELETE | `/api/v2/participants/{participantId}/avatar` | Admin |
| PUT | `/api/v2/participants/{id}` | Admin |
| PATCH | `/api/v2/participants/{id}` | Admin |
| DELETE | `/api/v2/participants/{id}` | Admin |
| DELETE | `/api/v2/participants/delete-many` | Admin |
| GET | `/api/v2/participants/{id}/meetings` | Авторизований User / Admin |

---

## Публічні файли зустрічей

| Метод | Endpoint | Доступ |
|---|---|---|
| POST | `/api/meetings/{meetingId}/attachments` | Admin |
| GET | `/api/meetings/{meetingId}/attachments` | Авторизований User / Admin |
| GET | `/api/meetings/{meetingId}/attachments/{attachmentId}/download` | Авторизований User / Admin |
| DELETE | `/api/meetings/{meetingId}/attachments/{attachmentId}` | Admin |

---

## Приватні файли учасників

Увесь `PrivateAttachmentsController` закритий атрибутом `[Authorize]`.

Додатково перевіряється відповідність `participantId` поточному авторизованому користувачу.

| Метод | Endpoint | Доступ |
|---|---|---|
| POST | `/api/participants/{participantId}/private-files/send` | Лише сам авторизований учасник-відправник |
| GET | `/api/participants/{participantId}/private-files/received` | Лише власник списку |
| GET | `/api/participants/{participantId}/private-files/sent` | Лише власник списку |
| GET | `/api/participants/{participantId}/private-files/{fileId}` | Відправник або отримувач файлу |
| GET | `/api/participants/{participantId}/private-files/{fileId}/download` | Відправник або отримувач файлу |
| DELETE | `/api/participants/{participantId}/private-files/{fileId}` | Відправник файлу або Admin |
| GET | `/api/private-files` | Admin |

Для приватних файлів одного `[Authorize]` недостатньо: контролер звіряє `participantId` із профілем поточного користувача, а сервіс додатково перевіряє право доступу до конкретного приватного файлу.

---

## JWT

Після успішного входу користувач отримує JWT access token.

Токен містить, зокрема:

- `sub` — ідентифікатор користувача ASP.NET Identity;
- `email`;
- `role`;
- `exp`;
- `iss`;
- `aud`.

Для роботи із захищеними endpoint-ами у Swagger необхідно натиснути **Authorize** та передати отриманий Bearer token.

Після зміни ролі користувача необхідно повторно виконати login і отримати новий JWT, оскільки вже виданий токен зберігає старе значення `role`.

---

## Коротка матриця доступу

| Операція | Anonymous | User | Admin |
|---|:---:|:---:|:---:|
| Реєстрація / login | ✅ | ✅ | ✅ |
| Перегляд списку зустрічей | ✅ | ✅ | ✅ |
| Перегляд списку учасників | ✅ | ✅ | ✅ |
| Детальна інформація | ❌ | ✅ | ✅ |
| Створення / зміна / видалення зустрічей | ❌ | ❌ | ✅ |
| Зміна / видалення учасників | ❌ | ❌ | ✅ |
| Перегляд публічних файлів | ❌ | ✅ | ✅ |
| Керування публічними файлами | ❌ | ❌ | ✅ |
| Робота зі своїми приватними файлами | ❌ | ✅ | ✅ |
| Перегляд усіх приватних файлів | ❌ | ❌ | ✅ |
| Керування ролями | ❌ | ❌ | ✅ |
