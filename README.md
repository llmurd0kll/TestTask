# 📘 URL Shortener — сокращатель ссылок на ASP.NET Core + EF Core + MariaDB

![Build](https://img.shields.io/badge/build-passing-brightgreen)
![Tests](https://img.shields.io/badge/tests-passing-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)
![Platform](https://img.shields.io/badge/platform-.NET%209-blueviolet)
![Database](https://img.shields.io/badge/database-MariaDB-orange)

Небольшой сервис сокращения ссылок с поддержкой редиректов, счётчика кликов и оптимизацией под высокую нагрузку.
---

## 🚀 Возможности

- Создание коротких ссылок  
- Редирект по короткому коду  
- Подсчёт количества переходов  
- Просмотр всех ссылок  
- Редактирование длинного URL  
- Удаление ссылок  
- Криптостойкая генерация кодов  
- Кэширование горячего пути (MemoryCache)  
- SQL‑инкремент кликов  
- Автоматические миграции  

---

## 🛠 Технологии

- ASP.NET Core MVC  
- Entity Framework Core  
- MariaDB / MySQL  
- MemoryCache  
- SQLite InMemory (для тестов)  
- xUnit  

---

## 📦 Установка и запуск

### 1. Клонировать репозиторий

```bash
git clone https://github.com/your/repo.git
cd repo
```

### 2. Настроить строку подключения

Файл `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "server=localhost;database=urlshortener;user=root;password=yourpass"
}
```

### 3. Запустить приложение

```bash
dotnet run
```

При старте автоматически применяются миграции.

---

## 🧪 Тестирование

Запуск тестов:

```bash
dotnet test
```

Для тестов используется SQLite InMemory, так как EF InMemory не поддерживает SQL‑команды.

---

## ⚙️ Архитектура

| Компонент | Описание |
|----------|----------|
| `UrlsController` | Обработка HTTP‑запросов |
| `UrlService` | Бизнес‑логика, работа с БД, кэширование |
| `CodeGenerator` | Генерация криптостойких коротких кодов |
| `AppDbContext` | Работа с БД через EF Core |
| `ShortUrl` | Модель данных |

---

## 🚀 Оптимизации под нагрузку

- Увеличенная длина короткого кода (10 символов)  
- `AsNoTracking` для чтения  
- Прямой SQL‑инкремент кликов  
- MemoryCache для редиректов  
- Разделение чтения и инкремента  
- Индекс по `ShortCode`  

---

## 📂 Структура проекта

```
/Controllers
    UrlsController.cs
/Services
    UrlService.cs
    CodeGenerator.cs
/Data
    AppDbContext.cs
/Models
    ShortUrl.cs
/Views
```

---

## 🔒 Безопасность

- Криптостойкая генерация кодов  
- Валидация URL  
- EscapeUriString при редиректе  
- Нет утечек данных в логах  

---

## 📄 Лицензия

MIT